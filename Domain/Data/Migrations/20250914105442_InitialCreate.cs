using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DOB = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PhoneNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsBlocked = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Validations",
                columns: table => new
                {
                    Token = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Validations", x => x.Token);
                });

            migrationBuilder.CreateTable(
                name: "Exams",
                columns: table => new
                {
                    EID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    TIDs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalQuestions = table.Column<int>(type: "int", nullable: true),
                    TotalMarks = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Duration = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: true),
                    ApprovedByUserID = table.Column<int>(type: "int", nullable: true),
                    DisplayedQuestions = table.Column<int>(type: "int", nullable: true),
                    AdminRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exams", x => x.EID);
                    table.ForeignKey(
                        name: "FK_Exams_User_ApprovedByUserID",
                        column: x => x.ApprovedByUserID,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Exams_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Topics",
                columns: table => new
                {
                    TID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Subject = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: true),
                    ApprovedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topics", x => x.TID);
                    table.ForeignKey(
                        name: "FK_Topics_User_ApprovedByUserID",
                        column: x => x.ApprovedByUserID,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamFeedbacks",
                columns: table => new
                {
                    EID = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Feedback = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamFeedbacks", x => new { x.EID, x.UserId });
                    table.ForeignKey(
                        name: "FK_ExamFeedbacks_Exams_EID",
                        column: x => x.EID,
                        principalTable: "Exams",
                        principalColumn: "EID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamFeedbacks_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EID = table.Column<int>(type: "int", nullable: false),
                    Attempts = table.Column<int>(type: "int", nullable: true),
                    Score = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Results", x => new { x.UserId, x.EID });
                    table.ForeignKey(
                        name: "FK_Results_Exams_EID",
                        column: x => x.EID,
                        principalTable: "Exams",
                        principalColumn: "EID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Results_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    QID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TID = table.Column<int>(type: "int", nullable: true),
                    EID = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Marks = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Options = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrectOptions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.QID);
                    table.ForeignKey(
                        name: "FK_Questions_Exams_EID",
                        column: x => x.EID,
                        principalTable: "Exams",
                        principalColumn: "EID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Questions_Topics_TID",
                        column: x => x.TID,
                        principalTable: "Topics",
                        principalColumn: "TID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuestionReports",
                columns: table => new
                {
                    QID = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Feedback = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionReports", x => new { x.QID, x.UserId });
                    table.ForeignKey(
                        name: "FK_QuestionReports_Questions_QID",
                        column: x => x.QID,
                        principalTable: "Questions",
                        principalColumn: "QID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionReports_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Responses",
                columns: table => new
                {
                    EID = table.Column<int>(type: "int", nullable: false),
                    QID = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Resp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Resp_Score = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    IsSubmittedFresh = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Responses", x => new { x.EID, x.QID, x.UserId });
                    table.ForeignKey(
                        name: "FK_Responses_Exams_EID",
                        column: x => x.EID,
                        principalTable: "Exams",
                        principalColumn: "EID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Responses_Questions_QID",
                        column: x => x.QID,
                        principalTable: "Questions",
                        principalColumn: "QID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Responses_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamFeedbacks_UserId",
                table: "ExamFeedbacks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_ApprovedByUserID",
                table: "Exams",
                column: "ApprovedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_UserId",
                table: "Exams",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionReports_UserId",
                table: "QuestionReports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_EID",
                table: "Questions",
                column: "EID");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_TID",
                table: "Questions",
                column: "TID");

            migrationBuilder.CreateIndex(
                name: "IX_Responses_QID",
                table: "Responses",
                column: "QID");

            migrationBuilder.CreateIndex(
                name: "IX_Responses_UserId",
                table: "Responses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Results_EID",
                table: "Results",
                column: "EID");

            migrationBuilder.CreateIndex(
                name: "IX_Topics_ApprovedByUserID",
                table: "Topics",
                column: "ApprovedByUserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamFeedbacks");

            migrationBuilder.DropTable(
                name: "QuestionReports");

            migrationBuilder.DropTable(
                name: "Responses");

            migrationBuilder.DropTable(
                name: "Results");

            migrationBuilder.DropTable(
                name: "Validations");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "Exams");

            migrationBuilder.DropTable(
                name: "Topics");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
