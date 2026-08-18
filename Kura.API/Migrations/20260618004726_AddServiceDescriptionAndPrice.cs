using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kura.API.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceDescriptionAndPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "DoctorServices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "DoctorServices",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "DoctorServices");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "DoctorServices");
        }
    }
}
