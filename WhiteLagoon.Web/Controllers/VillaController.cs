using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Services.Interface;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Web.Controllers;

[Authorize]
public class VillaController(IVillaService villaService) : Controller
{
    public IActionResult Index()
    {
        var result = villaService.GetAllVillas();
        return View(result);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(Villa villa)
    {
        if (villa.Description == villa.Name)
        {
            ModelState.AddModelError("", "The description cannot exactly match the Name");
        }
        if (ModelState.IsValid)
        {
            villaService.CreateVilla(villa);
            TempData["success"] = "The villa has been created successfully";
            return RedirectToAction(nameof(Index));
        }
        return View();
    }

    public IActionResult Update(int id)
    {
        var villa = villaService.GetVillaById(id);
        if (villa == null)
            return RedirectToAction("Error", "Home");
        return View(villa);
    }

    [HttpPost]
    public IActionResult Update(Villa villa)
    {
        if (ModelState.IsValid && villa.Id > 0)
        {
            villaService.UpdateVilla(villa);
            TempData["success"] = "The villa has been updated successfully";
            return RedirectToAction(nameof(Index));
        }
        return View();
    }

    public IActionResult Delete(int id)
    {
        var villa = villaService.GetVillaById(id);
        if (villa is null)
            return RedirectToAction("Error", "Home");
        return View(villa);
    }

    [HttpPost]
    public IActionResult Delete(Villa villa)
    {
        bool deleted = villaService.DeleteVilla(villa.Id);
        if (deleted)
        {
            TempData["success"] = "The villa has been deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        else
        {
            TempData["error"] = "The villa could not be deleted.";
        }
        return View();
    }
}
