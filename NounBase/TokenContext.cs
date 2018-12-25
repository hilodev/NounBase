/*  https://github.com/hilodev/NounBase */
using Microsoft.EntityFrameworkCore;
namespace NounBase
{
    public class TokenContext : DbContext
    {
        public TokenContext(DbContextOptions<TokenContext> options) : base(options) { }
        public DbSet<Key> Keys { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<KeyValue> KeyValues { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("Keys");
        }
    }
    public static class SqliteConsoleContextFactory
    {
        public static TokenContext Create(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TokenContext>();
            optionsBuilder.UseSqlite(connectionString);
            var context = new TokenContext(optionsBuilder.Options);
            context.Database.EnsureCreated();
            return context;
        }
    }
}