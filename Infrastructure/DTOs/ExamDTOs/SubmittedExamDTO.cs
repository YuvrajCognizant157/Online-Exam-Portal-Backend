namespace Infrastructure.DTOs.ExamDTOs
{
    public class SubmittedExamDTO
    {
        public int EID { get; set; }
        public int UserId { get; set; }
        //public int? DisplayedQuestions { get; set; }
        public virtual List<ReceivedResponseDTO> Responses { get; set; } = new List<ReceivedResponseDTO>();
    }
}