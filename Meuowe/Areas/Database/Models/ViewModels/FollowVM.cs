using System;
using Meuowe.Areas.Database.Models.DatabaseObjects;

namespace Meuowe.Areas.Database.Models.ViewModels
{
    public class FollowVM
    {
        public int Relation { get; set; }
        public string ProfilePhoto { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string ProfileBiography { get; set; }
        public UserFollowDBO UserFollow { get; set; }
        public string AzurePhoto { get; set; }
    }
}
