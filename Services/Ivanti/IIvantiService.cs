using ConsumerATM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsumerATM.Services.Ivanti
{
    public interface IIvantiService
    {
        void SetHttpAuthorization(string keyName, string keyValue);
        Task<List<IvantiDirectory>> GetActivIvantyDirectory(string url);
        Task<bool> SendEventToIvanti(IvantiEvent ivantiEvents, string url);
        Task<IvantiEvent> CreateEvent(KafkaMessage kafkaMessage, List<IvantiDirectory> ivantiDirectories);
    }
}
