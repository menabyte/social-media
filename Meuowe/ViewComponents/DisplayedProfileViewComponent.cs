using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Meuowe.Areas.Database.Models;
using Meuowe.Areas.Database.Models.Identity;
using Meuowe.Areas.Database.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Meuowe.ViewComponents
{
    public class DisplayedProfileViewComponent : ViewComponent
    {
        private readonly MeuoweDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;


        public DisplayedProfileViewComponent(MeuoweDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<IViewComponentResult> InvokeAsync(string userView)
        {
            ProfileVM profileVM = await GetItemsAsync(userView);
            return View(profileVM);
        }

        private async Task<ProfileVM> GetItemsAsync(string userView)
        {
            string currentUser = _userManager.GetUserId(HttpContext.User);

            ProfileVM profileVM = new ProfileVM();

            string userId;

            // Get profile from passed in user view value that represents a user name.
            if (!String.IsNullOrEmpty(userView))
            {
                userId = await (from user in _context.ApplicationUsers
                                where user.UserName.Equals(userView)
                                select user.Id).FirstOrDefaultAsync();
                if (currentUser.Equals(userId))
                {
                    profileVM.Relation = 1;
                }
                else
                {
                    profileVM.Relation = 0;
                }
            }
            // Get default profile of user logged in if user view value is empty.
            else
            {
                userId = currentUser;
                profileVM.Relation = 1;

            }

            var applicationUser = await (from au in _context.ApplicationUsers
                                         where au.Id.Equals(userId)
                                         select au).FirstOrDefaultAsync();


            var userFollow = await (from uf in _context.UserFollows
                                    where uf.UserParentId.Equals(currentUser)
                                     && uf.UserChildId.Equals(userId)
                                    select uf).FirstOrDefaultAsync();

            var userFollowingCount = await (from follow in _context.UserFollows
                                            join user in _context.ApplicationUsers
                                            on follow.UserParentId equals user.Id
                                            where user.Id.Equals(userId)
                                            select follow).Distinct().CountAsync();

            var userFollowersCount = await (from follow in _context.UserFollows
                                            join user in _context.ApplicationUsers
                                            on follow.UserChildId equals user.Id
                                            where user.Id.Equals(userId)
                                            select follow).Distinct().CountAsync();



            // 2. Connect to Azure Storage account.
            var connectionString = _configuration.GetConnectionString("AccessKey");
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            // 3. Use container for users profile photos.
            string containerName = "profilephotos";
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // 4. Create new blob and upload to azure storage account.
            BlobClient blobClient = containerClient.GetBlobClient("profile-" + applicationUser.Id + ".png");

            if (!blobClient.Exists())
            {
                blobClient = containerClient.GetBlobClient("profiledefault.png");
            }

            //BlobDownloadInfo download = await blobClient.DownloadAsync();

            byte[] result = null;
            using (var ms = new MemoryStream())
            {
                blobClient.DownloadTo(ms);
                result = ms.ToArray();
            }


            string base64String = Convert.ToBase64String(result);

            string image = String.Format("data:image/png;base64,{0}", base64String);

            profileVM.FollowersCount = userFollowersCount;
            profileVM.FollowingCount = userFollowingCount;
            profileVM.AzurePhoto = image;
            profileVM.UserName = applicationUser.UserName;
            profileVM.DisplayName = applicationUser.DisplayName;
            profileVM.UserId = applicationUser.Id;

            if (null != userFollow)
            {
                profileVM.UserFollow = userFollow;
            }


            return profileVM;
        }
    }
}
