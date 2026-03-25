using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Couture.Orders.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogModelId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CatalogModelId",
                schema: "orders",
                table: "orders",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CatalogModelId",
                schema: "orders",
                table: "orders");
        }
    }
}
