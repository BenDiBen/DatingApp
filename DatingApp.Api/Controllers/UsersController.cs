using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.Api.Data;
using DatingApp.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DataContext context;
        private readonly IMapper mapper;

        public UsersController(DataContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        // GET api/values
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var users = await this.context.Users.GetUsers();
            var usersToReturn = this.mapper.Map<IEnumerable<UserForListDto>>(users);

            return Ok(usersToReturn);
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var user = await this.context.Users.GetUser(id);

            if (user is null)
            {
                return this.NotFound();
            }

            var userToReturn = this.mapper.Map<UserForDetailedDto>(user);

            return this.Ok(userToReturn);
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdate)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var userFromRepo = await this.context.Users.GetUser(id);

            this.mapper.Map(userForUpdate, userFromRepo);

            try
            {
                await this.context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception exception)
            {
                throw new Exception($"Updating user with {id} failed on save.", exception);
            }
        }

    }
}