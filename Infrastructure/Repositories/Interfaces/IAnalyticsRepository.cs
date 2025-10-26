using Infrastructure.DTOs.Analytics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IAnalyticsRepository
    {
        public Task<ActionResult<AdminAnalyticsDto>> GetAdminAnalytics();
        public Task<ActionResult<ExaminerAnalyticsDto>> GetExaminerAnalytics(int examinerId);

        public Task<ActionResult<StudentAnalyticsDTO>> GetStudentAnalytics(int userId);

        public Task<int> GetTotalActiveExams();

        public Task<int> GetTotalActiveQuestions();

        public Task<List<TopicWiseQuestionsAttempted>> GetTopicWIseQuestionAttempted(int userId);
    }
}
