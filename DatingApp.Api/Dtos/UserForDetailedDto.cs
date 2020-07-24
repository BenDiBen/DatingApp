using System;
using System.Collections.Generic;
using DatingApp.Api.Models;

namespace DatingApp.Api.Dtos
{
    public class UserForDetailedDto : UserForListDto
    {
        public string Introduction { get; set; }
        public string LookingFor { get; set; }
        public string Interests { get; set; }
        public ICollection<PhotoForDetailedDto> Photos {get;set;}
    }
}