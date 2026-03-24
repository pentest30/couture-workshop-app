using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Couture.Orders.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "orders");

            migrationBuilder.CreateTable(
                name: "orders",
                schema: "orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    WorkType = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Fabric = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TechnicalNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EmbroideryStyle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ThreadColors = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Density = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EmbroideryZone = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BeadType = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Arrangement = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AffectedZones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReceptionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExpectedDeliveryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ActualDeliveryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    TotalPrice = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    AssignedTailorId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedEmbroidererId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedBeaderId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeliveryWithUnpaidReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    HasUnpaidBalance = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "order_photos",
                schema: "orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UploadedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UploadedBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_photos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_order_photos_orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "orders",
                        principalTable: "orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_status_transitions",
                schema: "orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<int>(type: "integer", nullable: true),
                    ToStatus = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TransitionedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TransitionedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_status_transitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_order_status_transitions_orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "orders",
                        principalTable: "orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_order_photos_OrderId",
                schema: "orders",
                table: "order_photos",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_order_status_transitions_OrderId_TransitionedAt",
                schema: "orders",
                table: "order_status_transitions",
                columns: new[] { "OrderId", "TransitionedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_orders_AssignedBeaderId",
                schema: "orders",
                table: "orders",
                column: "AssignedBeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_orders_AssignedEmbroidererId",
                schema: "orders",
                table: "orders",
                column: "AssignedEmbroidererId");

            migrationBuilder.CreateIndex(
                name: "IX_orders_AssignedTailorId",
                schema: "orders",
                table: "orders",
                column: "AssignedTailorId");

            migrationBuilder.CreateIndex(
                name: "IX_orders_ClientId",
                schema: "orders",
                table: "orders",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_orders_Code",
                schema: "orders",
                table: "orders",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_orders_ExpectedDeliveryDate",
                schema: "orders",
                table: "orders",
                column: "ExpectedDeliveryDate");

            migrationBuilder.CreateIndex(
                name: "IX_orders_ReceptionDate",
                schema: "orders",
                table: "orders",
                column: "ReceptionDate");

            migrationBuilder.CreateIndex(
                name: "IX_orders_Status",
                schema: "orders",
                table: "orders",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_photos",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "order_status_transitions",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "orders",
                schema: "orders");
        }
    }
}
