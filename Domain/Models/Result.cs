using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{

    [Table("Results")]
    public class Result
    {
        // Composite key UserId, EID defined in DbContext
        public int UserId { get; set; }
        public int EID { get; set; }

        public int? Attempts { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Score { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [ForeignKey(nameof(EID))]
        public Exam? Exam { get; set; }
    }
}