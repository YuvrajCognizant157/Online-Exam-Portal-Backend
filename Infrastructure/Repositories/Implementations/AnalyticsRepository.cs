using Domain.Data;
using Infrastructure.DTOs.Analytics;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Implementations
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly AppDbContext _context;

        public AnalyticsRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<ActionResult<AdminAnalyticsDto>> GetAdminAnalytics()
        {
            var dto = new AdminAnalyticsDto
            {
                TotalExams = await _context.Exams.CountAsync(),
                TotalQuestions = await _context.Questions.CountAsync(),
                TotalStudents = await _context.Users.Where(s => s.Role == "Student").CountAsync(),
                TotalExaminers = await _context.Users.Where(e => e.Role == "Examiner").CountAsync(),
                BlockedExaminers = await _context.Users.Where(e => e.IsBlocked == true && e.Role == "Examiner").CountAsync(),
                TopExams = await _context.Results
                   .GroupBy(r => r.Eid)
                   .Select(g => new TopExamDto
                   {
                       ExamId = g.Key,
                       ExamTitle = g.First().EidNavigation.Name,
                       AverageScore = (double)g.Average(r => r.Score)
                   })
                   .OrderByDescending(x => x.AverageScore)
                   .Take(5)
                   .ToListAsync()
            };

            return dto != null ? dto : new AdminAnalyticsDto();
        }

        public async Task<ActionResult<ExaminerAnalyticsDto>> GetExaminerAnalytics(int examinerId)
        {
            var dto = new ExaminerAnalyticsDto
            {
                TotalExamsCreated = await _context.Exams.Where(e => e.UserId == examinerId).CountAsync(),

                AverageScoresPerExam = await _context.Results
                    .Where(r => r.EidNavigation.UserId == examinerId)
                    .GroupBy(r => r.Eid)
                    .Select(g => new ExamScoreDto
                    {
                        ExamId = g.Key,
                        ExamTitle = g.First().EidNavigation.Name, // This is the correct pattern
                        AverageScore = (double)g.Average(r => r.Score)
                    }).ToListAsync(),

                QuestionApprovalStats = await _context.Questions
                    .Where(q => q.EidNavigation.UserId == examinerId)
                    .GroupBy(q => q.ApprovalStatus == 1)
                    .Select(g => new QuestionApprovalDto
                    {
                        IsApproved = g.Key,
                        Count = g.Count()
                    }).ToListAsync(),

                StudentParticipation = await _context.Results
                    .Where(r => r.EidNavigation.UserId == examinerId)
                    .GroupBy(r => r.Eid)
                    .Select(g => new ExamParticipationDto
                    {
                        ExamId = g.Key,
                        ExamTitle = g.First().EidNavigation.Name, // This is the correct pattern
                        StudentCount = g.Select(r => r.UserId).Distinct().Count()
                    }).ToListAsync(),

                // --- FIX 1 ---
                // Group by Tid and ApprovalStatus, not Subject
                TopicApprovalStats = await _context.Topics
                    .Where(t => t.ExaminerId == examinerId)
                    .GroupBy(t => new { t.Tid, IsApproved = t.ApprovalStatus == 1 })
                    .Select(g => new TopicApprovalDto
                    {
                        TopicId = g.Key.Tid,
                        Subject = g.First().Subject, // Get Subject using g.First()
                        IsApproved = g.Key.IsApproved,
                        Count = g.Count()
                    }).ToListAsync(),

                // --- FIX 2 ---
                // Group by Tid, not Subject
                AvgTopicScores = await _context.Responses
                    .Include(r => r.QidNavigation)
                    .ThenInclude(q => q.TidNavigation)
                    .Where(r => r.QidNavigation.TidNavigation.ExaminerId == examinerId)
                    .GroupBy(r => r.QidNavigation.Tid) // Group by the ID
                    .Select(g => new TopicScoreDto
                    {
                        TopicId = g.Key,
                        Subject = g.First().QidNavigation.TidNavigation.Subject, // Get Subject using g.First()
                        AverageScore = g.Average(r => r.RespScore ?? 0)
                    }).ToListAsync(),

                // --- FIX 3 ---
                // Group by Tid, not Subject
                TopicQuestionCounts = await _context.Questions
                    .Include(q => q.TidNavigation)
                    .Where(q => q.TidNavigation.ExaminerId == examinerId)
                    .GroupBy(q => q.Tid) // Group by the ID
                    .Select(g => new TopicQuestionCountDto
                    {
                        TopicId = g.Key,
                        Subject = g.First().TidNavigation.Subject, // Get Subject using g.First()
                        QuestionCount = g.Count()
                    }).ToListAsync(),

                QuestionTypeDistribution = await _context.Questions
                    .Include(q => q.TidNavigation)
                    .Where(q => q.TidNavigation.ExaminerId == examinerId)
                    .GroupBy(q => q.Type) // Grouping by Type is usually fine if it's varchar(100) or similar.
                                          // If Type is also nvarchar(max), group by an ID if one exists.
                    .Select(g => new QuestionTypeDto
                    {
                        Type = g.Key ?? "Uncategorized",
                        Count = g.Count()
                    }).ToListAsync(),

                SubmissionsOverTime = await _context.Results
                    .Include(r => r.EidNavigation)
                    .Where(r => r.EidNavigation.UserId == examinerId && r.CreatedAt.HasValue)
                    .GroupBy(r => r.CreatedAt.Value.Date)
                    .Select(g => new TimeSeriesDto
                    {
                        Date = g.Key,
                        SubmissionCount = g.Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync(),

                // --- FIX 4 ---
                // Group by Eid, not Name
                ExamPerformanceCorrelation = await _context.Results
                    .Include(r => r.EidNavigation)
                    .Where(r => r.EidNavigation.UserId == examinerId)
                    .GroupBy(r => r.Eid) // Group by the ID
                    .Select(g => new ExamPerformanceCorrelationDto
                    {
                        ExamId = g.Key,
                        ExamTitle = g.First().EidNavigation.Name, // Get Name using g.First()
                        AverageScore = (double)g.Average(r => r.Score),
                        StudentCount = g.Select(r => r.UserId).Distinct().Count()
                    }).ToListAsync(),
            };

            // --- FIX 5 (Revised) ---
            // Step 1: Fetch the raw data from SQL without any string manipulation.
            // We select into an anonymous type first.
            var rawQuestionScores = await _context.Responses
                    .Include(r => r.QidNavigation)
                    .ThenInclude(q => q.TidNavigation)
                .Where(r => r.QidNavigation.TidNavigation.ExaminerId == examinerId)
                .GroupBy(r => r.Qid) // Group by the Question ID
                .Select(g => new
                {
                    QuestionId = g.Key,
                    QuestionText = g.First().QidNavigation.Question1, // Get the FULL text
                    AverageScore = g.Average(r => r.RespScore ?? 0)
                })
                .ToListAsync(); // <-- Executes the query, brings data into C# memory

            // Step 2: Now that the data is in memory (a List<>),
            // perform the string manipulation using C#'s .Length and .Substring (LINQ-to-Objects).
            var allQuestionScores = rawQuestionScores
                .Select(g => new QuestionPerformanceDto
                {
                    QuestionId = g.QuestionId,
                    QuestionText = (g.QuestionText != null && g.QuestionText.Length > 50
                                    ? g.QuestionText.Substring(0, 50) + "..."
                                    : g.QuestionText) ?? "No Text", // Handle potential null text
                    AverageScore = g.AverageScore
                })
                .ToList();

            // Populate the DTO with the Top 5 hardest and easiest
            dto.HardestQuestions = allQuestionScores
                .OrderBy(q => q.AverageScore)
                .Take(5)
                .ToList();

            dto.EasiestQuestions = allQuestionScores
                .OrderByDescending(q => q.AverageScore)
                .Take(5)
                .ToList();

            return dto != null ? dto : new ExaminerAnalyticsDto();
        }

        public async Task<List<TopicWiseQuestionsAttempted>> GetTopicWIseQuestionAttempted(int userId)
        {
            var topicWiseAttempts = await _context.Responses
                .Include(r => r.QidNavigation)
                .ThenInclude(q => q.TidNavigation)
                .Where(r => r.UserId == userId)
                .GroupBy(r => new { r.QidNavigation.TidNavigation.Tid, r.QidNavigation.TidNavigation.Subject })
                .Select(g => new TopicWiseQuestionsAttempted
                {
                    TopicId = g.Key.Tid,
                    TopicName = g.Key.Subject,
                    QuestionsAttempted = g.Select(r => r.Qid).Distinct().Count()
                })
                .ToListAsync();
            return topicWiseAttempts;
        }


        public async Task<ActionResult<StudentAnalyticsDTO>> GetStudentAnalytics(int userId)
        {

            var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);

            if (!userExists)
                return new NotFoundResult();

            var analyticsDto = new StudentAnalyticsDTO
            {
                UserId = userId,
                AverageScoreMultipleAttempts = await _context.Results
                    .Where(r => r.UserId == userId)
                    .GroupBy(r => r.Eid)
                    .Select(g => new AverageScoreMultipleAttempts
                    {
                        ExamId = g.Key,
                        ExamTitle = g.First().EidNavigation.Name,
                        AverageScore = (double)g.Average(r => r.Score),
                        TotalAttempts = g.Count()
                    }).ToListAsync(),
                TotalExamsTaken = await _context.Responses
                    .Where(rd => rd.UserId == userId)
                    .Select(rd => rd.Eid)
                    .Distinct()
                    .CountAsync(),
                TotalQuestionsEncountered = await _context.Responses
                    .Where(rd => rd.UserId == userId)
                    .Select(rd => rd.Qid)
                    .Distinct()
                    .CountAsync(),
                ExamAttemptsRecords = new AttemptsRecords
                {
                    SingleAttempts = await _context.Results
                        .Where(r => r.UserId == userId)
                        .GroupBy(r => r.Eid)
                        .Where(g => g.Count() == 1)
                        .CountAsync(),
                    DoubleAttempts = await _context.Results
                        .Where(r => r.UserId == userId)
                        .GroupBy(r => r.Eid)
                        .Where(g => g.Count() == 2)
                        .CountAsync(),
                    TrippleAttempts = await _context.Results
                        .Where(r => r.UserId == userId)
                        .GroupBy(r => r.Eid)
                        .Where(g => g.Count() >= 3)
                        .CountAsync()
                },

               

                OverallAverageScoreTopicWise = await Task.Run(() =>
                {
                   
                    var rawAnalyticsData = (
                        from result in _context.Results
                        join exam in _context.Exams on result.Eid equals exam.Eid
                        where result.UserId == userId
                        select new
                        {
                            result.Score,
                            exam.Tids 
                        }
                    ).AsEnumerable(); 
                    var topicScores = rawAnalyticsData
                        .SelectMany(x =>
                        {                            
                            var topicIds = JsonConvert.DeserializeObject<List<int>>(x.Tids ?? "[]");                             
                            return topicIds.Select(tid => new { Score = x.Score, TopicId = tid });
                        })
                        .ToList(); 

                   
                    var allTopics = _context.Topics.ToList();

                    return topicScores
                        .Join(
                            allTopics,
                            ts => ts.TopicId,
                            topic => topic.Tid,
                            (ts, topic) => new { ts.Score, topic.Tid, topic.Subject }
                        )
                        .GroupBy(x => new { x.Tid, x.Subject })
                        .Select(g => new TopicWiseAverageScore
                        {
                            TopicId = g.Key.Tid,
                            Topic = g.Key.Subject,
                            AverageScore = (double)g.Average(x => x.Score ?? 0)
                        })
                        .ToList();
                }),
            };

            return analyticsDto;
        }

        public async Task<int> GetTotalActiveExams()
        {
            return await _context.Exams.CountAsync(e => e.ApprovalStatus == 1);
        }

        public async Task<int> GetTotalActiveQuestions()
        {
            return await _context.Questions.CountAsync(q => q.ApprovalStatus == 1 && q.EidNavigation.ApprovalStatus == 1);
        }
    }
}
