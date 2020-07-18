using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text;
using DatingApp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Api.Data
{
    public static class UserQueries
    {
        public static async Task<bool> ExistsAsync(this IQueryable<User> query, string userName)
        {
            return await query.AnyAsync(x => x.UserName == userName);
        }

        public static async Task<User> LoginAsync(this IQueryable<User> userSet, string userName, string password)
        {
            var user = await userSet.FirstOrDefaultAsync(x => x.UserName == userName);
            if (user is null || !VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                return null;
            }

            return user;
        }

        private static bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var shaM = new HMACSHA512(passwordSalt))
            {
                byte[] hashedPassword = shaM.ComputeHash(Encoding.UTF8.GetBytes(password));
                return hashedPassword.SequenceEqual(passwordHash);
            }
        }

        public static async Task<User> RegisterAsync(this DbSet<User> userSet, User user, string password)
        {
            if (await userSet.AnyAsync(x => x.UserName == user.UserName))
            {
                throw new InvalidOperationException("The user already exists.");
            }

            using (var shaM = new HMACSHA512())
            {
                var hashedPassword = shaM.ComputeHash(Encoding.UTF8.GetBytes(password));
                var registerUser = new User { UserName = user.UserName, PasswordSalt = shaM.Key, PasswordHash = hashedPassword};
                await userSet.AddAsync(registerUser);
                return registerUser;
            }
        }
    }
}