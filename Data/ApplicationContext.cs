using Codebridge.Models;
using Microsoft.EntityFrameworkCore;

namespace Codebridge.Data
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Dog> Dogs { get; set; }
        public ApplicationContext(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Dog>(dog =>
            {
                dog.Property(d => d.Id).ValueGeneratedOnAdd();
                dog.Property(d => d.Name).IsRequired();
                dog.Property(d => d.Color).IsRequired();
                dog.HasIndex(d => d.Name).IsUnique();

                dog.ToTable(t => 
                {
                    t.HasCheckConstraint("CK_Dog_Name_NotEmpty", "LENGTH(\"Name\") > 0");
                    t.HasCheckConstraint("CK_Dog_Weight_Positive", "\"Weight\" > 0");
                    t.HasCheckConstraint("CK_Dog_TailLength_Positive", "\"TailLength\" > 0");
                });
            });
        }
    }
}
