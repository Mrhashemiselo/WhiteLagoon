using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Services.Interface;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers;

public class HomeController(IVillaService villaService) : Controller
{

    public IActionResult Index()
    {
        HomeViewModel homeVM = new()
        {
            VillaList = villaService.GetAllVillas(),
            Nights = 1,
            CheckInDate = DateOnly.FromDateTime(DateTime.Now)
        };
        return View(homeVM);
    }

    [HttpPost]
    public IActionResult GetVillasByDate(int nights, DateOnly checkInDate)
    {

        HomeViewModel homeVM = new()
        {
            VillaList = villaService.GetVillasAvailabilityByDate(nights, checkInDate),
            Nights = nights,
            CheckInDate = checkInDate
        };
        return PartialView("_VillaListPartial", homeVM);
    }
}
