using System;
using System.ComponentModel.DataAnnotations;

namespace Meuowe.Areas.Database.Models.DatabaseObjects
{
    public class UserDefaultImageDBO
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(256, MinimumLength = 8)]
        public string Location { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ModifiedDate { get; set; }
    }
}
