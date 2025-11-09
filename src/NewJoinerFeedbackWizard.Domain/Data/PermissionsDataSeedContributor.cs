using NewJoinerFeedbackWizard.Constants;
using NewJoinerFeedbackWizard.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;

namespace NewJoinerFeedbackWizard.Data
{
    public class PermissionsDataSeedContributor(IPermissionManager permissionManager,
        IdentityRoleManager roleManager, IGuidGenerator guidGenerator,
        IdentityUserManager userManager) : IDataSeedContributor, ITransientDependency
    {
        private readonly IPermissionManager _permissionManager = permissionManager;
        private readonly IdentityRoleManager _roleManager = roleManager;
        private readonly IGuidGenerator _guidGenerator = guidGenerator;
        private readonly IdentityUserManager _userManager = userManager;

        public async Task SeedAsync(DataSeedContext context)
        {
            await SeedRoles();
            await SeedPermissions();
            await SeedUsers();
        }

        private async Task SeedUsers()
        {
            await SeedUserIfNotExist(UserRoles.Admin, "Hamada", "Elmohammady");
            await SeedUserIfNotExist(UserRoles.Manager, "Abdelrahman", "Ahmed");
            await SeedUserIfNotExist(UserRoles.Employee, "Osama", "Belal");
        }

        private async Task SeedUserIfNotExist(string role, string name, string surname)
        {
            var username = role.ToLower();
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                user = new IdentityUser(
                    _guidGenerator.Create(),
                    username,
                    $"{username}@ejada.com"
                );

                user.Name = name;
                user.Surname = surname;

                await _userManager.CreateAsync(user, $"A{username}#123");
                await _userManager.AddToRoleAsync(user, role);
            }

        }

        private async Task SeedPermissions()
        {
            var adminRole = await _roleManager.FindByNameAsync(UserRoles.Admin);
            await _permissionManager.SetForRoleAsync(adminRole.Name, SurveyPermissions.Delete, true);
            await _permissionManager.SetForRoleAsync(adminRole.Name, SurveyPermissions.ViewAll, true);

            var employeeRole = await _roleManager.FindByNameAsync(UserRoles.Employee);
            await _permissionManager.SetForRoleAsync(employeeRole.Name, SurveyPermissions.View, true);
            await _permissionManager.SetForRoleAsync(employeeRole.Name, SurveyPermissions.Submit, true);

            var managerRole = await _roleManager.FindByNameAsync(UserRoles.Manager);
            await _permissionManager.SetForRoleAsync(managerRole.Name, SurveyPermissions.ViewAll, true);
        }

        private async Task SeedRoles()
        {
            foreach (var roleName in UserRoles.AllRoles)
                await CreateRoleIfNotExistsAsync(roleName);
        }
        private async Task CreateRoleIfNotExistsAsync(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                role = new IdentityRole(
                    _guidGenerator.Create(),
                    roleName
                );
                await _roleManager.CreateAsync(role);
            }
        }
    }
}
