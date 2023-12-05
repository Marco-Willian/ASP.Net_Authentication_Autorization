using JwtAuth.Models;
using Microsoft.EntityFrameworkCore;

namespace JwtAuth.Data
{
    // Responsable for connecting to the database
    public class MovieContext : DbContext
    {
        // Constructor
        public MovieContext(DbContextOptions<MovieContext> options) : base(options)
        {
        }

        // DbSet for Movie
        public DbSet<Movie> Movies { get; set; }
    }
}
