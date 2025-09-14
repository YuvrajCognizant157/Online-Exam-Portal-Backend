using System;
using System.Collections.Generic;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Domain.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Exam> Exams { get; set; }

    public virtual DbSet<ExamFeedback> ExamFeedbacks { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<QuestionReport> QuestionReports { get; set; }

    public virtual DbSet<Response> Responses { get; set; }

    public virtual DbSet<Result> Results { get; set; }

    public virtual DbSet<Topic> Topics { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Validation> Validations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Data Source=LTIN617435;User ID=sa;Password=password-1;Initial Catalog=OEP_DB_V;Encrypt=false;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Table names (already set via [Table], but keep explicit if needed)
        modelBuilder.Entity<User>().ToTable("User");
        modelBuilder.Entity<Topic>().ToTable("Topics");
        modelBuilder.Entity<Exam>().ToTable("Exams");
        modelBuilder.Entity<Question>().ToTable("Questions");
        modelBuilder.Entity<Response>().ToTable("Responses");
        modelBuilder.Entity<Result>().ToTable("Results");
        modelBuilder.Entity<QuestionReport>().ToTable("QuestionReports");
        modelBuilder.Entity<ExamFeedback>().ToTable("ExamFeedbacks");
        modelBuilder.Entity<Validation>().ToTable("Validations");

        // Composite keys
        modelBuilder.Entity<Response>()
            .HasKey(r => new { r.EID, r.QID, r.UserId });

        modelBuilder.Entity<Result>()
            .HasKey(r => new { r.UserId, r.EID });

        modelBuilder.Entity<QuestionReport>()
            .HasKey(qr => new { qr.QID, qr.UserId });

        modelBuilder.Entity<ExamFeedback>()
            .HasKey(ef => new { ef.EID, ef.UserId });

        // Defaults for created/updated timestamps (match SQL GETDATE())
        modelBuilder.Entity<User>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETDATE()");
        modelBuilder.Entity<User>()
            .Property(u => u.UpdatedAt)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<Result>()
            .Property(r => r.CreatedAt)
            .HasDefaultValueSql("GETDATE()");
        modelBuilder.Entity<Result>()
            .Property(r => r.UpdatedAt)
            .HasDefaultValueSql("GETDATE()");

        // Relationships (some are optional by design)
        modelBuilder.Entity<Topic>()
            .HasOne(t => t.ApprovedByUser)
            .WithMany(u => u.TopicsApproved)
            .HasForeignKey(t => t.ApprovedByUserID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Exam>()
            .HasOne(e => e.ApprovedByUser)
            .WithMany(u => u.ExamsApproved)
            .HasForeignKey(e => e.ApprovedByUserID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Exam>()
            .HasOne(e => e.Creator)
            .WithMany(u => u.ExamsCreated)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Question>()
            .HasOne(q => q.Topic)
            .WithMany(t => t.Questions)
            .HasForeignKey(q => q.TID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Question>()
            .HasOne(q => q.Exam)
            .WithMany(e => e.Questions)
            .HasForeignKey(q => q.EID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Response>()
            .HasOne(r => r.Exam)
            .WithMany(e => e.Responses)
            .HasForeignKey(r => r.EID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Response>()
            .HasOne(r => r.Question)
            .WithMany(q => q.Responses)
            .HasForeignKey(r => r.QID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Response>()
            .HasOne(r => r.User)
            .WithMany(u => u.Responses)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Result>()
            .HasOne(r => r.User)
            .WithMany(u => u.Results)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Result>()
            .HasOne(r => r.Exam)
            .WithMany(e => e.Results)
            .HasForeignKey(r => r.EID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<QuestionReport>()
            .HasOne(qr => qr.Question)
            .WithMany(q => q.QuestionReports)
            .HasForeignKey(qr => qr.QID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<QuestionReport>()
            .HasOne(qr => qr.User)
            .WithMany(u => u.QuestionReports)
            .HasForeignKey(qr => qr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ExamFeedback>()
            .HasOne(ef => ef.Exam)
            .WithMany(e => e.ExamFeedbacks)
            .HasForeignKey(ef => ef.EID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ExamFeedback>()
            .HasOne(ef => ef.User)
            .WithMany(u => u.ExamFeedbacks)
            .HasForeignKey(ef => ef.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Validation.Token is primary key via DataAnnotation [Key]
        // Column types for decimal already set via attributes in model classes where appropriate
    }
}
