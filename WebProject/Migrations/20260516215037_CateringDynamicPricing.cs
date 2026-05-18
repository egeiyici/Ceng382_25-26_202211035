using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebProject.Migrations
{
    /// <inheritdoc />
    public partial class CateringDynamicPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PricePerPerson",
                table: "MenuItems",
                newName: "PricePerExtraPerson");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "MenuItems",
                newName: "BasePrice");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PricePerExtraPerson",
                table: "MenuItems",
                newName: "PricePerPerson");

            migrationBuilder.RenameColumn(
                name: "BasePrice",
                table: "MenuItems",
                newName: "Price");
        }
    }
}
