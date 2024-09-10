using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Interface;
public interface IAmenityService
{
    IEnumerable<Amenity> GetAllAmenities();
    Amenity GetAmenityById(int id);
    void CreateAmenity(Amenity amenity);
    void DeleteAmenity(Amenity amenity);
    void UpdateAmenity(Amenity amenity);
}
