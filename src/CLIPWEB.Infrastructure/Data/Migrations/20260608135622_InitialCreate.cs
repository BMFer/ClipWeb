using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CLIPWEB.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    WebsiteUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ContactEmail = table.Column<string>(type: "TEXT", maxLength: 320, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EditorProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DiscordUserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    DiscordUsername = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PreferredName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    TimeZone = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PrimaryPlatform = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SurveyCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EditorProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BrandId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    SourceContentUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    StyleGuideUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    StartDateUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDateUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Campaigns_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClipSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CampaignId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EditorProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClipUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SubmittedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClipSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClipSubmissions_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClipSubmissions_EditorProfiles_EditorProfileId",
                        column: x => x.EditorProfileId,
                        principalTable: "EditorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PublishedPosts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClipSubmissionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Platform = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PostUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Views = table.Column<long>(type: "INTEGER", nullable: false),
                    Likes = table.Column<long>(type: "INTEGER", nullable: true),
                    Comments = table.Column<long>(type: "INTEGER", nullable: true),
                    Shares = table.Column<long>(type: "INTEGER", nullable: true),
                    PostedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublishedPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublishedPosts_ClipSubmissions_ClipSubmissionId",
                        column: x => x.ClipSubmissionId,
                        principalTable: "ClipSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_BrandId",
                table: "Campaigns",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_IsActive",
                table: "Campaigns",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ClipSubmissions_CampaignId",
                table: "ClipSubmissions",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_ClipSubmissions_EditorProfileId",
                table: "ClipSubmissions",
                column: "EditorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ClipSubmissions_Status",
                table: "ClipSubmissions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EditorProfiles_DiscordUserId",
                table: "EditorProfiles",
                column: "DiscordUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PublishedPosts_ClipSubmissionId",
                table: "PublishedPosts",
                column: "ClipSubmissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PublishedPosts");

            migrationBuilder.DropTable(
                name: "ClipSubmissions");

            migrationBuilder.DropTable(
                name: "Campaigns");

            migrationBuilder.DropTable(
                name: "EditorProfiles");

            migrationBuilder.DropTable(
                name: "Brands");
        }
    }
}
