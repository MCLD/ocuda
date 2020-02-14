using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ocuda.Ops.DataProvider.SqlServer.Ops.Migrations
{
    public partial class ops_v100 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FriendlyName = table.Column<string>(nullable: true),
                    Xml = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<int>(nullable: true),
                    Username = table.Column<string>(maxLength: 255, nullable: true),
                    IsSysadmin = table.Column<bool>(nullable: false),
                    LastLdapCheck = table.Column<DateTime>(nullable: true),
                    LastLdapUpdate = table.Column<DateTime>(nullable: true),
                    LastRosterUpdate = table.Column<DateTime>(nullable: true),
                    ReauthenticateUser = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: true),
                    Nickname = table.Column<string>(maxLength: 255, nullable: true),
                    Title = table.Column<string>(maxLength: 255, nullable: true),
                    Email = table.Column<string>(maxLength: 255, nullable: true),
                    Phone = table.Column<string>(maxLength: 255, nullable: true),
                    EmployeeId = table.Column<int>(nullable: true),
                    ServiceStartDate = table.Column<DateTime>(nullable: true),
                    SupervisorId = table.Column<int>(nullable: true),
                    LastSeen = table.Column<DateTime>(nullable: true),
                    IsInLatestRoster = table.Column<bool>(nullable: true),
                    ExcludeFromRoster = table.Column<bool>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Users_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<int>(nullable: true),
                    Name = table.Column<string>(maxLength: 255, nullable: true),
                    Stub = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Categories_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClaimGroups",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<int>(nullable: true),
                    ClaimType = table.Column<string>(maxLength: 255, nullable: false),
                    GroupName = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClaimGroups_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClaimGroups_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoverIssueHeaders",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<int>(nullable: true),
                    BibId = table.Column<int>(nullable: false),
                    HasPendingIssue = table.Column<bool>(nullable: false),
                    LastResolved = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoverIssueHeaders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoverIssueHeaders_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoverIssueHeaders_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExternalResources",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<int>(nullable: true),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Url = table.Column<string>(maxLength: 255, nullable: false),
                    SortOrder = table.Column<int>(nullable: false),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalResources_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExternalResources_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FileLibraries",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<int>(nullable: true),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Stub = table.Column<string>(maxLength: 255, nullable: true),
                    SectionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileLibraries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileLibraries_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FileLibraries_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FileTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<int>(nullable: true),
                    Extension = table.Column<string>(maxLength: 16, nullable: false),
                    Icon = table.Column<string>(maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileTypes_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FileTypes_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LinkLibraries",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<int>(nullable: true),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    IsNavigation = table.Column<bool>(nullable: false),
                    Stub = table.Column<string>(maxLength: 255, nullable: true),
                    SectionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkLibraries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LinkLibraries_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LinkLibraries_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<int>(nullable: true),
                    SectionId = table.Column<int>(nullable: false),
                    Title = table.Column<string>(maxLength: 255, nullable: false),
                    Stub = table.Column<string>(maxLength: 255, nullable: false),
                    Content = table.Column<string>(nullable: false),
                    ShowOnHomePage = table.Column<bool>(nullable: false),
                    PublishedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Posts_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Posts_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RosterHeaders",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RosterHeaders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RosterHeaders_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RosterHeaders_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sections",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<int>(nullable: true),
                    Name = table.Column<string>(maxLength: 255, nullable: true),
                    EmbedVideoUrl = table.Column<string>(maxLength: 255, nullable: true),
                    Icon = table.Column<string>(maxLength: 255, nullable: true),
                    Stub = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sections_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sections_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<int>(nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Description = table.Column<string>(maxLength: 255, nullable: false),
                    Value = table.Column<string>(maxLength: 255, nullable: true),
                    Category = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SiteSettings_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SiteSettings_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserMetadataTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<int>(nullable: true),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    IsPublic = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMetadataTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMetadataTypes_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserMetadataTypes_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoverIssueDetails",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<int>(nullable: true),
                    CoverIssueHeaderId = table.Column<int>(nullable: false),
                    IsResolved = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoverIssueDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoverIssueDetails_CoverIssueHeaders_CoverIssueHeaderId",
                        column: x => x.CoverIssueHeaderId,
                        principalTable: "CoverIssueHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoverIssueDetails_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoverIssueDetails_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FileLibraryFileTypes",
                columns: table => new
                {
                    FileLibraryId = table.Column<int>(nullable: false),
                    FileTypeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileLibraryFileTypes", x => new { x.FileLibraryId, x.FileTypeId });
                    table.ForeignKey(
                        name: "FK_FileLibraryFileTypes_FileLibraries_FileLibraryId",
                        column: x => x.FileLibraryId,
                        principalTable: "FileLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FileLibraryFileTypes_FileTypes_FileTypeId",
                        column: x => x.FileTypeId,
                        principalTable: "FileTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<int>(nullable: true),
                    FileLibraryId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Description = table.Column<string>(maxLength: 255, nullable: true),
                    FileTypeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Files_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Files_FileLibraries_FileLibraryId",
                        column: x => x.FileLibraryId,
                        principalTable: "FileLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Files_FileTypes_FileTypeId",
                        column: x => x.FileTypeId,
                        principalTable: "FileTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Files_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Links",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<int>(nullable: true),
                    LinkLibraryId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Url = table.Column<string>(maxLength: 255, nullable: false),
                    Icon = table.Column<string>(maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Links", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Links_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Links_LinkLibraries_LinkLibraryId",
                        column: x => x.LinkLibraryId,
                        principalTable: "LinkLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Links_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PostCategories",
                columns: table => new
                {
                    PostId = table.Column<int>(nullable: false),
                    CategoryId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostCategories", x => new { x.PostId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_PostCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PostCategories_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RosterDetails",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<int>(nullable: true),
                    RosterHeaderId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    EmailAddress = table.Column<string>(maxLength: 255, nullable: true),
                    JobTitle = table.Column<string>(maxLength: 255, nullable: true),
                    EmployeeId = table.Column<int>(nullable: false),
                    PositionNum = table.Column<int>(nullable: false),
                    HireDate = table.Column<DateTime>(nullable: true),
                    RehireDate = table.Column<DateTime>(nullable: true),
                    ReportsToId = table.Column<int>(nullable: true),
                    ReportsToPos = table.Column<int>(nullable: true),
                    AsOf = table.Column<DateTime>(nullable: false),
                    IsVacant = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RosterDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RosterDetails_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RosterDetails_RosterHeaders_RosterHeaderId",
                        column: x => x.RosterHeaderId,
                        principalTable: "RosterHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RosterDetails_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SectionCategories",
                columns: table => new
                {
                    SectionId = table.Column<int>(nullable: false),
                    CategoryId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectionCategories", x => new { x.SectionId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_SectionCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SectionCategories_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SectionManagerGroups",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<int>(nullable: true),
                    SectionId = table.Column<int>(nullable: false),
                    GroupName = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectionManagerGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SectionManagerGroups_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SectionManagerGroups_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SectionManagerGroups_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserMetadata",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    UserMetadataTypeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMetadata", x => new { x.UserId, x.UserMetadataTypeId });
                    table.ForeignKey(
                        name: "FK_UserMetadata_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserMetadata_UserMetadataTypes_UserMetadataTypeId",
                        column: x => x.UserMetadataTypeId,
                        principalTable: "UserMetadataTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CreatedBy",
                table: "Categories",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_UpdatedBy",
                table: "Categories",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimGroups_CreatedBy",
                table: "ClaimGroups",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimGroups_UpdatedBy",
                table: "ClaimGroups",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CoverIssueDetails_CoverIssueHeaderId",
                table: "CoverIssueDetails",
                column: "CoverIssueHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_CoverIssueDetails_CreatedBy",
                table: "CoverIssueDetails",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CoverIssueDetails_UpdatedBy",
                table: "CoverIssueDetails",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CoverIssueHeaders_CreatedBy",
                table: "CoverIssueHeaders",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CoverIssueHeaders_UpdatedBy",
                table: "CoverIssueHeaders",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalResources_CreatedBy",
                table: "ExternalResources",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalResources_UpdatedBy",
                table: "ExternalResources",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_FileLibraries_CreatedBy",
                table: "FileLibraries",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_FileLibraries_UpdatedBy",
                table: "FileLibraries",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_FileLibraryFileTypes_FileTypeId",
                table: "FileLibraryFileTypes",
                column: "FileTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_CreatedBy",
                table: "Files",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Files_FileLibraryId",
                table: "Files",
                column: "FileLibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_FileTypeId",
                table: "Files",
                column: "FileTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_UpdatedBy",
                table: "Files",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_FileTypes_CreatedBy",
                table: "FileTypes",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_FileTypes_UpdatedBy",
                table: "FileTypes",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_LinkLibraries_CreatedBy",
                table: "LinkLibraries",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_LinkLibraries_UpdatedBy",
                table: "LinkLibraries",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Links_CreatedBy",
                table: "Links",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Links_LinkLibraryId",
                table: "Links",
                column: "LinkLibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_Links_UpdatedBy",
                table: "Links",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PostCategories_CategoryId",
                table: "PostCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_CreatedBy",
                table: "Posts",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_UpdatedBy",
                table: "Posts",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RosterDetails_CreatedBy",
                table: "RosterDetails",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RosterDetails_RosterHeaderId",
                table: "RosterDetails",
                column: "RosterHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_RosterDetails_UpdatedBy",
                table: "RosterDetails",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RosterHeaders_CreatedBy",
                table: "RosterHeaders",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RosterHeaders_UpdatedBy",
                table: "RosterHeaders",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SectionCategories_CategoryId",
                table: "SectionCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SectionManagerGroups_CreatedBy",
                table: "SectionManagerGroups",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SectionManagerGroups_SectionId",
                table: "SectionManagerGroups",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_SectionManagerGroups_UpdatedBy",
                table: "SectionManagerGroups",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_CreatedBy",
                table: "Sections",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_UpdatedBy",
                table: "Sections",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SiteSettings_CreatedBy",
                table: "SiteSettings",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SiteSettings_UpdatedBy",
                table: "SiteSettings",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserMetadata_UserMetadataTypeId",
                table: "UserMetadata",
                column: "UserMetadataTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMetadataTypes_CreatedBy",
                table: "UserMetadataTypes",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserMetadataTypes_UpdatedBy",
                table: "UserMetadataTypes",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Users_SupervisorId",
                table: "Users",
                column: "SupervisorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClaimGroups");

            migrationBuilder.DropTable(
                name: "CoverIssueDetails");

            migrationBuilder.DropTable(
                name: "DataProtectionKeys");

            migrationBuilder.DropTable(
                name: "ExternalResources");

            migrationBuilder.DropTable(
                name: "FileLibraryFileTypes");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "Links");

            migrationBuilder.DropTable(
                name: "PostCategories");

            migrationBuilder.DropTable(
                name: "RosterDetails");

            migrationBuilder.DropTable(
                name: "SectionCategories");

            migrationBuilder.DropTable(
                name: "SectionManagerGroups");

            migrationBuilder.DropTable(
                name: "SiteSettings");

            migrationBuilder.DropTable(
                name: "UserMetadata");

            migrationBuilder.DropTable(
                name: "CoverIssueHeaders");

            migrationBuilder.DropTable(
                name: "FileLibraries");

            migrationBuilder.DropTable(
                name: "FileTypes");

            migrationBuilder.DropTable(
                name: "LinkLibraries");

            migrationBuilder.DropTable(
                name: "Posts");

            migrationBuilder.DropTable(
                name: "RosterHeaders");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Sections");

            migrationBuilder.DropTable(
                name: "UserMetadataTypes");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
