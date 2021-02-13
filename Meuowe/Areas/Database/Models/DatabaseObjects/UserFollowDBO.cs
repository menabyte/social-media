using System;
using System.ComponentModel.DataAnnotations;

namespace Meuowe.Areas.Database.Models.DatabaseObjects
{
    public class UserFollowDBO
    {
        [Key]
        public int Id { get; set; }

        public string UserParentId { get; set; }

        public string UserChildId { get; set; }

        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; }
    }
}
