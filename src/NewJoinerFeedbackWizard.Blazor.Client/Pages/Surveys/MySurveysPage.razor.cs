using NewJoinerFeedbackWizard.Dtos.Survey;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewJoinerFeedbackWizard.Blazor.Client.Pages.Surveys
{
    public partial class MySurveysPage
    {
        private List<SurveyDto> MySurveys { get; set; } = new();
        private Guid? ExpandedSurveyId { get; set; }
        private bool IsLoading { get; set; } = true;

        private int AvgSatisfaction => MySurveys.Any() ? (int)MySurveys.Average(s => s.SatisfactionLevel) : 0;

        protected override async Task OnInitializedAsync()
        {
            await LoadMySurveys();
        }

        private async Task LoadMySurveys()
        {
            try
            {
                IsLoading = true;
                MySurveys = await SurveyAppService.GetMySubmittedSurveys();
            }
            catch (Exception ex)
            {
                await MessageService.Error($"Error loading surveys: {ex.Message}");
                MySurveys = new List<SurveyDto>();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CreateNewSurvey()
        {
            NavigationManager.NavigateTo("/surveys/new");
        }

        private async Task DeleteSurvey(Guid surveyId)
        {
            var confirmed = await MessageService.Confirm(L["Delete:Confirmation"]);
            if (confirmed)
            {
                try
                {
                    await SurveyAppService.Delete(surveyId);
                    await MessageService.Success(L["Delete:SurveyDeletedSuccessfully"]);
                    await LoadMySurveys();
                    ExpandedSurveyId = null;
                }
                catch (Exception ex)
                {
                    await MessageService.Error(L["Delete:FailedToDeleteSurvey"]);
                }
            }
        }

        private string GetSatisfactionColor(int level)
        {
            if (level >= 80) return "#16a34a"; // Green
            if (level >= 60) return "#2563eb"; // Blue
            if (level >= 40) return "#f97316"; // Orange
            return "#dc2626"; // Red
        }
    }
}
