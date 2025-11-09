using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using NewJoinerFeedbackWizard.Dtos.User;
using NewJoinerFeedbackWizard.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace NewJoinerFeedbackWizard.Services
{
    public class UserAppService : ApplicationService, IUserAppService
    {
        private readonly IIdentityUserRepository _userRepository;
        private readonly IdentityUserManager _userManager;

        public UserAppService(
            IIdentityUserRepository userRepository,
            IdentityUserManager userManager)
        {
            _userRepository = userRepository;
            _userManager = userManager;
        }

        [Authorize]
        public async Task<List<UserDto>> GetAllLeads()
        {
            // Get users with "Lead" role
            var users = await _userRepository.GetListAsync();
            var leads = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Lead"))
                {
                    leads.Add(new UserDto
                    {
                        Name = $"{user.Name} {user.Surname}",
                        Roles = roles.ToArray()
                    });
                }
            }

            return leads;
        }

        [Authorize]
        public async Task<List<UserDto>> GetAllManagers()
        {
            // Get users with "Manager" role
            var users = await _userRepository.GetListAsync();
            var managers = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Manager"))
                {
                    managers.Add(new UserDto
                    {
                        Name = $"{user.Name} {user.Surname}",
                        Roles = roles.ToArray()
                    });
                }
            }

            return managers;
        }
    

        public Task<UserDto> GetCurrentUserAsync()
        {
            var user = new UserDto
            {
                Name = $"{CurrentUser.Name ?? "Default"} {CurrentUser.SurName ?? "User"}",
                Roles = CurrentUser.Roles
            };
            return Task.FromResult(user);
        }
    }
}
