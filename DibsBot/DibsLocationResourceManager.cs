using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.Location;
using Microsoft.Identity.Client;

namespace DibsBot
{
    [Serializable]
    public class DibsLocationResourceManager : LocationResourceManager
    {
        public override string ConfirmationAsk =>
            "OK, I will use {0} to resolve your preffered timezone, Is that correct? Enter 'yes' or 'no'.";

        public override string TitleSuffix => "\nType or say a location, for example Chicago, IL";
    }
}