using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using AuthBot;
using AuthBot.Dialogs;
using AuthBot.Models;
using DibsBot.Helpers;
using GeoTimeZone;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Location;
using Microsoft.Bot.Connector;

namespace DibsBot.Dialogs
{
    [Serializable]
    public class GraphDialog : LuisDialog<object>
    {
        private const string DefaultTimeZone = "Central Standard Time";

        #pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task StartAsync(IDialogContext context)
        #pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            context.Wait(MessageReceived);
        }


        protected override async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            var message = await item;
            if (message.Text.Equals("help", StringComparison.CurrentCultureIgnoreCase))
            {
                var helptext =
                    "I'm able to return room availability based on your office 365 calendar. Based on date and time I'll check for available meeting rooms. \n\n" +
                    "Intents are parsed using luis.ai resolving date input, current version does not support booking a room but is on my very unoffical roadmap ;) \n\n" +
                    "**Examples:**\n\n" +
                    "* **login** - for logging in to office 365\n\n" +
                    "* **logout** - logout of current microsoft account.\n\n" +
                    "* **location** - selecting current location to get accurate timezone settings.\n\n" +
                    "* **available rooms** - checks for available rooms right now in 30min blocks\n\n" +
                    "* **rooms available at 9am** - checks for available 1 hours meeting slots at 9am today, or tomorrow depending on time of day.\n\n" +
                    "* **available rooms tomorrow between 10am and 12am** - looks for a 2 hour block starting at 10am\n\n";
                var msg = context.MakeMessage();
                msg.TextFormat = "markdown";
                msg.Text = helptext;
                await context.PostAsync(msg);
                context.Wait(this.MessageReceived);
            }
            else if (string.IsNullOrEmpty(await context.GetAccessToken(AuthSettings.Scopes)))
            {
                // Start authentication dialog
                await context.PostAsync($"Please sign in below.");
                await context.Forward(new AzureAuthDialog(AuthSettings.Scopes), this.ResumeAfterAuth, message, CancellationToken.None);
            }
            else if (message.Text == "logout")
            {
                // Process logout message
                await context.Logout();
                context.Wait(this.MessageReceived);
            }
            else if (!context.UserTimeZoneSet() || message.Text.Equals("location", StringComparison.CurrentCultureIgnoreCase))
            {
                var prompt = "I need to know your locations timezone to accurately provide meeting times, please let me know your location.";
                var apiKey = WebConfigurationManager.AppSettings["BingMapsApiKey"];
                var locationDialog = new LocationDialog(apiKey, message.ChannelId, prompt, LocationOptions.ReverseGeocode | LocationOptions.SkipFavorites | LocationOptions.SkipFinalConfirmation, LocationRequiredFields.Country | LocationRequiredFields.Region, new DibsLocationResourceManager());
                context.Call(locationDialog, this.ResumeAfterLocationDialogAsync);
            }
            else
            {
                // Process incoming message
                await base.MessageReceived(context, item);

            }
        }


        private async Task ResumeAfterAuth(IDialogContext context, IAwaitable<string> result)
        {
            var message = await result;

            await context.PostAsync(message);
            if(!context.UserTimeZoneSet())
                await context.PostAsync("Next I need to know what timezone you are in, type or say **location** to configure it.");

            context.Wait(MessageReceived);
        }

        private async Task ResumeAfterLocationDialogAsync(IDialogContext context, IAwaitable<Place> result)
        {
            Place place = await result;
            if (place != null)
            {
                var geoCord = place.GetGeoCoordinates();
                if (geoCord.Longitude.HasValue && geoCord.Latitude.HasValue)
                {
                    var tz = TimeZoneLookup.GetTimeZone(geoCord.Latitude.Value, geoCord.Longitude.Value);
                    try
                    {
                        context.SetUserTimeZoneName(tz.Result);
                        await context.PostAsync($"Time Zone set to {tz.Result}, You can now ask me for meeting room availability, type help for more information.");                        
                    }
                    catch (Exception)
                    {
                        // set default instead and inform the user.
                        context.SetUserTimeZoneName(DefaultTimeZone);
                        await context.PostAsync($"Unable to resolve timezone for {place.Name}. Time Zone set to Default {DefaultTimeZone}");

                    }
                }
                else
                {
                    // set default timezone if location return is missing coordinates
                    // set default instead and inform the user.
                    context.SetUserTimeZoneName(DefaultTimeZone);
                    await context.PostAsync($"Unable to resolve timezone for {place.Name}. Time Zone set to Default {DefaultTimeZone}");
                }
                
            }
            else
            {
                await context.PostAsync("OK, cancelled");
            }
            context.Done<string>(null);
        }
    }
}