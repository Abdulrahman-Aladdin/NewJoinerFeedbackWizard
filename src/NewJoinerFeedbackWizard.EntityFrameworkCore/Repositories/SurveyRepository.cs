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

        public async Task<List<Survey>> GetByManagerNameAsync(string managerName)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Where(s => s.ManagerName == managerName)
                .OrderByDescending(s => s.CreationTime)
                .ToListAsync();
        }

        public async Task<List<Survey>> GetBySubmittedByAsync(string submittedBy)
        {
            var dbSet = await GetDbSetAsync();

            // Expect submittedBy to be the user's GUID string (CurrentUser.Id.ToString()).
            if (Guid.TryParse(submittedBy, out var userId))
            {
                return await dbSet
                    .Where(s => s.CreatorId == userId)
                    .OrderByDescending(s => s.CreationTime)
                    .ToListAsync();
            }

            // If not a GUID, return empty list (avoid returning all records).
            return new List<Survey>();
        }
    }
}
