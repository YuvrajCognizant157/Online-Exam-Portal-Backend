using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DTOs.AuthDTOs
{
    public class ResendOtpResponseDTO
    {
        public int status { get; set; }
        public int? otp { get; set; }
    }
}
