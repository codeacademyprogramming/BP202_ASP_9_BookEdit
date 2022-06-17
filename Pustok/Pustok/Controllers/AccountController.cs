using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pustok.Models;
using Pustok.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pustok.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager,SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public IActionResult Index()
        {
            AccountIndexViewModel vm = new AccountIndexViewModel
            {
                LoginVM = new MemberLoginViewModel(),
                RegisterVM = new MemberRegisterViewModel()
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Register(MemberRegisterViewModel memberVM)
        {

            if (!ModelState.IsValid)
                return View("index",new AccountIndexViewModel {RegisterVM = memberVM });

            AppUser member = new AppUser
            {
                FullName = memberVM.FullName,
                Email = memberVM.Email,
                UserName = memberVM.RegisterUserName,
                IsAdmin = false
            };

            var result = await _userManager.CreateAsync(member, memberVM.RegisterPassword);

            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Code);
                }
                return View("index");
            }

            await _userManager.AddToRoleAsync(member, "Member");

            return RedirectToAction("index");
        }

        [HttpPost]
        public async Task<IActionResult> Login(MemberLoginViewModel memberVM)
        {
            if (!ModelState.IsValid)
                return View("index", new AccountIndexViewModel { LoginVM = memberVM ,RegisterVM = new MemberRegisterViewModel()});

            AppUser member = await _userManager.Users.FirstOrDefaultAsync(x => !x.IsAdmin && x.UserName == memberVM.LoginUserName);

            if (member == null)
            {
                ModelState.AddModelError("", "UserName or Password is incorrect");
                return View("index", new AccountIndexViewModel { LoginVM = memberVM, RegisterVM = new MemberRegisterViewModel() });
            }

            var result = await _signInManager.PasswordSignInAsync(member, memberVM.LoginPassword,false,false);

            if(!result.Succeeded)
            {
                ModelState.AddModelError("", "UserName or Password is incorrect");
                return View("index", new AccountIndexViewModel { LoginVM = memberVM, RegisterVM = new MemberRegisterViewModel() });
            }

            return RedirectToAction("index", "home");
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }
    }
}
