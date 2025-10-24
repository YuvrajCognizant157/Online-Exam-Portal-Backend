using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DTOs.AuthDTOs
{
    public class ForgotPasswordRequestDTO
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public bool VerifyWithEmail { get; set; }
    }
    public class ResetPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public int Otp { get; set; }

        [Required]
        [MinLength(8)]
        public string? NewPassword { get; set; }
    }
    public class ApiResponseDto
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; }
        public int? Otp { get; set; }

        public static ApiResponseDto CreateSuccess(string message, int? otp)
        {
            return new ApiResponseDto { Success = true, Message = message, Otp = otp };
        }

        public static ApiResponseDto CreateError(string message)
        {
            return new ApiResponseDto { Success = false, Message = message };
        }
    }
}
