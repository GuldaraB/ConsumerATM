using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsumerATM.Helper
{
    public class DurationParser
    {
        public static TimeSpan Parse(string duration)
        {
            int year = 0, week = 0, day = 0, hour = 0, minute = 0, second = 0;
            var regex = new Regex("((([0-9]+)y)?(([0-9]+)w)?(([0-9]+)d)?(([0-9]+)h)?(([0-9]+)m)?(([0-9]+)s)?(([0-9]+)ms)?|0)");
            var result = regex.Match(duration);
            //года
            if (!string.IsNullOrWhiteSpace(result.Groups[3].Value))
                year += Convert.ToInt32(result.Groups[3].Value);
            //недели
            if (!string.IsNullOrWhiteSpace(result.Groups[5].Value))
                week += Convert.ToInt32(result.Groups[5].Value);
            //дни
            if (!string.IsNullOrWhiteSpace(result.Groups[7].Value))
                day += Convert.ToInt32(result.Groups[7].Value);
            //часы
            if (!string.IsNullOrWhiteSpace(result.Groups[9].Value))
                hour += Convert.ToInt32(result.Groups[9].Value);
            //минуты
            if (!string.IsNullOrWhiteSpace(result.Groups[11].Value))
                minute += Convert.ToInt32(result.Groups[11].Value);
            //секунды
            if (!string.IsNullOrWhiteSpace(result.Groups[13].Value))
                second += Convert.ToInt32(result.Groups[13].Value);
            return new TimeSpan(year * 365 + week * 7 + day, hour, minute, second);
        }
    }
}
