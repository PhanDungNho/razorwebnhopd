using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace razorweb.Models
{
    public class AppUser : IdentityUser
    {
        [Column(TypeName = "varchar")]
        [MaxLength(400)]
        public string HomeAddress { get; set; }

        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }
    }
}
