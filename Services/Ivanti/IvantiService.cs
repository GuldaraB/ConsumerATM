using ConsumerATM.Models;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConsumerATM.Services.Ivanti
{
    public class IvantiService : IIvantiService
    {
        private HttpClient _httpClient = new HttpClient();
        private readonly IConfiguration _config;

        public IvantiService(IConfiguration config)
        {
            _config = config;
        }

        public void SetHttpAuthorization(string keyName, string keyValue)
        {
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(keyName, keyValue);
        }

        public async Task<List<IvantiDirectory>> GetActivIvantyDirectory(string url)
        {
            Log.Debug("Start retrieving Ivanti directorys");
            var ivantiDirectorys = new List<IvantiDirectory>();
            using (var response = await _httpClient.GetAsync(url))
            {
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var returnType = JsonSerializer.Deserialize<IvantiReturnType>(responseString);
                    if (returnType != null)
                    {
                        ivantiDirectorys = returnType.IvantiDirectorys.ToList();
                        Log.Debug($"Ivanti response: {response.StatusCode}. Ivanti directory with no ending time: {ivantiDirectorys.Count}");
                    }
                }
                else
                {
                    Log.Error($"Could not get the Ivanti directorys. Status code: {response.StatusCode}");
                }
            }
            return ivantiDirectorys;
        }

        public async Task<bool> SendEventToIvanti(IvantiEvent ivantiEvents, string url)
        {
            var jsonStr = JsonSerializer.Serialize(ivantiEvents);
            var httpContent = new StringContent(jsonStr, Encoding.UTF8, "application/json");
            using (var cts = new CancellationTokenSource(30000))
            {
                using (var response = await _httpClient.PostAsync(url, httpContent))
                {
                    Log.Debug($"Ivanti events response: {response.StatusCode}");
                    if (response.IsSuccessStatusCode)
                        return true;
                    return false;
                }
            }
        }

        public async Task<IvantiEvent> CreateEvent(KafkaMessage kafkaMessage, List<IvantiDirectory> ivantiDirectories)
        {
            var stringPars = kafkaMessage.CcomMsg.Content.Text.Split(" -#- ");
            var ATMWarning = stringPars[0].Split(null, 2);
            string status = "";
            string p2 = "";
            string p3 = "";
            string p4 = "";
            string p5 = "";
            foreach (var ivantiDirectory in ivantiDirectories)
            {
                if (ivantiDirectory.Name == ATMWarning[1])
                {
                    p2 = stringPars[1];
                    p3 = ivantiDirectory.HardwareType;
                    p4 = ivantiDirectory.ErrorStatus;
                    p5 = ivantiDirectory.ErrorLevel;
                    switch (ivantiDirectory.ErrorLevel)
                    {
                        case "Error":
                            status = "Open";
                            break;
                        case "Information":
                            status = "Closed";
                            break;
                        case "Warning":
                            status = "Warning";
                            break;
                    }
                    if (stringPars[1].Contains("csst1"))
                    {
                        var cassettes = stringPars[1].Split(";");
                        int firstSpaceIndex = cassettes[0].IndexOf(' ');
                        int secondSpaceIndex = cassettes[0].IndexOf(' ', firstSpaceIndex + 1);
                        if (secondSpaceIndex > 0)
                            p2 = cassettes[0].Substring(0, secondSpaceIndex);

                        List<string> cssValues = new List<string>();
                        
                        for (int i = 0; i < cassettes.Length; i++)
                        {
                            var cassette = cassettes[i].Split("=");
                            cssValues.Add(cassette.Length <= 9 & cassette.Length > 1 ? cassette[1] : "");
                        }
                        int indexChar = cssValues[7].IndexOf('C');
                        var ccl = indexChar >= 0 ? cssValues[7].Substring(0, indexChar) : "";
                        IvantiEvent ivantiEventCss = new IvantiEvent()
                        {
                            Description = kafkaMessage.CcomMsg.Content.Text,
                            Status = status,
                            Source = "Network Monitor",
                            EventStartDateTime = DateTime.Now,
                            P1 = ATMWarning[0],
                            P2 = p2,
                            P3 = p3,
                            P4 = p4,
                            P5 = p5,
                            P6 = ATMWarning[1],
                            csst1 = cssValues[0],
                            csst2 = cssValues[1],
                            csst3 = cssValues[2],
                            csst4 = cssValues[3],
                            csst5 = cssValues[4],
                            csst6 = cssValues[5],
                            csst7 = cssValues[6],
                            CCL_N = ccl,
                            CCL2_N = cssValues[8],
                        };
                        return ivantiEventCss;
                    }
                }
            }            
            IvantiEvent ivantiEvent = new IvantiEvent()
            {
                Description = kafkaMessage.CcomMsg.Content.Text,
                Status = status,
                Source = "Network Monitor",
                EventStartDateTime = DateTime.Now,
                P1 = ATMWarning[0],
                P2 = p2,
                P3 = p3,
                P4 = p4,
                P5 = p5,
                P6 = ATMWarning[1]
            };
            return ivantiEvent;
        }
    }
}
