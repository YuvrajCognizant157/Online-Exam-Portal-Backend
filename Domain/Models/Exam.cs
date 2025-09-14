using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{

    [Table("Exams")]
    public class Exam
    {
        [Key]
        public int EID { get; set; }

        public int? UserId { get; set; } // Creator/owner

        /// <summary>
        /// Original DB stored TIDs as NVARCHAR(MAX) — keep as string for now.
        /// If you want normalized many-to-many later, convert to join table.
        /// </summary>
        public string? TIDs { get; set; }

        public int? TotalQuestions { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? TotalMarks { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Duration { get; set; }

        public string? Description { get; set; }

        [MaxLength(255)]
        public string? Name { get; set; }

        public int? ApprovalStatus { get; set; }

        public int? ApprovedByUserID { get; set; }

        public int? DisplayedQuestions { get; set; }

        public string? AdminRemarks { get; set; }

        [ForeignKey(nameof(ApprovedByUserID))]
        public User? ApprovedByUser { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? Creator { get; set; }

        public ICollection<Question>? Questions { get; set; }
        public ICollection<Response>? Responses { get; set; }
        public ICollection<Result>? Results { get; set; }
        public ICollection<ExamFeedback>? ExamFeedbacks { get; set; }
    }
}
