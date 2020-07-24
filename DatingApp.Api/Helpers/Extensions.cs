using System;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace DatingApp.Api.Helpers
{
    public static class Extensions
    {
        public static void AddApplicationError(this HttpResponse response, string message)
        {
            response.Headers.Add("Application-Error", message);
            response.Headers.Add("Access-Control-Expose-Headers", "Application-Error");
            response.Headers.Add("Access-Control-Allow-Origin", "*");
        }
        
        public static void AddApplicationError(this HttpResponse response, IExceptionHandlerFeature error)
        {
            response.AddApplicationError(error.Error.Message);
        }

        public static int CurrentAge(this DateTime birthDate)
        {
            var age = DateTime.Today.Year - birthDate.Year;
            if (birthDate.AddYears(age) > DateTime.Today)
            {
                return --age;
            }

            return age;
        }
    }
}