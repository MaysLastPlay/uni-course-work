using CourseApp.classes.enums;

namespace CourseApp.classes.information
{
    public class PRequirements
    {
        public Gender PreferredGender { get; set; }
        public int MinAge { get; set; } = 18;
        public int MaxAge { get; set; } = 99;
        public string? CriteriaDescription { get; set; }
    }
}