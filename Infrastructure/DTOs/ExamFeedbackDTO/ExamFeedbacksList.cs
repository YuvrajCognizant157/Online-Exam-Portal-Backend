using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DTOs.ExamFeedbackDTO
{
    public class ExamFeedbacksList
    {

        public int Eid { get; set; }

        //int UserId { get; set; }

        public string? ExamName { get; set; }

        public string ? FeedbackText { get; set; }
    }
}
