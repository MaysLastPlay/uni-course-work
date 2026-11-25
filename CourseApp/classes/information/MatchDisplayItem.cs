using System.Collections.Generic;
using System.Windows;

namespace CourseApp.classes.information
{
    public class MatchDisplayItem
    {
        public Client Candidate { get; set; } = null!;
        public string FullName { get; set; } = "";
        public string AgeText { get; set; } = "";
        public string GenderText { get; set; } = "";
        public string DetailsText { get; set; } = "";
        public string? AvatarPath { get; set; }
        public string HeightText { get; set; } = "";
        public string WeightText { get; set; } = "";
        public Visibility HeightVisibility { get; set; } = Visibility.Collapsed;
        public Visibility WeightVisibility { get; set; } = Visibility.Collapsed;
        public List<string> Hobbies { get; set; } = new List<string>();
        public Visibility MutualBadgeVisibility { get; set; }
    }
}