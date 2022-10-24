using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

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
                //string? userId = ConfigurationManager.AppSettings["databaseAdminUserId"];
                //if (string.IsNullOrWhiteSpace(userId))
                //{
                //    throw new ConfigurationErrorsException(
                //        message: "Configuration file must contain \"databaseAdminUserId\"",
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