using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestingSystem.Migrations
{
    public partial class CategoryNameMadeUnique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletionDate",
                table: "TestResults",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2022, 10, 31, 3, 20, 3, 148, DateTimeKind.Local).AddTicks(2287),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2022, 10, 28, 20, 43, 22, 542, DateTimeKind.Local).AddTicks(4597));

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_Name",
                table: "Categories");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletionDate",
                table: "TestResults",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2022, 10, 28, 20, 43, 22, 542, DateTimeKind.Local).AddTicks(4597),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2022, 10, 31, 3, 20, 3, 148, DateTimeKind.Local).AddTicks(2287));
        }
    }
}
