﻿// <auto-generated />
using System;
using Fake_images.Models.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Fake_images.Migrations
{
    [DbContext(typeof(FakeImagesDbContext))]
    partial class FakeImagesDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Fake_images.Models.FakeImage", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("id"));

                    b.Property<int>("author_id")
                        .HasColumnType("int");

                    b.Property<DateTime>("finish_at")
                        .HasColumnType("datetime2");

                    b.Property<string>("name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("no_back_photo_url")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("original_back_url")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("original_photo_url")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("remove_bg_at")
                        .HasColumnType("datetime2");

                    b.Property<string>("resize_back_url")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("resize_photo_url")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("resized_at")
                        .HasColumnType("datetime2");

                    b.Property<string>("result_photo_url")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("upload_at")
                        .HasColumnType("datetime2");

                    b.HasKey("id");

                    b.HasIndex("author_id");

                    b.ToTable("FakeImages");
                });

            modelBuilder.Entity("Fake_images.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("email_verified_at")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Fake_images.Models.FakeImage", b =>
                {
                    b.HasOne("Fake_images.Models.User", "AuthorId")
                        .WithMany("FakeImages")
                        .HasForeignKey("author_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AuthorId");
                });

            modelBuilder.Entity("Fake_images.Models.User", b =>
                {
                    b.Navigation("FakeImages");
                });
#pragma warning restore 612, 618
        }
    }
}
