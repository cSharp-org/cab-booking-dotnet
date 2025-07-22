using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CabBooking.Core.Models;

namespace CabBooking.Core.Interfaces
{
    public interface IUserService
    {
        Task<object> GetByIdAsync(Guid id);
        Task<User> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task DeleteAsync(Guid id);
        Task<bool> VerifyEmailAsync(string token, string email);
        Task<bool> VerifyPhoneAsync(string code, string phoneNumber);
        Task<bool> ChangePasswordAsync(string currentPassword, string newPassword, Guid userId);
        Task<bool> ResetPasswordAsync(string token, string newPassword, string email);
        Task<string> GeneratePasswordResetTokenAsync(string email);
    }
} 