using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using Meuowe.Areas.Database.Models.DatabaseObjects;
using Meuowe.Areas.Database.Models.Identity;
using Microsoft.AspNetCore.Http;

namespace Meuowe.Areas.Database.Models.ViewModels
{
    public class ProfileVM
    {
        public int Relation { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public string ProfilePhoto { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string ProfileBiography { get; set; }
        public string UserId { get; set; }
        public UserFollowDBO UserFollow { get; set; }
        public string AzurePhoto { get; set;}
    }
}
