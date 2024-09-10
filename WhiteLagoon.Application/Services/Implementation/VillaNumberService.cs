using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Services.Interface;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Implementation;
public class VillaNumberService(IUnitOfWork unitOfWork) : IVillaNumberService
{
    public bool CheckVillaNumberExists(int villa_number)
    {
        return unitOfWork.VillaNumber
            .Any(t => t.Villa_Number == villa_number);
    }

    public void CreateVillaNumber(VillaNumber villaNumber)
    {
        unitOfWork.VillaNumber.Add(villaNumber);
        unitOfWork.Save();
    }

    public bool DeleteVillaNumber(int id)
    {
        try
        {
            var dbVillaNumber = unitOfWork.VillaNumber.Get(x => x.Villa_Number == id);
            if (dbVillaNumber is not null)
            {
                unitOfWork.VillaNumber.Remove(dbVillaNumber);
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

    public IEnumerable<VillaNumber> GetAllVillaNumbers()
    {
        return unitOfWork.VillaNumber.GetAll(includeProperties: "Villa");
    }

    public VillaNumber GetVillaNumberById(int id)
    {
        return unitOfWork.VillaNumber.Get(s => s.Villa_Number == id, includeProperties: "Villa");
    }

    public void UpdateVillaNumber(VillaNumber villaNumber)
    {
        unitOfWork.VillaNumber.Update(villaNumber);
        unitOfWork.Save();
    }
}
