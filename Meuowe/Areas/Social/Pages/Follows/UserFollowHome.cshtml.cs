using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Meuowe.Areas.Database.Models;
using Meuowe.Areas.Database.Models.DatabaseObjects;
using Meuowe.Areas.Database.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Meuowe.Areas.Social.Pages.Follows
{
    public class UserFollowHomeModel : PageModel
    {
        private readonly MeuoweDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserFollowHomeModel(MeuoweDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult OnGetFollowHome(string userView, string userFollow)
        {
            ViewData["userView"] = userView;
            ViewData["userFollow"] = userFollow;
            return Page();
        }

        public async Task<IActionResult> OnGetFollowsViewComponent(string userView, string userFollow)
        {
            return ViewComponent("DisplayedFollows", new { userView = userView, userFollow = userFollow });
        }

        public async Task<JsonResult> OnGetUserFollowAsync(string userName)
        {
            var returnString = "failure";
            string userId = _userManager.GetUserId(User);

            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(userName))
            {
                ApplicationUser applicationUser = await (from user in _context.ApplicationUsers
                                                         where user.UserName.Equals(userName)
                                                         select user).FirstOrDefaultAsync();

                UserFollowDBO userFollow = await _context.UserFollows.
                                                            Where(x => x.UserParentId.Equals(userId)
                                                                && x.UserChildId.Equals(applicationUser.Id)).
                                                            FirstOrDefaultAsync();
                if (null == userFollow)
                {
                    userFollow = new UserFollowDBO();
                    userFollow.UserParentId = userId;
                    userFollow.UserChildId = applicationUser.Id;
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
    }
}
