﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using wordman.SQLite;

namespace wordman.Migrations
{
    [DbContext(typeof(WordContext))]
    partial class WordContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity("wordman.SQLite.RelatedString", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Content");

                    b.Property<int>("Type");

                    b.Property<int>("WordID");

                    b.HasKey("ID");

                    b.HasIndex("WordID");

                    b.HasIndex("Type", "WordID", "Content")
                        .IsUnique();

                    b.ToTable("RelatedString");
                });

            modelBuilder.Entity("wordman.SQLite.RelatedWord", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("RelatedWordID");

                    b.Property<int>("Type");

                    b.Property<int>("WordID");

                    b.HasKey("ID");

                    b.HasIndex("WordID");

                    b.HasIndex("Type", "WordID", "RelatedWordID")
                        .IsUnique();

                    b.ToTable("RelatedWord");
                });

            modelBuilder.Entity("wordman.SQLite.Word", b =>
                {
                    b.Property<int>("WordID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Content");

                    b.Property<DateTime>("LastReferenced");

                    b.Property<int>("Referenced");

                    b.HasKey("WordID");

                    b.HasIndex("Content")
                        .IsUnique();

                    b.ToTable("Words");
                });

            modelBuilder.Entity("wordman.SQLite.RelatedString", b =>
                {
                    b.HasOne("wordman.SQLite.Word", "Word")
                        .WithMany("RelatedStrings")
                        .HasForeignKey("WordID")
                        .HasConstraintName("foreignKey_Word_RelatedStrings")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("wordman.SQLite.RelatedWord", b =>
                {
                    b.HasOne("wordman.SQLite.Word", "Word")
                        .WithMany("RelatedWords")
                        .HasForeignKey("WordID")
                        .HasConstraintName("foreignKey_Word_RelatedWords")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
