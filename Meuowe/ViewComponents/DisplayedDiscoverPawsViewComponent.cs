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
    public class DisplayedDiscoverPawsViewComponent : ViewComponent
    {
        private readonly MeuoweDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DisplayedDiscoverPawsViewComponent(MeuoweDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string discover, int pageIndex, int pageSize)
        {
            List<UserPawDBO> userPaws = await GetItemsAsync(discover, pageIndex, pageSize);
            return View(userPaws);
        }

        private async Task<List<UserPawDBO>> GetItemsAsync(string discover, int pageIndex, int pageSize)
        {
            string userId = _userManager.GetUserId(HttpContext.User);

            List<UserPawDBO> userPaws = new List<UserPawDBO>();
            int userPawsCount;
            int totalPages;

            switch (discover)
            {                
                case "forYou":
                    ViewData["currentView"] = discover;
                    userPawsCount = await (from c in _context.UserPaws
                                                  //where c.Message.ToLower().Contains("medicine")
                                                  select c).CountAsync();

                    totalPages = (userPawsCount / pageSize) + 1;


                    if (pageIndex < totalPages)
                    {
                        userPaws = await (from c in _context.UserPaws
                                          // where c.Message.ToLower().Contains("medicine")
                                          orderby c.CreatedDate descending
                                          select c)
                                             .Skip(pageIndex * pageSize)
                                             .Take(pageSize).ToListAsync();
                    }
                    break;

                case "news":
                    ViewData["currentView"] = discover;
                    userPawsCount = await (from c in _context.UserPaws
                                           where c.Message.ToLower().Contains("trump") ||
                                           c.Message.ToLower().Contains("bernie") ||
                                           c.Message.ToLower().Contains("hillary")
                                           orderby c.CreatedDate descending
                                           select c).CountAsync();

                    totalPages = (userPawsCount / pageSize) + 1;

                    if (pageIndex < totalPages)
                    {
                        userPaws = await (from c in _context.UserPaws
                                          where c.Message.ToLower().Contains("trump") ||
                                           c.Message.ToLower().Contains("bernie") ||
                                           c.Message.ToLower().Contains("hillary")
                                          orderby c.CreatedDate descending
                                          select c)
                                             .Skip(pageIndex * pageSize)
                                             .Take(pageSize).ToListAsync();
                    }
                    break;

                case "sports":
                    ViewData["currentView"] = discover;
                    userPawsCount = await (from c in _context.UserPaws
                                           where c.Message.ToLower().Contains("kobe") ||
                                           c.Message.ToLower().Contains("lakers") ||
                                           c.Message.ToLower().Contains("nba") ||
                                           c.Message.ToLower().Contains("lebron")
                                           orderby c.CreatedDate descending
                                           select c).CountAsync();

                    totalPages = (userPawsCount / pageSize) + 1;

                    if (pageIndex < totalPages)
                    {
                        userPaws = await (from c in _context.UserPaws
                                          where c.Message.ToLower().Contains("kobe") ||
                                           c.Message.ToLower().Contains("lakers") ||
                                           c.Message.ToLower().Contains("nba") ||
                                           c.Message.ToLower().Contains("lebron")
                                          orderby c.CreatedDate descending
                                          select c)
                                             .Skip(pageIndex * pageSize)
                                             .Take(pageSize).ToListAsync();
                    }
                    break;

                case "fun":
                    ViewData["currentView"] = discover;
                    userPawsCount = await (from c in _context.UserPaws
                                                 where c.Message.ToLower().Contains("happy") ||
                                                 c.Message.ToLower().Contains("tv") ||
                                                 c.Message.ToLower().Contains("radio")
                                           orderby c.CreatedDate descending
                                                 select c).CountAsync();

                    totalPages = (userPawsCount / pageSize) + 1;

                    if (pageIndex < totalPages)
                    {
                        userPaws = await (from c in _context.UserPaws
                                          where c.Message.ToLower().Contains("happy") ||
                                                c.Message.ToLower().Contains("tv") ||
                                                c.Message.ToLower().Contains("radio")
                                          orderby c.CreatedDate descending
                                          select c)
                                             .Skip(pageIndex * pageSize)
                                             .Take(pageSize).ToListAsync();
                    }
                    break;

                case "music":
                    ViewData["currentView"] = discover;
                    userPawsCount = await (from c in _context.UserPaws
                                           where c.Message.ToLower().Contains("drake") ||
                                                c.Message.ToLower().Contains("rap") ||
                                                c.Message.ToLower().Contains("uzi")
                                           orderby c.CreatedDate descending
                                           select c).CountAsync();

                    totalPages = (userPawsCount / pageSize) + 1;

                    if (pageIndex < totalPages)
                    {
                        userPaws = await (from c in _context.UserPaws
                                          where c.Message.ToLower().Contains("drake") ||
                                               c.Message.ToLower().Contains("rap") ||
                                               c.Message.ToLower().Contains("uzi")
                                          orderby c.CreatedDate descending
                                          select c)
                                             .Skip(pageIndex * pageSize)
                                             .Take(pageSize).ToListAsync();
                    }
                    break;

                default:
                    ViewData["currentView"] = discover;
                    userPawsCount = await (from c in _context.UserPaws
                                           //where c.Message.ToLower().Contains("medicine")
                                           orderby c.CreatedDate descending
                                           select c).CountAsync();

                    totalPages = (userPawsCount / pageSize) + 1;

                    if (pageIndex < totalPages)
                    {
                        userPaws = await (from c in _context.UserPaws
                                          //where c.Message.ToLower().Contains("medicine")
                                          orderby c.CreatedDate descending
                                          select c)
                         .Skip(pageIndex * pageSize)
                         .Take(pageSize).ToListAsync();
                    }
                    break;
            }

            ViewData["pageIndex"] = pageIndex;
            return userPaws.ToList();
        }
    }
}
