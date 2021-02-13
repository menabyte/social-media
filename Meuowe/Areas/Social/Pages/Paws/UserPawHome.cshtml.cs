using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Meuowe.Areas.Database.Models;
using Meuowe.Areas.Database.Models.Identity;
using Meuowe.Areas.Database.Models.DatabaseObjects;
using Meuowe.Areas.Database.Models.ViewModels;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace Meuowe.Areas.Social.Pages.Paws
{
    public class UserPawHomeModel : PageModel
    {
        private readonly MeuoweDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        private readonly int _pageSize = 9;

        public UserPawHomeModel(MeuoweDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        public IActionResult OnGetPawHome(string home, string userView)
        {
            ViewData["currentView"] = home;
            ViewData["userView"] = userView;
            return Page();
        }

        // Partial Views
        /****************************************************************/
        public IActionResult OnGetProfileViewComponent(string userView)
        {
            return ViewComponent("DisplayedProfile", new { userView = userView });
        }

        public async Task<IActionResult> OnGetPawViewComponent(int pawId)
        {
            UserPawDBO userPaw = await (from paw in _context.UserPaws
                                            where paw.Id.Equals(pawId)
                                        select paw
                                        ).FirstOrDefaultAsync();

            return ViewComponent("DisplayedPaw", new { userPaw = userPaw });
        }

        public IActionResult OnGetPawsViewComponent(string home, string userView, int pageIndex)
        {
            if (String.IsNullOrEmpty(home))
            {
                home = "home";
                pageIndex = 0;
            }
            return ViewComponent("DisplayedPaws", new { home = home, pageIndex = pageIndex, pageSize = _pageSize, userView = userView });
        }

        public PartialViewResult OnGetCreatePawPartial()
        {
            PawVM pawVM = new PawVM();

            return Partial("./Shared/_UserCreatePaw", pawVM);
        }

        public async Task<PartialViewResult> OnGetReplyUserPartialAsync(int pawId, string userPawId)
        {
            var userPaw = await _context.UserPaws.Where(x => x.Id.Equals(pawId)).FirstOrDefaultAsync();
            var userReplyTo = await _context.ApplicationUsers.Where(x => x.Id.Equals(userPawId)).FirstOrDefaultAsync();

            PawVM pawVM = new PawVM();
            pawVM.UserPaw = userPaw;
            pawVM.DisplayName = userReplyTo.DisplayName;
            pawVM.UserName = userReplyTo.UserName;
          
            return Partial("./Shared/_ModalReply", pawVM);
        }

        public async Task<PartialViewResult> OnGetSendMeuowePartialAsync(int pawId, string userPawId)
        {
            var userPaw = await _context.UserPaws.Where(x => x.Id.Equals(pawId)).FirstOrDefaultAsync();
            var userSendMeuowe = await _context.ApplicationUsers.Where(x => x.Id.Equals(userPawId)).FirstOrDefaultAsync();

            PawVM pawVM = new PawVM();
            pawVM.UserPaw = userPaw;
            pawVM.DisplayName = userSendMeuowe.DisplayName;
            pawVM.UserName = userSendMeuowe.UserName;

            return Partial("./Shared/_SendMeuowePartial", pawVM);
        }

        // Transactions
        /****************************************************************/
        public async Task<JsonResult> OnGetShowFollowers(string userView)
        {
            string userId;

            if (!String.IsNullOrEmpty(userView))
            {
                userId = await (from user in _context.ApplicationUsers
                                where user.UserName.Equals(userView)
                                select user.Id).FirstOrDefaultAsync();
            }

            else
            {
                userId = _userManager.GetUserId(HttpContext.User);
            }

            var userFollowersCount = await (from follow in _context.UserFollows
                                            join user in _context.ApplicationUsers
                                            on follow.UserChildId equals user.Id
                                            where user.Id.Equals(userId)
                                            select follow).Distinct().CountAsync();

            return new JsonResult(userFollowersCount);
        }

        public async Task<JsonResult> OnGetUserFollowAsync(string userChildId)
        {
            var returnString = "failure";
            string userId = _userManager.GetUserId(User);

            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(userChildId))
            {
                UserFollowDBO userFollow = await _context.UserFollows.
                                                            Where(x => x.UserParentId.Equals(userId)
                                                                && x.UserChildId.Equals(userChildId)).
                                                            FirstOrDefaultAsync();
                if (null == userFollow)
                {
                    userFollow = new UserFollowDBO();
                    userFollow.UserParentId = userId;
                    userFollow.UserChildId = userChildId;
                    userFollow.CreatedDate = DateTime.Now;

                    _context.UserFollows.Add(userFollow);
                    returnString = "success";
                    await _context.SaveChangesAsync();
                }
                else
                {
                    _context.UserFollows.Remove(userFollow);
                    returnString = "success";
                    await _context.SaveChangesAsync();
                }

            }
            return new JsonResult(returnString);
        }


        public async Task<JsonResult> OnPostUploadProfilePhoto(IFormFile ProfilePhoto)
        {
            var returnMessage = "failure";

            string userId = _userManager.GetUserId(User);


            // 1. Convert image to byte array for photo manipulation.
            byte[] result = null;
            using var fileStream = ProfilePhoto.OpenReadStream();
            using (var outStream = new MemoryStream())
            {
                var imageStream = Image.FromStream(fileStream);

                var height = (150 * imageStream.Height) / imageStream.Width;
                var thumbnail = imageStream.GetThumbnailImage(150, 150, null, IntPtr.Zero);

                using (var thumbnailStream = new MemoryStream())
                {
                    thumbnail.Save(thumbnailStream, ImageFormat.Png);
                    result =  thumbnailStream.ToArray();
                }
            }
            
            // 2. Connect to Azure Storage account.
            var connectionString = _configuration.GetConnectionString("AccessKey");
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            // 3. Use container for users profile photos.
            string containerName = "profilephotos";
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // 4. Create new blob and upload to azure storage account.
            var stream = new MemoryStream(result);
            BlobClient blobClient = containerClient.GetBlobClient("profile-"+ userId+".png");

            if (!blobClient.Exists())
            {
                await blobClient.UploadAsync(stream, true);
            }
            else
            {
                await blobClient.DeleteAsync();
                await blobClient.UploadAsync(stream, true);
            }

            return new JsonResult(returnMessage);
        }

        public async Task<JsonResult> OnGetShakePawAsync(string pawId)
        {
            var returnMessage = "failure";
            string userId = _userManager.GetUserId(User);

           if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(pawId))
            {
                UserShakeDBO shakePaw = await _context.UserShakes.Where(x => x.UserId.Equals(userId)
                    && x.PawId.Equals(Convert.ToInt32(pawId))).FirstOrDefaultAsync();

                if (null == shakePaw)
                {
                    shakePaw = new UserShakeDBO();
                    shakePaw.UserId = userId;
                    shakePaw.PawId = Convert.ToInt32(pawId);
                    shakePaw.CreatedDate = DateTime.Now;

                    _context.UserShakes.Add(shakePaw);
                    await _context.SaveChangesAsync();
                    returnMessage = "success";
                }
                else
                {
                    _context.UserShakes.Remove(shakePaw);
                    await _context.SaveChangesAsync();
                    returnMessage = "success";
                }
            }
            return new JsonResult(returnMessage);
        }

        public async Task<JsonResult> OnGetWagTailAsync(string pawId)
        {
            var returnMessage = "failure";
            string userId = _userManager.GetUserId(User);

            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(pawId))
            {
                UserWagDBO wagTail = await _context.UserWags.Where(x => x.UserId.Equals(userId)
                    && x.PawId.Equals(Convert.ToInt32(pawId))).FirstOrDefaultAsync();

                if (null == wagTail)
                {
                    wagTail = new UserWagDBO();
                    wagTail.UserId = userId;
                    wagTail.PawId = Convert.ToInt32(pawId);
                    wagTail.CreatedDate = DateTime.Now;

                    _context.UserWags.Add(wagTail);
                    await _context.SaveChangesAsync();
                    returnMessage = "success";
                }
                else
                {
                    _context.UserWags.Remove(wagTail);
                    await _context.SaveChangesAsync();
                    returnMessage = "success";
                }
            }

            return new JsonResult(returnMessage);
        }

        public async Task<JsonResult> OnPostCreateAsync(string PawMessage)
        {
            var returnMessage = "failure";
            var userName = User.Identity.Name;
            var userId = _userManager.GetUserId(User);

            if (!string.IsNullOrEmpty(userId))
            {
                UserPawDBO userPaw = new UserPawDBO();
                userPaw.UserId = userId;
                userPaw.ParentPawId = 0;
                userPaw.ChildPawId = 0;
                userPaw.CreatedDate = DateTime.Now;
                userPaw.Message = PawMessage;

                _context.UserPaws.Add(userPaw);
                await _context.SaveChangesAsync();
                returnMessage = userPaw.Id.ToString();
            }

            return new JsonResult(returnMessage);
        }

        public IActionResult OnPostDelete(string pawID)
        {
            var returnMessage = "failure";
            var userId = _userManager.GetUserId(User);

            if (!string.IsNullOrEmpty(userId))
            {
                UserPawDBO userPaw = (from p in _context.UserPaws
                                      where p.Id.Equals(pawID) && p.UserId.Equals(userId)
                                      select p).FirstOrDefault();

                if (null != userPaw)
                {
                    _context.Remove(userPaw);
                    _context.SaveChangesAsync();
                    returnMessage = "success";
                }
            }

            return new JsonResult(returnMessage);
        }

        public async Task<JsonResult> OnPostReplyAsync(int PawId, string PawMessage)
        {
            var returnMessage = "failure";
            var userName = User.Identity.Name;
            var userId = _userManager.GetUserId(User);

            if (!string.IsNullOrEmpty(userId))
            {
                UserPawDBO userPaw = new UserPawDBO();
                userPaw.UserId = userId;
                userPaw.ParentPawId = PawId;
                userPaw.ChildPawId = 0;
                userPaw.CreatedDate = DateTime.Now;
                userPaw.Message = PawMessage;

                _context.UserPaws.Add(userPaw);
                await _context.SaveChangesAsync();
                returnMessage = "success";
            }

            return new JsonResult(returnMessage);
        }

        // Advertisement embedded in paw.
        // Advertisement embedded in paw.
        public async Task<JsonResult> OnPostMeuoweAsync(int PawId, string MeuoweOffer)
        {
            var returnMessage = "failure";
            var userName = User.Identity.Name;
            var userId = _userManager.GetUserId(User);

            if (!string.IsNullOrEmpty(userId))
            {
                UserMeuoweDBO userMeuowe = new UserMeuoweDBO();
                userMeuowe.UserId = userId;
                userMeuowe.PawId = PawId;
                userMeuowe.Cost = Decimal.Parse(MeuoweOffer);
                userMeuowe.AcceptMeuowe = false;
                userMeuowe.CreatedDate = DateTime.Now;
                userMeuowe.ModifiedDate = DateTime.Now;

                UserMeuoweDBO existingUserMeuowe = await (from existingMeuowe in _context.UserMeuowes
                                                          where existingMeuowe.PawId.Equals(PawId)
                                                          && existingMeuowe.UserId.Equals(userId)
                                                          select existingMeuowe).FirstOrDefaultAsync();

                if (null == existingUserMeuowe)
                {
                    _context.UserMeuowes.Add(userMeuowe);
                    await _context.SaveChangesAsync();
                    returnMessage = "success";
                }
                else
                {
                    if (Decimal.Parse(MeuoweOffer) > existingUserMeuowe.Cost)
                    {
                        _context.UserMeuowes.Remove(existingUserMeuowe);
                        _context.UserMeuowes.Add(userMeuowe);
                        await _context.SaveChangesAsync();
                        returnMessage = "success";
                    }
                    else
                    {
                        returnMessage = "please make a higher offer";
                    }
                }
            }
            return new JsonResult(returnMessage);
        }
    }
}
