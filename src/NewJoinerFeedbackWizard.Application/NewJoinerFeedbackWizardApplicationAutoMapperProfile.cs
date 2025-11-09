using AutoMapper;
using NewJoinerFeedbackWizard.Dtos.Survey;
using NewJoinerFeedbackWizard.SurveyModel;

namespace NewJoinerFeedbackWizard;

public class NewJoinerFeedbackWizardApplicationAutoMapperProfile : Profile
{
    public NewJoinerFeedbackWizardApplicationAutoMapperProfile()
    {
        CreateMap<Survey, SurveyDto>().ReverseMap();
        CreateMap<CreateSurveyDto, Survey>();
    }
}
