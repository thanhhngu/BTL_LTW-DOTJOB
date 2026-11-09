using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_LTW_DOTJOB.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminAndCategoryModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobPosting_Category_CategoryId",
                table: "JobPosting");

            migrationBuilder.CreateIndex(
                name: "IX_Category_Name",
                table: "Category",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_JobPosting_Category_CategoryId",
                table: "JobPosting",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobPosting_Category_CategoryId",
                table: "JobPosting");

            migrationBuilder.DropIndex(
                name: "IX_Category_Name",
                table: "Category");

            migrationBuilder.AddForeignKey(
                name: "FK_JobPosting_Category_CategoryId",
                table: "JobPosting",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
