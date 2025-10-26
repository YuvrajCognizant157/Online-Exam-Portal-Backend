using Domain.Data;
using Infrastructure.DTOs.Analytics;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OEP.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsRepository analyticsRepository;

        public AnalyticsController(IAnalyticsRepository ar)
        {
            analyticsRepository = ar;
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdminAnalyticsDto>> GetAdminAnalyticsAction()
        {
            var analyticsData = await analyticsRepository.GetAdminAnalytics();
            if (analyticsData != null)
            {
                return Ok(analyticsData);
            }
            else return Ok("No data available");
        }

        [HttpGet("examiner/{examinerId}")]
        [Authorize(Roles = "Examiner")]
        public async Task<ActionResult<ExaminerAnalyticsDto>> GetExaminerAnalyticsAction(int examinerId)
        {
            var examinerAnalyticsData = await analyticsRepository.GetExaminerAnalytics(examinerId);

            if (examinerAnalyticsData != null) return Ok(examinerAnalyticsData);
            else return Ok("No Data available or Examiner Not Found");
        }

        [HttpGet("student/{userId}")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<StudentAnalyticsDTO>> GetStudentAnalyticsAction(int userId)
        {
            var studentAnalyticsData = await analyticsRepository.GetStudentAnalytics(userId);
            if (studentAnalyticsData != null) return Ok(studentAnalyticsData);
            else return Ok("No Data available or Student Not Found");
        }

        [HttpGet("total-active-exams")]
        public async Task<int> GetTotalActiveExamsAction()
        {
            return await analyticsRepository.GetTotalActiveExams();
        }

        [HttpGet("total-active-questions")]
        public async Task<int> GetTotalActiveQuestionsAction()
        {
            return await analyticsRepository.GetTotalActiveQuestions();
        }

        [HttpGet("topic-wise-questions-attempted/{userId}")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<List<TopicWiseQuestionsAttempted>>> GetTopicWiseQuestionsAttemptedAction(int userId)
        {
            var topicWiseData = await analyticsRepository.GetTopicWIseQuestionAttempted(userId);
            if (topicWiseData != null && topicWiseData.Count > 0)
            {
                return Ok(topicWiseData);
            }
            else return Ok("No Data available for the given User ID");
        }
    }
}
