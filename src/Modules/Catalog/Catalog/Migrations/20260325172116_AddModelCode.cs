using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Couture.Catalog.Migrations
{
    /// <inheritdoc />
    public partial class AddModelCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                schema: "catalog",
                table: "models",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_models_Code",
                schema: "catalog",
                table: "models",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_models_Code",
                schema: "catalog",
                table: "models");

            migrationBuilder.DropColumn(
                name: "Code",
                schema: "catalog",
                table: "models");
        }
    }
}
