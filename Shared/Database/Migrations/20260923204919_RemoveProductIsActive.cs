using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recyclage.Shared.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);
        }
    }
}
