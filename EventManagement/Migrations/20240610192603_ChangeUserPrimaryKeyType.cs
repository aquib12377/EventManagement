using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EventManagement.Migrations
{
    public partial class ChangeUserPrimaryKeyType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropPrimaryKey(
            //    name: "user_pkey",
            //    table: "user");

            //migrationBuilder.AlterColumn<string>(
            //    name: "user_id",
            //    table: "user",
            //    type: "varchar",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "serial4")
            //    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            //migrationBuilder.AddPrimaryKey(
            //    name: "PK_user",
            //    table: "user",
            //    column: "user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropPrimaryKey(
            //    name: "user_pkey",
            //    table: "user");

            //migrationBuilder.AlterColumn<int>(
            //    name: "user_id",
            //    table: "user",
            //    type: "serial4",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "varchar")
            //    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            //migrationBuilder.AddPrimaryKey(
            //    name: "PK_user",
            //    table: "user",
            //    column: "user_id");
        }


    }
}
