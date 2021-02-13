using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Meuowe.Areas.Database.Models;
using Meuowe.Areas.Database.Models.Identity;
using Meuowe.Areas.Database.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;
using System.IO;

namespace Meuowe.ViewComponents
{
    public class DisplayedSearchDropDownViewComponent : ViewComponent
    {
        private readonly MeuoweDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;


        public DisplayedSearchDropDownViewComponent(MeuoweDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<IViewComponentResult> InvokeAsync(string userName)
        {
            List<ProfileVM> profileVMs = await GetItemsAsync(userName);
            return View(profileVMs);
        }

        private async Task<List<ProfileVM>> GetItemsAsync(string userName)
        {
           var usersDropDown = await (from user in _context.ApplicationUsers
                                     where user.UserName.Contains(userName)
                                     select user).ToListAsync();

            List<ProfileVM> profileVMs = new List<ProfileVM>();
            foreach (var usersDropDow in usersDropDown)
            {
                ProfileVM profileVM = new ProfileVM();

                // 2. Connect to Azure Storage account.
                var connectionString = _configuration.GetConnectionString("AccessKey");
                BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

                // 3. Use container for users profile photos.
                string containerName = "profilephotos";
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                // 4. Create new blob and upload to azure storage account.
                BlobClient blobClient = containerClient.GetBlobClient("profile-" + usersDropDow.Id + ".png");

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

                profileVM.AzurePhoto = image;
                profileVM.UserName = usersDropDow.UserName;
                profileVM.DisplayName = usersDropDow.DisplayName;
                profileVMs.Add(profileVM);
            }

            return profileVMs;
        }
    }
}
