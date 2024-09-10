using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WhiteLagoon.Application.Services.Interface;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers;

public class VillaNumberController(IVillaService villaService,
    IVillaNumberService villaNumberService) : Controller
{

    public IActionResult Index()
    {
        var result = villaNumberService.GetAllVillaNumbers();
        return View(result);
    }

    public IActionResult Create()
    {

        VillaNumberViewModel villaNumberVM = new()
        {
            VillaList = villaService.GetAllVillas().Select(q => new SelectListItem()
            {
                Text = q.Name,
                Value = q.Id.ToString()
            })
        };
        return View(villaNumberVM);
    }

    [HttpPost]
    public IActionResult Create(VillaNumberViewModel obj)
    {
        bool roomNumberExists = villaNumberService.CheckVillaNumberExists(obj.VillaNumber.Villa_Number);
        if (ModelState.IsValid && !roomNumberExists)
        {
            villaNumberService.CreateVillaNumber(obj.VillaNumber);
            TempData["success"] = "The villa number has been created successfully";
            return RedirectToAction(nameof(Index));
        }

        if (roomNumberExists)
            TempData["error"] = "The villa number is already exists.";

        obj.VillaList = villaService.GetAllVillas()
            .Select(q => new SelectListItem()
            {
                Text = q.Name,
                Value = q.Id.ToString()
            });
        return View(obj);
    }

    public IActionResult Update(int VillaNumberId)
    {
        var villaNumberVM = new VillaNumberViewModel()
        {
            VillaList = villaService.GetAllVillas()
                .Select(q => new SelectListItem()
                {
                    Text = q.Name,
                    Value = q.Id.ToString()
                }),
            VillaNumber = villaNumberService.GetVillaNumberById(VillaNumberId)
        };
        if (villaNumberVM.VillaNumber == null)
            return RedirectToAction("Error", "Home");
        return View(villaNumberVM);
    }

    [HttpPost]
    public IActionResult Update(VillaNumberViewModel obj)
    {
        if (ModelState.IsValid)
        {
            villaNumberService.UpdateVillaNumber(obj.VillaNumber);
            TempData["success"] = "The villa number has been updated successfully";
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

    public IActionResult Delete(int VillaNumberId)
    {
        var villaNumberVM = new VillaNumberViewModel()
        {
            VillaList = villaService.GetAllVillas()
                .Select(q => new SelectListItem()
                {
                    Text = q.Name,
                    Value = q.Id.ToString()
                }),
            VillaNumber = villaNumberService.GetVillaNumberById(VillaNumberId)
        };
        if (villaNumberVM.VillaNumber == null)
            return RedirectToAction("Error", "Home");
        return View(villaNumberVM);
    }

    [HttpPost]
    public IActionResult Delete(VillaNumberViewModel villaNumberViewModel)
    {
        var result = villaNumberService.GetVillaNumberById(villaNumberViewModel.VillaNumber.Villa_Number);
        if (result is not null)
        {
            villaNumberService.DeleteVillaNumber(villaNumberViewModel.VillaNumber.Villa_Number);
            TempData["success"] = "The villa number has been deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        TempData["error"] = "The villa number could not be deleted.";
        return View();
    }
}
