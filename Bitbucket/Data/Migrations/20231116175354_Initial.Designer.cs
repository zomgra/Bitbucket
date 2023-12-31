﻿// <auto-generated />
using Bitbucket.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Bitbucket.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20231116175354_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.25")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("Bitbucket.Models.Shipment", b =>
                {
                    b.Property<string>("Barcode")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Barcode");

                    b.ToTable("Shipments");
                });
#pragma warning restore 612, 618
        }
    }
}
