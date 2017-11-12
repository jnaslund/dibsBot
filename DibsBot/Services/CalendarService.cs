using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DibsBot.Models;
using Microsoft.Bot.Builder.History;

namespace DibsBot.Services
{
    public class CalendarService
    {
        private GraphServiceClient _graphClient = null;

        public CalendarService(GraphServiceClient graphClient)
        {
            _graphClient = graphClient;
        }
        public async Task<MeetingTimeSuggestionsResult> FindMeetingSuggestion(FindRoomRequest request)
        {
            // Adding placeholder user to be able to remove the
            // Required flag on the user looking for available rooms.
            var attendees = new List<Attendee>()
            {
                new Attendee()
                {
                    EmailAddress = new EmailAddress() { Address = "work@makingwaves.com" }
                }
            };
            var locationConstraint = new LocationConstraint()
            {
                IsRequired = true,
                SuggestLocation = true,
                //Locations = new List<LocationConstraintItem>()
                //{
                //    new LocationConstraintItem()
                //    {
                //        LocationEmailAddress = "roomchi-library@makingwaves.com"
                //    },
                //    new LocationConstraintItem()
                //    {
                //        LocationEmailAddress = "roomchi-pink@makingwaves.com"
                //    },
                //    new LocationConstraintItem()
                //    {
                //        LocationEmailAddress = "roomchi-cabin@makingwaves.com"
                //    },
                //    new LocationConstraintItem()
                //    {
                //        LocationEmailAddress = "roomchi-fishbowl@makingwaves.com"
                //    }
                //}
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
            return await _graphClient.Me.FindMeetingTimes(Attendees: attendees, TimeConstraint: timeConstraint, MeetingDuration: new Duration(request.Duration), IsOrganizerOptional: true, LocationConstraint: locationConstraint)
                .Request()
                .PostAsync();
        }
    }
}