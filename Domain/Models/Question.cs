using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    [Table("Questions")]
    public class Question
    {
        [Key]
        public int QID { get; set; }

        public int? TID { get; set; }

        public int? EID { get; set; }

        [MaxLength(255)]
        public string? Type { get; set; }

        public string? QuestionText { get; set; } // renamed from Question to avoid name clash

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Marks { get; set; }

        public string? Options { get; set; } // NVARCHAR(MAX)

        public string? CorrectOptions { get; set; } // NVARCHAR(MAX)

        public int? ApprovalStatus { get; set; }

        [ForeignKey(nameof(TID))]
        public Topic? Topic { get; set; }

        [ForeignKey(nameof(EID))]
        public Exam? Exam { get; set; }

        public ICollection<Response>? Responses { get; set; }
        public ICollection<QuestionReport>? QuestionReports { get; set; }
    }
}