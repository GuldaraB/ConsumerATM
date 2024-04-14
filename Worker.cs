using Confluent.Kafka;
using ConsumerATM.Models;
using ConsumerATM.Services.Ivanti;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ConsumerATM
{
    public class Worker : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly IIvantiService _ivanti;
        private List<IvantiDirectory> _ivantiDirectories = new List<IvantiDirectory>();
        private System.Threading.Timer _timer;

        public Worker(IConfiguration config, IIvantiService ivanti)
        {
            _config = config;
            _ivanti = ivanti;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {            
            Log.Information("!!!STARTED APP!!!");
            var task = Task.Run(() => GetProcess(stoppingToken), stoppingToken);
        }

        public async void Count(object obj)
        {            
            var list = await _ivanti.GetActivIvantyDirectory(_config.GetValue<string>("Ivanti:GetIvantiUrl"));
            _ivantiDirectories = list;
        }

        private async Task GetProcess(CancellationToken ct)
        {
            _ivanti.SetHttpAuthorization(_config.GetValue<string>("Ivanti:IvantiKeyName"), _config.GetValue<string>("Ivanti:IvantiKeyValue"));
            _timer = new System.Threading.Timer(Count, null, 0, 15 * 60 * 1000);

            var consumerCfg = new ConsumerConfig
            {
                BootstrapServers = _config.GetValue<string>("Kafka:BootstrapServers"),
                GroupId = _config.GetValue<string>("Kafka:GroupId"),
                EnableAutoCommit = false,
                SaslUsername = _config.GetValue<string>("Kafka:Username"), 
                SaslPassword = _config.GetValue<string>("Kafka:Password"),
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.ScramSha256,
                EnableSslCertificateVerification = false,
                SessionTimeoutMs = _config.GetValue<int>("Kafka:SessionTimeoutMS"),
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            var topic = _config.GetValue<string>("Kafka:Consumer:TopicName");
            Log.Debug($"topic={topic}");
            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true; // prevent the process from terminating.
                cts.Cancel();
            };
            using (var consumer = new ConsumerBuilder<string, string>(
                       consumerCfg).Build())
            {
                consumer.Subscribe(topic);
                try
                {
                    while (!ct.IsCancellationRequested)
                    {
                        var cr = consumer.Consume(cts.Token);
                        Log.Debug(
                            $"Consumed event from topic {topic} with key {cr.Message.Key,-10} and value {cr.Message.Value}");
                        var message = JsonConvert.DeserializeObject<KafkaMessage>(cr.Message.Value);
                        var eventATM = await _ivanti.CreateEvent(message, _ivantiDirectories);
                        await _ivanti.SendEventToIvanti(eventATM, _config.GetValue<string>("Ivanti:IvantiPutUrl"));
                    }
                }
                catch (OperationCanceledException)
                {
                    Log.Error(" Ctrl-C was pressed...");// Ctrl-C was pressed.
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
                finally
                {
                    consumer.Close();
                }
            }
        }
    }
}
