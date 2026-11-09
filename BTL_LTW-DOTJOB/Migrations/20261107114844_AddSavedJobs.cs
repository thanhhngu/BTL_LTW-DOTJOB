using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_LTW_DOTJOB.Migrations
{
    /// <inheritdoc />
    public partial class AddSavedJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SavedJob",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    JobPostingId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SavedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedJob", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedJob_JobPosting_JobPostingId",
                        column: x => x.JobPostingId,
                        principalTable: "JobPosting",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavedJob_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SavedJob_JobPostingId",
                table: "SavedJob",
                column: "JobPostingId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedJob_UserId_JobPostingId",
                table: "SavedJob",
                columns: new[] { "UserId", "JobPostingId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SavedJob");
        }
    }
}
