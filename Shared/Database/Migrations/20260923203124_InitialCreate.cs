using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recyclage.Shared.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ProductType = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    DefaultUnit = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    DefaultUnitPrice = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.CheckConstraint("CK_Products_ProductType", "ProductType IN ('ForBuying', 'ForSale')");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name_ProductType",
                table: "Products",
                columns: new[] { "Name", "ProductType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
