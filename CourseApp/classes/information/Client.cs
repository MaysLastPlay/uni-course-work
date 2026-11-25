using CourseApp.classes.enums;
using System.Collections.Generic;

namespace CourseApp.classes.information
{
    public class Client
    {
        public string? RegNumber { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public Gender Gender { get; set; }
        public int Age { get; set; }
        public DateTime RegDate { get; set; }
        public string? PersonalDetails { get; set; }
        public PRequirements? Requirements { get; set; }
        public string? Reason { get; set; }
        public ClientStatus Status { get; set; } = ClientStatus.Active;

        public override string ToString() => $"{RegNumber} | {FullName} ({Gender}, {Age}р.)";
        public string? AvatarPath { get; set; }
        
        public List<string> PhotoPaths { get; set; } = new List<string>();
        public int? HeightCm { get; set; }
        public double? WeightKg { get; set; }
        public List<string> Hobbies { get; set; } = new List<string>();
}
    }