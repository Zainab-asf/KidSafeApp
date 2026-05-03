using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KidSafeApp.Backend.Data.Migrations
{
    public partial class FixUserPasswordColumnLength : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE [User] ALTER COLUMN [Password] nvarchar(512) NOT NULL;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE [User] ALTER COLUMN [Password] nvarchar(256) NOT NULL;");
        }
    }
}
