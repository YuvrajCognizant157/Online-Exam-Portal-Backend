using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    [Table("ExamFeedbacks")]
    public class ExamFeedback
    {
        // Composite key EID, UserId configured in DbContext
        public int EID { get; set; }
        public int UserId { get; set; }

        public string? Feedback { get; set; }

        [ForeignKey(nameof(EID))]
        public Exam? Exam { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
    }
}