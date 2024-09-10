using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using System.Security.Claims;
using WhiteLagoon.Application.Common.Utilities;
using WhiteLagoon.Application.Services.Interface;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Web.Controllers;
public class BookingController(IWebHostEnvironment webHostEnvironment,
    IVillaService villaService,
    IBookingService bookingService,
    IVillaNumberService villaNumberService,
    UserManager<ApplicationUser> userManager) : Controller
{
    [Authorize]
    public IActionResult Index()
    {
        return View();
    }

    [Authorize]
    public IActionResult FinalizeBooking(int villaId, DateOnly checkInDate, int nights)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        ApplicationUser user = userManager.FindByIdAsync(userId).GetAwaiter().GetResult();

        Booking booking = new()
        {
            VillaId = villaId,
            Villa = villaService.GetVillaById(villaId),
            CheckInDate = checkInDate,
            Nights = nights,
            CheckOutDate = checkInDate.AddDays(nights),
            UserId = userId,
            Phone = user.PhoneNumber,
            Email = user.Email,
            Name = user.Name
        };
        booking.TotalCost = booking.Villa.Price * nights;
        return View(booking);
    }

    [Authorize]
    [HttpPost]
    public IActionResult FinalizeBooking(Booking booking)
    {
        var villa = villaService.GetVillaById(booking.VillaId);
        booking.TotalCost = villa.Price * booking.Nights;
        booking.Status = SD.StatusPending;
        booking.BookingDate = DateTime.Now;

        if (!villaService.IsVillaAvailableByDate(villa.Id, booking.Nights, booking.CheckInDate))
        {
            TempData["error"] = "Room has been sold out!";
            return RedirectToAction(nameof(FinalizeBooking), new
            {
                VillaId = booking.VillaId,
                CheckInDate = booking.CheckInDate,
                Nights = booking.Nights
            });
        }

        bookingService.CreateBooking(booking);

        var domain = Request.Scheme + "://" + Request.Host.Value + "/";
        var options = new SessionCreateOptions
        {
            LineItems = new List<SessionLineItemOptions>(),
            Mode = "payment",
            SuccessUrl = domain + $"booking/BookingConfirmation?bookingId={booking.Id}",
            CancelUrl = domain + $"booking/FinalizeBooking?villaId={booking.VillaId}&checkInDate={booking.CheckInDate}&nights={booking.Nights}",
        };

        options.LineItems.Add(new SessionLineItemOptions
        {
            PriceData = new SessionLineItemPriceDataOptions()
            {
                UnitAmount = (long)(booking.TotalCost * 100),
                Currency = "usd",
                ProductData = new SessionLineItemPriceDataProductDataOptions()
                {
                    Name = villa.Name
                }
            },
            Quantity = 1
        });

        var service = new SessionService();
        Session session = service.Create(options);
        bookingService.UpdateStripePaymentId(booking.Id, session.Id, session.PaymentIntentId);
        Response.Headers.Add("Location", session.Url);
        return new StatusCodeResult(303);
    }

    [Authorize]
    public IActionResult BookingConfirmation(int bookingId)
    {
        var bookingFromDb = bookingService.GetBookingById(bookingId);

        if (bookingFromDb.Status == SD.StatusPending)
        {
            var service = new SessionService();
            Session session = service.Get(bookingFromDb.StripeSessionId);
            if (session.PaymentStatus == "paid")
            {
                bookingService.UpdateStatus(bookingFromDb.Id, SD.StatusApproved, 0);
                bookingService.UpdateStripePaymentId(bookingFromDb.Id, session.Id, session.PaymentIntentId);
            }
        }

        return View(bookingId);
    }

    [Authorize]
    public IActionResult BookingDetails(int bookingId)
    {
        Booking bookingFromDb = bookingService.GetBookingById(bookingId);

        if (bookingFromDb.VillaNumber == 0 && bookingFromDb.Status == SD.StatusApproved)
        {
            var availableVillaNumber = AssignAvailableVillaNumberByVilla(bookingFromDb.VillaId);
            bookingFromDb.VillaNumbers = villaNumberService.GetAllVillaNumbers()
                .Where(u => u.VillaId == bookingFromDb.VillaId
                       && availableVillaNumber.Any(x => x == u.Villa_Number))
                .ToList();
        }

        return View(bookingFromDb);
    }

    [HttpPost]
    [Authorize]
    public IActionResult GenerateInvoice(int id)
    {
        string basePath = webHostEnvironment.WebRootPath;

        WordDocument document = new();

        string dataPath = basePath + @"/exports/BookingDetails.docx";
        using FileStream fileStream = new(dataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        document.Open(fileStream, FormatType.Automatic);

        Booking bookingFromDb = bookingService.GetBookingById(id);

        TextSelection textSelection = document.Find("xx_customer_name", false, true);
        WTextRange textRange = textSelection.GetAsOneRange();
        textRange.Text = bookingFromDb.Name;

        using DocIORenderer renderer = new();

        MemoryStream stream = new();
        document.Save(stream, FormatType.Docx);
        stream.Position = 0;

        return File(stream, "application/docx", "BookingDetails.docx");
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin)]
    public IActionResult CheckIn(Booking booking)
    {
        bookingService.UpdateStatus(booking.Id, SD.StatusCheckedIn, booking.VillaNumber);
        TempData["success"] = "Booking updated successfully.";
        return RedirectToAction(nameof(BookingDetails), new { BookingId = booking.Id });
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin)]
    public IActionResult CheckOut(Booking booking)
    {
        bookingService.UpdateStatus(booking.Id, SD.StatusCompleted, booking.VillaNumber);
        TempData["success"] = "Booking updated successfully.";
        return RedirectToAction(nameof(BookingDetails), new { BookingId = booking.Id });
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin)]
    public IActionResult CancelBooking(Booking booking)
    {
        bookingService.UpdateStatus(booking.Id, SD.StatusCanceled, 0);
        TempData["success"] = "Booking cancelled successfully.";
        return RedirectToAction(nameof(BookingDetails), new { BookingId = booking.Id });
    }

    private List<int> AssignAvailableVillaNumberByVilla(int villaId)
    {
        List<int> availableVillaNumbers = new();
        var villaNumbers = villaNumberService.GetAllVillaNumbers()
            .Where(e => e.VillaId == villaId);
        var checkedInVilla = bookingService.GetCheckedInVillaNumbers(villaId);

        foreach (var villaNumber in villaNumbers)
        {
            if (!checkedInVilla.Contains(villaNumber.Villa_Number))
            {
                availableVillaNumbers.Add(villaNumber.Villa_Number);
            }
        }
        return availableVillaNumbers;
    }

    #region BY zarinPal
    //var expose = new Expose();
    //_payment = expose.CreatePayment();
    //    _authority = expose.CreateAuthority();
    //    _transactions = expose.CreateTransactions();
    //private Payment _payment;
    //private Authority _authority;
    //private Transactions _transactions;
    //[Authorize]
    //[HttpPost]
    //public async Task<IActionResult> FinalizeBooking(Booking booking)
    //{
    //    var villa = _unitOfWork.Villa.Get(a => a.Id == booking.VillaId);
    //    booking.TotalCost = villa.Price * booking.Nights;
    //    booking.Status = SD.StatusPending;
    //    booking.BookingDate = DateTime.Now;
    //    _unitOfWork.Booking.Add(booking);
    //    _unitOfWork.Save();
    //    TempData["Amount"] = booking.TotalCost.ToString();
    //    var domain = Request.Scheme + "://" + Request.Host.Value + "/";
    //    var req = await _payment.Request(new DtoRequest()
    //    {
    //        Amount = (int)(booking.TotalCost),
    //        CallbackUrl = domain + $"booking/BookingConfirmation?bookingId={booking.Id}.html",
    //        Description = "Villa rent payment",
    //        Email = booking.Email,
    //        Mobile = booking.Phone,
    //        MerchantId = "aaaaaaaaaassssssssssddddddddddwqerty"
    //    },
    //    Payment.Mode.sandbox
    //    );

    //    return Redirect($"https://sandbox.zarinpal.com/pg/StartPay/{req.Authority}");
    //}

    //[Authorize]
    //public async Task<IActionResult> BookingConfirmation(string authority, string status)
    //{
    //    int amount = int.Parse(TempData["Amount"].ToString());
    //    var verification = await _payment.Verification(new DtoVerification()
    //    {
    //        Amount = amount,
    //        Authority = authority,
    //        MerchantId = "aaaaaaaaaassssssssssddddddddddwqerty"
    //    },
    //    Payment.Mode.sandbox);

    //    if (status == "OK")
    //    {
    //        return View();
    //    }
    //    else
    //    {
    //        TempData["resultStatus"] = $"error, Code: {verification.Status.ToString()}";
    //        return View("PaymentFailed");
    //    }
    //}
    #endregion

    #region API Calls
    [HttpGet]
    [Authorize]
    public IActionResult GetAll(string status)
    {
        IEnumerable<Booking> objBookings;
        string userId = "";
        if (string.IsNullOrEmpty(status))
        {
            status = "";
        }

        if (!User.IsInRole(SD.Role_Admin))
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
        }

        objBookings = bookingService.GetAllBookings(userId, status);

        return Json(new { data = objBookings });
    }

    #endregion
}
