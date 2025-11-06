using Microsoft.EntityFrameworkCore;
using NewJoinerFeedbackWizard.EntityFrameworkCore;
using NewJoinerFeedbackWizard.SurveyModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace NewJoinerFeedbackWizard.Repositories
{
    public class SurveyRepository(IDbContextProvider<NewJoinerFeedbackWizardDbContext> dbContextProvider) : EfCoreRepository<NewJoinerFeedbackWizardDbContext, Survey, Guid>(dbContextProvider), ISurveyRepository
    {
        public async Task<Survey?> GetByEmployeeNameAsync(string employeeName)
        {
            var queryable = await GetQueryableAsync();
            return await queryable.FirstOrDefaultAsync(s => s.EmployeeName == employeeName);
        }
    }
}
