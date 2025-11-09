using NewJoinerFeedbackWizard.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace NewJoinerFeedbackWizard.Permissions;

public class NewJoinerFeedbackWizardPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(NewJoinerFeedbackWizardPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(NewJoinerFeedbackWizardPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<NewJoinerFeedbackWizardResource>(name);
    }
}