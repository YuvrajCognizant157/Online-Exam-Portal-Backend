﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DTOs.adminDTOs
{
    public class ToggleUserStatusDto
    {
        public int UserId { get; set; }
        public bool IsActive { get; set; }
    }
}
