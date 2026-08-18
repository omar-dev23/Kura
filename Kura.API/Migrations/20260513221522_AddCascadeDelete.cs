using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kura.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MedicalProfiles_PatientId",
                table: "MedicalProfiles");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalProfiles_PatientId",
                table: "MedicalProfiles",
                column: "PatientId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MedicalProfiles_PatientId",
                table: "MedicalProfiles");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalProfiles_PatientId",
                table: "MedicalProfiles",
                column: "PatientId");
        }
    }
}
