using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace ConsumerATM.Models
{
    public class IvantiDirectory
    {
        [JsonPropertyName("HARDWARE_TYPE")]
        public string HardwareType { get; set; }

        [JsonPropertyName("NAME")]
        public string Name { get; set; }

        [JsonPropertyName("ERROR_LEVEL")]
        public string ErrorLevel { get; set; }

        [JsonPropertyName("ERROR_STATUS")]
        public string ErrorStatus { get; set; }
    }

    public class IvantiReturnType
    {
        [JsonPropertyName("@odata.context")]
        public string Context { get; set; }
        [JsonPropertyName("@odata.count")]
        public int Data { get; set; }
        [JsonPropertyName("value")]
        public List<IvantiDirectory> IvantiDirectorys { get; set; }
    }
}
