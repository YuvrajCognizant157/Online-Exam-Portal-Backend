using Domain.Data;
using Domain.Models;
using Infrastructure.DTOs.ExamDTOs;
using Infrastructure.DTOs.ExamFeedbackDTO;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Implementations
{


    public class ExamFeedbackRepository : IExamFeedbackRepository
    {
        private readonly AppDbContext _context;

        public ExamFeedbackRepository(AppDbContext context)
        {
            _context = context;
        }

        public int AddFeedback(int examId, ExamFeedbackDto dto)
        {
            var exam = _context.Exams.FirstOrDefault(e => e.Eid == examId && e.ApprovalStatus == 1);
            if (exam == null)
            {
                return -1;
            }

            var existingFeedback = _context.ExamFeedbacks.FirstOrDefault(e => e.Eid == examId && e.UserId == dto.Userid);
            if (existingFeedback != null)
            {
                // Update existing feedback
                existingFeedback.Feedback = dto.Feedback;
            }
            else
            {
                // Add new feedback
                var feedback = new ExamFeedback
                {
                    Eid = examId,
                    UserId = dto.Userid,
                    Feedback = dto.Feedback
                };

                _context.ExamFeedbacks.Add(feedback);
            }

            return _context.SaveChanges();
        }

        public IEnumerable<ExamFeedback> GetAllFeedbacks(int examId)
        {
            return _context.ExamFeedbacks
                .Include(f => f.User)
                .Include(f => f.EidNavigation)
                .Where(f => f.Eid == examId)
                .ToList();
        }

        public IEnumerable<ExamFeedbackDto> GetStudentFeedback(int examId, int userId)
        {
            return _context.ExamFeedbacks
                .Where(f => f.Eid == examId && f.UserId == userId)
                .Select(f => new ExamFeedbackDto
                {
                    Feedback = f.Feedback
                })
                .ToList();
        }

        public async Task<List<ExamFeedbacksList>> GetAllAttemptedExamFeedback(int userId)
        {
            if (userId <= 0)
            {
                return new List<ExamFeedbacksList>();
            }

           
            return await _context.ExamFeedbacks
               
                .Where(feedback => feedback.UserId == userId)

         
                .Include(feedback => feedback.EidNavigation)

                // 4. Project the results into the DTO (ExamFeedbacksList)
                .Select(feedback => new ExamFeedbacksList
                {
                    Eid = feedback.Eid,
                    FeedbackText = feedback.Feedback,
                    ExamName = feedback.EidNavigation.Name,
                })
                .ToListAsync();
        }
    }

}

