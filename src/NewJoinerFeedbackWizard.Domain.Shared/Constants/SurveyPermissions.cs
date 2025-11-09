using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewJoinerFeedbackWizard.Permissions
{
    public static class SurveyPermissions
    {
        public const string AppName = "NewJoinerFeedbackWizard";
        public const string GroupName = $"{AppName}.Survey";
        public const string Submit = $"{GroupName}.Submit";
        public const string Delete = $"{GroupName}.Delete";
        public const string View = $"{GroupName}.View";
        public const string ViewAll = $"{GroupName}.ViewAll";
    }
}
