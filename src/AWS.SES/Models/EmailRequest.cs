using System.Collections.Generic;

namespace AWS.SES.Models
{
    public class EmailRequest
    {
        public string From { get; set; }
        public List<string> To { get; set; } = new List<string>();
        public string ReplyTo { get; set; }
        public string Subject { get; set; }
        public string HtmlBody { get; set; }
    }
}
