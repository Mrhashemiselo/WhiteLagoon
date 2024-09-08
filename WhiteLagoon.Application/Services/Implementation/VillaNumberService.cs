using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Services.Interface;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Implementation;
public class VillaNumberService(IUnitOfWork unitOfWork) : IVillaNumberService
{

    public void CreateVillaNumber(VillaNumber villaNumber)
    {
        unitOfWork.VillaNumber.Add(villaNumber);
        unitOfWork.Save();
    }

    public bool DeleteVillaNumber(int id)
    {
        try
        {
            var dbVilla = unitOfWork.VillaNumber.Get(x => x.Villa_Number == id);
            if (dbVilla is not null)
            {
                unitOfWork.VillaNumber.Remove(dbVilla);
                unitOfWork.Save();
                return true;
            }
            return false;
        }
        catch (Exception)
        {

            return false;
        }
    }

    public IEnumerable<VillaNumber> GetAllVillaNumber()
    {
        return unitOfWork.VillaNumber.GetAll();
    }

    public VillaNumber GetVillaNumberById(int id)
    {
        return unitOfWork.VillaNumber.Get(s => s.Villa_Number == id);
    }

    public void UpdateVillaNumber(VillaNumber villaNumber)
    {
        unitOfWork.VillaNumber.Update(villaNumber);
        unitOfWork.Save();
    }
}
