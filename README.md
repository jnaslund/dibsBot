# dibsBot
Mr Dibs helps you find available meeting rooms on your office 365 calendar leveraging Microsoft Graph Api and LUIS

The application have a few key dependcies that will need to be setup and configured.

* **Microsoft Bot Framework** - Bot needs to be registered as an app via https://dev.botframework.com/ and ment to be hosted in Azure.
* **LUIS** - Translates user input into intents for the bot to interpret, very useful for different dates, times and time ranges. For more info go to LUIS.ai
* **Microsoft Graph API** - Leveraging the Calendar api to find availalble meeting rooms. https://developer.microsoft.com/en-us/graph
* **Authbot** - Contains functionality to authenticate user against their microsoft account, this allows the bot to call graph api with proper permissions. https://github.com/MicrosoftDX/AuthBot

This was built, mainly for me to get a better understanding of the MS bot framework and get some utility out of it at the same time, so the code is a bit rough in places.
