using System;
using CourseApp.classes.enums;

namespace CourseApp.classes.information
{
    public class Meetings
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SenderRegNumber { get; set; } = "";
        public string SenderName { get; set; } = "";
        public string ReceiverRegNumber { get; set; } = "";
        public string ReceiverName { get; set; } = "";
        public DateTime ScheduledDate { get; set; }
        public string Location { get; set; } = "";
        public MeetingStatus Status { get; set; } = MeetingStatus.Pending;
    }
}
