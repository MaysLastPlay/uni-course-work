using System;

namespace CourseApp.classes.information
{
    public class ArchivedCouple
    {
        public string Client1Name { get; set; } = string.Empty;
        public string Client2Name { get; set; } = string.Empty;
        public DateTime ArchiveDate { get; set; }
        public string StatusInfo { get; set; } = "Активна пара (Архів)";
    }
}