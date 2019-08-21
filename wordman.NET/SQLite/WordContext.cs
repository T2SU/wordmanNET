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

        public DbSet<Antonym> Antonyms { get; set; }

        public DbSet<Synonym> Synonyms { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=word.db;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Antonym>()
                .HasOne(a => a.Word)
                .WithMany(w => w.Antonyms)
                .HasForeignKey(a => a.WordID)
                .HasConstraintName("foreignKey_Word_Antonyms");
            modelBuilder.Entity<Synonym>()
                .HasOne(s => s.Word)
                .WithMany(w => w.Synonyms)
                .HasForeignKey(a => a.WordID)
                .HasConstraintName("foreignKey_Word_Synonyms");
            modelBuilder.Entity<Example>()
                .HasOne(a => a.Word)
                .WithMany(w => w.Examples)
                .HasForeignKey(a => a.WordID)
                .HasConstraintName("foreignKey_Word_Examples");

            modelBuilder.Entity<Word>()
                .HasIndex(w => w.Content)
                .IsUnique();
        }
    }
}
