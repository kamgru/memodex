﻿// <auto-generated />
using System;
using Memodex.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Memodex.DataAccess.Migrations
{
    [DbContext(typeof(MemodexContext))]
    [Migration("20230807141956_AddTheme")]
    partial class AddTheme
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("mdx")
                .HasAnnotation("ProductVersion", "7.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Memodex.DataAccess.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ItemCount")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Categories", "mdx");
                });

            modelBuilder.Entity("Memodex.DataAccess.Challenge", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("CurrentStepIndex")
                        .HasColumnType("int");

                    b.Property<int>("DeckId")
                        .HasColumnType("int");

                    b.Property<bool>("IsFinished")
                        .HasColumnType("bit");

                    b.Property<int>("ProfileId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("DeckId");

                    b.HasIndex("ProfileId");

                    b.ToTable("Challenges", "mdx");
                });

            modelBuilder.Entity("Memodex.DataAccess.ChallengeStep", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("ChallengeId")
                        .HasColumnType("int");

                    b.Property<int>("FlashcardId")
                        .HasColumnType("int");

                    b.Property<int>("Index")
                        .HasColumnType("int");

                    b.Property<bool>("NeedsReview")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("ChallengeId");

                    b.ToTable("ChallengeSteps", "mdx");
                });

            modelBuilder.Entity("Memodex.DataAccess.Deck", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CategoryId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ItemCount")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.ToTable("Decks", "mdx");
                });

            modelBuilder.Entity("Memodex.DataAccess.Flashcard", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Answer")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("DeckId")
                        .HasColumnType("int");

                    b.Property<string>("Question")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("DeckId");

                    b.ToTable("Flashcards", "mdx");
                });

            modelBuilder.Entity("Memodex.DataAccess.Profile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AvatarPath")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PreferredTheme")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Profiles", "mdx");
                });

            modelBuilder.Entity("Memodex.DataAccess.Challenge", b =>
                {
                    b.HasOne("Memodex.DataAccess.Deck", "Deck")
                        .WithMany()
                        .HasForeignKey("DeckId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Memodex.DataAccess.Profile", "Profile")
                        .WithMany()
                        .HasForeignKey("ProfileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Deck");

                    b.Navigation("Profile");
                });

            modelBuilder.Entity("Memodex.DataAccess.ChallengeStep", b =>
                {
                    b.HasOne("Memodex.DataAccess.Challenge", "Challenge")
                        .WithMany("ChallengeSteps")
                        .HasForeignKey("ChallengeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Challenge");
                });

            modelBuilder.Entity("Memodex.DataAccess.Deck", b =>
                {
                    b.HasOne("Memodex.DataAccess.Category", "Category")
                        .WithMany("Decks")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");
                });

            modelBuilder.Entity("Memodex.DataAccess.Flashcard", b =>
                {
                    b.HasOne("Memodex.DataAccess.Deck", "Deck")
                        .WithMany("Flashcards")
                        .HasForeignKey("DeckId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Deck");
                });

            modelBuilder.Entity("Memodex.DataAccess.Category", b =>
                {
                    b.Navigation("Decks");
                });

            modelBuilder.Entity("Memodex.DataAccess.Challenge", b =>
                {
                    b.Navigation("ChallengeSteps");
                });

            modelBuilder.Entity("Memodex.DataAccess.Deck", b =>
                {
                    b.Navigation("Flashcards");
                });
#pragma warning restore 612, 618
        }
    }
}
