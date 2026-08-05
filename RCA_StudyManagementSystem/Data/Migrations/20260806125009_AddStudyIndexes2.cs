using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RCA_StudyManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddStudyIndexes2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudyHistology_StudyId",
                table: "StudyHistology");

            migrationBuilder.CreateIndex(
                name: "IX_StudyLookup_StudyId_IsActive",
                table: "StudyLookup",
                columns: new[] { "StudyId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_StudyHistology_StudyId_IsActive",
                table: "StudyHistology",
                columns: new[] { "StudyId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudyLookup_StudyId_IsActive",
                table: "StudyLookup");

            migrationBuilder.DropIndex(
                name: "IX_StudyHistology_StudyId_IsActive",
                table: "StudyHistology");

            migrationBuilder.CreateIndex(
                name: "IX_StudyHistology_StudyId",
                table: "StudyHistology",
                column: "StudyId");
        }
    }
}
