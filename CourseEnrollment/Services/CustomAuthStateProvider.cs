using CourseEnrollment.Models;
using CourseEnrollment.ViewModels;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CourseEnrollment.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(_currentUser));
        }

        public void MarkUserAsAuthenticated(StudentDTO user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

             var claims = new List<Claim>
             {
                 new(ClaimTypes.Name, user.Name ?? string.Empty),
                 new(ClaimTypes.Surname, user.Surname ?? string.Empty),
                 new(ClaimTypes.Email, user.Email ?? string.Empty),
                 new(ClaimTypes.NameIdentifier, user.Id.ToString() ?? string.Empty)
             };

            /* // ✅ Add multiple roles
             if (user.UserRoles != null)
             {
                 foreach (var role in user.UserRoles)
                 {
                     if (!string.IsNullOrWhiteSpace(role))
                         claims.Add(new Claim(ClaimTypes.Role, role));
                 }
             }*/

            var identity = new ClaimsIdentity(claims, "CustomAuth");
            _currentUser = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
        }

        public void MarkUserAsLoggedOut()
        {
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity()); // Not authenticated
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
        }
    }
}
