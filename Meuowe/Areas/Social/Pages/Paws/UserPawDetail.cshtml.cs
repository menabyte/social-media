using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Meuowe.Areas.Database.Models;
using Meuowe.Areas.Database.Models.DatabaseObjects;
using Meuowe.Areas.Database.Models.Identity;
using Meuowe.Areas.Database.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Meuowe.Areas.Social.Pages.Paws
{
    public class UserPawDetailModel : PageModel
    {
        private readonly MeuoweDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
                private readonly int _pageSize = 9;


        public UserPawDetailModel(MeuoweDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public void OnGetPawDetail(int pawId)
        {
            ViewData["PawId"] = pawId;
        }


        public async Task<IActionResult> OnGetPawViewComponent(int pawId)
        {
            UserPawDBO userPaw = await (from paw in _context.UserPaws
                                        where paw.Id.Equals(pawId)
                                        select paw
                                        ).FirstOrDefaultAsync();

            return ViewComponent("DisplayedPaw", new { userPaw = userPaw });
        }

        public IActionResult OnGetPawsViewComponent(string pawView, int pageIndex, int pawId)
        {
            if (String.IsNullOrEmpty(pawView))
            {
                pawView = "replies";
                pageIndex = 0;
            }
            return ViewComponent("DisplayedRepliesPaws", new { pawView = pawView, pageIndex = pageIndex, pawId = pawId, pageSize = _pageSize });
        }



        public async Task<PartialViewResult> OnGetPawRepliesPartialAsync(int pawId)
        {
            string userId = _userManager.GetUserId(User);

            List<PawVM> userPawVM = new List<PawVM>();

            var userPaws = await _context.UserPaws.Where(x => x.ParentPawId.Equals(pawId)).ToListAsync();

            for (int i = 0; i < userPaws.Count(); i++)
            {
                PawVM pawVM = new PawVM();
                pawVM.UserPaw = userPaws[i];

                var user = await _context.ApplicationUsers.
                                            Where(x => x.Id.Equals(userPaws[i].UserId)).
                                            FirstOrDefaultAsync();

                var userFollow = await _context.UserFollows.
                                                            Where(x => x.UserParentId.Equals(userId)
                                                                && x.UserChildId.Equals(userPaws[i].UserId)).
                                                            FirstOrDefaultAsync();

                var userShake = await _context.UserShakes.
                                                            Where(x => x.UserId.Equals(userId)
                                                                && x.PawId.Equals(userPaws[i].Id)).
                                                            FirstOrDefaultAsync();

                var userWagTail = await _context.UserWags.
                                                            Where(x => x.UserId.Equals(userId)
                                                                && x.PawId.Equals(userPaws[i].Id)).
                                                            FirstOrDefaultAsync();

                int userShakeCount = _context.UserShakes.
                                                              Where(x => x.PawId.Equals(userPaws[i].Id)).Count();

                int userWagCount = _context.UserWags.
                                                        Where(x => x.PawId.Equals(userPaws[i].Id)).Count();

                if (userId.Equals(user.Id))
                {
                    pawVM.Relation = 0;
                }
                else
                {
                    pawVM.Relation = 1;
                }

                if (null != userFollow)
                {
                    pawVM.UserFollow = userFollow;
                }

                if (null != userShake)
                {
                    pawVM.UserShake = userShake;
                }

                if (null != userWagTail)
                {
                    pawVM.UserWagTail = userWagTail;
                }

                pawVM.UserName = user.UserName;
                pawVM.DisplayName = user.DisplayName;
                pawVM.UserShakeCount = userShakeCount;
                pawVM.UserWagCount = userWagCount;

                userPawVM.Add(pawVM);
            }

            return Partial("./Shared/_PawPartial", userPawVM);
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

        // Transactions
        /****************************************************************/
        public async Task<JsonResult> OnGetUserFollowAsync(string pawId, string userChildId)
        {
            var returnMessage = "failure";
            string userId = _userManager.GetUserId(User);

            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(pawId) && !string.IsNullOrEmpty(userChildId))
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
                    await _context.SaveChangesAsync();
                    returnMessage = "success";
                }
                else
                {
                    returnMessage = "already following";
                }
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
    }
}
