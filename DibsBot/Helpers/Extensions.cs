using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Afk.ZoneInfo;
using Microsoft.Bot.Builder.Dialogs;

namespace DibsBot.Helpers
{
    public class DateTimeHelper
    {
        public static DateTime Floor(DateTime dateTime, TimeSpan interval)
        {
            return dateTime.AddTicks(-(dateTime.Ticks % interval.Ticks));
        }

        public static DateTime Ceiling(DateTime dateTime, TimeSpan interval)
        {
            var overflow = dateTime.Ticks % interval.Ticks;

            return overflow == 0 ? dateTime : dateTime.AddTicks(interval.Ticks - overflow);
        }

        public static DateTime Round(DateTime dateTime, TimeSpan interval)
        {
            var halfIntervelTicks = (interval.Ticks + 1) >> 1;

            return dateTime.AddTicks(halfIntervelTicks - ((dateTime.Ticks + halfIntervelTicks) % interval.Ticks));
        }
    }

    public static class DateTimeExtensions
    {
        public static DateTime ConvertToUniversalTime(this DateTime localTime, string timezoneName)
        {
            localTime = DateTime.SpecifyKind(localTime, DateTimeKind.Local);
            var zone = TzTimeInfo.GetZone(timezoneName);
            return zone.ToUniversalTime(localTime);

        }
    }


    public static class ContextExtensions
    {
        public static string CONTEXT_KEY_USERDATA_USERTIMEZONE = "dibs_userdata_usertimezone";

        public static bool UserTimeZoneSet(this IBotContext context)
        {
            if (!context.UserData.TryGetValue<string>(CONTEXT_KEY_USERDATA_USERTIMEZONE, out var userTimezone) || userTimezone == null)
                return false;
            return true;
        }

        public static string GetUserTimeZoneName(this IBotContext context)
        {
            if (!context.UserData.TryGetValue<string>(CONTEXT_KEY_USERDATA_USERTIMEZONE, out var userTimezone) || userTimezone == null)
                return null;

            return userTimezone;
        }

        public static void SetUserTimeZoneName(this IBotContext context, string timeZoneId)
        {
            context.UserData.SetValue<string>(CONTEXT_KEY_USERDATA_USERTIMEZONE, timeZoneId);          
        }
    }
}