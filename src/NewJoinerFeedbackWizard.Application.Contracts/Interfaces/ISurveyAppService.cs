using NewJoinerFeedbackWizard.Dtos.Survey;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace NewJoinerFeedbackWizard.Interfaces
{
    public interface ISurveyAppService : IApplicationService
    {
        Task CreateSurvey(CreateSurveyDto input);
        Task<SurveyDto?> GetSurveyByEmployeeName(string employeeName);
        Task<List<SurveyDto>> GetAllSurveys();
        Task Delete(Guid id);

        // for manager
        Task<List<SurveyDto>> GetSurveysByManager(string managerName);
        Task<List<SurveyDto>> GetMySubmittedSurveys();
    }
}
