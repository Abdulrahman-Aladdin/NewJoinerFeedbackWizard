using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;

namespace NewJoinerFeedbackWizard.Permissions
{
    public class SurveyPermissionsDefinitionProvider : PermissionDefinitionProvider
    {
        public override void Define(IPermissionDefinitionContext context)
        {
            var surveyGroup = context.AddGroup(SurveyPermissions.GroupName);
            surveyGroup.AddPermission(SurveyPermissions.Submit);
            surveyGroup.AddPermission(SurveyPermissions.Delete);
            surveyGroup.AddPermission(SurveyPermissions.View);
            surveyGroup.AddPermission(SurveyPermissions.ViewAll);
        }
    }
}
