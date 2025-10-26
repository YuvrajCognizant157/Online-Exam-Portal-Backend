using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Infrastructure.DTOs.QuestionsDTO
{
    // Dtos/UploadQuestionsDto.cs

    public class UploadQuestionsDto
    {
        // The excel file
        public IFormFile File { get; set; } = null!;

        // Optional exam id (Eid)
        public int? Eid { get; set; }
    }

    public class ImportResultDto
    {
        public int TotalRows { get; set; }
        public int Inserted { get; set; }
        public List<RowError> Errors { get; set; } = new();

        public class RowError
        {
            public int RowNumber { get; set; }
            public string Message { get; set; } = string.Empty;
        }

    }

}
