using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    public partial class prom_v100 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Class = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Emedia",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Stub = table.Column<string>(maxLength: 255, nullable: false),
                    RedirectUrl = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emedia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExternalResources",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Url = table.Column<string>(maxLength: 255, nullable: false),
                    SortOrder = table.Column<int>(nullable: false),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalResources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Features",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Icon = table.Column<string>(maxLength: 48, nullable: false),
                    ImagePath = table.Column<string>(maxLength: 255, nullable: true),
                    Stub = table.Column<string>(maxLength: 80, nullable: true),
                    BodyText = table.Column<string>(maxLength: 2000, nullable: true),
                    IconText = table.Column<string>(maxLength: 5, nullable: true),
                    SortOrder = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Features", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stub = table.Column<string>(maxLength: 255, nullable: false),
                    GroupType = table.Column<string>(maxLength: 255, nullable: true),
                    IsLocationRegion = table.Column<bool>(nullable: false),
                    SubscriptionUrl = table.Column<string>(maxLength: 255, nullable: true),
                    MapImage = table.Column<string>(maxLength: 255, nullable: true),
                    Description = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    IsDefault = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Navigations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChangeToLinkWhenExtraSmall = table.Column<bool>(nullable: false),
                    HideTextWhenExtraSmall = table.Column<bool>(nullable: false),
                    TargetNewWindow = table.Column<bool>(nullable: false),
                    Order = table.Column<int>(nullable: false),
                    Icon = table.Column<string>(maxLength: 255, nullable: true),
                    Link = table.Column<string>(maxLength: 255, nullable: true),
                    Name = table.Column<string>(maxLength: 255, nullable: true),
                    NavigationTextId = table.Column<int>(nullable: true),
                    NavigationId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Navigations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Navigations_Navigations_NavigationId",
                        column: x => x.NavigationId,
                        principalTable: "Navigations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NavigationTexts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    LanguageId = table.Column<int>(nullable: false),
                    AriaLabel = table.Column<string>(maxLength: 255, nullable: true),
                    Label = table.Column<string>(maxLength: 255, nullable: true),
                    Title = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NavigationTexts", x => new { x.Id, x.LanguageId });
                });

            migrationBuilder.CreateTable(
                name: "PageHeaders",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PageName = table.Column<string>(maxLength: 255, nullable: false),
                    Stub = table.Column<string>(maxLength: 255, nullable: false),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageHeaders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Segments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<DateTime>(nullable: true),
                    EndDate = table.Column<DateTime>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Segments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 255, nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Description = table.Column<string>(maxLength: 255, nullable: false),
                    Value = table.Column<string>(maxLength: 255, nullable: true),
                    Category = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SocialCards",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(maxLength: 70, nullable: false),
                    Description = table.Column<string>(maxLength: 200, nullable: false),
                    Image = table.Column<string>(maxLength: 255, nullable: false),
                    ImageAlt = table.Column<string>(maxLength: 420, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialCards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UrlRedirects",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsActive = table.Column<bool>(nullable: false),
                    IsPermanent = table.Column<bool>(nullable: false),
                    Description = table.Column<string>(maxLength: 255, nullable: true),
                    RequestPath = table.Column<string>(maxLength: 255, nullable: true),
                    Url = table.Column<string>(maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrlRedirects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmediaCategories",
                columns: table => new
                {
                    EmediaId = table.Column<int>(nullable: false),
                    CategoryId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmediaCategories", x => new { x.CategoryId, x.EmediaId });
                    table.ForeignKey(
                        name: "FK_EmediaCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmediaCategories_Emedia_EmediaId",
                        column: x => x.EmediaId,
                        principalTable: "Emedia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CategoryTexts",
                columns: table => new
                {
                    CategoryId = table.Column<int>(nullable: false),
                    LanguageId = table.Column<int>(nullable: false),
                    Text = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryTexts", x => new { x.CategoryId, x.LanguageId });
                    table.ForeignKey(
                        name: "FK_CategoryTexts_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CategoryTexts_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmediaTexts",
                columns: table => new
                {
                    EmediaId = table.Column<int>(nullable: false),
                    LanguageId = table.Column<int>(nullable: false),
                    Description = table.Column<string>(maxLength: 255, nullable: false),
                    Details = table.Column<string>(maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmediaTexts", x => new { x.EmediaId, x.LanguageId });
                    table.ForeignKey(
                        name: "FK_EmediaTexts_Emedia_EmediaId",
                        column: x => x.EmediaId,
                        principalTable: "Emedia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmediaTexts_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SegmentTexts",
                columns: table => new
                {
                    SegmentId = table.Column<int>(nullable: false),
                    LanguageId = table.Column<int>(nullable: false),
                    Header = table.Column<string>(maxLength: 255, nullable: true),
                    Text = table.Column<string>(maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentTexts", x => new { x.SegmentId, x.LanguageId });
                    table.ForeignKey(
                        name: "FK_SegmentTexts_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SegmentTexts_Segments_SegmentId",
                        column: x => x.SegmentId,
                        principalTable: "Segments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Code = table.Column<string>(maxLength: 5, nullable: true),
                    ImagePath = table.Column<string>(maxLength: 255, nullable: true),
                    Stub = table.Column<string>(maxLength: 80, nullable: false),
                    MapLink = table.Column<string>(maxLength: 255, nullable: true),
                    MapImagePath = table.Column<string>(maxLength: 100, nullable: true),
                    Address = table.Column<string>(maxLength: 100, nullable: true),
                    City = table.Column<string>(maxLength: 50, nullable: true),
                    Zip = table.Column<string>(maxLength: 10, nullable: true),
                    Phone = table.Column<string>(maxLength: 15, nullable: true),
                    DisplayGroupId = table.Column<int>(nullable: true),
                    DescriptionSegmentId = table.Column<int>(nullable: false),
                    PostFeatureSegmentId = table.Column<int>(nullable: true),
                    PreFeatureSegmentId = table.Column<int>(nullable: true),
                    Facebook = table.Column<string>(maxLength: 50, nullable: true),
                    SubscriptionLink = table.Column<string>(maxLength: 100, nullable: true),
                    EventLink = table.Column<string>(maxLength: 255, nullable: true),
                    HasEvents = table.Column<bool>(nullable: false),
                    AdministrativeArea = table.Column<string>(maxLength: 50, nullable: true),
                    Country = table.Column<string>(maxLength: 50, nullable: true),
                    State = table.Column<string>(maxLength: 50, nullable: true),
                    Type = table.Column<string>(maxLength: 50, nullable: true),
                    Email = table.Column<string>(maxLength: 50, nullable: true),
                    AreaServedName = table.Column<string>(maxLength: 50, nullable: true),
                    AreaServedType = table.Column<string>(maxLength: 50, nullable: true),
                    AddressType = table.Column<string>(maxLength: 50, nullable: true),
                    ContactType = table.Column<string>(maxLength: 50, nullable: true),
                    ParentOrganization = table.Column<string>(maxLength: 50, nullable: true),
                    IsAccessibleForFree = table.Column<bool>(nullable: false),
                    GeoLocation = table.Column<string>(maxLength: 255, nullable: true),
                    PAbbreviation = table.Column<string>(maxLength: 50, nullable: true),
                    PriceRange = table.Column<string>(maxLength: 50, nullable: true),
                    IsAlwaysOpen = table.Column<bool>(nullable: false),
                    LocatorName = table.Column<string>(maxLength: 50, nullable: true),
                    LocatorNotes = table.Column<string>(maxLength: 50, nullable: true),
                    SocialCardId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Locations_Groups_DisplayGroupId",
                        column: x => x.DisplayGroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Locations_SocialCards_SocialCardId",
                        column: x => x.SocialCardId,
                        principalTable: "SocialCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pages",
                columns: table => new
                {
                    LanguageId = table.Column<int>(nullable: false),
                    PageHeaderId = table.Column<int>(nullable: false),
                    Title = table.Column<string>(maxLength: 255, nullable: false),
                    Content = table.Column<string>(nullable: true),
                    IsPublished = table.Column<bool>(nullable: false),
                    SocialCardId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pages", x => new { x.LanguageId, x.PageHeaderId });
                    table.ForeignKey(
                        name: "FK_Pages_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pages_PageHeaders_PageHeaderId",
                        column: x => x.PageHeaderId,
                        principalTable: "PageHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pages_SocialCards_SocialCardId",
                        column: x => x.SocialCardId,
                        principalTable: "SocialCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UrlRedirectAccesses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UrlRedirectId = table.Column<int>(nullable: false),
                    AccessDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrlRedirectAccesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UrlRedirectAccesses_UrlRedirects_UrlRedirectId",
                        column: x => x.UrlRedirectId,
                        principalTable: "UrlRedirects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LocationFeatures",
                columns: table => new
                {
                    LocationId = table.Column<int>(nullable: false),
                    FeatureId = table.Column<int>(nullable: false),
                    Text = table.Column<string>(nullable: true),
                    RedirectUrl = table.Column<string>(maxLength: 255, nullable: true),
                    NewTab = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationFeatures", x => new { x.FeatureId, x.LocationId });
                    table.ForeignKey(
                        name: "FK_LocationFeatures_Features_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "Features",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LocationFeatures_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LocationGroups",
                columns: table => new
                {
                    LocationId = table.Column<int>(nullable: false),
                    GroupId = table.Column<int>(nullable: false),
                    HasSubscription = table.Column<bool>(nullable: false),
                    DisplayOrder = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationGroups", x => new { x.GroupId, x.LocationId });
                    table.ForeignKey(
                        name: "FK_LocationGroups_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LocationGroups_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LocationHours",
                columns: table => new
                {
                    LocationId = table.Column<int>(nullable: false),
                    DayOfWeek = table.Column<int>(nullable: false),
                    Open = table.Column<bool>(nullable: false),
                    OpenTime = table.Column<DateTime>(nullable: true),
                    CloseTime = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationHours", x => new { x.DayOfWeek, x.LocationId });
                    table.ForeignKey(
                        name: "FK_LocationHours_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LocationHoursOverrides",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationId = table.Column<int>(nullable: true),
                    Reason = table.Column<string>(maxLength: 255, nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Open = table.Column<bool>(nullable: false),
                    OpenTime = table.Column<DateTime>(nullable: true),
                    CloseTime = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationHoursOverrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationHoursOverrides_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoryTexts_LanguageId",
                table: "CategoryTexts",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_EmediaCategories_EmediaId",
                table: "EmediaCategories",
                column: "EmediaId");

            migrationBuilder.CreateIndex(
                name: "IX_EmediaTexts_LanguageId",
                table: "EmediaTexts",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationFeatures_LocationId",
                table: "LocationFeatures",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationGroups_LocationId",
                table: "LocationGroups",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationHours_LocationId",
                table: "LocationHours",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationHoursOverrides_LocationId",
                table: "LocationHoursOverrides",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_DisplayGroupId",
                table: "Locations",
                column: "DisplayGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_SocialCardId",
                table: "Locations",
                column: "SocialCardId");

            migrationBuilder.CreateIndex(
                name: "IX_Navigations_NavigationId",
                table: "Navigations",
                column: "NavigationId");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_PageHeaderId",
                table: "Pages",
                column: "PageHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_SocialCardId",
                table: "Pages",
                column: "SocialCardId");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentTexts_LanguageId",
                table: "SegmentTexts",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentTexts_SegmentId",
                table: "SegmentTexts",
                column: "SegmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UrlRedirectAccesses_UrlRedirectId",
                table: "UrlRedirectAccesses",
                column: "UrlRedirectId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoryTexts");

            migrationBuilder.DropTable(
                name: "EmediaCategories");

            migrationBuilder.DropTable(
                name: "EmediaTexts");

            migrationBuilder.DropTable(
                name: "ExternalResources");

            migrationBuilder.DropTable(
                name: "LocationFeatures");

            migrationBuilder.DropTable(
                name: "LocationGroups");

            migrationBuilder.DropTable(
                name: "LocationHours");

            migrationBuilder.DropTable(
                name: "LocationHoursOverrides");

            migrationBuilder.DropTable(
                name: "Navigations");

            migrationBuilder.DropTable(
                name: "NavigationTexts");

            migrationBuilder.DropTable(
                name: "Pages");

            migrationBuilder.DropTable(
                name: "SegmentTexts");

            migrationBuilder.DropTable(
                name: "SiteSettings");

            migrationBuilder.DropTable(
                name: "UrlRedirectAccesses");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Emedia");

            migrationBuilder.DropTable(
                name: "Features");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "PageHeaders");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropTable(
                name: "Segments");

            migrationBuilder.DropTable(
                name: "UrlRedirects");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "SocialCards");
        }
    }
}
