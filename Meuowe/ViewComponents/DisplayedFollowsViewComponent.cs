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
using Microsoft.EntityFrameworkCore;

namespace Meuowe.ViewComponents
{
    public class DisplayedFollowsViewComponent : ViewComponent
    {
        private readonly MeuoweDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DisplayedFollowsViewComponent(MeuoweDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string userView, string userFollow)
        {
            List<ApplicationUser> profiles = await GetItemsAsync(userView, userFollow);
            return View(profiles);
        }

        private async Task<List<ApplicationUser>> GetItemsAsync(string userView, string userFollow)
        {
            string userId;

            string currentUser = _userManager.GetUserId(HttpContext.User);


            // Get profile from passed in user view value that represents a user name.
            if (!String.IsNullOrEmpty(userView))
            {
                userId = await (from user in _context.ApplicationUsers
                                where user.UserName.Equals(userView)
                                select user.Id).FirstOrDefaultAsync();
            }
            else
            {
                userId = currentUser;
            }

            List<ApplicationUser> applicationUsers = new List<ApplicationUser>();
            switch (userFollow)
            {
                case "following":
                    applicationUsers = await (from user in _context.ApplicationUsers
                                             join follow in _context.UserFollows
                                             on user.Id equals follow.UserChildId
                                                where follow.UserParentId.Equals(userId)
                                                select user).Distinct().ToListAsync();
                    break;
                case "followers":
                    applicationUsers = await (from user in _context.ApplicationUsers
                                              join follow in _context.UserFollows
                                              on user.Id equals follow.UserParentId
                                              where follow.UserChildId.Equals(userId)
                                              select user).Distinct().ToListAsync();

                    break;
                default:
                    applicationUsers = await (from user in _context.ApplicationUsers
                                              join follow in _context.UserFollows
                                              on user.Id equals follow.UserChildId
                                              where follow.UserParentId.Equals(userId)
                                              select user).Distinct().ToListAsync();
                    break;
            }



            return applicationUsers;
        }
    }
}