using System.Windows;
using System.Windows.Media;

namespace CourseApp.classes.information
{
    public class MeetingDisplayItem
    {
        public string MeetingId { get; set; } = "";
        public string PartnerName { get; set; } = "";
        public string DirectionText { get; set; } = "";
        public string DateText { get; set; } = "";
        public string LocationText { get; set; } = "";
        public string StatusText { get; set; } = "";
        public Brush StatusBackground { get; set; } = Brushes.Transparent;
        public Brush StatusForeground { get; set; } = Brushes.Black;
        public Visibility AcceptVisibility { get; set; }
        public Visibility DeclineVisibility { get; set; }
        public Visibility CancelVisibility { get; set; }
    }
}