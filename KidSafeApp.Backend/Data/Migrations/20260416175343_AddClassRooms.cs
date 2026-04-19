using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KidSafeApp.Backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClassRooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClassRoom",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: false),
                    Grade = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Section = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassRoom", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClassRoomStudent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassRoomId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassRoomStudent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassRoomStudent_ClassRoom_ClassRoomId",
                        column: x => x.ClassRoomId,
                        principalTable: "ClassRoom",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassRoomStudent_User_StudentId",
                        column: x => x.StudentId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassRoomStudent_ClassRoomId_StudentId",
                table: "ClassRoomStudent",
                columns: new[] { "ClassRoomId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassRoomStudent_StudentId",
                table: "ClassRoomStudent",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassRoomStudent");

            migrationBuilder.DropTable(
                name: "ClassRoom");
        }
    }
}
