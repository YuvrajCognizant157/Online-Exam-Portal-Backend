using System;
using System.Collections.Generic;

namespace Domain.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? Email { get; set; }

    public string? FullName { get; set; }

    public string? Password { get; set; }

    public DateOnly? Dob { get; set; }

    public string? PhoneNo { get; set; }

    public DateOnly? RegistrationDate { get; set; }

    public string? Role { get; set; }

    public bool? IsBlocked { get; set; } = false;

    public int? Otp { get; set; }

    public DateTime? CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; set; }


    public virtual ICollection<ExamFeedback> ExamFeedbacks { get; set; } = new List<ExamFeedback>();

    public virtual ICollection<Exam> ExamUsers { get; set; } = new List<Exam>();

    public virtual ICollection<QuestionReport> QuestionReports { get; set; } = new List<QuestionReport>();

    public virtual ICollection<Response> Responses { get; set; } = new List<Response>();

    public virtual ICollection<Result> Results { get; set; } = new List<Result>();

    public virtual ICollection<Topic> ApprovedTopics { get; set; } = new List<Topic>();
    public virtual ICollection<Topic> CreatedTopics { get; set; } = new List<Topic>();
    public virtual ICollection<Exam> ExamReviewers { get; set; } = new List<Exam>();
    public virtual ICollection<QuestionReport> ReviewedQuestionReports { get; set; } = new List<QuestionReport>();
}
