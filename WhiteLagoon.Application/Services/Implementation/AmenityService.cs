using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Services.Interface;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Implementation;
public class AmenityService(IUnitOfWork unitOfWork) : IAmenityService
{
    public void CreateAmenity(Amenity amenity)
    {
        unitOfWork.Amenity.Add(amenity);
        unitOfWork.Save();
    }

    public IEnumerable<Amenity> GetAllAmenities()
    {
        return unitOfWork.Amenity.GetAll(includeProperties: "Villa");
    }

    public Amenity GetAmenityById(int id)
    {
        return unitOfWork.Amenity.Get(s => s.Id == id, includeProperties: "Villa");
    }

    public void DeleteAmenity(Amenity amenity)
    {
        unitOfWork.Amenity.Remove(amenity);
        unitOfWork.Save();
    }

    public void UpdateAmenity(Amenity amenity)
    {
        unitOfWork.Amenity.Update(amenity);
        unitOfWork.Save();
    }
}
