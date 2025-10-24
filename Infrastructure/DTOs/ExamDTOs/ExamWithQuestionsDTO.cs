using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DTOs.ExamDTOs
{
    public class ExamWithQuestionsDTO
    {
        public int Eid { get; set; }
        public string Name { get; set; }
        public int? TotalQuestions { get; set; }
        public List<QuestionDTO> Questions { get; set; }
        public int? approvalStatus { get; set; }
        public List<int> Tids { get; set; }
        public List<string> TopicNames { get; set; }
        public int MarksPerQuestion { get; set; }
        public int? UserId { get; set; }
    }

    public class QuestionDTO
    {
        public int Qid { get; set; }
        public string Type { get; set; }
        public string Options { get; set; }
        public decimal? Marks { get; set; }
        public string QuestionText { get; set; }
        public string CorrectOptions { get; set; }
        public int? ApprovalStatus { get; set; }
    }


}
