using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    [Table("Topics")]
    public class Topic
    {
        [Key]
        public int TID { get; set; }

        [MaxLength(255)]
        public string? Subject { get; set; }

        public int? ApprovalStatus { get; set; }

        public int? ApprovedByUserID { get; set; }

        [ForeignKey(nameof(ApprovedByUserID))]
        public User? ApprovedByUser { get; set; }

        public ICollection<Question>? Questions { get; set; }
    }
}