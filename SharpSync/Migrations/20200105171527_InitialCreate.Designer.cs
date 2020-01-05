﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SharpSync.Services.Common;

namespace SharpSync.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20200105171527_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.0");

            modelBuilder.Entity("SharpSync.Database.DestinationPath", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasColumnName("path")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("destinations");
                });

            modelBuilder.Entity("SharpSync.Database.SourcePath", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasColumnName("path")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("sources");
                });

            modelBuilder.Entity("SharpSync.Database.SyncRule", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("INTEGER");

                    b.Property<int>("DestinationId")
                        .HasColumnName("dst")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("ShouldZip")
                        .HasColumnName("zip")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SourceId")
                        .HasColumnName("src")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("DestinationId");

                    b.HasIndex("SourceId");

                    b.ToTable("rules");
                });

            modelBuilder.Entity("SharpSync.Database.SyncRule", b =>
                {
                    b.HasOne("SharpSync.Database.DestinationPath", "Destination")
                        .WithMany("SyncRules")
                        .HasForeignKey("DestinationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SharpSync.Database.SourcePath", "Source")
                        .WithMany("SyncRules")
                        .HasForeignKey("SourceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}