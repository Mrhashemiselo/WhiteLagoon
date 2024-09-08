using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Interface;
public interface IVillaService
{
    IEnumerable<Villa> GetAllVillas();
    Villa GetVillaById(int id);
    void CreateVilla(Villa villa);
    void UpdateVilla(Villa villa);
    bool DeleteVilla(int id);
}
