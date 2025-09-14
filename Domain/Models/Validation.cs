using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    [Table("Validations")]
    public class Validation
    {
        // Original SQL had only Token column (no PK). For EF Core, we must define a key.
        // Use Token as the key here.
        [Key]
        public string Token { get; set; } = null!;
    }
}
