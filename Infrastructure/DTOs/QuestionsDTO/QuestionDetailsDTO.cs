using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DTOs.QuestionsDTO
{
    public class TopicDetails
    {
        public int Tid { get; set; }
        public string TopicName { get; set; }
    }

    public class QuestionDetailsDTO
    {
        public int Qid { get; set; }

        public TopicDetails Topics { get; set; }

        public int Eid { get; set; }

        public string ExamTitle { get; set; }

        public string Type { get; set; }

        public string Question { get; set; }

        public decimal Marks { get; set; }

        public string Options { get; set; }
        public string CorrectOptions { get; set; }
    }
}
