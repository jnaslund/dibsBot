using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DibsBot.Models;
using Microsoft.Bot.Builder.History;
using System.Web.Configuration;

namespace DibsBot.Services
{
    public class CalendarService
    {
        private readonly GraphServiceClient _graphClient = null;

        public CalendarService(GraphServiceClient graphClient)
        {
            _graphClient = graphClient;
        }

        public async Task<MeetingTimeSuggestionsResult> FindMeetingSuggestion(FindRoomRequest request)
        {

            List<Attendee> attendees = new List<Attendee>();
            // Adding placeholder user to be able to remove the
            // Required flag on the user looking for available rooms.
            // An empty collection causes findMeetingTimes to look for free time slots for only the organizer.
            if (!string.IsNullOrEmpty(WebConfigurationManager.AppSettings["PlaceholderAttendeeEmail"]))
            {
                attendees = new List<Attendee>()
                {
                    new Attendee()
                    {
                        EmailAddress = new EmailAddress()
                        {
                            Address = WebConfigurationManager.AppSettings["PlaceholderAttendeeEmail"]
                        }
                    }
                };
            }
            var locationConstraint = new LocationConstraint()
            {
                IsRequired = true,
                SuggestLocation = true,
            };
            var timeConstraint = new TimeConstraint()
            {
                ActivityDomain = ActivityDomain.Unrestricted,
                Timeslots = new List<TimeSlot>()
                {
                    new TimeSlot()
                    {
                        Start = new DateTimeTimeZone()
                        {
                            DateTime = request.From.ToString(),
                            TimeZone = "UTC"
                        },
                        End = new DateTimeTimeZone()
                        {
                            DateTime = request.To.ToString(),
                            TimeZone = "UTC"
                        }
                    }
                }

            };
            return await _graphClient.Me.FindMeetingTimes(attendees, TimeConstraint: timeConstraint, MeetingDuration: new Duration(request.Duration), IsOrganizerOptional: attendees.Any(), LocationConstraint: locationConstraint)
                .Request()
                .PostAsync();
        }
    }
}