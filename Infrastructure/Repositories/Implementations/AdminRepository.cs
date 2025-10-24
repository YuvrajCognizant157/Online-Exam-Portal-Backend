using Domain.Data;
using Domain.Models;
using Infrastructure.DTOs.adminDTOs;
using Infrastructure.DTOs.ExamDTOs;
using Infrastructure.DTOs.QuestionFeedbackDTO;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Implementations
{
    public class AdminRepository : IAdminRepository
    {
        private readonly AppDbContext _context;

        public AdminRepository(AppDbContext context)
        {
            _context = context;

        }

        public async Task<List<Exam>> ExamsToBeApprovedList(int reviewerId)
        {
            List<Exam> ExamList = new List<Exam> { };
            ExamList = await _context.Exams.Include(q => q.Questions).Where(e => e.SubmittedForApproval == true && e.ReviewerId == reviewerId).ToListAsync();
            return ExamList;

        }
        public async Task<int> ApproveExamAsync(ExamApprovalStatusDTO dto)
        {

            var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Eid == dto.ExamId);
            if (exam == null) return 0;

            if (dto.Status.ToLower() == "approve") { exam.setApprovalStatus(1); }
            else if (dto.Status.ToLower() == "reject") { exam.setApprovalStatus(0); }
            //exam.ApprovedByUserId = dto.userId;
            exam.SubmittedForApproval = false;
            exam.ReviewerId = dto.UserId;
            return await _context.SaveChangesAsync();

        }
        public async Task<Exam> GetExamWithQuestionsForAdminAsync(int examId)

        {

            // include all questions related to this exam

            return await _context.Exams

                .Include(e => e.Questions)

                .FirstOrDefaultAsync(e => e.Eid == examId);

        }

        public async Task<string> BlockUserAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u=>u.UserId==userId);
            if (user == null) return "User not found";

            if (user.Role == "Admin") { return "You are not allowed to block admin"; }
            if (user.IsBlocked==true) { return $"User with ID {userId} is already blocked"; }
            user.IsBlocked = true;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return $"User with ID {userId} has been successfully blocked";
        }
        public async Task<int> ToggleUserStatusAsync(int userId, bool isActive)

        {

            var user = await _context.Users.FindAsync(userId);

            if (user == null) return 0;

            user.IsBlocked = !isActive; // if isActive=false, block user

            await _context.SaveChangesAsync();

            return 1;

        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()

        {

            return await _context.Users
                .Where(u=>u.Role!="Admin")

                .Select(u => new User

                {

                    UserId = u.UserId,

                    FullName = u.FullName,

                    Email = u.Email,

                    Role = u.Role,

                    IsBlocked = u.IsBlocked

                })

                .ToListAsync();

        }



        public async Task<IEnumerable<ExamFeedbackViewDTO>> GetExamFeedbacksAsync(int userId)
        {
            return await _context.ExamFeedbacks
                .Where(ef => ef.EidNavigation.ApprovalStatus == 1 &&
                             ef.EidNavigation.ReviewerId == userId) // adjust property name
                .Select(ef => new ExamFeedbackViewDTO
                {
                    Eid = ef.Eid,
                    StudentId = ef.UserId,
                    Feedback = ef.Feedback
                })
                .ToListAsync();
        }


        public async Task<List<QuestionReport>> GetAllReportedQuestionsAsync(int adminId)
        {
            return await _context.QuestionReports.Where(r => r.ReviewerId == adminId).Select(r => new QuestionReport { Qid = r.Qid, Feedback = r.Feedback, UserId = r.UserId }).ToListAsync();
        }

        public async Task<Question?> GetReportedQuestionByIdAsync(int qid)

        {

            var report = await _context.QuestionReports

                .Include(r => r.QidNavigation)

                .FirstOrDefaultAsync(r => r.Qid == qid);

            return report?.QidNavigation;

        }


        public async Task<bool> UpdateReportedQuestionStatusAsync(QuestionReviewDTO dto)
        {


            var question = await _context.Questions.FirstOrDefaultAsync(q => q.Qid == dto.qid);

            if (question == null) return false;

            try
            {

                question.ApprovalStatus = dto.status;

                await _context.SaveChangesAsync();

                var reportToDelete = await _context.QuestionReports.FindAsync(dto.qid, dto.studentId);

                if (reportToDelete != null)
                {
                    _context.QuestionReports.Remove(reportToDelete);
                    await _context.SaveChangesAsync();
                }
                else
                {

                    Console.WriteLine("Delete failed: QuestionReport not found for QID: {0} and StudentID: {1}", dto.qid, dto.studentId);
                }


            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public async Task<int> AddAdminRemarksAsync(int examId, string remarks)

        {

            var feedback = await _context.ExamFeedbacks

                .Include(f => f.EidNavigation)

                .FirstOrDefaultAsync(f => f.Eid == examId);

            if (feedback == null)

                return 0;


            feedback.EidNavigation.AdminRemarks = remarks;

            if (feedback.EidNavigation.ApprovalStatus != 0)
                feedback.EidNavigation.ApprovalStatus = 0;

            await _context.SaveChangesAsync();

            return 1;

        }


        public async Task<List<ApproveTopicsDTO>> TopicsToBeApprovedAsync(int userId)
        {
            return await _context.Topics.Where(t => t.SubmittedForApproval == true && t.ApprovedByUserId == userId)
                .Select(t => new ApproveTopicsDTO { Id = t.Tid, TopicName = t.Subject }).ToListAsync();
        }

        public async Task<int> ApproveOrRejectTopic(int topicId, int userId, string Action)
        {
            var topic = await _context.Topics.FirstOrDefaultAsync(t => t.Tid == topicId);
            if (topic == null)
                return 0;
            if (Action.ToLower() == "approve")
            {
                topic.SetApprovalStatus(1);
            }

            else if (Action.ToLower() == "reject")
            {
                topic.SetApprovalStatus(0);
            }


            topic.SubmittedForApproval = false;
            await _context.SaveChangesAsync();

            return 1;
        }
    }

}

