using NewJoinerFeedbackWizard.Dtos.User;
using NewJoinerFeedbackWizard.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace NewJoinerFeedbackWizard.Services
{
    public class UserAppService : ApplicationService, IUserAppService
    {
        public Task<List<UserDto>> GetAllLeads()
        {
            throw new NotImplementedException();
        }

        public Task<List<UserDto>> GetAllManagers()
        {
            throw new NotImplementedException();
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
