using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using OEP.Controllers;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.DTOs.ExamDTOs;
using Infrastructure.DTOs.ResultDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OEPtest.Controllers
{
    [TestFixture]
    public class ResultsControllerTests
    {
        private Mock<IResultRespository> _mockRepo;
        private ResultsController _controller;

        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<IResultRespository>();
            _controller = new ResultsController(_mockRepo.Object);
        }

        [TearDown]
        public void Cleanup()
        {
            _controller?.Dispose(); //  Fixes NUnit warning
        }

        [Test]
        public async Task ViewExamResultsAction_ShouldReturnOk_WithResultsList()
        {
            // Arrange
            int examId = 1;
            int userId = 101;
            var examResults = new List<ExamResultsDTO>
            {
                new ExamResultsDTO { UserId = userId, Eid = examId, Score = 85, ExamName = "Math Exam" },
                new ExamResultsDTO { UserId = userId, Eid = examId, Score = 90, ExamName = "Science Exam" }
            };

            _mockRepo.Setup(repo => repo.ViewExamResults(examId, userId))
                     .ReturnsAsync(examResults);

            // Act
            var result = await _controller.ViewExamResultsAction(examId, userId) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            var returnedResults = result.Value as List<ExamResultsDTO>;
            Assert.AreEqual(2, returnedResults.Count);
        }

        [Test]
        public async Task CreateExamResultsAction_ShouldReturnOk_WhenStatusIsPositive()
        {
            // Arrange
            int examId = 1;
            int userId = 101;
            var response = new CreateResultDTO(1, "Result created successfully");

            _mockRepo.Setup(repo => repo.CreateExamResults(examId, userId))
                     .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateExamResultsAction(examId, userId) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(response, result.Value);
        }
    }
}
