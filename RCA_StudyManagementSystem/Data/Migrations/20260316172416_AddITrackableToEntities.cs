using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RCA_StudyManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddITrackableToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "ReimbursementEntityRCAContact",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedUserId",
                table: "ReimbursementEntityRCAContact",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "ReimbursementEntityRCAContact",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedUserId",
                table: "ReimbursementEntityRCAContact",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "ReimbursementEntity",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedUserId",
                table: "ReimbursementEntity",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "ReimbursementEntity",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedUserId",
                table: "ReimbursementEntity",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "RCAContact",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedUserId",
                table: "RCAContact",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "RCAContact",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedUserId",
                table: "RCAContact",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "PathReportExport",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedUserId",
                table: "PathReportExport",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "PathReportExport",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedUserId",
                table: "PathReportExport",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "ReimbursementEntityRCAContact");

            migrationBuilder.DropColumn(
                name: "CreatedUserId",
                table: "ReimbursementEntityRCAContact");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "ReimbursementEntityRCAContact");

            migrationBuilder.DropColumn(
                name: "ModifiedUserId",
                table: "ReimbursementEntityRCAContact");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "ReimbursementEntity");

            migrationBuilder.DropColumn(
                name: "CreatedUserId",
                table: "ReimbursementEntity");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "ReimbursementEntity");

            migrationBuilder.DropColumn(
                name: "ModifiedUserId",
                table: "ReimbursementEntity");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "RCAContact");

            migrationBuilder.DropColumn(
                name: "CreatedUserId",
                table: "RCAContact");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "RCAContact");

            migrationBuilder.DropColumn(
                name: "ModifiedUserId",
                table: "RCAContact");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "PathReportExport");

            migrationBuilder.DropColumn(
                name: "CreatedUserId",
                table: "PathReportExport");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "PathReportExport");

            migrationBuilder.DropColumn(
                name: "ModifiedUserId",
                table: "PathReportExport");
        }
    }
}
