using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UploadApi.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email_verified_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FakeImages",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    author_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    original_photo_url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    original_back_url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    upload_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    resize_photo_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    resize_back_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    resized_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    no_back_photo_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    remove_bg_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    result_photo_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    finish_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FakeImages", x => x.id);
                    table.ForeignKey(
                        name: "FK_FakeImages_Users_author_id",
                        column: x => x.author_id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FakeImages_author_id",
                table: "FakeImages",
                column: "author_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FakeImages");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
