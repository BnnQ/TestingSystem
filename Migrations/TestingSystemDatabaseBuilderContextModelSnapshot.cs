﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TestingSystem.Models.Contexts;

#nullable disable

namespace TestingSystem.Migrations
{
    [DbContext(typeof(TestingSystemDatabaseBuilderContext))]
    partial class TestingSystemDatabaseBuilderContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("TestingSystem.Models.AnswerOption", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnOrder(1);

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<bool>("IsCorrect")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(false);

                    b.Property<int>("QuestionId")
                        .HasColumnType("int");

                    b.Property<int>("SerialNumberInQuestion")
                        .HasColumnType("int")
                        .HasColumnOrder(2);

                    b.HasKey("Id");

                    b.HasIndex("QuestionId");

                    b.ToTable("AnswerOptions");

                    b.HasCheckConstraint("CK_AnswerOptions_Content", "[Content] != ''");

                    b.HasCheckConstraint("CK_AnswerOptions_SerialNumberInQuestion", "[SerialNumberInQuestion] > 0");
                });

            modelBuilder.Entity("TestingSystem.Models.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnOrder(1);

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("Id");

                    b.ToTable("Categories");

                    b.HasCheckConstraint("CK_Categories_Name", "[Name] != ''");
                });

            modelBuilder.Entity("TestingSystem.Models.Question", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnOrder(1);

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasMaxLength(512)
                        .HasColumnType("nvarchar(512)");

                    b.Property<int?>("NumberOfSecondsToAnswer")
                        .HasColumnType("int");

                    b.Property<double>("PointsCost")
                        .HasColumnType("float");

                    b.Property<int>("SerialNumberInTest")
                        .HasColumnType("int")
                        .HasColumnOrder(3);

                    b.Property<int>("TestId")
                        .HasColumnType("int")
                        .HasColumnOrder(2);

                    b.HasKey("Id");

                    b.HasIndex("TestId");

                    b.ToTable("Questions");

                    b.HasCheckConstraint("CK_Questions_Content", "[Content] != ''");

                    b.HasCheckConstraint("CK_Questions_PointsCost", "[PointsCost] > 0");
                });

            modelBuilder.Entity("TestingSystem.Models.Student", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnOrder(1);

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("EncryptedName")
                        .IsRequired()
                        .HasColumnType("VARCHAR(128)")
                        .HasColumnOrder(2);

                    b.Property<string>("EncryptedPassword")
                        .IsRequired()
                        .HasColumnType("VARCHAR(128)")
                        .HasColumnOrder(3);

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("Id");

                    b.ToTable("Students");

                    b.HasCheckConstraint("CK_Students_EncryptedName", "[EncryptedName] != ''");

                    b.HasCheckConstraint("CK_Students_EncryptedPassword", "[EncryptedPassword] != ''");
                });

            modelBuilder.Entity("TestingSystem.Models.Teacher", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnOrder(1);

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("EncryptedName")
                        .IsRequired()
                        .HasColumnType("VARCHAR(128)")
                        .HasColumnOrder(2);

                    b.Property<string>("EncryptedPassword")
                        .IsRequired()
                        .HasColumnType("VARCHAR(128)")
                        .HasColumnOrder(3);

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("Id");

                    b.ToTable("Teachers");

                    b.HasCheckConstraint("CK_Teachers_EncryptedName", "[EncryptedName] != ''");

                    b.HasCheckConstraint("CK_Teachers_EncryptedPassword", "[EncryptedPassword] != ''");
                });

            modelBuilder.Entity("TestingSystem.Models.Test", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnOrder(1);

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("CategoryId")
                        .HasColumnType("int");

                    b.Property<bool>("IsAccountingForIncompleteAnswersEnabled")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true);

                    b.Property<int>("MaximumPoints")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<int?>("NumberOfSecondsToAnswerEachQuestion")
                        .HasColumnType("int");

                    b.Property<int?>("NumberOfSecondsToComplete")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.ToTable("Tests");

                    b.HasCheckConstraint("CK_Tests_MaximumPoints", "[MaximumPoints] > 0");

                    b.HasCheckConstraint("CK_Tests_Name", "[Name] != ''");
                });

            modelBuilder.Entity("TestsTeacherOwners", b =>
                {
                    b.Property<int>("OwnedTestsId")
                        .HasColumnType("int");

                    b.Property<int>("OwnerTeachersId")
                        .HasColumnType("int");

                    b.HasKey("OwnedTestsId", "OwnerTeachersId");

                    b.HasIndex("OwnerTeachersId");

                    b.ToTable("TestsTeacherOwners");
                });

            modelBuilder.Entity("TestingSystem.Models.AnswerOption", b =>
                {
                    b.HasOne("TestingSystem.Models.Question", "Question")
                        .WithMany("AnswerOptions")
                        .HasForeignKey("QuestionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Question");
                });

            modelBuilder.Entity("TestingSystem.Models.Question", b =>
                {
                    b.HasOne("TestingSystem.Models.Test", "Test")
                        .WithMany("Questions")
                        .HasForeignKey("TestId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Test");
                });

            modelBuilder.Entity("TestingSystem.Models.Test", b =>
                {
                    b.HasOne("TestingSystem.Models.Category", "Category")
                        .WithMany("Tests")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");
                });

            modelBuilder.Entity("TestsTeacherOwners", b =>
                {
                    b.HasOne("TestingSystem.Models.Test", null)
                        .WithMany()
                        .HasForeignKey("OwnedTestsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TestingSystem.Models.Teacher", null)
                        .WithMany()
                        .HasForeignKey("OwnerTeachersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("TestingSystem.Models.Category", b =>
                {
                    b.Navigation("Tests");
                });

            modelBuilder.Entity("TestingSystem.Models.Question", b =>
                {
                    b.Navigation("AnswerOptions");
                });

            modelBuilder.Entity("TestingSystem.Models.Test", b =>
                {
                    b.Navigation("Questions");
                });
#pragma warning restore 612, 618
        }
    }
}
