using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Villa> Villas { get; set; }
    public DbSet<VillaNumber> VillaNumbers { get; set; }
    public DbSet<Amenity> Amenities { get; set; }
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<Booking> Bookings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        #region SeedData
        modelBuilder.Entity<Villa>().HasData(
            new Villa()
            {
                Id = 1,
                Name = "Royal",
                ImageUrl = "~/Images/royal.jpg",
                Description = "Hotel Royal is a hotel located in many cities around the world. It is a hotel brand that operates multiple properties in different locations.",
                Occupancy = 4,
                Sqft = 250,
                Price = 200
            },
            new Villa()
            {
                Id = 2,
                Name = "Poor",
                ImageUrl = "~/Images/poor.jpg",
                Description = "The term \"poor hotel\" doesn't have a widely recognized or standard definition. It's a somewhat subjective and vague term that could mean different things to different people. ",
                Occupancy = 1,
                Sqft = 50,
                Price = 50
            },
            new Villa()
            {
                Id = 3,
                Name = "Kings",
                ImageUrl = "~/Images/kings.jpg",
                Description = "A KingsHotel typically refers to a hotel that has the word Kings in its name, implying it is a hotel that aims to provide an upscale, high-end, or luxurious experience for its guests.",
                Occupancy = 6,
                Sqft = 500,
                Price = 400
            }
            );
        modelBuilder.Entity<VillaNumber>().HasData(
            new VillaNumber()
            {
                Villa_Number = 101,
                VillaId = 1
            },
            new VillaNumber()
            {
                Villa_Number = 102,
                VillaId = 1
            },
            new VillaNumber()
            {
                Villa_Number = 103,
                VillaId = 1
            },
            new VillaNumber()
            {
                Villa_Number = 104,
                VillaId = 1
            },
            new VillaNumber()
            {
                Villa_Number = 201,
                VillaId = 2
            },
            new VillaNumber()
            {
                Villa_Number = 202,
                VillaId = 2
            },
            new VillaNumber()
            {
                Villa_Number = 203,
                VillaId = 2
            },
            new VillaNumber()
            {
                Villa_Number = 301,
                VillaId = 3
            },
            new VillaNumber()
            {
                Villa_Number = 302,
                VillaId = 3
            }
            );
        modelBuilder.Entity<Amenity>().HasData(
            new Amenity()
            {
                Id = 1,
                Name = "Private pool",
                Description = "A small private heated pool",
                VillaId = 1
            },
            new Amenity()
            {
                Id = 2,
                Name = "Private pool",
                Description = "A small private cold water pool",
                VillaId = 2
            },
            new Amenity()
            {
                Id = 3,
                Name = "Private pool",
                Description = "A small private heated pool and a small private cold water pool",
                VillaId = 3
            },
            new Amenity()
            {
                Id = 4,
                Name = "Parking",
                Description = "A parking space for the car",
                VillaId = 1
            },
            new Amenity()
            {
                Id = 5,
                Name = "Parking",
                Description = "free parking",
                VillaId = 2
            },
            new Amenity()
            {
                Id = 6,
                Name = "Free WIFI",
                Description = "Unlimited free WIFI",
                VillaId = 1
            },
            new Amenity()
            {
                Id = 7,
                Name = "Free WIFI",
                Description = "Unlimited free WIFI",
                VillaId = 2
            },
            new Amenity()
            {
                Id = 8,
                Name = "Free WIFI",
                Description = "Unlimited free WIFI",
                VillaId = 3
            }
            );
        #endregion
    }
}
