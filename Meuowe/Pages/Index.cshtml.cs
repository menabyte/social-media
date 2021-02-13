using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Meuowe.Areas.Database.Models;
using Meuowe.Areas.Database.Models.DatabaseObjects;
using Meuowe.Areas.Database.Models.Identity;
using Meuowe.Areas.Database.Models.ViewModels;
using Meuowe.Areas.Identity.Pages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meuowe.Pages
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly MeuoweDbContext _context;
        private readonly int _pageSize = 6;


        public IndexModel(SignInManager<ApplicationUser> signInManager,
            ILogger<LoginModel> logger,
            UserManager<ApplicationUser> userManager,
            MeuoweDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;

        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            public string UserName { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string discover, string returnUrl = null)
        {
            ViewData["currentView"] = discover;
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnGetSearchDropDownViewComponent(string userName)
        {
            return ViewComponent("DisplayedSearchDropDown", new { userName = userName });
        }


        // Partial Views
        /****************************************************************/
        public async Task<IActionResult> OnGetPawViewComponent(int pawId)
        {
            UserPawDBO userPaw = await (from paw in _context.UserPaws
                                        where paw.Id.Equals(pawId)
                                        select paw
                                        ).FirstOrDefaultAsync();

            return ViewComponent("DisplayedPaw", new { userPaw = userPaw });
        }


        public IActionResult OnGetPawsViewComponent(string discover, int pageIndex)
        {
            if (String.IsNullOrEmpty(discover))
            {
                discover = "forYou";
                pageIndex = 0;
            }
            return ViewComponent("DisplayedDiscoverPaws", new { discover = discover, pageIndex = pageIndex, pageSize = _pageSize });
        }

        public async Task<PartialViewResult> OnGetReplyUserPartialAsync(int pawId, string userPawId)
        {

            string userId = _userManager.GetUserId(User);

            var userPaw = await _context.UserPaws.Where(x => x.Id.Equals(pawId)).FirstOrDefaultAsync();
            var userReplyTo = await _context.ApplicationUsers.Where(x => x.Id.Equals(userPawId)).FirstOrDefaultAsync();

            PawVM pawVM = new PawVM();
            pawVM.UserPaw = userPaw;
            pawVM.DisplayName = userReplyTo.DisplayName;
            pawVM.UserName = userReplyTo.UserName;

            return Partial("~/Areas/Social/Pages/Shared/_ModalReply.cshtml", pawVM);
        }

        public async Task<PartialViewResult> OnGetSendMeuowePartialAsync(int pawId, string userPawId)
        {
            var userPaw = await _context.UserPaws.Where(x => x.Id.Equals(pawId)).FirstOrDefaultAsync();
            var userSendMeuowe = await _context.ApplicationUsers.Where(x => x.Id.Equals(userPawId)).FirstOrDefaultAsync();

            PawVM pawVM = new PawVM();
            pawVM.UserPaw = userPaw;
            pawVM.DisplayName = userSendMeuowe.DisplayName;
            pawVM.UserName = userSendMeuowe.UserName;

            return Partial("~/Areas/Social/Pages/Shared/_SendMeuowePartial.cshtml", pawVM);
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(Input.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return RedirectToPage("/Paws/UserPawHome", new { area = "Social" });
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("/Identity/LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("/Identity/Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        // Transactions
        /****************************************************************/
        public async Task<JsonResult> OnGetUserFollowAsync(string pawId, string userChildId)
        {
            var returnMessage = "failure";
            string userId = _userManager.GetUserId(User);

            if (!string.IsNullOrEmpty(userId))
            {
                if (!string.IsNullOrEmpty(pawId) && !string.IsNullOrEmpty(userChildId))
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
                        _context.UserFollows.Remove(userFollow);
                        await _context.SaveChangesAsync();
                        returnMessage = "success";
                    }
                }
            }
            else
            {
                returnMessage = "please login";
            }
            return new JsonResult(returnMessage);
        }

        public async Task<JsonResult> OnGetShakePawAsync(string pawId)
        {
            var returnMessage = "failure";
            string userId = _userManager.GetUserId(User);

            if (!string.IsNullOrEmpty(userId))
            {
                if (!string.IsNullOrEmpty(pawId))
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
            }
            else
            {
                returnMessage = "please login";
            }
            return new JsonResult(returnMessage);
        }

        public async Task<JsonResult> OnGetWagTailAsync(string pawId)
        {
            var returnMessage = "failure";
            string userId = _userManager.GetUserId(User);

            if (!string.IsNullOrEmpty(userId))
            {
                if (!string.IsNullOrEmpty(pawId))
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
            }
            else
            {
                returnMessage = "please login";
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
            else
            {
                returnMessage = "please login";
            }

            return new JsonResult(returnMessage);
        }

        public IActionResult OnPostDelete(int pawID)
        {
            var returnMessage = "failure";
            var userId = _userManager.GetUserId(User);

            if (!string.IsNullOrEmpty(userId))
            {
                UserPawDBO userPaw = (from p in _context.UserPaws
                                      where p.Id.Equals(pawID) && p.UserId.Equals(userId)
                                      select p).FirstOrDefault();

                if (null !=  userPaw)
                {
                    _context.Remove(userPaw);
                    _context.SaveChangesAsync();
                    returnMessage = "success";
                }
            }

            return new JsonResult(returnMessage);
        }

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
