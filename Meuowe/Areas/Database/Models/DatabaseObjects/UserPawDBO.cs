using System;
using System.ComponentModel.DataAnnotations;

namespace Meuowe.Areas.Database.Models.DatabaseObjects
{
    public class UserPawDBO
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }

        public int ParentPawId { get; set; }

        public int ChildPawId { get; set; }

        [Required]
        [StringLength(512, MinimumLength = 1)]
        public string Message { get; set; }

        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; }
    }
}
