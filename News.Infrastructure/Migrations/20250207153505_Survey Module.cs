using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace News.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SurveyModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          
            migrationBuilder.CreateTable(
                name: "Surveys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceDiscovery = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VisitFrequency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsLoadingSpeedSatisfactory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NavigationEaseRating = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApplicationUserId = table.Column<int>(type: "int", nullable: false),
                    ApplicationUserId1 = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Surveys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Surveys_AspNetUsers_ApplicationUserId1",
                        column: x => x.ApplicationUserId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });
           
            migrationBuilder.CreateIndex(
                name: "IX_Surveys_ApplicationUserId1",
                table: "Surveys",
                column: "ApplicationUserId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Surveys");        
        }
    }
}
