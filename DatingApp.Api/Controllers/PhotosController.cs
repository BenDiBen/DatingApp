using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.Api.Data;
using DatingApp.Api.Dtos;
using DatingApp.Api.Helpers;
using DatingApp.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DatingApp.Api.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/[controller]")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly DataContext context;
        private readonly IMapper mapper;
        private readonly IOptions<CloudinarySettings> cloudinaryConfig;

        public Cloudinary cloudinary { get; private set; }

        public PhotosController(DataContext context, IMapper mapper, IOptions<CloudinarySettings> cloudinaryConfig)
        {
            this.context = context;
            this.mapper = mapper;
            this.cloudinaryConfig = cloudinaryConfig;

            Account account = new Account
            {
                ApiKey = cloudinaryConfig.Value.ApiKey,
                ApiSecret = cloudinaryConfig.Value.ApiSecret,
                Cloud = cloudinaryConfig.Value.CloudName
            };

            this.cloudinary = new Cloudinary(account);
        }

        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo = await this.context.Photos.FirstOrDefaultAsync(x => x.Id == id);

            if (photoFromRepo is null)
            {
                return this.NotFound();
            }

            var photo = this.mapper.Map<PhotoForReturnDto>(photoFromRepo);

            return this.Ok(photo);
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, [FromForm] PhotoForCreationDto photoForCreation)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return this.Unauthorized();
            }

            var userFromRepo = await this.context.Users.GetUser(userId);
            var file = photoForCreation.File;

            if (file == null || file.Length <= 0)
            {
                return this.BadRequest("The file is empty.");
            }

            var uploadResult = new ImageUploadResult();

            using (var stream = file.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.Name, stream),
                    Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                };

                uploadResult = this.cloudinary.Upload(uploadParams);
            }

            if (!uploadResult.StatusCode.IsSuccessStatusCode())
            {
                return this.StatusCode(500, "The file could not be uploaded.");
            }

            photoForCreation.Url = uploadResult.Url.ToString();
            photoForCreation.PublicId = uploadResult.PublicId;

            var photo = this.mapper.Map<Photo>(photoForCreation);
            photo.IsMain = userFromRepo.Photos.Any(u => u.IsMain);

            userFromRepo.Photos.Add(photo);
            await this.context.SaveChangesAsync();

            var photoToReturn = this.mapper.Map<PhotoForReturnDto>(photo);

            return this.CreatedAtRoute("GetPhoto", new { userId = userId, id = photo.Id }, photoToReturn);
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return this.Unauthorized();
            }

            var photoFromRepo = await this.context.Photos.FirstOrDefaultAsync(x => x.Id == id);

            if (photoFromRepo == null)
            {
                return this.NotFound($"Photo with ID {id} does not exist.");
            }

            if (photoFromRepo.UserId != userId)
            {
                return this.Unauthorized();
            }

            if (photoFromRepo.IsMain)
            {
                return this.BadRequest("This is already the main photo.");
            }

            var currentMainPhoto = await this.context.Photos.GetMainPhotoForUserAsync(userId);
            if (currentMainPhoto != null)
            {
                currentMainPhoto.IsMain = false;
            }

            photoFromRepo.IsMain = true;

            if (await this.context.SaveChangesAsync() > 0)
            {
                return this.NoContent();
            }

            return this.BadRequest("Could not set photo to main");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return this.Unauthorized();
            }

            var photoFromRepo = await this.context.Photos.FirstOrDefaultAsync(x => x.Id == id);

            if (photoFromRepo == null)
            {
                return this.NotFound($"Photo with ID {id} does not exist.");
            }

            if (photoFromRepo.UserId != userId)
            {
                return this.Unauthorized();
            }

            if (photoFromRepo.IsMain)
            {
                return this.BadRequest("Cannot delete the main photo.");
            }

            if (!(photoFromRepo.PublicId is null))
            {
                var result = this.cloudinary.Destroy(new DeletionParams(photoFromRepo.PublicId));

                if (!result.StatusCode.IsSuccessStatusCode())
                {
                    return this.BadRequest("The photo could not be deleted.");
                }
            }

            this.context.Photos.Remove(photoFromRepo);

            if (await this.context.SaveChangesAsync() > 0)
            {
                return this.Ok();
            }

            return this.BadRequest("The photo could not be deleted.");
        }
    }
}