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
    public class UserPawMeuoweModel : PageModel
    {
        private readonly MeuoweDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserPawMeuoweModel(MeuoweDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public void OnGetPawMeuowe(int pawId, string pawUserId)
        {
            ViewData["PawId"] = pawId;
            ViewData["PawUserId"] = pawUserId;
        }

        // Partial Views
        /****************************************************************/
        public async Task<PartialViewResult> OnGetPawMeuowePartialAsync(int pawId, string pawUserId)
        {
            string userId = _userManager.GetUserId(User);

            List<MeuoweOfferVM> meuoweOffers = new List<MeuoweOfferVM>();
            if (userId == pawUserId)
            {

                meuoweOffers = await (from meuowe in _context.UserMeuowes
                                      join user in _context.ApplicationUsers
                                        on meuowe.UserId equals user.Id
                                        select 
                                        new MeuoweOfferVM
                                        {
                                            UserMeuowe = meuowe,
                                            UserName = user.UserName
                                        } ).ToListAsync();
            }

           
            return Partial("./Shared/_PawMeuowePartial", meuoweOffers);
        }

        public async Task<IActionResult> OnPostAcceptMeuoweAsync(int? MeuoweId)
        {
            string userId = _userManager.GetUserId(User);

            if (null != MeuoweId)
            {
                UserMeuoweDBO userMeuowe = await (from meuowe in _context.UserMeuowes
                                                  where meuowe.Id.Equals(MeuoweId)
                                                  select meuowe).FirstOrDefaultAsync();

                if (null != userMeuowe)
                {
                    string meuoweUserId = await (from user in _context.ApplicationUsers
                                                          where user.Id.Equals(userMeuowe.UserId)
                                                          select user.Id
                                                          ).FirstOrDefaultAsync();

                    UserPawDBO userPaw = await (from paw in _context.UserPaws
                                                where paw.Id.Equals(userMeuowe.PawId)
                                                select paw).FirstOrDefaultAsync();

                    userPaw.UserId = meuoweUserId;
                    await _context.SaveChangesAsync();

                    _context.UserMeuowes.Remove(userMeuowe);
                    await _context.SaveChangesAsync();
                }
            }
            return new RedirectToPageResult("/Paws/UserPawHome", new { area = "Social" });
        }
    }
}
