using ConsumerATM.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsumerATM.Services.Convertor
{
    public class ConvertorService : IConvertorService
    {
        private readonly IConfigurationRoot _configuration;
        public ConvertorService(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }
        public async Task<string> ConvertToEvent(KafkaMessage message)
        {
            var stringPars = message.CcomMsg.Content.Text.Split(' ');

            return stringPars.ToString();
        }
    }
}
