using Domain.Models;
using Infrastructure.DTOs.ExamDTOs;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace OEP.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class ExamFeedbackController : ControllerBase
    {
        private readonly IExamFeedbackRepository _repository;

        public ExamFeedbackController(IExamFeedbackRepository repository)
        {
            _repository = repository;
        }

        // POST /exam-feedback/{ExamID}
        [HttpPost("{examId}")]
        [Authorize(Roles = "Student")]
        public IActionResult SubmitFeedback(int examId, [FromBody] ExamFeedbackDto dto)
        {

            int status = _repository.AddFeedback(examId, dto);
            return status >= 0 ? Ok(new { Success = true }) : Ok(new { Success = false });
        }

        // GET /exam-feedbacks/{ExamID}
        [HttpGet("exam-feedbacks/{examId}")]
        [Authorize(Roles = "Examiner,Admin")]
        public ActionResult<IEnumerable<ExamFeedback>> GetAllFeedbacks(int examId)
        {
            var feedbacks = _repository.GetAllFeedbacks(examId);
            return Ok(feedbacks);
        }

        // GET /exam-feedback/{ExamID}
        [HttpGet("{examId}")]
        [Authorize(Roles = "Student")]
        public ActionResult<IEnumerable<ExamFeedbackDto>> GetStudentFeedback(int examId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var feedbacks = _repository.GetStudentFeedback(examId, userId);
            return Ok(feedbacks);
        }

        [HttpGet("all-exam-feedbacks-s/{userId}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetFeedbacksByUser(int userId)
        {
            var feedbacks = await _repository.GetAllAttemptedExamFeedback(userId);
            return Ok(feedbacks);
        }


    }

}

