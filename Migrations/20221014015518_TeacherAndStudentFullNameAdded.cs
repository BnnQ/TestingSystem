using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestingSystem.Migrations
{
    public partial class TeacherAndStudentFullNameAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "EncryptedPassword",
                table: "Teachers",
                type: "VARCHAR(128)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(128)")
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<string>(
                name: "EncryptedName",
                table: "Teachers",
                type: "VARCHAR(128)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(128)")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Teachers",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "EncryptedPassword",
                table: "Students",
                type: "VARCHAR(128)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(128)")
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<string>(
                name: "EncryptedName",
                table: "Students",
                type: "VARCHAR(128)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(128)")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Students",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Students");

            migrationBuilder.AlterColumn<string>(
                name: "EncryptedPassword",
                table: "Teachers",
                type: "VARCHAR(128)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(128)")
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<string>(
                name: "EncryptedName",
                table: "Teachers",
                type: "VARCHAR(128)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(128)")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<string>(
                name: "EncryptedPassword",
                table: "Students",
                type: "VARCHAR(128)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(128)")
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<string>(
                name: "EncryptedName",
                table: "Students",
                type: "VARCHAR(128)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(128)")
                .OldAnnotation("Relational:ColumnOrder", 2);
        }
    }
}
