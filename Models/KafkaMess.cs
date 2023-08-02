using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsumerATM.Models
{
    public class CcomMsg
    {
        public string SvcType { get; set; }
        public string To { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public MsgData MsgData { get; set; }
        public Content Content { get; set; }
    }

    public class Content
    {
        public string Type { get; set; }
        public string Text { get; set; }
    }

    public class MsgData
    {
        public string Parm { get; set; }
    }

    public class KafkaMessage
    {
        public string MsgId { get; set; }
        public string ChainMsgId { get; set; }
        public CcomMsg CcomMsg { get; set; }
    }
}
