using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    [Table("QuestionReports")]
    public class QuestionReport
    {
        // Composite key QID, UserId configured in DbContext
        public int QID { get; set; }
        public int UserId { get; set; }

        public string? Feedback { get; set; }

        [ForeignKey(nameof(QID))]
        public Question? Question { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
    }
}