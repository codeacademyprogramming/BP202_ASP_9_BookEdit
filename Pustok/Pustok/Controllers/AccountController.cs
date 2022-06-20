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

            string token = await _userManager.GenerateEmailConfirmationTokenAsync(member);

            var url = Url.Action("ConfirmEmail", "Account", new { email = member.Email, token = token }, Request.Scheme);

            await _userManager.AddToRoleAsync(member, "Member");

            return Ok(new { URL = url });

            return RedirectToAction("index");
        }

        public async Task<IActionResult> ConfirmEmail(string email,string token)
        {
            AppUser member = await _userManager.FindByEmailAsync(email);
            if (member == null)
                return RedirectToAction("error", "dashboard");

            var result = await _userManager.ConfirmEmailAsync(member, token);

            if (result.Succeeded)
                return RedirectToAction("index");
            else
                return RedirectToAction("error", "dashboard");
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

            if (!member.EmailConfirmed)
            {
                ModelState.AddModelError("", "Please,confirm your email account!");
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

        public IActionResult Forgot()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Forgot(MemberForgotPasswordViewModel memberVM)
        {
            if (!ModelState.IsValid) return View();

            AppUser member = await _userManager.FindByEmailAsync(memberVM.Email);
            if(member == null)
            {
                ModelState.AddModelError("Email", "Email movcud deyil!");
                return View();
            }

            string token = await _userManager.GeneratePasswordResetTokenAsync(member);

            var url = Url.Action("ResetPassword", "Account", new { email = member.Email, token = token }, Request.Scheme);

            return Ok(new { URL = url });
        }

        public async  Task<IActionResult> ResetPassword(string email,string token)
        {
            AppUser member = await _userManager.Users.FirstOrDefaultAsync(x => !x.IsAdmin && x.NormalizedEmail == email.ToUpper());
            if (member == null)
                return RedirectToAction("error","dashboard");

            if (!await _userManager.VerifyUserTokenAsync(member, _userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", token))
                return RedirectToAction("error", "dashboard");


            MemberResetPasswordViewModel vm = new MemberResetPasswordViewModel
            {
                Email = email,
                Token = token
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(MemberResetPasswordViewModel resetVM)
        {
            if (!ModelState.IsValid) return View();

            AppUser member = await _userManager.Users.FirstOrDefaultAsync(x => !x.IsAdmin && x.NormalizedEmail == resetVM.Email.ToUpper());

            if (member == null)
                return RedirectToAction("error", "dashboard");


            var result = await _userManager.ResetPasswordAsync(member, resetVM.Token, resetVM.Password);

            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }
                return View();
            }

            return RedirectToAction("index");
        }


    }
}
