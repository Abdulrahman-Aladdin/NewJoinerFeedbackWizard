using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace NewJoinerFeedbackWizard.SurveyModel
{
    public interface ISurveyRepository : IRepository<Survey, Guid>
    {
        Task<Survey?> GetByEmployeeNameAsync(string employeeName);
        Task<List<Survey>> GetByManagerNameAsync(string managerName);
        Task<List<Survey>> GetBySubmittedByAsync(string submittedBy);

    }
}
