using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
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
        var villaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenities")
            .ToList();
        foreach (var villa in villaList)
        {
            if (villa.Id % 2 == 0)
                villa.IsAvailable = false;
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
