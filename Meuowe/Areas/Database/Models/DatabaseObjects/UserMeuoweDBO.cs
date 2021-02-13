using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Meuowe.Areas.Database.Models.DatabaseObjects
{
    public class UserMeuoweDBO
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }


        [Required]
        public int PawId { get; set; }

        [Required]
        public bool AcceptMeuowe { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Cost { get; set; }


        [Required]
        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime? ModifiedDate { get; set; }
    }
}
