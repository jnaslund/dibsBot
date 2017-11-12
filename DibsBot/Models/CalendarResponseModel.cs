using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DibsBot.Models
{
 
    public class MeetingTimeSuggestionResponse
    {
        public string odatacontext { get; set; }
        public string emptySuggestionsReason { get; set; }
        public Meetingtimesuggestion[] meetingTimeSuggestions { get; set; }
    }

    public class Meetingtimesuggestion
    {
        public int confidence { get; set; }
        public string organizerAvailability { get; set; }
        public Meetingtimeslot meetingTimeSlot { get; set; }
        public object[] attendeeAvailability { get; set; }
        public Location[] locations { get; set; }
    }

    public class Meetingtimeslot
    {
        public Start start { get; set; }
        public End end { get; set; }
    }

    public class Start
    {
        public DateTime dateTime { get; set; }
        public string timeZone { get; set; }
    }

    public class End
    {
        public DateTime dateTime { get; set; }
        public string timeZone { get; set; }
    }

    public class Location
    {
        public string displayName { get; set; }
        public string locationEmailAddress { get; set; }
    }

}