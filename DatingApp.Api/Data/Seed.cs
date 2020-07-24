using System.Collections.Generic;
using System.Linq;
using DatingApp.Api.Models;
using Newtonsoft.Json;

namespace DatingApp.Api.Data
{
    public class Seed
    {
        public static void SeedUsers(DataContext context)
        {
            if (context.Users.Any())
            {
                return;
            }

            var userData = System.IO.File.ReadAllText("Migrations/UserSeedData.json");
            var users = JsonConvert.DeserializeObject<List<User>>(userData);
            foreach (var user in users)
            {
                UserQueries.CreatePasswordHash("password", out var passwordHash, out var passwordSalt);
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                user.UserName = user.UserName.ToLower();
                context.Users.Add(user);
            }

            context.SaveChanges();
        }
    }
}