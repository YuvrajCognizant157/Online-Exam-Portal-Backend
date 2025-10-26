using ClosedXML.Excel;
using Domain.Data;
using Domain.Models;
using Infrastructure.DTOs.QuestionsDTO;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Globalization;
using System.Globalization;
using System.Text.Json;


namespace Infrastructure.Repositories.Implementations
{
    public class QuestionRepository : IQuestionRepository
    {

        private readonly AppDbContext _context;

        public QuestionRepository(AppDbContext dbContext)
        {
            _context = dbContext;
        }

        public async Task<int> AddQuestion(AddQuestionDTO question, int eid)
        {

            bool topicExistsAndApproved = await _context.Topics.AnyAsync(t => t.Tid == question.Tid && t.ApprovalStatus == 1);
            if (!topicExistsAndApproved)
                return 0;

            int? MarksPerQuestion = await _context.Exams
                .Where(e => e.Eid == eid)
                .Select(e => e.MarksPerQuestion)
                .FirstOrDefaultAsync();

            Question quest = new()
            {
                Type = question.type,
                Question1 = question.question,
                Marks = MarksPerQuestion,
                Options = question.options,
                Tid = question.Tid,
                CorrectOptions = JsonConvert.SerializeObject(question.correctOptions),
                ApprovalStatus = question.ApprovalStatus
            };

            quest.Eid = eid;

            await _context.Questions.AddAsync(quest);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> AddBatchQuestionsToExam(AddQuestionsByBatchDTO questions, int eid)
        {
            int? MarksPerQuestion = await _context.Exams
                .Where(e => e.Eid == eid)
                .Select(e => e.MarksPerQuestion)
                .FirstOrDefaultAsync();

            List<Question> questionList = new();
            foreach (var question in questions.Questions)
            {
                Question quest = new()
                {
                    Eid = eid,
                    Tid = questions.Tid,
                    Type = question.Type,
                    Question1 = question.Question,
                    Marks = MarksPerQuestion,
                    Options = question.Options,
                    CorrectOptions = JsonConvert.SerializeObject(question.CorrectOptions),
                    ApprovalStatus = question.ApprovalStatus
                };
                questionList.Add(quest);
            }

            await _context.Questions.AddRangeAsync(questionList);
            return await _context.SaveChangesAsync();
        }

        public async Task<List<Question>> GetQuestionsByExamId(int examId)
        {
            List<Question> questions = await _context.Questions.Where(q => q.Eid == examId).ToListAsync();
            return questions;
        }

        public Question GetQuestionById(int questionId)
        {
            return _context.Questions.FirstOrDefault(q => q.Qid == questionId);

        }

        public async Task<(List<ListQuestionsDTO> Questions, int TotalCount)> GetQuestionsByExaminerID(int examinerId, int page, int pageSize)
        {
            // 1. Define the base query (without pagination)
            var baseQuery = _context.Questions
                .Include(q => q.EidNavigation)
                .Where(q => q.EidNavigation != null && q.EidNavigation.UserId == examinerId);

            // 2. Get the total count of all matching questions
            var totalCount = await baseQuery.CountAsync();

            // 3. Apply Skip and Take for pagination
            var questions = await baseQuery
                .Skip((page - 1) * pageSize) // Skip items from previous pages
                .Take(pageSize)              // Take only the current page's items
                .Select(q => new ListQuestionsDTO
                {
                    QuestionName = q.Question1,
                    QuestionId = q.Qid,
                    QuestionType = q.Type
                })
                .ToListAsync();

            return (questions, totalCount);
        }

        public async Task<int> UpdateQuestion(UpdateQuestionDTO question, int qid)
        {

            var existingQuestion = await _context.Questions.FirstOrDefaultAsync(q => q.Qid == qid);

            if (existingQuestion == null)
                return 0;

            if (question.type != null)
                existingQuestion.Type = question.type;

            if (question.question != null)
                existingQuestion.Question1 = question.question;

            if (question.options != null)
                existingQuestion.Options = question.options;

            if (question.correctOptions != null)
                existingQuestion.CorrectOptions = JsonConvert.SerializeObject(question.correctOptions);

            if (question.ApprovalStatus.HasValue)
                existingQuestion.ApprovalStatus = question.ApprovalStatus;

            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteQuestion(int qid)
        {
            // Find the question
            var question = await _context.Questions.FirstOrDefaultAsync(q => q.Qid == qid);
            if (question == null)
                return 0;

            var exam = new Exam();
            // Check and update Exam if approvalStatus is 1
            if (question.Eid.HasValue)
            {
                exam = await _context.Exams.FirstOrDefaultAsync(e => e.Eid == question.Eid.Value);
                if (exam != null && exam.ApprovalStatus == 1)
                {
                    exam.ApprovalStatus = 0;
                    if (question.Marks.HasValue && exam.TotalMarks.HasValue)
                        exam.TotalMarks -= question.Marks.Value;
                }


            }

            // Delete related QuestionReports
            var questionReports = await _context.QuestionReports.Where(qr => qr.Qid == qid).ToListAsync();
            if (questionReports.Any())
                _context.QuestionReports.RemoveRange(questionReports);

            // Delete related Responses
            var responses = await _context.Responses.Where(r => r.Qid == qid).ToListAsync();
            if (responses.Any())
                _context.Responses.RemoveRange(responses);

            // Delete the Question itself
            _context.Questions.Remove(question);

            return await _context.SaveChangesAsync();
        }

        public async Task<ImportResultDto> ImportQuestionsFromExcelAsync(IFormFile file, int? eid = null)
        {
            var result = new ImportResultDto();

            if (file == null || file.Length == 0)
            {
                result.Errors.Add(new ImportResultDto.RowError { RowNumber = 0, Message = "File is empty or missing." });
                return result;
            }

            // Validate extension
            var allowedExt = new[] { ".xlsx", ".xls" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExt.Contains(ext))
            {
                result.Errors.Add(new ImportResultDto.RowError { RowNumber = 0, Message = "Only .xlsx/.xls files are supported." });
                return result;
            }


            //extract the exam tids and match also marks per question
            var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Eid == eid);

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Position = 0;

            using var workbook = new XLWorkbook(ms);
            var worksheet = workbook.Worksheets.First(); // assume first sheet
            var firstRowUsed = worksheet.FirstRowUsed();
            if (firstRowUsed == null)
            {
                result.Errors.Add(new ImportResultDto.RowError { RowNumber = 0, Message = "Worksheet is empty." });
                return result;
            }

            // Find header row and column indexes for expected headers (case-insensitive)
            var headerRow = firstRowUsed.RowUsed();
            var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var cell in headerRow.CellsUsed())
            {
                var hv = cell.GetString()?.Trim() ?? string.Empty;
                if (!string.IsNullOrEmpty(hv) && !headers.ContainsKey(hv))
                    headers[hv] = cell.Address.ColumnNumber;
            }

            // expected header names (as per your description)
            var expected = new[] { "questionname", "question", "question1", "option1", "option2", "option3", "option4", "type", "correctoptions", "tid" };
            // The excel may call question column "questionname" or "question" or "question1" — we check variants.

            // Helper to get column number by key possibilities
            int? GetCol(params string[] names)
            {
                foreach (var n in names)
                    if (headers.TryGetValue(n, out var c)) return c;
                return null;
            }

            var colQuestion = GetCol("questionname", "question", "question1");
            var colOpt1 = GetCol("option1", "option 1", "opt1");
            var colOpt2 = GetCol("option2", "option 2", "opt2");
            var colOpt3 = GetCol("option3", "option 3", "opt3");
            var colOpt4 = GetCol("option4", "option 4", "opt4");
            var colType = GetCol("type");
            var colCorrect = GetCol("correctoptions", "correct options", "correctoptionscomma", "correct");
            var colTid = GetCol("tid");

            if (colQuestion == null || colOpt1 == null || colOpt2 == null || colOpt3 == null || colOpt4 == null || colCorrect == null)
            {
                result.Errors.Add(new ImportResultDto.RowError { RowNumber = headerRow.RowNumber(), Message = "Missing required columns. Required columns: questionname, option1..option4, correctOptions,tid" });
                return result;
            }

            // Check if the 'type' column is also mandatory. 
            // If it is, add it to the check above:
            if (colType == null)
            {
                result.Errors.Add(new ImportResultDto.RowError { RowNumber = headerRow.RowNumber(), Message = "Missing required column: type" });
                return result;
            }


            // Start reading rows after header
            var row = headerRow.RowNumber() + 1;
            var lastRow = worksheet.LastRowUsed().RowNumber();

            var questionsToAdd = new List<Question>();

            for (; row <= lastRow; row++)
            {
                result.TotalRows++;
                try
                {
                    var r = worksheet.Row(row);

                    // read cell values (trimmed)
                    string readCell(int? col) => col == null ? string.Empty : r.Cell(col.Value).GetString().Trim();

                    #region Tid-Verify
                    // Tid per-row override: if excel has Tid and non-empty, use that, else use provided tid parameter
                    int effectiveTid = 2;
                    if (colTid != null)
                    {
                        var tidCell = readCell(colTid);
                        if (!string.IsNullOrWhiteSpace(tidCell) && int.TryParse(tidCell, out var parsedTid))
                            effectiveTid = parsedTid;
                    }

                    //Also check if the question tid is present in the exam
                    List<int> parsedTids = new List<int>();
                    if (exam?.Tids != null)
                    {
                        var tids = JsonConvert.DeserializeObject<List<int>>(exam.Tids);
                        if (tids != null)
                            parsedTids = tids;
                    }

                    bool notPresent = true;
                    foreach (var t in parsedTids)
                    {
                        if (effectiveTid == t) { notPresent = false; }
                    }
                    if (notPresent == true)
                    {

                        result.Errors.Add(new ImportResultDto.RowError
                        {
                            RowNumber = row,
                            Message = $"TID {effectiveTid} is not associated with the current exam."
                        });

                        continue;
                    }
                    #endregion


                    var questionText = readCell(colQuestion);
                    if (string.IsNullOrWhiteSpace(questionText))
                    {
                        result.Errors.Add(new ImportResultDto.RowError { RowNumber = row, Message = "Question text is empty." });
                        continue;
                    }

                    #region Option-Extraction..
                    var opt1 = readCell(colOpt1);
                    var opt2 = readCell(colOpt2);
                    var opt3 = readCell(colOpt3);
                    var opt4 = readCell(colOpt4);

                    // build options dictionary with keys 1..4
                    var optionsDict = new Dictionary<int, string>();
                    if (!string.IsNullOrWhiteSpace(opt1)) optionsDict[1] = opt1;
                    if (!string.IsNullOrWhiteSpace(opt2)) optionsDict[2] = opt2;
                    if (!string.IsNullOrWhiteSpace(opt3)) optionsDict[3] = opt3;
                    if (!string.IsNullOrWhiteSpace(opt4)) optionsDict[4] = opt4;

                    if (optionsDict.Count == 0)
                    {
                        result.Errors.Add(new ImportResultDto.RowError { RowNumber = row, Message = "At least one option is required." });
                        continue;
                    }
                    #endregion

                    // determine type: read from the 'type' column in the Excel row
                    var perRowType = readCell(colType);
                    // Default to "MCQ" if the cell is empty. 
                    // If the 'type' column MUST exist, add a check for colType == null earlier.
                    var typeValue = string.IsNullOrWhiteSpace(perRowType) ? "MCQ" : perRowType.Trim();
                    typeValue = typeValue.Trim().ToUpperInvariant();

                    if (typeValue != "MCQ" && typeValue != "MSQ")
                    {
                        result.Errors.Add(new ImportResultDto.RowError { RowNumber = row, Message = $"Invalid Type '{typeValue}'. Allowed: MCQ or MSQ." });
                        continue;
                    }

                    // correct options: may be comma-separated like "1,3" or "2"
                    var correctRaw = readCell(colCorrect);
                    if (string.IsNullOrWhiteSpace(correctRaw))
                    {
                        result.Errors.Add(new ImportResultDto.RowError { RowNumber = row, Message = "CorrectOptions is empty." });
                        continue;
                    }

                    var correctParts = correctRaw.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                                .Select(p => p.Trim())
                                                .Where(p => p.Length > 0)
                                                .ToList();

                    var correctList = new List<int>();
                    var parseFailed = false;
                    foreach (var cp in correctParts)
                    {
                        if (int.TryParse(cp, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
                        {
                            if (!optionsDict.ContainsKey(v))
                            {
                                result.Errors.Add(new ImportResultDto.RowError { RowNumber = row, Message = $"Correct option number {v} does not exist among provided options." });
                                parseFailed = true;
                                break;
                            }
                            correctList.Add(v);
                        }
                        else
                        {
                            result.Errors.Add(new ImportResultDto.RowError { RowNumber = row, Message = $"Could not parse correct option value '{cp}' as integer." });
                            parseFailed = true;
                            break;
                        }
                    }
                    if (parseFailed) continue;

                    if (typeValue == "MCQ" && correctList.Count != 1)
                    {
                        result.Errors.Add(new ImportResultDto.RowError { RowNumber = row, Message = "MCQ must have exactly one correct option." });
                        continue;
                    }
                    if (typeValue == "MSQ" && correctList.Count == 0)
                    {
                        result.Errors.Add(new ImportResultDto.RowError { RowNumber = row, Message = "MSQ must have at least one correct option." });
                        continue;
                    }


                    // Prepare Question entity
                    var q = new Question
                    {

                        Tid = effectiveTid,
                        Eid = eid,
                        Type = typeValue,
                        Question1 = questionText,
                        // Marks left null unless you want to parse a marks column.
                        Marks = exam.MarksPerQuestion ?? 0,

                        // Options JSON: {"1":"Option A","2":"Option B",...}
                        Options = System.Text.Json.JsonSerializer.Serialize(optionsDict.ToDictionary(k => k.Key.ToString(), k => k.Value)),
                        // CorrectOptions as json array of ints
                        CorrectOptions = System.Text.Json.JsonSerializer.Serialize(correctList),
                        ApprovalStatus = exam.ApprovalStatus
                    };

                    questionsToAdd.Add(q);
                }
                catch (Exception exRow)
                {
                    // catch row-level parse errors and continue
                    result.Errors.Add(new ImportResultDto.RowError { RowNumber = row, Message = $"Unhandled error while processing row: {exRow.Message}" });
                    continue;
                }
            } // end rows loop

            // Save to DB in a transaction
            if (questionsToAdd.Any())
            {
                using var tx = await _context.Database.BeginTransactionAsync();
                try
                {
                    await _context.Questions.AddRangeAsync(questionsToAdd);
                    var saved = await _context.SaveChangesAsync();
                    await tx.CommitAsync();

                    result.Inserted = questionsToAdd.Count;
                }
                catch (Exception exSave)
                {
                    await tx.RollbackAsync();
                    result.Errors.Add(new ImportResultDto.RowError { RowNumber = 0, Message = $"Failed to save to DB: {exSave.Message}" });
                }
            }

            return result;
        }
    }
}
