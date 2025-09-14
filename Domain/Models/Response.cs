using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{

    [Table("Responses")]
    public class Response
    {
        // Composite key EID, QID, UserId — defined in DbContext.OnModelCreating
        public int EID { get; set; }
        public int QID { get; set; }
        public int UserId { get; set; }

        public string? Resp { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Resp_Score { get; set; }

        public bool? IsSubmittedFresh { get; set; }

        [ForeignKey(nameof(EID))]
        public Exam? Exam { get; set; }

        [ForeignKey(nameof(QID))]
        public Question? Question { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
    }
}