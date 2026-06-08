using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CLIPWEB.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewFeedbackToClipSubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAtUtc",
                table: "ClipSubmissions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "ReviewedByDiscordUserId",
                table: "ClipSubmissions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewerNote",
                table: "ClipSubmissions",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewedAtUtc",
                table: "ClipSubmissions");

            migrationBuilder.DropColumn(
                name: "ReviewedByDiscordUserId",
                table: "ClipSubmissions");

            migrationBuilder.DropColumn(
                name: "ReviewerNote",
                table: "ClipSubmissions");
        }
    }
}
