using Domain.Data;
using Domain.Models;
using Infrastructure.DTOs.UserDTOs;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }


        public List<UserDetailsDTO> GetAllUsers()
        {

            var users = _context.Users.ToList();

            var userDtos = users.Select(user => new UserDetailsDTO
            {
                FullName = user.FullName,
                Email = user.Email,
                Dob = (DateOnly)user.Dob,
                PhoneNo = user.PhoneNo,
                Role = user.Role,
                IsBlocked = (bool)user.IsBlocked
            }).ToList();

            return userDtos;

        }

        public UserDetailsDTO? GetUserById(int id)
        {

            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
                return null;
            return new UserDetailsDTO
            {
                FullName = user.FullName,
                Dob = user.Dob.GetValueOrDefault(),
                IsBlocked = user.IsBlocked.GetValueOrDefault(),
                PhoneNo = user.PhoneNo,
                Role = user.Role,
                RegistrationDate = user.RegistrationDate
            };

        }

        public List<UserDetailsDTO> GetUsersByRole(string role)
        {

            var users = _context.Users.Where(u => u.Role == role).ToList();

            var userDtos = users.Select(user => new UserDetailsDTO
            {
                FullName = user.FullName,
                Email = user.Email,
                Dob = (DateOnly)user.Dob,
                PhoneNo = user.PhoneNo,
                Role = user.Role,
                IsBlocked = (bool)user.IsBlocked
            }).ToList();

            return userDtos;

        }

        public int UpdateUser(int id, UpdateUserDTO dto)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (existingUser == null)
                return 0;

            if (!string.IsNullOrEmpty(dto.FullName))
                existingUser.FullName = dto.FullName;

            if (dto.Dob != null)
                existingUser.Dob = dto.Dob;

            if (!string.IsNullOrEmpty(dto.PhoneNo))
                existingUser.PhoneNo = dto.PhoneNo;


            if (!string.IsNullOrEmpty(dto.Email))
                existingUser.Email = dto.Email;


            return _context.SaveChanges();
        }

        public int DeleteUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
                return 0;

            _context.Users.Remove(user);
            return _context.SaveChanges();
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                                 .FirstOrDefaultAsync(u => u.Email == email && u.IsBlocked == false);
        }
        public async Task<bool> UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }


    }
}



