using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KidSafeApp.Backend.Data.Migrations
{
    public partial class ExpandUserPasswordColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE [User] ALTER COLUMN [Password] nvarchar(max) NOT NULL;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE [User] ALTER COLUMN [Password] nvarchar(256) NOT NULL;");
        }
    }
}
