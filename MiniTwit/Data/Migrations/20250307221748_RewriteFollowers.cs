using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniTwit.Data.Migrations
{
    /// <inheritdoc />
    public partial class RewriteFollowers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserFollows",
                columns: table => new
                {
                    FollowerId = table.Column<int>(type: "INTEGER", nullable: false),
                    FollowingId = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFollows", x => new { x.FollowerId, x.FollowingId });
                    table.ForeignKey(
                        name: "FK_UserFollows_AspNetUsers_FollowerId",
                        column: x => x.FollowerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_UserFollows_AspNetUsers_FollowingId",
                        column: x => x.FollowingId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_UserFollows_FollowingId",
                table: "UserFollows",
                column: "FollowingId"
            );

            migrationBuilder.Sql(
                "INSERT INTO UserFollows (FollowerId, FollowingId) SELECT WhoId, WhomId FROM Followers;"
            );

            migrationBuilder.DropTable(name: "Followers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Followers",
                columns: table => new
                {
                    WhoId = table.Column<int>(type: "INTEGER", nullable: false),
                    WhomId = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Followers", x => new { x.WhoId, x.WhomId });
                }
            );

            migrationBuilder.Sql(
                "INSERT INTO Followers (WhoId, WhomId) SELECT FollowerId, FollowingId FROM UserFollows;"
            );

            migrationBuilder.DropTable(name: "UserFollows");
        }
    }
}
