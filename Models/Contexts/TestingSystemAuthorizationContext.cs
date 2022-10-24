using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Configuration;
using System.IO;
using System.Windows;

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

            base.OnModelCreating(modelBuilder);
        }

    }
}