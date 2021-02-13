using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Meuowe.Areas.Database.Models;
using Meuowe.Areas.Database.Models.DatabaseObjects;
using Meuowe.Areas.Database.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Meuowe.ViewComponents
{
    public class DisplayedPawsViewComponent : ViewComponent
    {
        private readonly MeuoweDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DisplayedPawsViewComponent(MeuoweDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        public async Task<IViewComponentResult> InvokeAsync(string home, int pageIndex, int pageSize, string userView)
        {
            List<UserPawDBO> userPaws = await GetItemsAsync(home, pageIndex, pageSize, userView);
            return View(userPaws);
        }


        private async Task<List<UserPawDBO>> GetItemsAsync(string home, int pageIndex, int pageSize, string userView)
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

            

            List<UserPawDBO> userPaws = new List<UserPawDBO>();
            int userPawsCount;
            int totalPages;

            switch (home)
            {
                case "home":
                    ViewData["currentView"] = home;
                    userPawsCount = await (from paw in _context.UserPaws
                                           where paw.UserId.Equals(userId)
                                           select paw).Distinct().CountAsync();

                    totalPages = (userPawsCount / pageSize) + 1;

                    if (pageIndex < totalPages)
                    {
                        userPaws = await (from paw in _context.UserPaws
                                          where paw.UserId.Equals(userId)
                                          orderby paw.CreatedDate descending
                                          select paw)
                                          .Skip(pageIndex * pageSize)
                         .Take(pageSize).Distinct().ToListAsync();
                    }
                    break;

                case "shake":
                    ViewData["currentView"] = home;
                    userPawsCount = await (from paw in _context.UserPaws
                                           join shake in _context.UserShakes
                                           on paw.Id equals shake.PawId
                                           where shake.UserId.Equals(userId)
                                           select paw).Distinct().CountAsync();

                    totalPages = (userPawsCount / pageSize) + 1;

                    if (pageIndex < totalPages)
                    {
                        userPaws = await (from paw in _context.UserPaws
                                          join shake in _context.UserShakes
                                          on paw.Id equals shake.PawId
                                          where shake.UserId.Equals(userId)
                                          orderby paw.CreatedDate descending
                                          select paw)
                                          .Skip(pageIndex * pageSize)
                         .Take(pageSize).Distinct().ToListAsync();
                    }
                    break;

                case "reply":
                    ViewData["currentView"] = home;
                    userPawsCount = await (from paw in _context.UserPaws
                                           where paw.UserId.Equals(userId) && paw.ParentPawId > 0
                                           orderby paw.CreatedDate descending
                                           select paw).CountAsync();

                    totalPages = (userPawsCount / pageSize) + 1;

                    if (pageIndex < totalPages)
                    {
                        userPaws = await (from paw in _context.UserPaws
                                          where paw.UserId.Equals(userId) && paw.ParentPawId > 0
                                          orderby paw.CreatedDate descending
                                          select paw)
                         .Skip(pageIndex * pageSize)
                         .Take(pageSize).ToListAsync();
                    }
                    break;

                case "wag":
                    ViewData["currentView"] = home;
                    userPawsCount = await (from paw in _context.UserPaws
                                           join wag in _context.UserWags
                                           on paw.Id equals wag.PawId
                                           where wag.UserId.Equals(userId)
                                           select paw).Distinct().CountAsync();

                    totalPages = (userPawsCount / pageSize) + 1;

                    if (pageIndex < totalPages)
                    {
                        userPaws = await (from paw in _context.UserPaws
                                          join wag in _context.UserWags
                                          on paw.Id equals wag.PawId
                                          where wag.UserId.Equals(userId)
                                          orderby paw.CreatedDate descending
                                          select paw)
                                          .Skip(pageIndex * pageSize)
                         .Take(pageSize).Distinct().ToListAsync();
                    }
                    break;

                case "tree":
                    ViewData["currentView"] = home;
                    userPawsCount = await (from paw in _context.UserPaws
                                           join follow in _context.UserFollows
                                           on paw.UserId equals follow.UserChildId
                                           where follow.UserParentId.Equals(userId)
                                           select paw).Distinct().CountAsync();

                    totalPages = (userPawsCount / pageSize) + 1;

                    if (pageIndex < totalPages)
                    {
                        userPaws = await (from paw in _context.UserPaws
                                          join follow in _context.UserFollows
                                          on paw.UserId equals follow.UserChildId
                                          where follow.UserParentId.Equals(userId)
                                          orderby paw.CreatedDate descending
                                          select paw)
                        .Skip(pageIndex * pageSize)
                         .Take(pageSize).Distinct().ToListAsync();
                    }
                    break;

                case "trail":
                    ViewData["currentView"] = home;
                    userPawsCount = await (from paw in _context.UserPaws
                                           where paw.ParentPawId.Equals(0)
                                           orderby paw.CreatedDate descending
                                           select paw).CountAsync();

                    totalPages = (userPawsCount / pageSize) + 1;

                    if (pageIndex < totalPages)
                    {
                        userPaws = await (from paw in _context.UserPaws
                                          where paw.ParentPawId.Equals(0)
                                          orderby paw.CreatedDate descending
                                          select paw)
                                             .Skip(pageIndex * pageSize)
                                             .Take(pageSize).ToListAsync();
                    }
                    break;

                default:
                    ViewData["currentView"] = home;
                    userPawsCount = await (from paw in _context.UserPaws
                                           where paw.UserId.Equals(userId)
                                           select paw).Distinct().CountAsync();

                    totalPages = (userPawsCount / pageSize) + 1;

                    if (pageIndex < totalPages)
                    {
                        userPaws = await (from paw in _context.UserPaws
                                          where paw.UserId.Equals(userId)
                                          orderby paw.CreatedDate descending
                                          select paw)
                                          .Skip(pageIndex * pageSize)
                         .Take(pageSize).Distinct().ToListAsync();
                    }
                    break;
            }
            ViewData["pageIndex"] = pageIndex;
            return userPaws.ToList();
        }
    }
}
