using Microsoft.AspNetCore.Hosting;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utilities;
using WhiteLagoon.Application.Services.Interface;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Implementation;
public class VillaService(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment) : IVillaService
{

    public void CreateVilla(Villa villa)
    {
        if (villa.Image != null)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
            string imagePath = Path.Combine(webHostEnvironment.WebRootPath, @"Images\VillaImages");

            using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
            villa.Image.CopyTo(fileStream);

            villa.ImageUrl = @"\Images\VillaImages\" + fileName;
        }
        else
        {
            villa.ImageUrl = "https://placehold.co/600x400/EEE/31343C";
        }
        unitOfWork.Villa.Add(villa);
        unitOfWork.Save();
    }

    public bool DeleteVilla(int id)
    {
        try
        {
            var dbVilla = unitOfWork.Villa.Get(x => x.Id == id);
            if (dbVilla is not null)
            {
                if (!string.IsNullOrEmpty(dbVilla.ImageUrl))
                {
                    var oldImageUrl = Path.Combine(webHostEnvironment.WebRootPath, dbVilla.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImageUrl))
                    {
                        System.IO.File.Delete(oldImageUrl);
                    }
                }
                unitOfWork.Villa.Remove(dbVilla);
                unitOfWork.Save();
            }
            return true;
        }
        catch (Exception)
        {

            return false;
        }

    }

    public IEnumerable<Villa> GetAllVillas()
    {
        return unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity");
    }

    public Villa GetVillaById(int id)
    {
        return unitOfWork.Villa.Get(s => s.Id == id, includeProperties: "VillaAmenity");
    }

    public IEnumerable<Villa> GetVillasAvailabilityByDate(int nights, DateOnly checkInDate)
    {
        var villaList = unitOfWork.Villa
            .GetAll(includeProperties: "VillaAmenities")
            .ToList();
        var villaNumbersList = unitOfWork.VillaNumber
            .GetAll()
            .ToList();
        var bookedVillas = unitOfWork.Booking
            .GetAll(s => s.Status == SD.StatusApproved || s.Status == SD.StatusCheckedIn)
            .ToList();

        foreach (var villa in villaList)
        {
            int roomAvailable = SD.VillaRoomsAvailable_Count
                (villa.Id, villaNumbersList, checkInDate, nights, bookedVillas);
            villa.IsAvailable = roomAvailable > 0 ? true : false;
        }

        return villaList;
    }

    public bool IsVillaAvailableByDate(int villaId, int nights, DateOnly checkInDate)
    {
        var villaNumbersList = unitOfWork.VillaNumber
           .GetAll()
           .ToList();
        var bookedVillas = unitOfWork.Booking
            .GetAll(s => s.Status == SD.StatusApproved || s.Status == SD.StatusCheckedIn)
            .ToList();

        int roomAvailable = SD.VillaRoomsAvailable_Count
            (villaId, villaNumbersList, checkInDate, nights, bookedVillas);

        return roomAvailable > 0;
    }

    public void UpdateVilla(Villa villa)
    {
        if (villa.Image != null)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
            string imagePath = Path.Combine(webHostEnvironment.WebRootPath, @"Images\VillaImages");

            if (!string.IsNullOrEmpty(villa.ImageUrl))
            {
                var oldImagePath = Path.Combine(webHostEnvironment.WebRootPath, villa.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath))
                    System.IO.File.Delete(oldImagePath);
            }

            using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
            villa.Image.CopyTo(fileStream);

            villa.ImageUrl = @"\Images\VillaImages\" + fileName;
        }

        unitOfWork.Villa.Update(villa);
        unitOfWork.Save();
    }
}
