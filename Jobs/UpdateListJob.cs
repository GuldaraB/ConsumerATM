using ConsumerATM.Models;
using ConsumerATM.Services.Ivanti;
using Microsoft.Extensions.Configuration;
using Quartz;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsumerATM.Jobs
{
    [DisallowConcurrentExecution]
    public class UpdateListJob : IJob
    {
        private static List<IvantiDirectory> ivantiDirectories = new List<IvantiDirectory>();
        private readonly IConfiguration _config;
        private readonly IIvantiService _ivanti;

        public UpdateListJob(IIvantiService ivanti, IConfiguration config)
        {
            _ivanti = ivanti;
            _config = config;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                Console.WriteLine("Start execute");
                _ivanti.SetHttpAuthorization(_config.GetValue<string>("Ivanti:IvantiKeyName"), _config.GetValue<string>("Ivanti:IvantiKeyValue"));
                var list = await _ivanti.GetActivIvantyDirectory(_config.GetValue<string>("Ivanti:GetIvantiUrl"));
                ivantiDirectories = list;
                Console.WriteLine(list.Count);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
        public static List<IvantiDirectory> GetDataList()
        {
            return ivantiDirectories;
        }
    }
}
