using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace TestingSystem.Models.Contexts
{
    public class TestingSystemTeacherContext : DbContext
    {
        public virtual DbSet<Category> Categories { get; set; } = null!;
        public virtual DbSet<Test> Tests { get; set; } = null!;
        public virtual DbSet<Teacher> Teachers { get; set; } = null!;


        public TestingSystemTeacherContext() : base()
        {
            //empty
        }
        public TestingSystemTeacherContext(DbContextOptions<TestingSystemTeacherContext> options) : base(options)
        {
            //empty
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //string? userId = ConfigurationManager.AppSettings["databaseTeacherUserId"];
                //if (string.IsNullOrWhiteSpace(userId))
                //{
                //    throw new ConfigurationErrorsException(
                //        message: "Configuration file must contain \"databaseTeacherUserId\"",
                //        filename: "App.config",
                //        line: 4);
                //}

                //string? password = ConfigurationManager.AppSettings["databasePassword"];
                //if (string.IsNullOrWhiteSpace(password))
                //{
                //    throw new ConfigurationErrorsException(
                //        message: "Configuration file must contain \"databasePassword\"",
                //        filename: "App.config",
                //        line: 5);
                //}

                SqlConnectionStringBuilder connectionStringBuilder = new();
                connectionStringBuilder.DataSource = "SQL8001.site4now.net";
                connectionStringBuilder.InitialCatalog = "db_a8dd9e_testingsystem";
                //connectionStringBuilder.UserID = userId;
                //connectionStringBuilder.Password = password;
                connectionStringBuilder.UserID = "db_a8dd9e_testingsystem_admin";
                connectionStringBuilder.Password = "49Exra2ix";

                optionsBuilder
                    .UseSqlServer(connectionStringBuilder.ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AnswerOption>()
                .HasOne(answerOption => answerOption.Question)
                .WithMany(question => question.AnswerOptions)
                .HasForeignKey(answerOption => answerOption.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<AnswerOption>(answerOptionModel =>
            {
                answerOptionModel
                .HasCheckConstraint("CK_AnswerOptions_Content", "[Content] != ''")
                .HasCheckConstraint("CK_AnswerOptions_SerialNumberInQuestion", "[SerialNumberInQuestion] > 0")
                .HasKey(answerOption => answerOption.Id);

                answerOptionModel.Property(answerOption => answerOption.Id)
                .HasColumnOrder(1)
                .UseIdentityColumn()
                .IsRequired();

                answerOptionModel.Property(answerOption => answerOption.SerialNumberInQuestion)
                .HasColumnOrder(2)
                .IsRequired();

                answerOptionModel.Property(answerOption => answerOption.Content)
                .HasMaxLength(255)
                .IsRequired();

                answerOptionModel.Property(answerOption => answerOption.IsCorrect)
                .HasDefaultValue(false)
                .IsRequired();

                answerOptionModel.Property(answerOption => answerOption.QuestionId)
                .IsRequired();
            });
            modelBuilder.Entity<AnswerOption>()
                .ToTable("AnswerOptions");

            modelBuilder.Entity<Question>()
                .HasMany(question => question.AnswerOptions)
                .WithOne(answerOption => answerOption.Question)
                .HasForeignKey(answerOption => answerOption.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Question>()
                .HasOne(question => question.Test)
                .WithMany(test => test.Questions)
                .HasForeignKey(question => question.TestId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Question>(questionModel =>
            {
                questionModel
                .HasCheckConstraint("CK_Questions_Content", "[Content] != ''")
                .HasCheckConstraint("CK_Questions_PointsCost", "[PointsCost] > 0")
                .Ignore(question => question.NumberOfAnswerOptions)
                .Ignore(question => question.IsAutoAnswerOptionNumberingEnabled)
                .HasKey(question => question.Id);

                questionModel.Property(question => question.Id)
                .HasColumnOrder(1)
                .UseIdentityColumn()
                .IsRequired();

                questionModel.Property(question => question.TestId)
                .HasColumnOrder(2)
                .IsRequired();

                questionModel.Property(question => question.SerialNumberInTest)
                .HasColumnOrder(3)
                .IsRequired();

                questionModel.Property(question => question.Content)
                .HasMaxLength(512)
                .IsRequired();

                questionModel.Property(question => question.NumberOfSecondsToAnswer)
                .IsRequired(false);

                questionModel.Property(question => question.TestId)
                .IsRequired();
            });
            modelBuilder.Entity<Question>()
                .ToTable("Questions");

            modelBuilder.Entity<Test>()
                .HasMany(test => test.Questions)
                .WithOne(question => question.Test)
                .HasForeignKey(question => question.TestId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Test>()
                .HasOne(test => test.Category)
                .WithMany(category => category.Tests)
                .HasForeignKey(test => test.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Test>()
                .HasMany(test => test.OwnerTeachers)
                .WithMany(teacher => teacher.OwnedTests)
                .UsingEntity("TestsTeacherOwners");
            modelBuilder.Entity<Test>(testModel =>
            {
                testModel
                .HasCheckConstraint("CK_Tests_Name", "[Name] != ''")
                .HasCheckConstraint("CK_Tests_MaximumPoints", "[MaximumPoints] > 0")
                .Ignore(test => test.NumberOfQuestions)
                .Ignore(test => test.IsAutoCalculationOfQuestionsCostEnabled)
                .Ignore(test => test.IsAutoQuestionNumberingEnabled)
                .HasKey(test => test.Id);

                testModel.Property(test => test.Id)
                .HasColumnOrder(1)
                .UseIdentityColumn()
                .IsRequired();

                testModel.Property(test => test.Name)
                .HasMaxLength(255)
                .IsRequired();

                testModel.Property(test => test.MaximumPoints)
                .IsRequired();

                testModel.Property(test => test.NumberOfSecondsToAnswerEachQuestion)
                .IsRequired(false);

                testModel.Property(test => test.NumberOfSecondsToComplete)
                .IsRequired(false);

                testModel.Property(test => test.IsAccountingForIncompleteAnswersEnabled)
                .HasDefaultValue(true)
                .IsRequired();

                testModel.Property(test => test.CategoryId)
                .IsRequired();
            });
            modelBuilder.Entity<Test>()
                .ToTable("Tests");

            modelBuilder.Entity<Category>()
                .HasMany(category => category.Tests)
                .WithOne(test => test.Category)
                .HasForeignKey(test => test.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Category>(categoryModel =>
            {
                categoryModel
                .HasCheckConstraint("CK_Categories_Name", "[Name] != ''")
                .HasKey(category => category.Id);

                categoryModel.Property(category => category.Id)
                .HasColumnOrder(1)
                .UseIdentityColumn()
                .IsRequired();

                categoryModel.Property(category => category.Name)
                .HasMaxLength(128)
                .IsRequired();
            });
            modelBuilder.Entity<Category>()
                .ToTable("Categories");

            modelBuilder.Entity<Teacher>()
                .HasMany(teacher => teacher.OwnedTests)
                .WithMany(test => test.OwnerTeachers)
                .UsingEntity("TestsTeacherOwners");
            modelBuilder.Entity<Teacher>(teacherModel =>
            {
                teacherModel
                .HasCheckConstraint("CK_Teachers_Name", "[Name] != ''")
                .HasCheckConstraint("CK_Teachers_HashedPassword", "[HashedPassword] != ''")
                .HasKey(teacher => teacher.Id);

                teacherModel.Property(teacher => teacher.Id)
                .HasColumnOrder(1)
                .UseIdentityColumn()
                .IsRequired();

                teacherModel.Property(teacher => teacher.Name)
                .HasColumnOrder(2)
                .HasColumnType("NVARCHAR(20)")
                .IsRequired();

                teacherModel.Property(teacher => teacher.HashedPassword)
                .HasColumnOrder(3)
                .HasColumnType("VARCHAR(128)")
                .IsRequired();

                teacherModel.Property(teacher => teacher.FullName)
                .HasMaxLength(128)
                .IsRequired();
            });
            modelBuilder.Entity<Teacher>()
                .ToTable("Teachers");

            base.OnModelCreating(modelBuilder);
        }

    }
}