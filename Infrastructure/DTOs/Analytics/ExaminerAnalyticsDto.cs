using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DTOs.Analytics
{
    public class ExaminerAnalyticsDto
    {
        public CreatedExamsStatusDto ExamsStatusInformation { get; set; }
        public List<ExamScoreDto> AverageScoresPerExam { get; set; }
        public List<QuestionApprovalDto> QuestionApprovalStats { get; set; }
        public List<ExamParticipationDto> StudentParticipation { get; set; }
        public List<TopicQuestionCountDto> TopicQuestionCounts { get; set; }
        public List<TopicScoreDto> AvgTopicScores { get; set; }
        public List<TopicApprovalDto> TopicApprovalStats { get; set; }
        public List<QuestionTypeDto> QuestionTypeDistribution { get; set; }
        public List<TimeSeriesDto> SubmissionsOverTime { get; set; }
        public List<QuestionPerformanceDto> HardestQuestions { get; set; }
        public List<QuestionPerformanceDto> EasiestQuestions { get; set; }
        public List<ExamPerformanceCorrelationDto> ExamPerformanceCorrelation { get; set; }
    }
    public class CreatedExamsStatusDto
    {
        public int TotalExamsCreated { get; set; }
        public int ApprovedExams { get; set; }
        public int SubmittedForApprovalExams { get; set; }
        public int RejectedExams { get; set; }
        public int PendingExams { get; set; }
    }
    public class ExamScoreDto
    {
        public int ExamId { get; set; }
        public string ExamTitle { get; set; }
        public double AverageScore { get; set; }
    }

    public class QuestionApprovalDto
    {
        public bool IsApproved { get; set; }
        public int Count { get; set; }
    }

    public class ExamParticipationDto
    {
        public int ExamId { get; set; }
        public string ExamTitle { get; set; }
        public int StudentCount { get; set; }
    }

    public class TopicQuestionCountDto
    {
        public int TopicId { get; set; }
        public string Subject { get; set; }
        public int QuestionCount { get; set; }
    }
    public class TopicScoreDto
    {
        public int TopicId { get; set; }
        public string Subject { get; set; }
        public decimal AverageScore { get; set; }
    }
    public class TopicApprovalDto
    {
        public int TopicId { get; set; }
        public string Subject { get; set; }
        public bool IsApproved { get; set; }
        public int Count { get; set; }
    }
    public class QuestionTypeDto
    {
        public string Type { get; set; }
        public int Count { get; set; }
    }

    public class TimeSeriesDto
    {
        public DateTime Date { get; set; }
        public int SubmissionCount { get; set; }
    }

    public class QuestionPerformanceDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public decimal AverageScore { get; set; }
    }

    public class ExamPerformanceCorrelationDto
    {
        public int ExamId { get; set; }
        public string ExamTitle { get; set; }
        public double AverageScore { get; set; }
        public int StudentCount { get; set; }
    }
}
