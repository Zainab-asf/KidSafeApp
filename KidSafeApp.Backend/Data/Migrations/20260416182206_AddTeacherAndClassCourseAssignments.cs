using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KidSafeApp.Backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherAndClassCourseAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TeacherId",
                table: "ClassRoom",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClassRoomCourseAssignment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassRoomId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassRoomCourseAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassRoomCourseAssignment_ClassRoom_ClassRoomId",
                        column: x => x.ClassRoomId,
                        principalTable: "ClassRoom",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassRoomCourseAssignment_Course_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Course",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassRoom_TeacherId",
                table: "ClassRoom",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassRoomCourseAssignment_ClassRoomId_CourseId",
                table: "ClassRoomCourseAssignment",
                columns: new[] { "ClassRoomId", "CourseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassRoomCourseAssignment_CourseId",
                table: "ClassRoomCourseAssignment",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassRoom_User_TeacherId",
                table: "ClassRoom",
                column: "TeacherId",
                principalTable: "User",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassRoom_User_TeacherId",
                table: "ClassRoom");

            migrationBuilder.DropTable(
                name: "ClassRoomCourseAssignment");

            migrationBuilder.DropIndex(
                name: "IX_ClassRoom_TeacherId",
                table: "ClassRoom");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "ClassRoom");
        }
    }
}
