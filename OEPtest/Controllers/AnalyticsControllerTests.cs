using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using OEP.Controllers;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.DTOs.Analytics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OEPtest.Controllers
{
    [TestFixture]
    public class AnalyticsControllerTests
    {
        private Mock<IAnalyticsRepository> _mockAnalyticsRepo;
        private AnalyticsController _controller;

        [SetUp]
        public void Setup()
        {
            _mockAnalyticsRepo = new Mock<IAnalyticsRepository>();
            _controller = new AnalyticsController(_mockAnalyticsRepo.Object);
        }

     

        [Test]
        public async Task GetTotalActiveExamsAction_ShouldReturnCorrectCount()
        {
            // Arrange
            _mockAnalyticsRepo.Setup(repo => repo.GetTotalActiveExams())
                              .ReturnsAsync(7);

            // Act
            var result = await _controller.GetTotalActiveExamsAction();

            // Assert
            Assert.AreEqual(7, result);
        }
    }
}