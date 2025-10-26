using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using OEP.Controllers;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.DTOs.ExamDTOs;
using Domain.Models;
using System.Collections.Generic;

namespace OEPtest.Controllers
{
    [TestFixture]
    public class ExamFeedbackControllerTests
    {
        private Mock<IExamFeedbackRepository> _mockRepo;
        private ExamFeedbackController _controller;

        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<IExamFeedbackRepository>();
            _controller = new ExamFeedbackController(_mockRepo.Object);
        }


        [Test]
        public void GetAllFeedbacks_ShouldReturnOk_WithFeedbackList()
        {
            // Arrange
            int examId = 1;
            var feedbackList = new List<ExamFeedback>
            {
                new ExamFeedback { Feedback = "Good exam", UserId = 101 },
                new ExamFeedback { Feedback = "Needs improvement", UserId = 102 }
            };

            _mockRepo.Setup(repo => repo.GetAllFeedbacks(examId)).Returns(feedbackList);

            // Act
            var result = _controller.GetAllFeedbacks(examId);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.AreEqual(200, okResult.StatusCode);
            var returnedFeedbacks = okResult.Value as IEnumerable<ExamFeedback>;
            Assert.AreEqual(2, ((List<ExamFeedback>)returnedFeedbacks).Count);
        }
    }
}
