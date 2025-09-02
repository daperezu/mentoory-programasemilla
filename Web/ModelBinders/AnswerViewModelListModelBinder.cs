using System.Text.RegularExpressions;
using LinaSys.Web.Areas.Diagnostics.Models.DiagnosisForms;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LinaSys.Web.ModelBinders;

public class AnswerViewModelListModelBinder : IModelBinder
{
    private static readonly Regex _inputPattern = new(@"^Answers_(\d+)_(\d+)(_followup_input)?$", RegexOptions.Compiled);

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var form = bindingContext.HttpContext.Request.Form;
        var answers = new Dictionary<(long QuestionId, long AnswerOptionId), (string UserInput, string? FollowUp)>();

        foreach (var field in form)
        {
            var match = _inputPattern.Match(field.Key);
            if (!match.Success)
            {
                continue;
            }

            long questionId = long.Parse(match.Groups[1].Value);
            long answerOptionId = long.Parse(match.Groups[2].Value);
            bool isFollowUp = match.Groups[3].Success;

            var key = (questionId, answerOptionId);

            if (!answers.ContainsKey(key))
            {
                answers[key] = (UserInput: string.Empty, null);
            }

            var (userInput, followUpInput) = answers[key];

            if (isFollowUp)
            {
                followUpInput = field.Value;
            }
            else
            {
                userInput = field.Value;
            }

            answers[key] = (userInput ?? string.Empty, followUpInput);
        }

        var result = answers.Select(kvp =>
            new AnswerViewModel(kvp.Key.QuestionId, kvp.Key.AnswerOptionId, kvp.Value.UserInput, kvp.Value.FollowUp)).ToList();

        bindingContext.Result = ModelBindingResult.Success(result);
        return Task.CompletedTask;
    }
}
