using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recyclage.Shared.Database.Migrations
{
    /// <inheritdoc />
    public partial class SplitPartnerLedgers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AyoubPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Details = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AyoubPayments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MustafaReturns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Details = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MustafaReturns", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AyoubPayments_Date",
                table: "AyoubPayments",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_MustafaReturns_Date",
                table: "MustafaReturns",
                column: "Date");

            // Existing partner rows are copied into the two new ledgers before the combined
            // table is dropped, so the split does not lose any data.
            migrationBuilder.Sql(
                """
                INSERT INTO "AyoubPayments" ("Date", "Amount", "Details", "CreatedAt")
                SELECT "Date", "PaidByAyoub", "Details", "CreatedAt"
                FROM "PartnerTransactions"
                WHERE CAST("PaidByAyoub" AS REAL) <> 0;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO "MustafaReturns" ("Date", "Amount", "Details", "CreatedAt")
                SELECT "Date", "ReturnedToMustafa", "Details", "CreatedAt"
                FROM "PartnerTransactions"
                WHERE CAST("ReturnedToMustafa" AS REAL) <> 0;
                """);

            migrationBuilder.DropTable(
                name: "PartnerTransactions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PartnerTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Date = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Details = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    PaidByAyoub = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ReturnedToMustafa = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerTransactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartnerTransactions_Date",
                table: "PartnerTransactions",
                column: "Date");

            migrationBuilder.Sql(
                """
                INSERT INTO "PartnerTransactions" ("Date", "PaidByAyoub", "ReturnedToMustafa", "Details", "CreatedAt")
                SELECT "Date", "Amount", '0.0', "Details", "CreatedAt"
                FROM "AyoubPayments";
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO "PartnerTransactions" ("Date", "PaidByAyoub", "ReturnedToMustafa", "Details", "CreatedAt")
                SELECT "Date", '0.0', "Amount", "Details", "CreatedAt"
                FROM "MustafaReturns";
                """);

            migrationBuilder.DropTable(
                name: "AyoubPayments");

            migrationBuilder.DropTable(
                name: "MustafaReturns");
        }
    }
}
