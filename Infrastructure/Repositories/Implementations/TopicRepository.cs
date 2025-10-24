using Domain.Data;
using Domain.Models;
using Infrastructure.DTOs;
using Infrastructure.DTOs.TopicDTOs;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Collections.Generic; // for KeyNotFoundException usage if needed
using System;

namespace Infrastructure.Repositories.Implementations
{
    public class TopicRepository : ITopicRepository
    {
        private readonly AppDbContext _context;
        public TopicRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<GetTopicsDTO>> GetTopics()
        {
            List<GetTopicsDTO> topicList = await _context.Topics.Where(t => t.ApprovalStatus == 1 && t.ApprovedByUserId != null)
                .Select(t =>
                    new GetTopicsDTO
                    {
                        tid = t.Tid,
                        subject = t.Subject ?? "No Topic",
                        approvalStatus = t.ApprovalStatus
                    }).ToListAsync();

            return topicList;

        }

        public async Task<List<GetTopicsDTO>> GetExaminerTopics(int examinerId)
        {
            List<GetTopicsDTO> topicList = await _context.Topics.Where(t => t.ExaminerId == examinerId)
                .Select(t =>
                    new GetTopicsDTO
                    {
                        tid = t.Tid,
                        subject = t.Subject ?? "No Topic",
                        approvalStatus = t.ApprovalStatus,
                        submittedForApproval = t.SubmittedForApproval
                    }).ToListAsync();

            return topicList;

        }

        public async Task<Topic> GetTopics(int topicId)
        {
            var topic = await _context.Topics
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.Tid == topicId && t.ApprovalStatus == 1 && t.ApprovedByUserId != null);

            if (topic == null)
            {
                throw new KeyNotFoundException($"Topic with id {topicId} not found or not approved.");
            }

            return topic;
        }

        public List<Topic> GetTopicsForQuestions(int examId)
        {
            var exam = _context.Exams.FirstOrDefault(e => e.Eid == examId && e.SubmittedForApproval == false);
            if (exam == null)
            {
                return new List<Topic> { };
            }
            Console.WriteLine(exam.Tids);

            var topicIds = JsonConvert.DeserializeObject<List<int>>(exam.Tids);

            // Fetch topics from the database
            var topics = _context.Topics
                .Where(t => topicIds.Contains(t.Tid))
                .ToList();

            return topics;

        }
        public int CreateTopic(string TopicName, int examinerId)
        {
            Topic topic = new Topic
            {
                Subject = TopicName,
                ExaminerId = examinerId
            };
            var CreatedTopic = _context.Topics.Add(topic);
            return _context.SaveChanges();
        }
        public int UpdateTopic(string TopicName, int Tid)
        {
            var topicToUpdate = _context.Topics.Where(t => t.Tid == Tid && t.ApprovalStatus == 0).FirstOrDefault();
            if (topicToUpdate == null)
            {
                return 0;
            }

            topicToUpdate.Subject = TopicName;
            return _context.SaveChanges();
        }
        public int DeleteTopic(int topicId)
        {
            var topic = _context.Topics.Where(t => t.Tid == topicId && t.ApprovalStatus == 0).FirstOrDefault();

            if (topic != null)
            {
                _context.Topics.Remove(topic);
                return _context.SaveChanges();
            }
            return 0;
        }

        public async Task<int> SubmitTopicForApproval(int topicId)
        {
            var topic = await _context.Topics.FirstOrDefaultAsync(t => t.Tid == topicId);

            //Assigning a admin id
            var random = new Random();
            var adminIds = await _context.Users
                .Where(u => u.Role == "Admin" && u.IsBlocked == false)
                .Select(a => a.UserId)
                .ToListAsync();
            var randomAdminId = adminIds.OrderBy(x => random.Next()).FirstOrDefault();
            topic.ApprovedByUserId = randomAdminId != null ? randomAdminId : 7;
            topic.SubmittedForApproval = true;

            return await _context.SaveChangesAsync();

        }
    }
}
