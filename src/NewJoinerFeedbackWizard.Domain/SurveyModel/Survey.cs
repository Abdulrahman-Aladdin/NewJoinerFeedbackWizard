using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace NewJoinerFeedbackWizard.SurveyModel
{
    public class Survey : AuditedAggregateRoot<Guid>
    {
        public string EmployeeName { get; set; }
        public DateTime JoiningDate { get; set; }
        public string LeadName { get; set; }
        public string ManagerName { get; set; }
        public int SatisfactionLevel { get; set; }
        public string Recommendations { get; set; }
        public string StrengthsObserved { get; set; }
        public string MissingAreas { get; set; }
    }
}
