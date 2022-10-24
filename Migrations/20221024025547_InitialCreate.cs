using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestingSystem.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.CheckConstraint("CK_Categories_Name", "[Name] != ''");
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "NVARCHAR(20)", nullable: false),
                    HashedPassword = table.Column<string>(type: "VARCHAR(128)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.CheckConstraint("CK_Students_Name", "[Name] != ''");
                    table.CheckConstraint("CK_Students_HashedPassword", "[HashedPassword] != ''");
                });

            migrationBuilder.CreateTable(
                name: "Teachers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "NVARCHAR(20)", nullable: false),
                    HashedPassword = table.Column<string>(type: "VARCHAR(128)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teachers", x => x.Id);
                    table.CheckConstraint("CK_Teachers_Name", "[Name] != ''");
                    table.CheckConstraint("CK_Teachers_HashedPassword", "[HashedPassword] != ''");
                });

            migrationBuilder.CreateTable(
                name: "Tests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MaximumPoints = table.Column<int>(type: "int", nullable: false),
                    NumberOfSecondsToAnswerEachQuestion = table.Column<int>(type: "int", nullable: true),
                    NumberOfSecondsToComplete = table.Column<int>(type: "int", nullable: true),
                    IsAccountingForIncompleteAnswersEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tests", x => x.Id);
                    table.CheckConstraint("CK_Tests_MaximumPoints", "[MaximumPoints] > 0");
                    table.CheckConstraint("CK_Tests_Name", "[Name] != ''");
                    table.ForeignKey(
                        name: "FK_Tests_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TestId = table.Column<int>(type: "int", nullable: false),
                    SerialNumberInTest = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    PointsCost = table.Column<double>(type: "float", nullable: false),
                    NumberOfSecondsToAnswer = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.CheckConstraint("CK_Questions_Content", "[Content] != ''");
                    table.CheckConstraint("CK_Questions_PointsCost", "[PointsCost] > 0");
                    table.ForeignKey(
                        name: "FK_Questions_Tests_TestId",
                        column: x => x.TestId,
                        principalTable: "Tests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestsTeacherOwners",
                columns: table => new
                {
                    OwnedTestsId = table.Column<int>(type: "int", nullable: false),
                    OwnerTeachersId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestsTeacherOwners", x => new { x.OwnedTestsId, x.OwnerTeachersId });
                    table.ForeignKey(
                        name: "FK_TestsTeacherOwners_Teachers_OwnerTeachersId",
                        column: x => x.OwnerTeachersId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestsTeacherOwners_Tests_OwnedTestsId",
                        column: x => x.OwnedTestsId,
                        principalTable: "Tests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnswerOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SerialNumberInQuestion = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnswerOptions", x => x.Id);
                    table.CheckConstraint("CK_AnswerOptions_Content", "[Content] != ''");
                    table.CheckConstraint("CK_AnswerOptions_SerialNumberInQuestion", "[SerialNumberInQuestion] > 0");
                    table.ForeignKey(
                        name: "FK_AnswerOptions_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnswerOptions_QuestionId",
                table: "AnswerOptions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_TestId",
                table: "Questions",
                column: "TestId");

            migrationBuilder.CreateIndex(
                name: "IX_Tests_CategoryId",
                table: "Tests",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TestsTeacherOwners_OwnerTeachersId",
                table: "TestsTeacherOwners",
                column: "OwnerTeachersId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnswerOptions");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "TestsTeacherOwners");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "Teachers");

            migrationBuilder.DropTable(
                name: "Tests");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
