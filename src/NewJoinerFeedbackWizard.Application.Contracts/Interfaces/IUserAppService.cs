using NewJoinerFeedbackWizard.Dtos.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace NewJoinerFeedbackWizard.Interfaces
{
    public interface IUserAppService : IApplicationService
    {
        Task<UserDto> GetCurrentUserAsync();
        Task<List<UserDto>> GetAllManagers();
        Task<List<UserDto>> GetAllLeads();
    }
}
