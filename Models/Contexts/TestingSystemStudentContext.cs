﻿using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Configuration;
using System.IO;

namespace TestingSystem.Models.Contexts
{
    public class TestingSystemStudentContext : DbContext
    {
        public virtual DbSet<Category> Categories { get; set; } = null!;

        
        public TestingSystemStudentContext() : base()
        {
            //empty
        }
        public TestingSystemStudentContext(DbContextOptions<TestingSystemStudentContext> options) : base(options)
        {
            //empty
        }


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
                    .UseSqlServer(connectionStringBuilder.ConnectionString)
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
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

            base.OnModelCreating(modelBuilder);
        }

    }
}