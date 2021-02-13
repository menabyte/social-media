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
    public class DisplayedRepliesPawsViewComponent : ViewComponent
    {
        private readonly MeuoweDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DisplayedRepliesPawsViewComponent(MeuoweDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string pawView, int pageIndex, int pawId, int pageSize)
        {
            List<UserPawDBO> userPaws = await GetItemsAsync(pawView, pageIndex, pawId, pageSize);
            return View(userPaws);
        }

        private async Task<List<UserPawDBO>> GetItemsAsync(string pawView, int pageIndex, int pawId, int pageSize)
        {
            string userId = _userManager.GetUserId(HttpContext.User);

            List<UserPawDBO> userPaws = new List<UserPawDBO>();
            int userPawsCount;
            int totalPages;

            switch (pawView)
            {
                case "replies":
                    ViewData["currentView"] = pawView;
                    userPawsCount = await _context.UserPaws.Where(x => x.ParentPawId.Equals(pawId)).CountAsync();
                    totalPages = (userPawsCount / pageSize) + 1;
                    if (pageIndex < totalPages)
                    {
                        userPaws = await _context.UserPaws.Where(x => x.ParentPawId.Equals(pawId)).ToListAsync();
                    }
                    break;

                default:
                    ViewData["currentView"] = pawView;
                    userPawsCount = await _context.UserPaws.Where(x => x.ParentPawId.Equals(pawId)).CountAsync();
                    totalPages = (userPawsCount / pageSize) + 1;
                    if (pageIndex < totalPages)
                    {
                        userPaws = await _context.UserPaws.Where(x => x.ParentPawId.Equals(pawId)).ToListAsync();
                    }
                    break;
            }
            ViewData["pageIndex"] = pageIndex;
            return userPaws.ToList();
        }
    }
}
