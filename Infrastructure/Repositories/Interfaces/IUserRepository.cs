using Domain.Models;
using Infrastructure.DTOs.UserDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IUserRepository
    {
        List<UserDetailsDTO> GetAllUsers();
        UserDetailsDTO? GetUserById(int id);
        List<UserDetailsDTO> GetUsersByRole(string role);
        int UpdateUser(int id, UpdateUserDTO dto);
        int DeleteUser(int id);
        Task<User> GetUserByEmailAsync(string? email);
        Task<bool> UpdateUserAsync(User user);
    }
}
