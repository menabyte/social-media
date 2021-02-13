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
    public class DisplayedPawViewComponent : ViewComponent
    {
        private readonly MeuoweDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;


        public DisplayedPawViewComponent(MeuoweDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<IViewComponentResult> InvokeAsync(UserPawDBO userPaw)
        {
            PawVM pawVM = await GetItemsAsync(userPaw);
            return View(pawVM);
        }

        private async Task<PawVM> GetItemsAsync(UserPawDBO userPaw)
        {
            string userId = _userManager.GetUserId(HttpContext.User);

            PawVM pawVM = new PawVM();

            var user = await (from usr in _context.ApplicationUsers
                              where usr.Id.Equals(userPaw.UserId)
                              select usr).FirstOrDefaultAsync();


            var userShake = await (from us in _context.UserShakes
                                   where us.UserId.Equals(userId)
                                    && us.PawId.Equals(userPaw.Id)
                                   select us).FirstOrDefaultAsync();


            var userWagTail = await (from uw in _context.UserWags
                                     where uw.UserId.Equals(userId)
                                      && uw.PawId.Equals(userPaw.Id)
                                     select uw).FirstOrDefaultAsync();


            int userShakeCount = await _context.UserShakes.
                                                        Where(x => x.PawId.Equals(userPaw.Id)).CountAsync();

            int userWagCount = await _context.UserWags.
                                                        Where(x => x.PawId.Equals(userPaw.Id)).CountAsync();

            if (null != userId && userId.Equals(user.Id))
            {
                pawVM.Relation = 0;
            }
            else
            {
                pawVM.Relation = 1;
            }

            if (null != userShake)
            {
                pawVM.UserShake = userShake;
            }

            if (null != userWagTail)
            {
                pawVM.UserWagTail = userWagTail;
            }

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

            pawVM.UserPaw = userPaw;
            pawVM.UserName = user.UserName;
            pawVM.DisplayName = user.DisplayName;
            pawVM.UserShakeCount = userShakeCount;
            pawVM.UserWagCount = userWagCount;
            pawVM.AzurePhoto = image;

            return pawVM;
        }
    }
}
