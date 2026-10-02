using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recyclage.Shared.Database.Migrations
{
    /// <inheritdoc />
    public partial class SplitTransportCosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The old single TransportCost was added into Total, so it moves to the
            // column that still feeds the total. Renaming rather than copying keeps the
            // stored amounts and every already-computed Total consistent.
            migrationBuilder.RenameColumn(
                name: "TransportCost",
                table: "Sales",
                newName: "TransportPaidByMe");

            migrationBuilder.RenameColumn(
                name: "TransportCost",
                table: "PurchaseInvoices",
                newName: "TransportPaidByMe");

            // Transport paid by the partner is new, and is deliberately excluded from
            // Total, so existing rows start at zero.
            migrationBuilder.AddColumn<decimal>(
                name: "TransportPaidByPartner",
                table: "Sales",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransportPaidByPartner",
                table: "PurchaseInvoices",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rolling back collapses two columns into the one the old Total was built
            // from. The partner-paid amounts have nowhere to go — the old schema had no
            // field for them — so they are dropped rather than folded into TransportCost,
            // which would inflate every restored Total.
            migrationBuilder.DropColumn(
                name: "TransportPaidByPartner",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "TransportPaidByPartner",
                table: "PurchaseInvoices");

            migrationBuilder.RenameColumn(
                name: "TransportPaidByMe",
                table: "Sales",
                newName: "TransportCost");

            migrationBuilder.RenameColumn(
                name: "TransportPaidByMe",
                table: "PurchaseInvoices",
                newName: "TransportCost");
        }
    }
}
