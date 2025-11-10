using AutoMapper.Internal.Mappers;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewJoinerFeedbackWizard.Dtos.Survey;
using NewJoinerFeedbackWizard.Interfaces;
using NewJoinerFeedbackWizard.Permissions;
using NewJoinerFeedbackWizard.SurveyModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace NewJoinerFeedbackWizard.Services
{
    public class SurveyAppService(ISurveyRepository surveyRepository) : ApplicationService, ISurveyAppService
    {
        private readonly ISurveyRepository _surveyRepository = surveyRepository;

        [Authorize($"{SurveyPermissions.Submit}")]
        public async Task CreateSurvey(CreateSurveyDto input)
        {
            await _surveyRepository.InsertAsync(ObjectMapper.Map<CreateSurveyDto, Survey>(input));
        }

        [Authorize($"{SurveyPermissions.ViewAll}")]
        public async Task<List<SurveyDto>> GetAllSurveys()
        {
            return ObjectMapper.Map<List<Survey>, List<SurveyDto>>(await _surveyRepository.GetListAsync());
        }

        [Authorize($"{SurveyPermissions.View}")]
        public async Task<SurveyDto?> GetSurveyByEmployeeName(string employeeName)
        {
            var survey = await _surveyRepository.GetByEmployeeNameAsync(employeeName);
            return survey != null ? ObjectMapper.Map<Survey, SurveyDto>(survey) : null;
        }

        [Authorize($"{SurveyPermissions.Delete}")]
        public async Task Delete(Guid id)
        {
            await _surveyRepository.DeleteAsync(id);
        }


        [Authorize(SurveyPermissions.ViewAll)]
        public async Task<List<SurveyDto>> GetSurveysByManager(string managerName)
        {
            var surveys = await _surveyRepository.GetByManagerNameAsync(managerName);
            return ObjectMapper.Map<List<Survey>, List<SurveyDto>>(surveys);
        }

        [Authorize(SurveyPermissions.View)]
        public async Task<List<SurveyDto>> GetMySubmittedSurveys()
        {
            //var currentUserName = $"{CurrentUser.Name} {CurrentUser.SurName}";
            //var surveys = await _surveyRepository.GetBySubmittedByAsync(currentUserName);
            //return ObjectMapper.Map<List<Survey>, List<SurveyDto>>(surveys);
            var currentUserId = CurrentUser.Id;
            if (currentUserId == null)
            {
                return new List<SurveyDto>();
            }

            var surveys = await _surveyRepository.GetBySubmittedByAsync(currentUserId.ToString());
            return ObjectMapper.Map<List<Survey>, List<SurveyDto>>(surveys);
        }

        [Authorize(SurveyPermissions.ViewAll)]
        public async Task<Byte[]> ExportToExcel(string managerName)
        {
            var surveys = await _surveyRepository.GetByManagerNameAsync(managerName);
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Surveys");

            // Add headers
            var headers = new[]
            {
                "Employee Name", "Joining Date", "Lead Name", "Manager Name",
                "Satisfaction Level", "Strengths Observed", "Areas for Improvement",
                "Recommendations", "Creation Time"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
            }

            // Add data
            for (int i = 0; i < surveys.Count; i++)
            {
                var survey = surveys[i];
                worksheet.Cell(i + 2, 1).Value = survey.EmployeeName;
                worksheet.Cell(i + 2, 2).Value = survey.JoiningDate.ToString("yyyy-MM-dd");
                worksheet.Cell(i + 2, 3).Value = survey.LeadName;
                worksheet.Cell(i + 2, 4).Value = survey.ManagerName;
                worksheet.Cell(i + 2, 5).Value = survey.SatisfactionLevel;
                worksheet.Cell(i + 2, 6).Value = survey.StrengthsObserved;
                worksheet.Cell(i + 2, 7).Value = survey.MissingAreas;
                worksheet.Cell(i + 2, 8).Value = survey.Recommendations;
                worksheet.Cell(i + 2, 9).Value = survey.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");

                if (survey.SatisfactionLevel < 40)
                {
                    worksheet.Row(i + 2).Style.Fill.BackgroundColor = XLColor.LightPink;
                } else if (survey.SatisfactionLevel >= 40 && survey.SatisfactionLevel <= 70)
                {
                    worksheet.Row(i + 2).Style.Fill.BackgroundColor = XLColor.LightYellow;
                }
                else
                {
                    worksheet.Row(i + 2).Style.Fill.BackgroundColor = XLColor.LightGreen;
                }
            }

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            workbook.Dispose();
            stream.Close();
            return stream.ToArray();
        }
    }
}
    

