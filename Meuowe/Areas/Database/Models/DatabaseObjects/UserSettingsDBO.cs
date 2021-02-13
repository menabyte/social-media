using System;
using System.ComponentModel.DataAnnotations;

namespace Meuowe.Areas.Database.Models.DatabaseObjects
{
    public class UserSettingsDBO
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public Boolean IsPublic { get; set; }

        [Required]
        public Boolean IsAcceptingMessages { get; set; }

        [Required]
        public Boolean IsAllowingMeuowes { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ModifiedDate { get; set; }
    }
}
