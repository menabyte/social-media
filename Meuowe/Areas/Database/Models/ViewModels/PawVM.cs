using System;
using Meuowe.Areas.Database.Models.DatabaseObjects;

namespace Meuowe.Areas.Database.Models.ViewModels
{
    public class PawVM
    {
        public int Relation { get; set; }
        public string DisplayName { get; set; }
        public string UserName { get; set; }
        public UserPawDBO UserPaw { get; set; }
        public UserFollowDBO UserFollow { get; set; }
        public UserShakeDBO UserShake { get; set; }
        public UserWagDBO UserWagTail { get; set; }
        public int UserShakeCount { get; set; }
        public int UserWagCount { get; set; }
        public int UserReplyCount { get; set; }
        public string AzurePhoto { get; set; }
    }
}
