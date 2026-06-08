using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CLIPWEB.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSurveyFieldsToEditorProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CanSelfPublish",
                table: "EditorProfiles",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClipsPerWeek",
                table: "EditorProfiles",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPreference",
                table: "EditorProfiles",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentNiche",
                table: "EditorProfiles",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EditingSoftware",
                table: "EditorProfiles",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExperienceLevel",
                table: "EditorProfiles",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PortfolioUrl",
                table: "EditorProfiles",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SurveyCompletedAtUtc",
                table: "EditorProfiles",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanSelfPublish",
                table: "EditorProfiles");

            migrationBuilder.DropColumn(
                name: "ClipsPerWeek",
                table: "EditorProfiles");

            migrationBuilder.DropColumn(
                name: "ContactPreference",
                table: "EditorProfiles");

            migrationBuilder.DropColumn(
                name: "ContentNiche",
                table: "EditorProfiles");

            migrationBuilder.DropColumn(
                name: "EditingSoftware",
                table: "EditorProfiles");

            migrationBuilder.DropColumn(
                name: "ExperienceLevel",
                table: "EditorProfiles");

            migrationBuilder.DropColumn(
                name: "PortfolioUrl",
                table: "EditorProfiles");

            migrationBuilder.DropColumn(
                name: "SurveyCompletedAtUtc",
                table: "EditorProfiles");
        }
    }
}
