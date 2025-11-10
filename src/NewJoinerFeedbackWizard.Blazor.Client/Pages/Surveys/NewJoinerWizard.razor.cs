using NewJoinerFeedbackWizard.Dtos.Survey;
using NewJoinerFeedbackWizard.Dtos.User;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewJoinerFeedbackWizard.Blazor.Client.Pages.Surveys
{
    public partial class NewJoinerWizard
    {
        private CreateSurveyDto Survey { get; set; } = new();
        private int CurrentStep { get; set; } = 1;
        private bool IsSubmitting { get; set; }
        private List<UserDto> Leads { get; set; } = new();
        private List<UserDto> Managers { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            Survey.JoiningDate = DateTime.Now;
            Survey.SatisfactionLevel = 50;

            // Load leads and managers
            try
            {
                Leads = await UserAppService.GetAllLeads();
                Managers = await UserAppService.GetAllManagers();
            }
            catch
            {
                // If not implemented yet, use text inputs instead
                Leads = new List<UserDto>();
                Managers = new List<UserDto>();
            }
        }

        private void NextStep()
        {
            if (IsCurrentStepValid() && CurrentStep < 3)
            {
                CurrentStep++;
            }
        }

        private void PreviousStep()
        {
            if (CurrentStep > 1)
            {
                CurrentStep--;
            }
        }

        private bool IsCurrentStepValid()
        {
            return CurrentStep switch
            {
                1 => !string.IsNullOrWhiteSpace(Survey.EmployeeName) &&
                     !string.IsNullOrWhiteSpace(Survey.LeadName) &&
                     !string.IsNullOrWhiteSpace(Survey.ManagerName),
                2 => !string.IsNullOrWhiteSpace(Survey.Recommendations) &&
                     !string.IsNullOrWhiteSpace(Survey.StrengthsObserved) &&
                     !string.IsNullOrWhiteSpace(Survey.MissingAreas),
                _ => true
            };
        }

        private async Task SubmitSurvey()
        {
            try
            {
                IsSubmitting = true;
                await SurveyAppService.CreateSurvey(Survey);

                await MessageService.Success("Survey submitted successfully!");
                NavigationManager.NavigateTo("/");
            }
            catch (Exception ex)
            {
                await MessageService.Error($"Error submitting survey: {ex.Message}");
            }
            finally
            {
                IsSubmitting = false;
            }
        }

        private string GetStepClass(int step)
        {
            if (CurrentStep == step)
                return "bg-primary text-white shadow";
            else if (CurrentStep > step)
                return "bg-primary text-white";
            else
                return "bg-light text-secondary";
        }

        private string GetStepTextClass(int step)
        {
            return CurrentStep >= step ? "text-primary" : "text-muted";
        }

        private string GetProgressLineClass(int step)
        {
            return CurrentStep > step ? "bg-primary" : "bg-light";
        }

        private string GetStepLabel(int step)
        {
            return step switch
            {
                1 => "Information",
                2 => "Feedback",
                3 => "Submit",
                _ => ""
            };
        }
    }
}
