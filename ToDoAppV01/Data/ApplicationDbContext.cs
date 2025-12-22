using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ToDoAppV01.Models;

namespace ToDoAppV01.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<ToDoList> ToDoLists { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
