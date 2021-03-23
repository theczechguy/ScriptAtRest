using Microsoft.EntityFrameworkCore.Migrations;

namespace ScriptAtRestServer.Migrations.SqliteMigrations
{
    public partial class AddScriptTypeToScriptEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Scripts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Scripts");
        }
    }
}
