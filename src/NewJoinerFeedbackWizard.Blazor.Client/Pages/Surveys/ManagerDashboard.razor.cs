using Microsoft.JSInterop;
using NewJoinerFeedbackWizard.Constants;
using NewJoinerFeedbackWizard.Dtos.Survey;
using NewJoinerFeedbackWizard.Dtos.User;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace NewJoinerFeedbackWizard.Blazor.Client.Pages.Surveys
{
    public partial class ManagerDashboard
    {
        private List<SurveyDto> AllSurveys { get; set; } = new();
        private List<SurveyDto> FilteredSurveys { get; set; } = new();
        private string SearchTerm { get; set; } = string.Empty;
        private Guid? ExpandedSurveyId { get; set; }
        private bool IsLoading { get; set; } = true;
        private UserDto? CurrentUserInfo { get; set; }

        private int TotalCount => AllSurveys.Count;
        private int ThisMonthCount => AllSurveys.Count(s => s.CreationTime.Month == DateTime.Now.Month && s.CreationTime.Year == DateTime.Now.Year);
        private int AvgSatisfaction => AllSurveys.Any() ? (int)AllSurveys.Average(s => s.SatisfactionLevel) : 0;
        private int UniqueEmployees => AllSurveys.Select(s => s.EmployeeName).Distinct().Count();

        private bool IsDownloadInProgress = false;

        protected override async Task OnInitializedAsync()
        {
            await LoadCurrentUser();
            await LoadSurveys();
        }

        private async Task LoadCurrentUser()
        {
            try
            {
                CurrentUserInfo = await UserAppService.GetCurrentUserAsync();
            }
            catch (Exception ex)
            {
                await MessageService.Error($"Error loading user info: {ex.Message}");
            }
        }

        private async Task LoadSurveys()
        {
            try
            {
                IsLoading = true;

                // If user is manager, get their team surveys, otherwise get all
                if (CurrentUserInfo != null && CurrentUserInfo.Roles.Contains("Manager"))
                {
                    AllSurveys = await SurveyAppService.GetSurveysByManager(CurrentUserInfo.Name);
                }
                else
                {
                    AllSurveys = await SurveyAppService.GetAllSurveys();
                }

                ApplyFilters();
            }
            catch (Exception ex)
            {
                await MessageService.Error($"Error loading surveys: {ex.Message}");
                AllSurveys = new List<SurveyDto>();
                FilteredSurveys = new List<SurveyDto>();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnSearchChanged()
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            FilteredSurveys = AllSurveys.Where(s =>
            {
                var matchesSearch = string.IsNullOrWhiteSpace(SearchTerm) ||
                                   s.EmployeeName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                                   s.LeadName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase);
                return matchesSearch;
            }).ToList();
        }

        private string GetSatisfactionColor(int level)
        {
            if (level >= 80) return "#16a34a"; // Green
            if (level >= 60) return "#2563eb"; // Blue
            if (level >= 40) return "#f97316"; // Orange
            return "#dc2626"; // Red
        }

        private async Task ExportToExcel()
        {
            if (CurrentUserInfo != null && CurrentUserInfo.Roles.Contains("Manager"))
            {
                var userName = CurrentUserInfo.Name;
                IsDownloadInProgress = true;
                var excelData = await SurveyAppService.ExportToExcel(userName);
                if (excelData != null)
                {
                    await JS.InvokeVoidAsync("downloadFileFromBytes", $"SurveysOf{userName}At{DateTime.Now.ToString("yyyy-MM-dd_hh:mm:ss")}.xlsx", excelData);
                }
                IsDownloadInProgress = false;
            }
            
        }
    }
}
