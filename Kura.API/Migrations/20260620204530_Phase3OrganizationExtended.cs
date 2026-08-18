using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kura.API.Migrations
{
    /// <inheritdoc />
    public partial class Phase3OrganizationExtended : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Specialty",
                table: "Organizations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalDevices",
                table: "Organizations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalDrugs",
                table: "Organizations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "OrganizationLabDoctors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationLabDoctors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationLabDoctors_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationPharmacists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationPharmacists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationPharmacists_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationSpecialties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationSpecialties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationSpecialties_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationLabDoctors_OrganizationId",
                table: "OrganizationLabDoctors",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationPharmacists_OrganizationId",
                table: "OrganizationPharmacists",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationSpecialties_OrganizationId",
                table: "OrganizationSpecialties",
                column: "OrganizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganizationLabDoctors");

            migrationBuilder.DropTable(
                name: "OrganizationPharmacists");

            migrationBuilder.DropTable(
                name: "OrganizationSpecialties");

            migrationBuilder.DropColumn(
                name: "Specialty",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "TotalDevices",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "TotalDrugs",
                table: "Organizations");
        }
    }
}
