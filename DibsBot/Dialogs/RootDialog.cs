using AuthBot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Services.Description;
using System.Web.UI;
using AuthBot.Dialogs;
using AuthBot.Models;
using Chronic;
using DibsBot.Helpers;
using DibsBot.Models;
using DibsBot.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Microsoft.Graph;
using Newtonsoft.Json.Linq;

namespace DibsBot.Dialogs
{
    [LuisModel("5c61bcbe-8691-4330-8ed8-4d4ecab875fc", "f95da37a84344be7abef8face07f74c9")]
    [Serializable]
    public class RootDialog : GraphDialog
    {
        private const string EntityDateTimeRange = "builtin.datetimeV2.datetimerange";
        private const string EntityTimeRange = "builtin.datetimeV2.timerange";
        private const string EntityDatetimeDuration = "builtin.datetimeV2.duration";
        private const string EntityDate = "builtin.datetimeV2.datetime";
        private const string EntityTime = "builtin.datetimeV2.time";

        // Hard coding this is a hack, either need to add a question to the user for their location, or
        // look at accessing email settings via the graph api. That requires specific permissions though.

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry, I'm not sure what you are tring to achieve here... Type 'help' if you need assistance";
            await context.PostAsync(message);
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("AvailableRooms")]
        public async Task GetAvailableRooms(IDialogContext context, LuisResult result)
        {
  
            var entityInput = "The next hour";
            var request = new FindRoomRequest();
            var entity = result.Entities.FirstOrDefault();
            if (entity != null && entity.Resolution.Any())
            {
                entityInput = entity.Entity;
                Dictionary<string, object> val;
                if(((List<object>)entity.Resolution["values"]).Count > 1)
                    val = ((List<object>)entity.Resolution["values"])[1] as Dictionary<string, object>;
                else
                    val = ((List<object>) entity.Resolution["values"]).FirstOrDefault() as Dictionary<string, object>;

                switch (entity.Type)
                {
                    case EntityDatetimeDuration:
                        if (val != null && ((string) val["type"]) == "duration")
                        {
                            var seconds = ((string) val["value"]);
                            request.From = DateTimeHelper.Floor(DateTime.Now, new TimeSpan(0, 30, 0));
                            request.To = request.From;
                            request.Duration = new TimeSpan(0, 0, int.Parse(seconds));
                        }
                        break;
                    case EntityDateTimeRange:
                        if (val != null && ((string) val["type"]) == "datetimerange")
                        {
                            var start = ((string) val["start"]);
                            var end = ((string) val["end"]);
                            request.From = DateTime.Parse(start).ConvertToUniversalTime(context.GetUserTimeZoneName());
                            request.To = DateTime.Parse(end).ConvertToUniversalTime(context.GetUserTimeZoneName());
                            request.Duration = request.To - request.From;
                        }
                        break;
                    case EntityTimeRange:
                        if (val != null && ((string) val["type"]) == "timerange")
                        {
                            var start = ((string) val["start"]);
                            var end = ((string) val["end"]);
                            var serverDate = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Unspecified);

                            var from = serverDate.Add(TimeSpan.Parse(start, CultureInfo.InvariantCulture)).ConvertToUniversalTime(context.GetUserTimeZoneName());
                            var to = serverDate.Add(TimeSpan.Parse(end, CultureInfo.InvariantCulture)).ConvertToUniversalTime(context.GetUserTimeZoneName());
                            if (from < DateTime.Now)
                            {
                                from = from.AddDays(1);
                                to = to.AddDays(1);
                            }
                            request.From = from;
                            request.To = to;
                            var diff = to - from;
                            if (diff.TotalMinutes < 60)
                                request.Duration = new TimeSpan(0, 30, 0);
                        }

                        break;
                    case EntityDate:
                        if (val != null && ((string) val["type"]) == "datetime")
                        {
                            var value = ((string) val["value"]);
                            request.From = DateTimeHelper.Floor(DateTime.Parse(value), new TimeSpan(0, 30, 0)).ConvertToUniversalTime(context.GetUserTimeZoneName());
                            request.To = request.From;
                        }
                        break;
                    case EntityTime:
                        if (val != null && ((string) val["type"]) == "time")
                        {
                            var value = ((string) val["value"]);
                            var localTime = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Unspecified);
                            var date = localTime.Add(TimeSpan.Parse(value, CultureInfo.InvariantCulture)).ConvertToUniversalTime(context.GetUserTimeZoneName());
                            if (date < DateTime.Now)
                            {
                                date = date.AddDays(1);
                            }
                            request.From = date;
                            request.To = date;
                            request.Duration = new TimeSpan(0, 30, 0);
                        }
                        break;
                    default:
                        var now = DateTimeHelper.Floor(DateTime.Now, new TimeSpan(0, 30, 0)).ConvertToUniversalTime(context.GetUserTimeZoneName());
                        request.From = now;
                        request.To = now.AddHours(1);
                        request.Duration = new TimeSpan(0, 30, 0);
                        break;
                }
            }
            else
            {
                var now = DateTimeHelper.Floor(DateTime.Now, new TimeSpan(0, 30, 0));
                request.From = now;
                request.To = now.AddHours(1);
                request.Duration = new TimeSpan(0, 30, 0);
            }

            var graphClient = GraphHelper.GetAuthenticadedClient(await context.GetAccessToken(AuthSettings.Scopes), context.GetUserTimeZoneName());            
            var calendarService = new CalendarService(graphClient);
            var response = await calendarService.FindMeetingSuggestion(request);
            var messageContent = string.Empty;
            if (response.MeetingTimeSuggestions.Any())
            {
                var sb = new StringBuilder();
                sb.Append($"{entityInput}, the following rooms are available: \n\n");
                foreach (var mSuggestion in response.MeetingTimeSuggestions)
                {
                    var start = DateTime.Parse(mSuggestion.MeetingTimeSlot.Start.DateTime).ToShortTimeString();
                    var end = DateTime.Parse(mSuggestion.MeetingTimeSlot.End.DateTime).ToShortTimeString();
                    var filteredRooms = mSuggestion.Locations.Where(l => !string.IsNullOrEmpty(l.LocationEmailAddress));
                    sb.Append($"**{start} - {end}** \n\n");
                    if (filteredRooms.Any())
                    {
                        foreach (var location in filteredRooms)
                        {
                            sb.Append($"* {location.DisplayName}\n\n");
                        }
                    }
                    else
                    {
                        sb.Append("No meeting rooms available\n\n");
                    }

                }
                messageContent = sb.ToString();
            }
            else
            {
                messageContent = "I'm unable to find any available meeting rooms.";
            }

            var message = context.MakeMessage();
            message.TextFormat = "markdown";
            message.Text = messageContent;
            await context.PostAsync(message);
            context.Wait(this.MessageReceived);
        }
    }
}