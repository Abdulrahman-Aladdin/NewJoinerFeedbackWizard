using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NewJoinerFeedbackWizard.Constants;
using NewJoinerFeedbackWizard.Dtos.Survey;
using NewJoinerFeedbackWizard.Dtos.User;
using NewJoinerFeedbackWizard.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Components.Messages;
using static System.Net.WebRequestMethods;

namespace NewJoinerFeedbackWizard.Blazor.Client.Pages.Surveys
{
    public partial class ManagerDashboard
    {
        [Inject]
        private IJSRuntime JS { get; set; }
        [Inject]
        private ISurveyAppService SurveyAppService { get; set; }
        [Inject]
        private IUserAppService UserAppService { get; set; }
        [Inject]
        private IUiMessageService MessageService { get; set; }

        private List<SurveyDto> AllSurveys { get; set; } = [];
        private List<SurveyDto> FilteredSurveys { get; set; } = [];
        private string SearchTerm { get; set; } = string.Empty;
        private Guid? ExpandedSurveyId { get; set; }
        private bool IsLoading { get; set; } = true;
        private UserDto? CurrentUserInfo { get; set; }

        private int TotalCount => AllSurveys.Count;
        private int ThisMonthCount => AllSurveys.Count(s => s.CreationTime.Month == DateTime.Now.Month && s.CreationTime.Year == DateTime.Now.Year);
        private int AvgSatisfaction => AllSurveys.Any() ? (int)AllSurveys.Average(s => s.SatisfactionLevel) : 0;
        private int UniqueEmployees => AllSurveys.Select(s => s.EmployeeName).Distinct().Count();

        private bool IsDownloadInProgress = false;
        private bool HasDeletePermission = false;

        private bool IsDeleteModalVisible = false;
        private SurveyDto? SurveyToDelete;

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
                if (CurrentUserInfo.Roles.Contains("admin"))
                {
                    HasDeletePermission = true;
                }
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

        private void ClearFilters()
        {
            SearchTerm = string.Empty;
            FilteredSurveys = AllSurveys;
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
            var excelData = await GetExcelData();
            if (!excelData.IsNullOrEmpty())
            {
                IsDownloadInProgress = true;
                var userName = CurrentUserInfo?.Name ?? "UnknownUser";
                await JS.InvokeVoidAsync("downloadFileFromBytes", $"SurveysOf{userName}At{DateTime.Now.ToString("yyyy-MM-dd_hh:mm:ss")}.xlsx", excelData);
            }
            IsDownloadInProgress = false;
        }

        private async Task<Byte[]> GetExcelData()
        {
            if (IsInRole("Manager"))
            {
                var userName = CurrentUserInfo.Name;
                var excelData = await SurveyAppService.ExportToExcel(userName);
                return excelData;
            }
            else if (IsInRole("admin"))
            {
                var excelData = await SurveyAppService.ExportAllToExcel();
                return excelData;
            }
            return [];
        }

        private bool IsInRole(string role)
        {
            return CurrentUserInfo != null && CurrentUserInfo.Roles.Contains(role);
        }

        private async Task ShowDeleteConfirmation(Guid surveyId)
        {
            IsDeleteModalVisible = true;
            SurveyToDelete = AllSurveys.FirstOrDefault(s => s.Id == surveyId);
        }

        private async Task CloseDeleteModal()
        {
            IsDeleteModalVisible = false;
            SurveyToDelete = null;
        }

        private async Task ConfirmDelete()
        {
            if (SurveyToDelete is not null)
            {
                await SurveyAppService.Delete(SurveyToDelete.Id);

                AllSurveys.Remove(SurveyToDelete);
                ClearFilters();
            }

            CloseDeleteModal();
        }
    }
}
