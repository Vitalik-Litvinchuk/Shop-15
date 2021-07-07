using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Shop_15.Models
{
    public class AppUser : IdentityUser
    {
        //[DisplayName("Full name")]
        //public string FullName { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Surname { get; set; }

        [Range(1, int.MaxValue)]
        public int Age { get; set; }

        public bool Gender { get; set; }

        public string Country { get; set; }

        public string City { get; set; }
    }
}
