using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_LTW_DOTJOB.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Application_JobPostingId",
                table: "Application");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Application_JobPostingId_UserId",
                table: "Application",
                columns: new[] { "JobPostingId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_Email",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_Application_JobPostingId_UserId",
                table: "Application");

            migrationBuilder.CreateIndex(
                name: "IX_Application_JobPostingId",
                table: "Application",
                column: "JobPostingId");
        }
    }
}
