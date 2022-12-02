using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestingSystem.Migrations.Somee
{
    public partial class SomeeInitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletionDate",
                table: "TestResults",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2022, 12, 2, 15, 37, 36, 519, DateTimeKind.Local).AddTicks(1646),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2022, 10, 31, 3, 20, 3, 148, DateTimeKind.Local).AddTicks(2287));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletionDate",
                table: "TestResults",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2022, 10, 31, 3, 20, 3, 148, DateTimeKind.Local).AddTicks(2287),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2022, 12, 2, 15, 37, 36, 519, DateTimeKind.Local).AddTicks(1646));
        }
    }
}
