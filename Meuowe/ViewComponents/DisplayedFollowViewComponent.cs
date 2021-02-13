using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Meuowe.Areas.Database.Models;
using Meuowe.Areas.Database.Models.DatabaseObjects;
using Meuowe.Areas.Database.Models.Identity;
using Meuowe.Areas.Database.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Meuowe.ViewComponents
{
    public class DisplayedFollowViewComponent : ViewComponent
    {
        private readonly MeuoweDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;


        public DisplayedFollowViewComponent(MeuoweDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<IViewComponentResult> InvokeAsync(ApplicationUser user)
        {
            FollowVM followVM = await GetItemsAsync(user);
            return View(followVM);
        }

        private async Task<FollowVM> GetItemsAsync(ApplicationUser user)
        {
            string currentUser = _userManager.GetUserId(HttpContext.User);


            // 2. Connect to Azure Storage account.
            var connectionString = _configuration.GetConnectionString("AccessKey");
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            // 3. Use container for users profile photos.
            string containerName = "profilephotos";
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // 4. Create new blob and upload to azure storage account.
            BlobClient blobClient = containerClient.GetBlobClient("profile-" + user.Id + ".png");

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

            FollowVM followVM = new FollowVM();


            var userFollow = await (from uf in _context.UserFollows
                                    where uf.UserParentId.Equals(currentUser)
                                     && uf.UserChildId.Equals(user.Id)
                                    select uf).FirstOrDefaultAsync();

            followVM.DisplayName = user.DisplayName;
            followVM.UserName = user.UserName;
            followVM.ProfileBiography = user.Biography;
            followVM.AzurePhoto = image;
            followVM.UserFollow = userFollow;

            return followVM;
        }
    }
}
