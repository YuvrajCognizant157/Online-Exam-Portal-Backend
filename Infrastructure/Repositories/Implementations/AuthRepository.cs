using Domain.Data;
using Domain.Models;
using Infrastructure.DTOs.AuthDTOs;
using Infrastructure.DTOs.UserDTOs;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Services;
using Microsoft.Extensions.Logging;


namespace Infrastructure.Repositories.Implementations
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _context;

        private readonly EmailService _emailService;
        private readonly PasswordHashingService passwordHashing;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AuthRepository> _logger;

        public AuthRepository(IUserRepository userRepository, AppDbContext context, ILogger<AuthRepository> logger, EmailService emailService, PasswordHashingService pH)
        {
            _userRepository = userRepository;
            _context = context;
            _logger = logger;
            _emailService = emailService;
            passwordHashing = pH;
        }

        public int RegisterAdminOrExaminer(RegistrationInputDTO examinerDTO, string role)
        {



            bool emailExists = _context.Users.Any(u => u.Email == examinerDTO.Email);
            bool phoneExists = _context.Users.Any(u => u.PhoneNo == examinerDTO.PhoneNo);

            if (emailExists || phoneExists)
            {
                _logger.LogWarning("Attempt to register with existing Email or Phone. Email: {Email}, Phone: {Phone}", examinerDTO.Email, examinerDTO.PhoneNo);
                return -1;

            }



            var user = new User
            {
                FullName = examinerDTO.FullName,
                Email = examinerDTO.Email,
                Password = passwordHashing.HashPassword(examinerDTO.Password),
                Role = role,
                PhoneNo = examinerDTO.PhoneNo,
                Dob = examinerDTO.Dob,
            };

            _context.Users.Add(user);
            _logger.LogInformation("New Admin/Examiner is registered with UserId : {@userid} at {@time}", user.UserId, user.CreatedAt);
            return _context.SaveChanges();
        }
        public StudentRegisterResponseDTO RegisterStudent(RegisterUserDTO dto)
        {
            // Logic for sending verification email
            Random random = new Random();
            int newOtp = random.Next(100000, 999999);

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Password = passwordHashing.HashPassword(dto.Password),
                Role = "Student",
                PhoneNo = dto.PhoneNo,
                Dob = dto.Dob,
                Otp = newOtp
            };

            _context.Users.Add(user);
            int status = _context.SaveChanges();

            if (dto.VerifyEmail)
            {

                string emailSubject = "Verify Your Email";
                string emailBody = $"Dear {dto.FullName},\n\nYour OTP for email verification is: {newOtp}\n\nThank you!";

                try
                {
                    _emailService.SendSimpleEmail(dto.Email, emailSubject, emailBody);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to send OTP email to {Email}. Error: {ErrorMessage}", dto.Email, ex.Message);
                    // Return failure, user is not saved
                    return new StudentRegisterResponseDTO
                    {
                        status = 0,
                        UserId = -1,
                        Message = "Failed to send verification email."
                    };
                }
            }




            if (status > 0)
            {
                if (dto.VerifyEmail)
                {
                    _logger.LogInformation("New Student registration Started with UserId : {@userid} at {@time}. Verify Email!", user.UserId, user.CreatedAt);
                    return new StudentRegisterResponseDTO
                    {
                        status = status,
                        UserId = user.UserId,
                        Message = "User Saved! Please verify Email via OTP-verification"
                    };
                }
                else
                {
                    _logger.LogInformation("New Student registration Started with UserId : {@userid} at {@time}. Direct verification.", user.UserId, user.CreatedAt);
                    return new StudentRegisterResponseDTO
                    {
                        status = status,
                        UserId = user.UserId,
                        Otp = newOtp,
                        Message = "User Saved! See the console for OTP."
                    };
                }
            }

            // Default failure
            return new StudentRegisterResponseDTO
            {
                status = 0,
                UserId = -1,
                Message = "Registration failed."
            };
        }
        public User? Login(string email, string password)
        {

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            var hashedPassword = user.Password;

            if (!passwordHashing.VerifyPassword(password, hashedPassword))
            {
               return null;
            }

            if (user?.Otp != null)
            {
                _logger.LogWarning("User with userId {@userid} attempted to login without verifying OTP at {@time}", user.UserId, DateTime.Now);
                return null;
            }

            if (user.IsBlocked == true) return null;
            else
            {
                _logger.LogInformation("{@Role} with userId {@userid} logged in at {@time}", user.Role, user.UserId, user.CreatedAt);
                return user;
            }

        }

        public bool ValidateToken(string token)
        {
            return _context.Validations.Any(v => v.Token == token);
        }

        public bool VerifyOTP(VerifyOtpRequestDTO dto, int userId)
        {
            Console.WriteLine("Checking if otp incoming is correct: " + dto.otp + " " + dto.AllowSuccessMail);
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId && u.Otp == dto.otp);
            if (user != null)
            {
                user.Otp = null;
                user.RegistrationDate = DateOnly.FromDateTime(DateTime.Now);
                _context.SaveChanges();

                if (dto.AllowSuccessMail)
                {

                    string emailSubject = "Email Verified Successfully";
                    string emailBody = $"Dear {user.FullName},\n\nYour Email OTP verification was successful.\nYou are officially Verified Now!!\n\nThank you!";

                    try
                    {
                        _emailService.SendSimpleEmail(user.Email, emailSubject, emailBody);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Failed to send confirmation email to {Email}. Error: {ErrorMessage}", user.Email, ex.Message);
                        return false;
                    }
                }

                _logger.LogInformation("OTP verified for UserId : {@userid} at {@time}", user.UserId, DateTime.Now);
                return true;
            }
            return false;
        }

        public ResendOtpResponseDTO ResendOTP(int userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                Random random = new Random();
                int newOtp = random.Next(100000, 999999);
                user.Otp = newOtp;
                int status = _context.SaveChanges();
                _logger.LogInformation("OTP resent for UserId : {@userid} at {@time}", user.UserId, DateTime.Now);
                return new ResendOtpResponseDTO { status = 1, otp = newOtp };
            }
            return new ResendOtpResponseDTO { status = -1, otp = -1 };
        }
        public async Task<ResendOtpResponseDTO> RequestOtpForgotPassword(ForgotPasswordRequestDTO request)
        {
            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                return new ResendOtpResponseDTO { status = -1, otp = 0 };
            }

            Random random = new Random();
            int otp = random.Next(100000, 999999);

            user.Otp = otp;
            user.UpdatedAt = DateTime.UtcNow;
            UpdateUserDTO toBeUpdatedUser = new UpdateUserDTO { FullName = user.FullName, Dob = user.Dob, PhoneNo = user.PhoneNo, Email = user.Email };

            _userRepository.UpdateUser(user.UserId, dto: toBeUpdatedUser);

            if (request.VerifyWithEmail)
            {
                string emailSubject = "Verify Your Email";
                string emailBody = $"Dear {user.FullName},\n\nYour OTP for email verification is: {otp}\n\nThank you!";
                // Send the real email
                _emailService.SendSimpleEmail(user.Email, emailSubject, emailBody);
                return new ResendOtpResponseDTO { status = 1, otp = null };
            }
            else
            {
                // Log the OTP as requested for testing
                _logger.LogWarning($"User: {user.Email}, OTP: {otp}");
                return new ResendOtpResponseDTO { status = 1, otp = otp };
            }
        }
        public async Task<int> ResetPassword(ResetPasswordRequestDto request)
        {
            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                return -1;

            }

            // Check if OTP is valid
            if (user.Otp == null || user.Otp != request.Otp)
            {
                return -2;
            }



            // --- CRITICAL SECURITY STEP ---
            user.Password = passwordHashing.HashPassword(request.NewPassword);

            // Invalidate the OTP after use
            user.Otp = null;
            user.UpdatedAt = DateTime.UtcNow;

    

            await _userRepository.UpdateUserAsync(user);

            return 1;
        }
    }
}
