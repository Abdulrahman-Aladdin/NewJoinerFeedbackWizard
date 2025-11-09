using Microsoft.AspNetCore.Components;
using NewJoinerFeedbackWizard.Dtos.User;
using NewJoinerFeedbackWizard.Interfaces;
using System.Threading.Tasks;

namespace NewJoinerFeedbackWizard.Blazor.Client.Pages;

public partial class Index
{
    [Inject]
    private IUserAppService _userAppService { get; set; } = default!;
    UserDto? CurrentUser { get; set; } = null;
    private bool IsLoading { get; set; } = true;

    protected async override Task OnInitializedAsync()
    {
        CurrentUser = await _userAppService.GetCurrentUserAsync();
        IsLoading = false;
    }
}
