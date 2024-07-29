using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utilities;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers;

[Authorize(Roles = SD.Role_Admin)]
public class AmenityController(IUnitOfWork unitOfWork) : Controller
{

    public IActionResult Index()
    {
        var result = unitOfWork.Amenity.GetAll(includeProperties: "Villa");
        return View(result);
    }

    public IActionResult Create()
    {

        AmenityViewModel amenityVM = new()
        {
            VillaList = unitOfWork.Villa.GetAll()
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
            unitOfWork.Amenity.Add(obj.Amenity);
            unitOfWork.Save();
            TempData["success"] = "The amenity has been created successfully";
            return RedirectToAction(nameof(Index));
        }

        obj.VillaList = unitOfWork.Villa.GetAll()
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
            VillaList = unitOfWork.Villa.GetAll()
                .Select(q => new SelectListItem()
                {
                    Text = q.Name,
                    Value = q.Id.ToString()
                }),
            Amenity = unitOfWork.Amenity.Get(a => a.Id == AmenityId)
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
            unitOfWork.Amenity.Update(obj.Amenity);
            unitOfWork.Save();
            TempData["success"] = "The amenity has been updated successfully";
            return RedirectToAction(nameof(Index));
        }
        obj.VillaList = unitOfWork.Villa.GetAll()
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
            VillaList = unitOfWork.Villa.GetAll()
                .Select(q => new SelectListItem()
                {
                    Text = q.Name,
                    Value = q.Id.ToString()
                }),
            Amenity = unitOfWork.Amenity.Get(a => a.Id == AmenityId)
        };
        if (amenityVM.Amenity == null)
            return RedirectToAction("Error", "Home");
        return View(amenityVM);
    }

    [HttpPost]
    public IActionResult Delete(AmenityViewModel amenityViewModel)
    {
        var result = unitOfWork.Amenity
            .Get(x => x.Id == amenityViewModel.Amenity.Id);
        if (result is not null)
        {
            unitOfWork.Amenity.Remove(result);
            unitOfWork.Save();
            TempData["success"] = "The amenity has been deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        TempData["error"] = "The amenity could not be deleted.";
        return View();
    }
}
