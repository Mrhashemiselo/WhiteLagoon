using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers;

public class VillaNumberController(IUnitOfWork unitOfWork) : Controller
{

    public IActionResult Index()
    {
        var result = unitOfWork.VillaNumber.GetAll(includeProperties: "Villa");
        return View(result);
    }

    public IActionResult Create()
    {

        VillaNumberViewModel villaNumberVM = new()
        {
            VillaList = unitOfWork.Villa.GetAll().Select(q => new SelectListItem()
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
        bool roomNumberExists = unitOfWork.VillaNumber.Any(t => t.Villa_Number == obj.VillaNumber.Villa_Number);
        if (ModelState.IsValid && !roomNumberExists)
        {
            unitOfWork.VillaNumber.Add(obj.VillaNumber);
            unitOfWork.Save();
            TempData["success"] = "The villa number has been created successfully";
            return RedirectToAction(nameof(Index));
        }

        if (roomNumberExists)
            TempData["error"] = "The villa number is already exists.";

        obj.VillaList = unitOfWork.Villa.GetAll()
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
            VillaList = unitOfWork.Villa.GetAll()
                .Select(q => new SelectListItem()
                {
                    Text = q.Name,
                    Value = q.Id.ToString()
                }),
            VillaNumber = unitOfWork.VillaNumber.Get(a => a.Villa_Number == VillaNumberId)
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
            unitOfWork.VillaNumber.Update(obj.VillaNumber);
            unitOfWork.Save();
            TempData["success"] = "The villa number has been updated successfully";
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

    public IActionResult Delete(int VillaNumberId)
    {
        var villaNumberVM = new VillaNumberViewModel()
        {
            VillaList = unitOfWork.Villa.GetAll()
                .Select(q => new SelectListItem()
                {
                    Text = q.Name,
                    Value = q.Id.ToString()
                }),
            VillaNumber = unitOfWork.VillaNumber.Get(a => a.Villa_Number == VillaNumberId)
        };
        if (villaNumberVM.VillaNumber == null)
            return RedirectToAction("Error", "Home");
        return View(villaNumberVM);
    }

    [HttpPost]
    public IActionResult Delete(VillaNumberViewModel villaNumberViewModel)
    {
        var result = unitOfWork.VillaNumber
            .Get(x => x.Villa_Number == villaNumberViewModel.VillaNumber.Villa_Number);
        if (result is not null)
        {
            unitOfWork.VillaNumber.Remove(result);
            unitOfWork.Save();
            TempData["success"] = "The villa number has been deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        TempData["error"] = "The villa number could not be deleted.";
        return View();
    }
}
