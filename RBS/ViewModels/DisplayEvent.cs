using System;

namespace RBS.ViewModels
{
    public class DisplayEvent
    {
        public string Subject { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public DisplayEvent(string subject, string start, string end)
        {
            this.Subject = subject;
            this.Start = DateTime.Parse(start);
            this.End = DateTime.Parse(end);
        }
    }
}