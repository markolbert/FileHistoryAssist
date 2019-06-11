using System.Collections.Generic;
using J4JSoftware.Logging;
using Twilio.Types;

namespace FileHistoryService
{
    public class TwilioConfig : ITwilioConfig
    {
        public string AccountSID { get; set; }
        public string AccountToken { get; set; }
        public string FromNumber { get; set; }
        public List<PhoneNumber> Recipients { get; } = new List<PhoneNumber>();
    }
}
