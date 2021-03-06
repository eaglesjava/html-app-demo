﻿using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Microsoft.AspNet.Identity.Owin;
using WebApi.Models;

namespace WebApi.Controllers {

    [Authorize]
    [RoutePrefix("api/account")]
    public class AccountController : ApiController {

        private readonly ApplicationUserManager userManager;
        private readonly ApplicationSignInManager signInManager;

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager) {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }


        [Route("")]
        public async Task<IHttpActionResult> GetUser() {
            var user = await userManager.FindByNameAsync(User.Identity.Name);
            IHttpActionResult result = Ok(user);
            return result;
        }

        [AllowAnonymous]
        [Route("")]
        public async Task<IHttpActionResult> PostRegister([FromBody] RegisterModel model) {
            if (!ModelState.IsValid) {
                return new InvalidModelStateResult(ModelState, this);
            }

            var user = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.Email
            };
            var identityResult = await userManager.CreateAsync(user);

            IHttpActionResult result;

            if (identityResult.Succeeded) {
                result = Ok();
            }
            else {
                var messageBuilder = new StringBuilder();
                foreach (var error in identityResult.Errors) {
                    messageBuilder.AppendLine(error);
                }
                result = BadRequest(messageBuilder.ToString());
            }
            return result;
        }

        [AllowAnonymous]
        [Route("login")]
        public async Task<IHttpActionResult> PostLogin([FromBody]LoginModel model) {
            if (!ModelState.IsValid) {
                return new InvalidModelStateResult(ModelState, this);
            }

            var signInStatus = await signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                shouldLockout: false
            );

            IHttpActionResult result;

            switch (signInStatus) {
            case SignInStatus.Success:
                var user = await userManager.FindByEmailAsync(model.Email);
                result = Ok(user);
                break;
            case SignInStatus.LockedOut:
                result = ResponseMessage(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "User is locked out.",
                    Content = new StringContent("User is locked out.")
                });
                break;
            case SignInStatus.RequiresVerification:
                result = ResponseMessage(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "User is not verified.",
                    Content = new StringContent("User is not verified.")
                });
                break;
            //case SignInStatus.Failure:
            default:
                result = BadRequest("login failed, try again.");
                break;
            }
            return result;
        }

    }
}