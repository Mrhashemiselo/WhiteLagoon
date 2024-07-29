using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Web.Controllers;

[Authorize]
public class VillaController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment) : Controller
{
    public IActionResult Index()
    {
        var result = unitOfWork.Villa.GetAll();
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
            TempData["success"] = "The villa has been created successfully";
            return RedirectToAction(nameof(Index));
        }
        return View();
    }

    public IActionResult Update(int id)
    {
        var villa = unitOfWork.Villa.Get(x => x.Id == id);
        if (villa == null)
            return RedirectToAction("Error", "Home");
        return View(villa);
    }

    [HttpPost]
    public IActionResult Update(Villa villa)
    {
        if (ModelState.IsValid && villa.Id > 0)
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
            TempData["success"] = "The villa has been updated successfully";
            return RedirectToAction(nameof(Index));
        }
        return View();
    }

    public IActionResult Delete(int id)
    {
        var villa = unitOfWork.Villa.Get(x => x.Id == id);
        if (villa is null)
            return RedirectToAction("Error", "Home");
        return View(villa);
    }

    [HttpPost]
    public IActionResult Delete(Villa villa)
    {
        var dbVilla = unitOfWork.Villa.Get(x => x.Id == villa.Id);
        if (dbVilla is not null)
        {
            if (!string.IsNullOrEmpty(dbVilla.ImageUrl))
            {
                var oldImageUrl = Path.Combine(webHostEnvironment.WebRootPath, dbVilla.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImageUrl))
                    System.IO.File.Delete(oldImageUrl);
            }
            unitOfWork.Villa.Remove(dbVilla);
            unitOfWork.Save();
            TempData["success"] = "The villa has been deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        TempData["error"] = "The villa could not be deleted.";
        return View();
    }
}
