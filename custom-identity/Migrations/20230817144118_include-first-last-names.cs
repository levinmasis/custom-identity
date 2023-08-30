using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace custom_identity.Migrations
{
    /// <inheritdoc />
    public partial class includefirstlastnames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "__Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "__Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "__Users");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "__Users");
        }
    }
}
