using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ToDoAppV01.Models;

namespace ToDoAppV01.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<ToDoList> ToDoLists { get; set; } = null!;
        public DbSet<ToDoItem> ToDoItems { get; set; } = null!;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ToDoList>()
                   .HasMany(l => l.Items)
                   .WithOne(i => i.ToDoList!)
                   .HasForeignKey(i => i.ToDoListId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
