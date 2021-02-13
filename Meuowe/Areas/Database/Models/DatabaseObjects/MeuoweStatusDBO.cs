using System;
using System.ComponentModel.DataAnnotations;

namespace Meuowe.Areas.Database.Models.DatabaseObjects
{
    public class MeouweStatusDBO
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
