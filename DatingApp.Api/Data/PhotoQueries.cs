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
    public static class PhotoQueries
    {

        public static async Task<Photo> GetMainPhotoForUserAsync(this IQueryable<Photo> photoSet, int userId)
        {
            var photo = await photoSet.FirstOrDefaultAsync(x => x.UserId == userId && x.IsMain);
            return photo;
        }
    }
}