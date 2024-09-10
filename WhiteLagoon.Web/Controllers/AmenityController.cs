using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WhiteLagoon.Application.Common.Utilities;
using WhiteLagoon.Application.Services.Interface;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers;

[Authorize(Roles = SD.Role_Admin)]
public class AmenityController(IAmenityService amenityService,
    IVillaService villaService) : Controller
{

    public IActionResult Index()
    {
        var result = amenityService.GetAllAmenities();
        return View(result);
    }

    public IActionResult Create()
    {

        AmenityViewModel amenityVM = new()
        {
            VillaList = villaService.GetAllVillas()
                .Select(q => new SelectListItem()
                {
                    Text = q.Name,
                    Value = q.Id.ToString()
                })
        };
        return View(amenityVM);
    }

    [HttpPost]
    public IActionResult Create(AmenityViewModel obj)
    {
        if (ModelState.IsValid)
        {
            amenityService.CreateAmenity(obj.Amenity);
            TempData["success"] = "The amenity has been created successfully";
            return RedirectToAction(nameof(Index));
        }

        obj.VillaList = villaService.GetAllVillas()
            .Select(q => new SelectListItem()
            {
                Text = q.Name,
                Value = q.Id.ToString()
            });
        return View(obj);
    }

    public IActionResult Update(int AmenityId)
    {
        var amenityVM = new AmenityViewModel()
        {
            VillaList = villaService.GetAllVillas()
                .Select(q => new SelectListItem()
                {
                    Text = q.Name,
                    Value = q.Id.ToString()
                }),
            Amenity = amenityService.GetAmenityById(AmenityId)
        };
        if (amenityVM.Amenity == null)
            return RedirectToAction("Error", "Home");
        return View(amenityVM);
    }

    [HttpPost]
    public IActionResult Update(AmenityViewModel obj)
    {
        if (ModelState.IsValid)
        {
            amenityService.UpdateAmenity(obj.Amenity);
            TempData["success"] = "The amenity has been updated successfully";
            return RedirectToAction(nameof(Index));
        }
        obj.VillaList = villaService.GetAllVillas()
            .Select(d => new SelectListItem()
            {
                Text = d.Name,
                Value = d.Id.ToString()
            });
        return View(obj);
    }

    public IActionResult Delete(int AmenityId)
    {
        var amenityVM = new AmenityViewModel()
        {
            VillaList = villaService.GetAllVillas()
                .Select(q => new SelectListItem()
                {
                    Text = q.Name,
                    Value = q.Id.ToString()
                }),
            Amenity = amenityService.GetAmenityById(AmenityId)
        };
        if (amenityVM.Amenity == null)
            return RedirectToAction("Error", "Home");
        return View(amenityVM);
    }

    [HttpPost]
    public IActionResult Delete(AmenityViewModel amenityViewModel)
    {
        var result = amenityService.GetAmenityById(amenityViewModel.Amenity.Id);
        if (result is not null)
        {
            amenityService.DeleteAmenity(result);
            TempData["success"] = "The amenity has been deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        TempData["error"] = "The amenity could not be deleted.";
        return View();
    }
}
