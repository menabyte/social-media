using System;
using System.ComponentModel.DataAnnotations;

namespace Meuowe.Areas.Database.Models.DatabaseObjects
{
    public class UserWagDBO
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }

        public int PawId { get; set; }

        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; }
    }
}
