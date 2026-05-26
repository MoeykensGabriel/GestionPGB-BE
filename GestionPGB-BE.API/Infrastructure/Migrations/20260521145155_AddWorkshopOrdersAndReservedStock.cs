using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionPGB_BE.API.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkshopOrdersAndReservedStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "reserved_stock",
                table: "products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "workshop_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    license_plate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    callback_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workshop_orders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "workshop_order_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workshop_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: true),
                    product_code = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    requested_quantity = table.Column<int>(type: "integer", nullable: false),
                    reserved_quantity = table.Column<int>(type: "integer", nullable: false),
                    shortage_quantity = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workshop_order_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_workshop_order_items_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_workshop_order_items_workshop_orders_workshop_order_id",
                        column: x => x.workshop_order_id,
                        principalTable: "workshop_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_workshop_order_items_product_id",
                table: "workshop_order_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_workshop_order_items_workshop_order_id",
                table: "workshop_order_items",
                column: "workshop_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_workshop_orders_license_plate",
                table: "workshop_orders",
                column: "license_plate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "workshop_order_items");

            migrationBuilder.DropTable(
                name: "workshop_orders");

            migrationBuilder.DropColumn(
                name: "reserved_stock",
                table: "products");
        }
    }
}
