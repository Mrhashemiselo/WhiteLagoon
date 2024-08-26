using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utilities;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers;

public class HomeController(IUnitOfWork unitOfWork) : Controller
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public IActionResult Index()
    {
        HomeViewModel homeVM = new()
        {
            VillaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenities"),
            Nights = 1,
            CheckInDate = DateOnly.FromDateTime(DateTime.Now)
        };
        return View(homeVM);
    }

    [HttpPost]
    public IActionResult GetVillasByDate(int nights, DateOnly checkInDate)
    {
        var villaList = _unitOfWork.Villa
            .GetAll(includeProperties: "VillaAmenities")
            .ToList();
        var villaNumbersList = _unitOfWork.VillaNumber
            .GetAll()
            .ToList();
        var bookedVillas = _unitOfWork.Booking
            .GetAll(s => s.Status == SD.StatusApproved || s.Status == SD.StatusCheckedIn)
            .ToList();

        foreach (var villa in villaList)
        {
            int roomAvailable = SD.VillaRoomsAvailable_Count(villa.Id, villaNumbersList, checkInDate, nights, bookedVillas);
            villa.IsAvailable = roomAvailable > 0 ? true : false;
        }
        HomeViewModel homeVM = new()
        {
            VillaList = villaList,
            Nights = nights,
            CheckInDate = checkInDate
        };
        return PartialView("_VillaListPartial", homeVM);
    }
}
