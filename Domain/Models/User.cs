using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{

    [Table("User")]
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [MaxLength(255)]
        public string? Email { get; set; }

        [MaxLength(255)]
        public string? FullName { get; set; }

        [Column("Password")]
        [MaxLength(255)]
        public string? Password { get; set; }

        public DateTime? DOB { get; set; }

        [MaxLength(255)]
        public string? PhoneNo { get; set; }

        public DateTime? RegistrationDate { get; set; }

        [Column("Role")]
        [MaxLength(255)]
        public string? Role { get; set; }

        public bool IsBlocked { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<Topic>? TopicsApproved { get; set; }
        public ICollection<Exam>? ExamsCreated { get; set; }
        public ICollection<Exam>? ExamsApproved { get; set; }
        public ICollection<Response>? Responses { get; set; }
        public ICollection<Result>? Results { get; set; }
        public ICollection<QuestionReport>? QuestionReports { get; set; }
        public ICollection<ExamFeedback>? ExamFeedbacks { get; set; }
    }
}
