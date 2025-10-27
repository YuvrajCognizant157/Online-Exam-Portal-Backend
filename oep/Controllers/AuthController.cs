using Domain.Models;
using Infrastructure.DTOs.AuthDTOs;
using Infrastructure.DTOs.UserDTOs;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OEP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly TokenService _tokenService;


        public AuthController(IAuthRepository authRepository, TokenService tokenService)
        {
            _authRepository = authRepository;
            _tokenService = tokenService;
        }


        // POST: api/auth/register
        [HttpPost("student/register")]
        public IActionResult RegisterStudentAction([FromBody] RegisterUserDTO dto)
        {
            var result = _authRepository.RegisterStudent(dto);

            if (result.status > 0)
            {
                return Ok(new
                {
                    msg = result.Message,
                    result.UserId,
                    result.Otp
                });
            }

            return BadRequest(new { error = result.Message ?? "Registration failed" });

        }

        [HttpPost("internal/register")]
        public IActionResult RegisterAction([FromBody] RegistrationInputDTO inputdto)
        {
            string tokenEncrypted = inputdto.Token;
            if (!_authRepository.ValidateToken(tokenEncrypted))
            {
                BadRequest("Input Details are incorrect.");
            }
            // Use the injected service to get the role
            string? userRole = _tokenService.GetRoleFromToken(tokenEncrypted);

            bool emailVerified = _tokenService.VerifyEmailFromToken(tokenEncrypted, inputdto.Email);

            if(!emailVerified)
            {
                return BadRequest("Email verification failed.");
            }

            string role = !string.IsNullOrEmpty(userRole) ? userRole : "Examiner";

            var result = _authRepository.RegisterAdminOrExaminer(inputdto, role);            

            //return result > 0 ? Ok("User registered successfully") : BadRequest("Registration failed");

            return result > 0
                ? Ok(new { message = "User registered successfully", role = role })
                : BadRequest(new { error = "Registration failed" });

        }

        // POST: api/auth/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO dto)
        {
            var user = _authRepository.Login(dto.Email, dto.Password);
            if (user == null)
            {
                return Unauthorized("Invalid credentials Or You are Blocked Or Verify your Email.");
            }

            string token = _tokenService.GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        [HttpPost("student/verifyotp/{userId}")]
        public IActionResult VerifyOTP([FromBody] VerifyOtpRequestDTO dto, int userId)
        {
            if (_authRepository.VerifyOTP(dto, userId))
                return Ok(new { msg = "OTP verified successfully" });

            return Unauthorized("Incorrect OTP!!");
        }

        [HttpGet("student/resendotp/{userId}")]
        public IActionResult ResendOTP(int userId)
        {
            ResendOtpResponseDTO result = _authRepository.ResendOTP(userId);
            return result.status > 0 ? Ok(new { msg = "OTP Resent Successfully", result.otp }) : BadRequest("Failed to resend OTP");
        }


        [HttpPost("request-otp")]
        public async Task<IActionResult> RequestOtpAction([FromBody] ForgotPasswordRequestDTO request)
        {
            ResendOtpResponseDTO data = await _authRepository.RequestOtpForgotPassword(request);

            if (data.status > 0)
            {
                return Ok(ApiResponseDto.CreateSuccess("OTP has been processed.", data.otp));
            }
            else return NotFound(ApiResponseDto.CreateError("User not found."));

        }

        [HttpPost("reset")]
        public async Task<IActionResult> ResetPasswordAction([FromBody] ResetPasswordRequestDto request)
        {

            if (request.NewPassword == "" || request.NewPassword == null)
            {
                return NotFound(ApiResponseDto.CreateError("New Password was not received"));
            }
            int status = await _authRepository.ResetPassword(request);
            if (status == -2)
            {
                return BadRequest(ApiResponseDto.CreateError("Invalid or expired OTP."));

            }
            else if (status == -1)
            {
                return NotFound(ApiResponseDto.CreateError("User not found."));
            }
            else if (status == 1)
            {
                return Ok(ApiResponseDto.CreateSuccess("Password has been reset successfully.", null));
            }
            else
            {
                return StatusCode(500, new { Success = false, Message = "Internal Server Error" });
            }
        }
    }
}
