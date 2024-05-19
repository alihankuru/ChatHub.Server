﻿using ChatHub.EntityLayer.Concrete;
using ChatHub.Server.Dtos;
using GenericFileService.Files;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignInResult=Microsoft.AspNetCore.Identity.SignInResult;
namespace ChatHub.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Register([FromForm] RegisterDto request, CancellationToken cancellationToken)
        {
            string avatar = FileService.FileSaveToServer(request.File, "Images/Avatar/");
            AppUser appUser = new()
            {
                Email = request.Email,
                UserName = request.UserName,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Photo=avatar,


            };

            IdentityResult result= await userManager.CreateAsync(appUser,request.Password);

            if(!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(appUser);
        }


        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto request, CancellationToken cancellationToken)
        {
            AppUser? appUser = await userManager.FindByIdAsync(request.Id.ToString());

            if(appUser is null)
            {
                return BadRequest(new {Message="Kullanıcı bulunamadı"});
            }

            IdentityResult result = await userManager.ChangePasswordAsync(appUser, request.CurrentPassword, request.NewPassword);
            
            if(! result.Succeeded)
            {
                return BadRequest(result.Errors.Select(s=>s.Description));
            }

            return NoContent();

        }


        [HttpGet]
        public async Task<IActionResult> ForgotPassword(string email, CancellationToken cancellationToken)
        {
            AppUser? appUser=await userManager.FindByEmailAsync(email);

            if(appUser is null)
            {
                return BadRequest(new { Message = "Kullanıcı Bulunamadı" });
            }

            string token = await userManager.GeneratePasswordResetTokenAsync(appUser);

            return Ok(new {Token=token});
        }


        [HttpPost]
        public async Task<IActionResult> ChangePasswordUsingToken(ChangePasswordUsingTokenDto request, CancellationToken cancellationToken)
        {
            AppUser? appUser = await userManager.FindByEmailAsync(request.Email);

            if (appUser is null)
            {
                return BadRequest(new { Message = "Kullanıcı Bulunamadı" });
            }

            IdentityResult result = await userManager.ResetPasswordAsync(appUser,request.Token,request.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(s=>s.Description));
            }
            return NoContent();


        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto request, CancellationToken cancellationToken)
        {
            AppUser? appUser = await userManager.Users.FirstOrDefaultAsync(p => p.Email == request.UserNameOrEmail || p.UserName == request.UserNameOrEmail, cancellationToken);


            if (appUser is null)
            {
                return BadRequest(new { Message = "Kullanıcı Bulunamadı" });
            }
            appUser.Status = "Çevrimiçi";
            bool result=await userManager.CheckPasswordAsync(appUser, request.Password);
            if (!result) return BadRequest(new { Message = "Şifre Yanlış " });



            return Ok(appUser);
        }

        [HttpPost]
        public async Task<IActionResult> LoginWithSignInManager(LoginDto request, CancellationToken cancellationToken)
        {
            AppUser? appUser = await userManager.Users.FirstOrDefaultAsync(p => p.Email == request.UserNameOrEmail || p.UserName == request.UserNameOrEmail, cancellationToken);


            if (appUser is null)
            {
                return BadRequest(new { Message = "Kullanıcı Bulunamadı" });
            }

            SignInResult result=  await signInManager.CheckPasswordSignInAsync(appUser,request.Password,true);


            if (result.IsLockedOut)
            {
                TimeSpan? timeSpan = appUser.LockoutEnd - DateTime.UtcNow;

                if( timeSpan is not null )
                {
                    return StatusCode(500, $"Şifrenizi 3 kere yanlış girdiğiniz için kullanıcınız {timeSpan.Value.TotalSeconds} saniye girişe yasaklanmıştır.");
                }
                else
                {
                    return StatusCode(500, $"Şifrenizi 3 kere yanlış girdiğiniz için kullanıcınız 30 saniye girişe yasaklanmıştır.");

                }
            }

            if (!result.Succeeded)
            {
                return StatusCode(500, "Şifreniz Yanlış");
            }


            if (result.IsNotAllowed)
            {
                return StatusCode(500, "Mail adresiniz onaylı değil");
            }

           



            return Ok(new { Token = "Token" });
        }


    }
}
