using Microsoft.Extensions.Configuration;
using NewJoinerFeedbackWizard.Localization;
using NewJoinerFeedbackWizard.MultiTenancy;
using NewJoinerFeedbackWizard.Permissions;
using System;
using System.Threading.Tasks;
using Volo.Abp.Account.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Identity.Blazor;
using Volo.Abp.SettingManagement.Blazor.Menus;
using Volo.Abp.TenantManagement.Blazor.Navigation;
using Volo.Abp.UI.Navigation;

namespace NewJoinerFeedbackWizard.Blazor.Client.Menus;

public class NewJoinerFeedbackWizardMenuContributor : IMenuContributor
{
    private readonly IConfiguration _configuration;

    public NewJoinerFeedbackWizardMenuContributor(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
        else if (context.Menu.Name == StandardMenus.User)
        {
            await ConfigureUserMenuAsync(context);
        }
    }

    private  Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<NewJoinerFeedbackWizardResource>();

        context.Menu.Items.Insert(
            0,
            new ApplicationMenuItem(
                NewJoinerFeedbackWizardMenus.Home,
                l["Menu:Home"],
                "/",
                icon: "fas fa-home"
            )
        );

        var surveyMenu = new ApplicationMenuItem(
            NewJoinerFeedbackWizardMenus.Surveys,
            l["Menu:Surveys"] ?? "Surveys",
            icon: "fas fa-chart-bar"
        );

        surveyMenu.AddItem(
            new ApplicationMenuItem(
                NewJoinerFeedbackWizardMenus.NewSurvey,
                l["Menu:NewSurvey"] ?? "New Survey",
                "/surveys/new",
                icon: "fas fa-plus-circle"
            ).RequirePermissions(SurveyPermissions.Submit)
        );
        surveyMenu.AddItem(
            new ApplicationMenuItem(
                NewJoinerFeedbackWizardMenus.MySurveys,
                l["Menu:MySurveys"] ?? "My Surveys",
                "/surveys/my-surveys",
                icon: "fas fa-folder-open"
            ).RequirePermissions(SurveyPermissions.Submit)
        );
        surveyMenu.AddItem(
            new ApplicationMenuItem(
                NewJoinerFeedbackWizardMenus.MySurveys,
                l["Menu:ManagerDashboard"] ?? "Manager Dashboard",
                "/surveys/manager-dashboard",
                icon: "fas fa-folder-open"
            ).RequirePermissions(SurveyPermissions.ViewAll)
        );

        context.Menu.AddItem(surveyMenu);

        var administration = context.Menu.GetAdministration();

        if (MultiTenancyConsts.IsEnabled)
        {
            administration.SetSubItemOrder(TenantManagementMenuNames.GroupName, 1);
        }
        else
        {
            administration.TryRemoveMenuItem(TenantManagementMenuNames.GroupName);
        }

        administration.SetSubItemOrder(IdentityMenuNames.GroupName, 2);
        administration.SetSubItemOrder(SettingManagementMenus.GroupName, 3);

        return Task.CompletedTask;
    }

    private Task ConfigureUserMenuAsync(MenuConfigurationContext context)
    {
        var accountStringLocalizer = context.GetLocalizer<AccountResource>();

        var authServerUrl = _configuration["AuthServer:Authority"] ?? "";

        context.Menu.AddItem(new ApplicationMenuItem(
            "Account.Manage",
            accountStringLocalizer["MyAccount"],
            $"{authServerUrl.EnsureEndsWith('/')}Account/Manage",
            icon: "fa fa-cog",
            order: 1000,
            target: "_blank").RequireAuthenticated());

        return Task.CompletedTask;
    }
}
