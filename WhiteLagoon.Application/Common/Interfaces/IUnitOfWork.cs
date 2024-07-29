namespace WhiteLagoon.Application.Common.Interfaces;

public interface IUnitOfWork
{
    IVillaRepository Villa { get; }
    IVillaNumberRepository VillaNumber { get; }
    IBookingRepository Booking { get; }
    IAmenityRepository Amenity { get; }
    IApplicationUserRepository User { get; }
    void Save();
}
