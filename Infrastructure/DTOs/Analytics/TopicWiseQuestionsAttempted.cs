using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DTOs.Analytics
{
    public class TopicWiseQuestionsAttempted
    {
        public int TopicId { get; set; }
        public string TopicName { get; set; }
        public int QuestionsAttempted { get; set; }
    }
}
