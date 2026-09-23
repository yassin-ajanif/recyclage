using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recyclage.Shared.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddAppSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CompanyCapital = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    BackupEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    BackupIntervalHours = table.Column<int>(type: "INTEGER", nullable: false),
                    BackupIntervalUnit = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    BackupRetentionDays = table.Column<int>(type: "INTEGER", nullable: false),
                    BackupDirectory = table.Column<string>(type: "TEXT", nullable: false),
                    LastBackupDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");
        }
    }
}
