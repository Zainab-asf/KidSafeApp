using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KidSafeApp.Backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseAssignment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    ChildId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseAssignment_Course_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Course",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseAssignment_User_ChildId",
                        column: x => x.ChildId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseAssignment_ChildId",
                table: "CourseAssignment",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseAssignment_CourseId_ChildId",
                table: "CourseAssignment",
                columns: new[] { "CourseId", "ChildId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseAssignment");
        }
    }
}
