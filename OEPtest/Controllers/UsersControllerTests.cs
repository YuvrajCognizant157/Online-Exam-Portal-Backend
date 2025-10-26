using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using OEP.Controllers;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.DTOs.UserDTOs;
using System.Collections.Generic;

namespace OEPtest.Controllers
{
    [TestFixture]
    public class UsersControllerTests
    {
        private Mock<IUserRepository> _mockUserRepo;
        private Mock<IExamRepository> _mockExamRepo;
        private UsersController _controller;

        [SetUp]
        public void Setup()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _mockExamRepo = new Mock<IExamRepository>();
            _controller = new UsersController(_mockUserRepo.Object, _mockExamRepo.Object);
        }

        [Test]
        public void GetUsers_ShouldReturnOk_WithUsersList()
        {
            
            var role = "Admin";
            var users = new List<UserDetailsDTO>
            {
                new UserDetailsDTO { FullName = "John Doe", Email = "john@example.com", Role = "Admin" },
                new UserDetailsDTO { FullName = "Jane Smith", Email = "jane@example.com", Role = "Admin" }
            };

            _mockUserRepo.Setup(repo => repo.GetUsersByRole(role)).Returns(users);

            
            var result = _controller.GetUsers(role) as OkObjectResult;

           
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            var returnedUsers = result.Value as List<UserDetailsDTO>;
            Assert.AreEqual(2, returnedUsers.Count);
        }

        [Test]
        public void GetUserById_ShouldReturnOk_WhenUserExists()
        {
            
            var userId = 1;
            var user = new UserDetailsDTO { FullName = "John Doe", Email = "john@example.com", Role = "Admin" };

            _mockUserRepo.Setup(repo => repo.GetUserById(userId)).Returns(user);

            
            var result = _controller.GetUserById(userId) as OkObjectResult;

            
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            var returnedUser = result.Value as UserDetailsDTO;
            Assert.AreEqual("John Doe", returnedUser.FullName);
        }

        [Test]
        public void GetUserById_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            
            var userId = 99;
            _mockUserRepo.Setup(repo => repo.GetUserById(userId)).Returns((UserDetailsDTO)null);

           
            var result = _controller.GetUserById(userId) as NotFoundObjectResult;

            
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
            Assert.AreEqual("User not found", result.Value);
        }
    }
}
