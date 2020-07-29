using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text;
using DatingApp.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

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
            var user = await userSet.Include(x => x.Photos).FirstOrDefaultAsync(x => x.UserName == userName);
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

            CreatePasswordHash(password, out var hashedPassword, out var passwordSalt);
            var registerUser = new User { UserName = user.UserName, PasswordSalt = passwordSalt, PasswordHash = hashedPassword };
            await userSet.AddAsync(registerUser);
            return registerUser;
        }

        internal static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var shaM = new HMACSHA512())
            {
                passwordSalt = shaM.Key;
                passwordHash = shaM.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        internal static async Task<User> GetUser(this DbSet<User> userSet, int id)
        {
            return await userSet.Include(x => x.Photos).FirstOrDefaultAsync(x => x.Id == id);
        }

        internal static async Task<IEnumerable<User>> GetUsers(this DbSet<User> userSet)
        {
            return await userSet.Include(x => x.Photos).ToListAsync();
        }
    }
}