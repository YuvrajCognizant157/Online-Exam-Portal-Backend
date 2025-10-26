using Domain.Data;
using Domain.Models;
using Infrastructure.DTOs.ExamDTOs;
using Infrastructure.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data;
using System.Text.RegularExpressions;
namespace Infrastructure.Repositories.Implementations
{
    public class ExamRepository : IExamRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ExamRepository> _logger;

        public ExamRepository(AppDbContext dbContext, ILogger<ExamRepository> logger)
        {
            _context = dbContext;
            _logger = logger;
        }

        //CRUD
        public async Task<AddExamResponseDTO> AddExam(AddExamDTO dto)
        {
            Exam exam = new Exam
            {

                Name = dto.Name,
                Description = dto.Description,
                TotalQuestions = dto.TotalQuestions,
                Duration = dto.Duration,
                Tids = JsonConvert.SerializeObject(dto.Tids),
                DisplayedQuestions = dto.DisplayedQuestions,
                UserId = dto.userId,
                SubmittedForApproval = false,
                MarksPerQuestion = dto.MarksPerQuestion,
                TotalMarks = dto.MarksPerQuestion * dto.DisplayedQuestions

            };

            await _context.Exams.AddAsync(exam);
            _logger.LogInformation("New Exam {@name} created by {@userName}", exam.Name, exam.UserId);
            var status = await _context.SaveChangesAsync();


            AddExamResponseDTO res = new AddExamResponseDTO(status, exam.Eid);
            return res;

        }
        public async Task<int> UpdateExam(int examId, UpdateExamDTO dto)
        {

            Exam ToBeUpdatedExam = await _context.Exams.FirstOrDefaultAsync(e => e.Eid == examId);
            if (ToBeUpdatedExam.SubmittedForApproval == true || ToBeUpdatedExam.ApprovalStatus == 1)
            {
                return -1;
            }
            if (ToBeUpdatedExam == null)
            {
                return 0;
            }

            if (dto.TotalQuestions != null && ToBeUpdatedExam.TotalQuestions != dto.TotalQuestions)
            {
                ToBeUpdatedExam.TotalQuestions = (int)dto.TotalQuestions;
            }
            if (dto.Name != null)
            {
                ToBeUpdatedExam.Name = dto.Name;
            }
            if (dto.Description != null)
            {
                ToBeUpdatedExam.Description = dto.Description;
            }
            if (dto.Duration != null)
            {
                ToBeUpdatedExam.Duration = dto.Duration;
            }
            if (dto.Tids != null)
            {
                ToBeUpdatedExam.Tids = JsonConvert.SerializeObject(dto.Tids);
            }
            if (dto.DisplayedQuestions != null)
            {
                ToBeUpdatedExam.DisplayedQuestions = dto.DisplayedQuestions;
                ToBeUpdatedExam.TotalMarks = ToBeUpdatedExam.MarksPerQuestion * dto.DisplayedQuestions;

            }
            if (dto.MarksPerQuestion != null)
            {
                ToBeUpdatedExam.MarksPerQuestion = dto.MarksPerQuestion;
                ToBeUpdatedExam.TotalMarks = dto.MarksPerQuestion * ToBeUpdatedExam.DisplayedQuestions;
            }
            return await _context.SaveChangesAsync();
        }
        public async Task<int> DeleteExam(int examId)
        {

            try
            {
                var exam = _context.Exams.FirstOrDefault(e => e.Eid == examId);
                if (exam != null && (exam.ApprovalStatus == 1 || exam.ApprovalStatus == 0))
                {
                    exam.setApprovalStatus(-1);
                    int status = _context.SaveChanges();
                    _logger.LogInformation("Exam {@name} deleted by {@userName} at {@time}", exam.Name, exam.UserId ?? -1, DateTime.Now);
                    return status;
                }
                else if (exam == null)
                {
                    _logger.LogInformation("Exam with examId {@examid} doesn't exists. ", examId);
                    return -1;
                }
                else
                {
                    _logger.LogInformation("Exam with examId {@examid} already Deleted. ", examId);
                    return 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _logger.LogError("Server was not able to delete the exam with examid {@exmid}. Returned with Error {@error}", examId, ex.Message);
                return -99;
            }

        }
        public async Task<List<Exam>> GetExamsForExaminer(int userid)
        {
            var examdata = await _context.Exams.Include(e => e.Questions).Where(e => e.UserId == userid && e.ApprovalStatus != -1).ToListAsync();
            return examdata;
        }
        public async Task<ExamWithQuestionsDTO> GetExamByIdForExaminer(int examId)
        {
            var examData = await _context.Exams
                .Where(e => e.Eid == examId && e.ApprovalStatus >= 0)
                .Select(e => new
                {
                    e.UserId,
                    e.Eid,
                    ExamName = e.Name,
                    e.TotalQuestions,
                    approvalStatus = e.ApprovalStatus,
                    MarksPerQuestion = e.MarksPerQuestion ?? 0,
                    TidsString = e.Tids,
                    Questions = e.Questions.Select(q => new QuestionDTO
                    {
                        Qid = q.Qid,
                        Type = q.Type,
                        Options = q.Options,
                        Marks = q.Marks ?? 0,
                        QuestionText = q.Question1,
                        CorrectOptions = q.CorrectOptions,
                        ApprovalStatus = q.ApprovalStatus
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (examData == null)
            {
                return null;
            }


            List<int> listids = JsonConvert.DeserializeObject<List<int>>(examData.TidsString);



            // Fetch topic names
            var topicNames = await _context.Topics
                .Where(t => listids.Contains(t.Tid))
                .Select(t => t.Subject ?? $"Topic {t.Tid}")
                .ToListAsync();


            return new ExamWithQuestionsDTO
            {
                UserId = examData.UserId,
                Eid = examData.Eid,
                Name = examData.ExamName,
                TotalQuestions = examData.TotalQuestions,
                approvalStatus = examData.approvalStatus,
                MarksPerQuestion = examData.MarksPerQuestion,
                Questions = examData.Questions,

                Tids = listids ?? [],
                TopicNames = topicNames ?? []
            };


        }
        public async Task<List<Exam>> GetExamsAttemptedByUser(int UserId)
        {
            var user = await _context.Users
                   .Include(u => u.ExamUsers)
                   .ThenInclude(eu => eu.Results)
                   .FirstOrDefaultAsync(u => u.UserId == UserId);

            return (List<Exam>)user.ExamUsers;

        }
        public async Task<int> GetExamAttempts(int userId, int examId)
        {
            var result = await _context.Results.FirstOrDefaultAsync(r => r.UserId == userId && r.Eid == examId);
            if (result == null) return 0;
            return (int)result.Attempts;
        }

        //Student functions
        public async Task<List<GetExamDataDTO>> GetExamsForStudents(int studentId)
        {
            var examdata = await _context.Exams
                .Include(e => e.Results.Where(s => s.Eid == e.Eid))
                .Where(e => e.Questions != null && e.ApprovalStatus == 1)
            .Select(e => new GetExamDataDTO
            {
                eid = e.Eid,
                name = e.Name,
                description = e.Description,
                totalMarks = e.TotalMarks ?? 0m,
                duration = e.Duration ?? 0m,
                Tids = e.Tids,
                displayedQuestions = e.DisplayedQuestions ?? 0,
                AttemptNo = e.Results
                                .Where(r => r.Eid == e.Eid && r.UserId == studentId)
                                .Max(r => (int?)r.Attempts) ?? 0
            }).ToListAsync();
            if (examdata == null) return new List<GetExamDataDTO> { };
            else return examdata;
        }
        public async Task<StudentExamViewDTO> GetExams(int examId)
        {
            return await _context.Exams.Where(e => e.Eid == examId && e.ApprovalStatus == 1 && e.Questions != null)
                .Select(e => new StudentExamViewDTO
                {
                    Name = e.Name,
                    Description = e.Description,
                    DisplayedQuestions = e.DisplayedQuestions,
                    Duration = e.Duration,
                    Tids = e.Tids

                })
                .FirstOrDefaultAsync();
        }
        public async Task<StartExamResponseDTO> StartExam(int examId, int userId)
        {

            var hasUnprocessedAttempt = await _context.Responses
                   .AnyAsync(r => r.Eid == examId && r.UserId == userId && r.IsSubmittedFresh == true);

            if (hasUnprocessedAttempt)
            {
                var examIdParam = new SqlParameter("@examId", examId);
                var userIdParam = new SqlParameter("@userId", userId);

                var result = await _context.Database.ExecuteSqlRawAsync(
                    "EXEC CreateExamResult @examId, @userId",
                    examIdParam, userIdParam);

                _logger.LogInformation("Result created for user= {@userid} and exam = {@examid}", userId, examId);

            }

            var allAttempts = await _context.Results.Where(r => r.UserId == userId && r.Eid == examId).MaxAsync(r => (int?)r.Attempts) ?? 0;
            if (allAttempts >= 3)
            {
                return null;
            }

            var list = await _context.Exams.Include(e => e.Questions)
                .Where(e => e.Eid == examId && e.ApprovalStatus == 1)
                .Select(e => new StartExamResponseDTO
                {
                    EID = e.Eid,
                    TotalMarks = e.TotalMarks,
                    Duration = e.Duration,
                    Name = e.Name,
                    DisplayedQuestions = e.DisplayedQuestions,
                    Questions = e.Questions
                            .Where(q => q.ApprovalStatus == 1)
                            .Select(
                            q => new StartExamQuestionsDTO
                            {
                                Qid = q.Qid,
                                Type = q.Type,
                                QuestionName = q.Question1,
                                Marks = q.Marks,
                                Options = q.Options,
                                ApprovalStatus = q.ApprovalStatus
                            }
                    ).ToList(),
                }).ToListAsync();
            var Data = list.FirstOrDefault();

            if (Data != null)
            {
                var random = new Random();
                var selectedQuestions = Data.Questions
                    .OrderBy(q => random.Next())
                    .Take((int)Data.DisplayedQuestions)
                    .ToList();

                var response = new StartExamResponseDTO
                {
                    EID = Data.EID,
                    TotalMarks = Data.TotalMarks,
                    Duration = Data.Duration,
                    Name = Data.Name,
                    DisplayedQuestions = Data.DisplayedQuestions,
                    Questions = selectedQuestions
                };

                return response;
            }


            return Data;
        }
        public async Task<int> SubmitExam(SubmittedExamDTO submittedData)
        {

            var allAttempts = await _context.Results.Where(r => r.UserId == submittedData.UserId && r.Eid == submittedData.EID).MaxAsync(r => (int?)r.Attempts) ?? 0;

            if (allAttempts >= 3)
            {
                return -1;
            }

            var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Eid == submittedData.EID);
            if (exam == null)
            {
                return -1;
            }

            foreach (var responseDto in submittedData.Responses)
            {

                var existingResponse = await _context.Responses
                        .FirstOrDefaultAsync(r => r.UserId == submittedData.UserId && r.Eid == submittedData.EID && r.Qid == responseDto.Qid);

                if (existingResponse == null)
                {
                    var newResponse = new Response
                    {
                        Eid = submittedData.EID,
                        Qid = responseDto.Qid,
                        UserId = submittedData.UserId,
                        Resp = JsonConvert.SerializeObject(responseDto.Resp),
                        RespScore = 0,
                        IsSubmittedFresh = true
                    };

                    await _context.Responses.AddAsync(newResponse);
                }
                else
                {
                    existingResponse.Resp = JsonConvert.SerializeObject(responseDto.Resp);
                    existingResponse.IsSubmittedFresh = true;
                    existingResponse.RespScore = null;

                }


            }

            int status = await _context.SaveChangesAsync();
            return status;

        }

        public async Task<int> SubmitExamForApproval(int examId)
        {
            var exam = await _context.Exams.Include(e => e.Questions).FirstOrDefaultAsync(e => e.Eid == examId);

            //checking if exam has total questions available
            if (exam.Questions.Count != exam.TotalQuestions)
            {
                return -2;
            }

            if (exam.Description == null || exam.TotalQuestions == null || exam.TotalMarks == null || exam.Questions == null || exam.DisplayedQuestions == null || exam.Duration == null || exam.Name == null || exam.Tids == null)
            {
                return -1;
            }
            exam.SubmittedForApproval = true;

            //Assigning a admin id
            var random = new Random();
            var adminIds = await _context.Users
                .Where(u => u.Role == "Admin")
                .Select(a => a.UserId)
                .ToListAsync();
            var randomAdminId = adminIds.OrderBy(x => random.Next()).FirstOrDefault();
            exam.ReviewerId = randomAdminId;

            return await _context.SaveChangesAsync();
        }

    }
}


