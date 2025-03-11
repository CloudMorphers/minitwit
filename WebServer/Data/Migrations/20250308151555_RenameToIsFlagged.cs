using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniTwit.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameToIsFlagged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Flagged",
                table: "Messages",
                newName: "IsFlagged");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsFlagged",
                table: "Messages",
                newName: "Flagged");
        }
    }
}
