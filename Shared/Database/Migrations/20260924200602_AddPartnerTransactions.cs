using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recyclage.Shared.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddPartnerTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PartnerTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    PaidByAyoub = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ReturnedToMustafa = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Details = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerTransactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartnerTransactions_Date",
                table: "PartnerTransactions",
                column: "Date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartnerTransactions");
        }
    }
}
