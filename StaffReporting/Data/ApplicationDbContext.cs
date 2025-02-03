using Management.Models;
using Microsoft.EntityFrameworkCore;

namespace Management.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :
            base(options)
        {

        }
        public virtual DbSet<Work> Works { get; set; }
        public virtual DbSet<WriteUp> WriteUps { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<Dept> Dept { get; set; }
        public virtual DbSet<Desi> Desi { get; set; }
        public virtual DbSet<Linkpaths> Linkpaths { get; set; }
        public virtual DbSet<Tasklist> Tasklist { get; set; }
        public virtual DbSet<Comment> Comment { get; set; }
        public virtual DbSet<WorkAssignment> WorkAssignments { get; set; }
    }
}
