﻿using Domain.Models;
using Infrastructure.DTOs.QuestionsDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IQuestionRepository
    {
        public Task<int> AddQuestion(AddQuestionDTO question, int eid);

        public Task<int> AddBatchQuestionsToExam(AddQuestionsByBatchDTO questions, int eid);

        public Task<List<Question>> GetQuestionsByExamId(int examId);

        public Question GetQuestionById(int questionId);

        public Task<(List<ListQuestionsDTO> Questions, int TotalCount)> GetQuestionsByExaminerID(int examinerId, int page, int pageSize);

        public Task<int> UpdateQuestion(UpdateQuestionDTO question, int qid);

        public Task<int> DeleteQuestion(int questionId);

    }
}
