using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Configuration;
using System.IO;

namespace TestingSystem.Models.Contexts
{
    public class TestingSystemAuthorizationContext : DbContext
    {
        public virtual DbSet<Student> Students { get; set; } = null!;
        public virtual DbSet<Teacher> Teachers { get; set; } = null!;


        public TestingSystemAuthorizationContext() : base()
        {
            //empty
        }
        public TestingSystemAuthorizationContext(DbContextOptions<TestingSystemAuthorizationContext> options) : base(options)
        {
            //empty
        }



        /// <exception cref="ConfigurationErrorsException"></exception>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                DirectoryInfo? applicationDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent;
                if (applicationDirectory is null)
                    throw new DirectoryNotFoundException("Application directory not found");
                string pathToConfigFile = Path.Combine(applicationDirectory.FullName, "App.config");
                if (!File.Exists(pathToConfigFile))
                    throw new FileNotFoundException("Application configuration file not found", "App.config");

                ExeConfigurationFileMap fileMap = new();
                fileMap.ExeConfigFilename = pathToConfigFile;
                Configuration config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

                string? dataSource = config.AppSettings.Settings["databaseDataSource"]?.Value;
                if (string.IsNullOrWhiteSpace(dataSource))
                {
                    throw new ConfigurationErrorsException(
                        message: "Configuration file must contain \"databaseDataSource\"",
                        filename: "App.config",
                        line: 4);
                }

                string? initialCatalog = config.AppSettings.Settings["databaseInitialCatalog"]?.Value;
                if (string.IsNullOrWhiteSpace(dataSource))
                {
                    throw new ConfigurationErrorsException(
                        message: "Configuration file must contain \"databaseInitialCatalog\")",
                        filename: "App.config",
                        line: 5);
                }

                string? userId = config.AppSettings.Settings["databaseAdminUserId"]?.Value;
                if (string.IsNullOrWhiteSpace(userId))
                {
                    throw new ConfigurationErrorsException(
                        message: "Configuration file must contain \"databaseAdminUserId\"",
                        filename: "App.config",
                        line: 6);
                }

                string? password = config.AppSettings.Settings["databasePassword"]?.Value;
                if (string.IsNullOrWhiteSpace(password))
                {
                    throw new ConfigurationErrorsException(
                        message: "Configuration file must contain \"databasePassword\"",
                        filename: "App.config",
                        line: 7);
                }

                SqlConnectionStringBuilder connectionStringBuilder = new();
                connectionStringBuilder.DataSource = dataSource;
                connectionStringBuilder.InitialCatalog = initialCatalog;
                connectionStringBuilder.UserID = userId;
                connectionStringBuilder.Password = password;

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
            modelBuilder.Entity<Test>()
                .HasMany(test => test.TestResults)
                .WithOne(testResult => testResult.Test)
                .HasForeignKey(testResult => testResult.TestId)
                .OnDelete(DeleteBehavior.Cascade);
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

            modelBuilder.Entity<Student>()
                .HasMany(student => student.TestResults)
                .WithOne(testResult => testResult.Student)
                .HasForeignKey(testResult => testResult.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Student>(studentModel =>
            {
                studentModel
                .HasCheckConstraint("CK_Students_Name", "[Name] != ''")
                .HasCheckConstraint("CK_Students_HashedPassword", "[HashedPassword] != ''")
                .HasKey(student => student.Id);

                studentModel.Property(student => student.Id)
                .HasColumnOrder(1)
                .UseIdentityColumn()
                .IsRequired();

                studentModel.Property(student => student.Name)
                .HasColumnOrder(2)
                .HasColumnType("NVARCHAR(20)")
                .IsRequired();

                studentModel.Property(student => student.HashedPassword)
                .HasColumnOrder(3)
                .HasColumnType("VARCHAR(128)")
                .IsRequired();

                studentModel.Property(student => student.FullName)
                .HasMaxLength(128)
                .IsRequired();
            });

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

            modelBuilder.Entity<TestResult>()
                .HasOne(testResult => testResult.Test)
                .WithMany(test => test.TestResults)
                .HasForeignKey(testResult => testResult.TestId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<TestResult>()
                .HasOne(testResult => testResult.Student)
                .WithMany(student => student.TestResults)
                .HasForeignKey(testResult => testResult.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<TestResult>(testResultModel =>
            {
                testResultModel
                .HasChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot)
                .HasCheckConstraint("CK_TestResults_CompletionDate", "[CompletionDate] > '2022-10-01'")
                .HasKey(testResult => testResult.Id);

                testResultModel.Property(testResult => testResult.Id)
                .HasColumnOrder(1)
                .UseIdentityColumn()
                .IsRequired();

                testResultModel.Property(testResult => testResult.TestId)
                .HasColumnOrder(2)
                .IsRequired();

                testResultModel.Property(testResult => testResult.StudentId)
                .HasColumnOrder(3)
                .IsRequired();

                testResultModel.Property(testResult => testResult.CompletionDate)
                .HasDefaultValue(DateTime.Now)
                .IsRequired();
            });
            modelBuilder.Entity<TestResult>()
                .ToTable("TestResults");

            base.OnModelCreating(modelBuilder);
        }

    }
}