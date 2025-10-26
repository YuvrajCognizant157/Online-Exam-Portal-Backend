using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DTOs.UserDTOs
{
    public class UserDetailsDTO
    {

        public string? FullName { get; set; }
        public string? Email { get; set; }
        public DateOnly? Dob { get; set; }
        public string? PhoneNo { get; set; }
        public string? Role { get; set; }
        public bool? IsBlocked { get; set; }
        public DateOnly? RegistrationDate { get; set; }

    }
}
