using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DibsBot.Models
{
    public class FindRoomRequest
    {
        public DateTime From { get; set; } = DateTime.Now;
        public DateTime To { get; set; } = DateTime.Now.AddHours(1);
        public TimeSpan Duration { get; set; } = new TimeSpan(1, 0, 0);
    }
}