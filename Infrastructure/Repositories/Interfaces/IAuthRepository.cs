using Domain.Models;
using Infrastructure.DTOs.AuthDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        StudentRegisterResponseDTO RegisterStudent(RegisterUserDTO dto);
        int RegisterAdminOrExaminer(RegistrationInputDTO examinerDTO, string role);
        User? Login(string email, string password);
        bool ValidateToken(string token);

        bool VerifyOTP(VerifyOtpRequestDTO dto, int otp);

        ResendOtpResponseDTO ResendOTP(int userId);
        Task<ResendOtpResponseDTO> RequestOtpForgotPassword(ForgotPasswordRequestDTO request);
        Task<int> ResetPassword(ResetPasswordRequestDto request);

    }
}
