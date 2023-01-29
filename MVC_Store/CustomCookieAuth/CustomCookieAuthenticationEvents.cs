using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using MVC_Store.Models.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Principal;

namespace MVC_Store.CustomCookieAuth
{
    public class CustomCookieAuthenticationEvents : CookieAuthenticationEvents
    {
        private readonly Db _dbContext;

        public CustomCookieAuthenticationEvents(Db dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            if (context.HttpContext.User == null)
                return;

            var userPrincipal = context.Principal.Identity;

            string userName = context.Principal.Identity.Name;

            string[] roles = null;

            UserDTO dto = _dbContext.Users.FirstOrDefault(x => x.Username == userName);

            if (dto == null)
                return;

            roles = _dbContext.UserRoles.Where(x => x.UserId == dto.Id).Select(x => x.Role.Name).ToArray();

            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName),
                new Claim(type: "RememberMe", value: context.Principal.Claims.FirstOrDefault(x => x.Type == "RememberMe").Value)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, role));
            }

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");

            var newUserObj = new ClaimsPrincipal(claimsIdentity);

            await context.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, newUserObj);
        }
    }
}
