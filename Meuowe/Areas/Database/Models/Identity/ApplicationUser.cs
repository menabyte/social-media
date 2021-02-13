using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Meuowe.Areas.Database.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; }

        [Display(Name = "Biography")]
        public string Biography { get; set; }

        [Display(Name = "Photo")]
        public string Photo { get; set; }

        public static implicit operator ApplicationUser(Task<ApplicationUser> v)
        {
            throw new NotImplementedException();
        }
    }
}
