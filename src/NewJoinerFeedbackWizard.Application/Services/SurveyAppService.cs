using AutoMapper.Internal.Mappers;
using Microsoft.AspNetCore.Authorization;
using NewJoinerFeedbackWizard.Dtos.Survey;
using NewJoinerFeedbackWizard.Interfaces;
using NewJoinerFeedbackWizard.Permissions;
using NewJoinerFeedbackWizard.SurveyModel;
using System;
using System.Collections.Generic;
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
            var currentUserName = $"{CurrentUser.Name} {CurrentUser.SurName}";
            var surveys = await _surveyRepository.GetBySubmittedByAsync(currentUserName);
            return ObjectMapper.Map<List<Survey>, List<SurveyDto>>(surveys);
        }
    }
}
