using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniTwit.Data.Migrations
{
    /// <inheritdoc />
    public partial class UseDateTimeForPubDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PublishDate",
                table: "Messages",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
            );

            migrationBuilder.Sql("UPDATE Messages SET PublishDate = datetime(PubDate, 'unixepoch');");

            migrationBuilder.DropColumn(name: "PubDate", table: "Messages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PubDate",
                table: "Messages",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L
            );

            migrationBuilder.Sql("UPDATE Messages SET PubDate = strftime('%s', PublishDate);");

            migrationBuilder.DropColumn(name: "PublishDate", table: "Messages");
        }
    }
}
