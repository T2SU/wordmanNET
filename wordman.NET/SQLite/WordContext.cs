using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wordman.SQLite
{
    public class WordContext : DbContext
    {
        public DbSet<Word> Words { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=word.db;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RelatedWord>()
                .HasOne(a => a.Word)
                .WithMany(w => w.RelatedWords)
                .HasForeignKey(a => a.WordID)
                .HasConstraintName("foreignKey_Word_RelatedWords");
            modelBuilder.Entity<RelatedString>()
                .HasOne(a => a.Word)
                .WithMany(w => w.RelatedStrings)
                .HasForeignKey(a => a.WordID)
                .HasConstraintName("foreignKey_Word_RelatedStrings");


            modelBuilder.Entity<Word>()
                .HasIndex(w => w.Content)
                .IsUnique();

            modelBuilder.Entity<RelatedWord>()
                .HasIndex(r => new { r.Type, r.WordID, r.RelatedWordID }).IsUnique();
            modelBuilder.Entity<RelatedString>()
                .HasIndex(r => new { r.Type, r.WordID, r.Content }).IsUnique();
        }
    }
}
