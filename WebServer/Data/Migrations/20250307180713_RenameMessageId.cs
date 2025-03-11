using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniTwit.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameMessageId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MessageId",
                table: "Messages",
                newName: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Messages",
                newName: "MessageId");
        }
    }
}
