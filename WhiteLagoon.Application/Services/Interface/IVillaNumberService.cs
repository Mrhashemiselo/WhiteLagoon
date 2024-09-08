using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Interface;
public interface IVillaNumberService
{
    IEnumerable<VillaNumber> GetAllVillaNumber();
    VillaNumber GetVillaNumberById(int id);
    void CreateVillaNumber(VillaNumber villaNumber);
    void UpdateVillaNumber(VillaNumber villaNumber);
    bool DeleteVillaNumber(int id);
}
