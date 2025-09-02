using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using FluentValidation;
using LinaSys.Diagnostics.Domain.Aggregates.Form;
using LinaSys.Diagnostics.Domain.Enums;
using LinaSys.Diagnostics.Domain.Repositories;
using LinaSys.KnowledgeStructure.Domain.Repositories;
using LinaSys.Shared.Application;
using LinaSys.Shared.Application.MediatR;

namespace LinaSys.Orchestration.Application.Diagnostics.Commands;

public sealed record UpsertDiagnosisFormFromCsvOrchestrationCommand(Stream CsvStream, string FormName, int KnowledgeStructureId) : IBaseRequest;

public sealed class UpsertDiagnosisFormFromCsvOrchestrationCommandValidator : AbstractValidator<UpsertDiagnosisFormFromCsvOrchestrationCommand>
{
    public UpsertDiagnosisFormFromCsvOrchestrationCommandValidator()
    {
        RuleFor(x => x.CsvStream).NotNull();
        RuleFor(x => x.FormName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.KnowledgeStructureId).GreaterThan(0);
    }
}

public sealed class UpsertDiagnosisFormFromCsvOrchestrationCommandHandler(
    IFormRepository formRepository,
    IKnowledgeStructureRepository knowledgeStructureRepository,
    IBlockRepository blockRepository) : BaseCommandHandler<UpsertDiagnosisFormFromCsvOrchestrationCommand>
{
    private static readonly int _answerBlockSize = Enum.GetNames<AnswerStructOffset>().Length;
    private static readonly int _rowHeaderSize = Enum.GetNames<RowHeaderStructOffset>().Length;

    private Dictionary<string, (long ModuleId, Dictionary<string, long> Topics)> _modulesTopics = [];

    private enum AnswerStructOffset
    {
        AnswerText = 0,
        AnswerScore = 1,
        AnswerFoda = 2,
        AnswerFodaExplanation = 3,
        AnswerOdsr = 4,
        AnswerOdsrExplanation = 5,
        AnswerSuggestedQuestion = 6,
    }

    private enum RowHeaderStructOffset
    {
        ModuleName = 0,
        TopicName = 1,
        BlockName = 2,
        Question = 3,
        AnswerType = 4,
        AppliesToPhase = 5,
        IsUsedForMentoring = 6,
        IsUsedForDiagnosis = 7,
    }

    public override async Task<Result> Handle(UpsertDiagnosisFormFromCsvOrchestrationCommand request, CancellationToken cancellationToken)
    {
        await FillModulesTopicsTopologyAsync(request.KnowledgeStructureId, cancellationToken).ConfigureAwait(false);

        using var reader = new StreamReader(request.CsvStream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HeaderValidated = null, MissingFieldFound = null, });

        var (isValid, headers, validationResult) = await IsValidCsvStructureAsync(csv);
        if (!isValid)
        {
            return validationResult!;
        }

        if (await FormAlreadyExists(request.FormName, cancellationToken).ConfigureAwait(false))
        {
            return Failure(ResultErrorCodes.DiagnosisForm_NameAlreadyExists, (nameof(request.FormName), "Another form with the same name already exists."));
        }

        var form = new Form(request.FormName);
        var questionOrder = 1;

        while (await csv.ReadAsync())
        {
            var row = Row.ReadRowFromCsv(csv, headers);

            long? topicId = null;
            if (row.ModuleName is not null && row.TopicName is not null)
            {
                topicId = GetTopicIdByNameInModuleAsync(row.ModuleName, row.TopicName);
            }

            var blockId = await GetBlockIdByNameAsync(row.BlockName, cancellationToken).ConfigureAwait(false);

            var question = form.AddQuestion(
                topicId,
                blockId,
                row.Question,
                row.AnswerType,
                row.AppliesToPhase,
                row.IsUsedForMentoring,
                row.IsUsedForDiagnosis,
                questionOrder++);

            var answerOrder = 1;
            foreach (var answer in row.AnswerBlocks)
            {
                question.Question.AddAnswerOption(
                    answer.AnswerText,
                    answer.Score,
                    answer.Foda,
                    answer.FodaExplanation,
                    answer.Odsr,
                    answer.OdsrExplanation,
                    answer.SuggestedQuestion,
                    answerOrder++);
            }
        }

        formRepository.Add(form);
        return Success();
    }

    private static async Task<(bool IsValid, string[] Headers, Result? ResultIfFalse)> IsValidCsvStructureAsync(CsvReader csv)
    {
        await csv.ReadAsync().ConfigureAwait(false);
        csv.ReadHeader();
        var headers = csv.HeaderRecord!;

        if (headers.Length < _rowHeaderSize)
        {
            return (false, headers, Failure(ResultErrorCodes.DiagnosisForm_InvalidCsv, ("csv", "CSV must contain at least 6 columns for base question fields.")));
        }

        if ((headers.Length - _rowHeaderSize) % _answerBlockSize != 0)
        {
            return (false, headers, Failure(ResultErrorCodes.DiagnosisForm_InvalidCsv, ("csv", $"CSV answer option columns must be in groups of {_answerBlockSize}: Answer, Score, Foda, FodaExplanation, Odsr, OdsrExplanation, SuggestedQuestion.")));
        }

        return (true, headers, null);
    }

    private async Task FillModulesTopicsTopologyAsync(long knowledgeStructureId, CancellationToken cancellationToken)
    {
        var knowledgeStructure = await knowledgeStructureRepository.GetWithModulesAndTopicsByIdAsync(knowledgeStructureId, cancellationToken).ConfigureAwait(false);
        if (knowledgeStructure is null)
        {
            throw new Exception($"Knowledge structure with ID {knowledgeStructureId} not found.");
        }

        foreach (var module in knowledgeStructure.KnowledgeStructureModules)
        {
            var topics = module.KnowledgeStructureTopics.Select(s => s.Topic).ToDictionary(k => k.Name, t => t.Id);
            _modulesTopics[module.Module.Name] = (module.Module.Id, topics);
        }
    }

    private Task<bool> FormAlreadyExists(string name, CancellationToken cancellationToken)
    {
        return formRepository.ExistsByNameAsync(name, cancellationToken);
    }

    private long GetModuleIdByName(string moduleName)
    {
        if (_modulesTopics.TryGetValue(moduleName, out var moduleInfo))
        {
            return moduleInfo.ModuleId;
        }

        throw new Exception($"Module with name '{moduleName}' not found in the knowledge structure.");
    }

    private long GetTopicIdByNameInModuleAsync(string moduleName, string topicName)
    {
        if (_modulesTopics.TryGetValue(moduleName, out var moduleInfo) && moduleInfo.Topics.TryGetValue(topicName, out var topicId))
        {
            return topicId;
        }

        throw new Exception($"Topic with name '{topicName}' not found in module '{moduleName}'.");
    }

    private async Task<long> GetBlockIdByNameAsync(string blockName, CancellationToken cancellationToken)
    {
        var block = await blockRepository.GetByNameAsync(blockName, cancellationToken).ConfigureAwait(false);

        if (block is not null)
        {
            return block.Id;
        }

        throw new Exception($"Block with name '{blockName}' not found.");
    }

    private class Answer
    {
        public string AnswerText { get; set; }

        public FodaType Foda { get; set; }

        public string FodaExplanation { get; set; }

        public OdsrType Odsr { get; set; }

        public string OdsrExplanation { get; set; }

        public int Score { get; set; }

        public string? SuggestedQuestion { get; set; }
    }

    private class Row
    {
        public List<Answer> AnswerBlocks { get; set; } = [];

        public AnswerType AnswerType { get; set; }

        public QuestionPhase AppliesToPhase { get; set; }

        public string BlockName { get; set; }

        public bool IsUsedForDiagnosis { get; set; }

        public bool IsUsedForMentoring { get; set; }

        public string? ModuleName { get; set; }

        public string Question { get; set; }

        public string? TopicName { get; set; }

        public static Row ReadRowFromCsv(CsvReader csv, string[] headers)
        {
            var answerTypeStr = csv[(int)RowHeaderStructOffset.AnswerType] ?? throw new Exception("AnswerType offset not present.");
            var appliesToPhaseStr = csv[(int)RowHeaderStructOffset.AppliesToPhase] ?? throw new Exception("AppliesToPhase offset not present.");
            var isUsedForMentoringStr = csv[(int)RowHeaderStructOffset.IsUsedForMentoring] ?? throw new Exception("IsUsedForMentoring offset not present.");
            var isUsedForDiagnosisStr = csv[(int)RowHeaderStructOffset.IsUsedForDiagnosis] ?? throw new Exception("IsUsedForDiagnosis offset not present.");
            var parsedIsUsedForMentoring = isUsedForMentoringStr.ToLowerInvariant() is "x" or "1" or "si" or "s";
            var parsedIsUsedForDiagnosis = isUsedForDiagnosisStr.ToLowerInvariant() is "x" or "1" or "si" or "s";
            var topicNameStr = csv[(int)RowHeaderStructOffset.TopicName] ?? string.Empty;
            var moduleNameStr = csv[(int)RowHeaderStructOffset.ModuleName] ?? string.Empty;

            var row = new Row
            {
                TopicName = string.IsNullOrWhiteSpace(topicNameStr) ? null : topicNameStr,
                ModuleName = string.IsNullOrWhiteSpace(moduleNameStr) ? null : moduleNameStr,
                BlockName = csv[(int)RowHeaderStructOffset.BlockName] ?? throw new Exception("Block offset not present."),
                Question = csv[(int)RowHeaderStructOffset.Question] ?? throw new Exception("Question offset not present."),
                AnswerType = TranslateAnswerType(answerTypeStr),
                AppliesToPhase = TranslateQuestionPhase(appliesToPhaseStr),
                IsUsedForMentoring = parsedIsUsedForMentoring,
                IsUsedForDiagnosis = parsedIsUsedForDiagnosis,
            };

            if (row.TopicName is not null && row.ModuleName is null)
            {
                throw new Exception("When a topic is set, the Module must be set too.");
            }

            var answerStartIndex = _rowHeaderSize;

            for (var i = answerStartIndex; i < headers.Length; i += _answerBlockSize)
            {
                var answerText = csv[(int)AnswerStructOffset.AnswerText + i];
                if (string.IsNullOrWhiteSpace(answerText))
                {
                    break;
                }

                var scoreStr = csv[(int)AnswerStructOffset.AnswerScore + i] ?? throw new Exception("Answer SCORE offset not present.");
                var fodaStr = csv[(int)AnswerStructOffset.AnswerFoda + i] ?? throw new Exception("Answer FODA offset not present.");
                var fodaExplanation = csv[(int)AnswerStructOffset.AnswerFodaExplanation + i] ?? throw new Exception("Answer FODA Explanation offset not present.");
                var odsrStr = csv[(int)AnswerStructOffset.AnswerOdsr + i] ?? throw new Exception("Answer ODSR offset not present.");
                var odsrExplanation = csv[(int)AnswerStructOffset.AnswerOdsrExplanation + i] ?? throw new Exception("Answer ODSR Explanation offset not present.");
                var suggestedQuestion = csv[(int)AnswerStructOffset.AnswerSuggestedQuestion + i] ?? throw new Exception("Answer Suggested question offset not present.");

                row.AnswerBlocks.Add(new Answer
                {
                    AnswerText = answerText.Trim(),
                    Score = int.TryParse(scoreStr, out var score) ? score : 0,
                    Foda = TranslateFodaType(fodaStr),
                    FodaExplanation = fodaExplanation.Trim(),
                    Odsr = TranslateOdsrType(odsrStr),
                    OdsrExplanation = odsrExplanation.Trim(),
                    SuggestedQuestion = string.IsNullOrWhiteSpace(suggestedQuestion) ? null : suggestedQuestion.Trim(),
                });
            }

            return row;
        }

        private static AnswerType TranslateAnswerType(string answerStr)
        {
            if (Enum.TryParse(answerStr, true, out AnswerType result))
            {
                return result;
            }

            return answerStr.ToLowerInvariant().Trim().Replace(" ", string.Empty) switch
            {
                "abierta" => AnswerType.FreeText,
                "nacionalidad" => AnswerType.Nationality,
                "tipoidentificacion" or "tipoidentificación" => AnswerType.IdType,
                "identificacion" or "identificación" => AnswerType.PersonId,
                "fecha" => AnswerType.Date,
                "numero" or "número" => AnswerType.Numeric,
                "genero" or "género" => AnswerType.Gender,
                "estadocivil" => AnswerType.MaritalStatus,
                "telefono" or "teléfono" => AnswerType.PhoneNumber,
                "correo" => AnswerType.Email,
                "desplegable" => AnswerType.SingleChoice,
                "opciónmultiple" or "opcionmultiple" or "opcionmúltiple" or "opciónmúltiple" or "variasopciones" => AnswerType.MultiChoice,
                _ => throw new Exception($"AnswerType: {answerStr} is not valid."),
            };
        }

        private static FodaType TranslateFodaType(string fodaStr)
        {
            if (Enum.TryParse(fodaStr, true, out FodaType result))
            {
                return result;
            }

            return fodaStr.ToLowerInvariant() switch
            {
                "f" or "F" => FodaType.Fortalezas,
                "o" or "O" => FodaType.Oportunidades,
                "d" or "D" => FodaType.Debilidades,
                "a" or "A" => FodaType.Amenazas,
                _ => FodaType.NoDefinido,
            };
        }

        private static OdsrType TranslateOdsrType(string odsrStr)
        {
            if (Enum.TryParse(odsrStr, true, out OdsrType result))
            {
                return result;
            }

            return odsrStr.ToLowerInvariant().Trim().Replace(" ", string.Empty) switch
            {
                "r" or "R" or "reorientacion" or "reorientación" => OdsrType.Reorientacion,
                "d" or "D" => OdsrType.Defensiva,
                "o" or "O" => OdsrType.Ofensiva,
                "s" or "S" => OdsrType.Supervivencia,
                _ => OdsrType.NoDefinido,
            };
        }

        private static QuestionPhase TranslateQuestionPhase(string appliesToPhaseStr)
        {
            if (Enum.TryParse(appliesToPhaseStr, true, out QuestionPhase result))
            {
                return result;
            }

            return appliesToPhaseStr.ToLowerInvariant() switch
            {
                "ingreso" => QuestionPhase.Start,
                "salida" => QuestionPhase.Start,
                "ambos" or "ambas" => QuestionPhase.Both,
                _ => QuestionPhase.Undefined,
            };
        }
    }
}
