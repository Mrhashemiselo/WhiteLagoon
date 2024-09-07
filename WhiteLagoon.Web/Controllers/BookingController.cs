using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utilities;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Web.Controllers;
public class BookingController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public BookingController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

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

        ApplicationUser user = _unitOfWork.User.Get(s => s.Id == userId);

        Booking booking = new()
        {
            VillaId = villaId,
            Villa = _unitOfWork.Villa.Get(u => u.Id == villaId, includeProperties: "VillaAmenities"),
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
        var villa = _unitOfWork.Villa.Get(w => w.Id == booking.VillaId);
        booking.TotalCost = villa.Price * booking.Nights;
        booking.Status = SD.StatusPending;
        booking.BookingDate = DateTime.Now;

        var villaNumbersList = _unitOfWork.VillaNumber
            .GetAll()
            .ToList();
        var bookedVillas = _unitOfWork.Booking
            .GetAll(s => s.Status == SD.StatusApproved || s.Status == SD.StatusCheckedIn)
            .ToList();

        int roomAvailable = SD.VillaRoomsAvailable_Count
            (villa.Id, villaNumbersList, booking.CheckInDate, booking.Nights, bookedVillas);
        if (roomAvailable == 0)
        {
            TempData["error"] = "Room has been sold out!";
            return RedirectToAction(nameof(FinalizeBooking), new
            {
                VillaId = booking.VillaId,
                CheckInDate = booking.CheckInDate,
                Nights = booking.Nights
            });
        }

        _unitOfWork.Booking.Add(booking);
        _unitOfWork.Save();

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
        _unitOfWork.Booking.UpdateStripePaymentId(booking.Id, session.Id, session.PaymentIntentId);
        _unitOfWork.Save();
        Response.Headers.Add("Location", session.Url);
        return new StatusCodeResult(303);
    }

    [Authorize]
    public IActionResult BookingConfirmation(int bookingId)
    {
        var bookingFromDb = _unitOfWork.Booking.Get(a => a.Id == bookingId
            , includeProperties: "User,Villa");

        if (bookingFromDb.Status == SD.StatusPending)
        {
            var service = new SessionService();
            Session session = service.Get(bookingFromDb.StripeSessionId);
            if (session.PaymentStatus == "paid")
            {
                _unitOfWork.Booking.UpdateStatus(bookingFromDb.Id, SD.StatusApproved, 0);
                _unitOfWork.Booking.UpdateStripePaymentId(bookingFromDb.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();
            }
        }

        return View(bookingId);
    }

    [Authorize]
    public IActionResult BookingDetails(int bookingId)
    {
        Booking bookingFromDb = _unitOfWork.Booking.Get(u => u.Id == bookingId,
            includeProperties: "Villa,User");

        if (bookingFromDb.VillaNumber == 0 && bookingFromDb.Status == SD.StatusApproved)
        {
            var availableVillaNumber = AssignAvailableVillaNumberByVilla(bookingFromDb.VillaId);
            bookingFromDb.VillaNumbers = _unitOfWork.VillaNumber.GetAll(u => u.VillaId == bookingFromDb.VillaId
            && availableVillaNumber.Any(x => x == u.Villa_Number)).ToList();
        }

        return View(bookingFromDb);
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin)]
    public IActionResult CheckIn(Booking booking)
    {
        _unitOfWork.Booking.UpdateStatus(booking.Id, SD.StatusCheckedIn, booking.VillaNumber);
        _unitOfWork.Save();
        TempData["success"] = "Booking updated successfully.";
        return RedirectToAction(nameof(BookingDetails), new { BookingId = booking.Id });
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin)]
    public IActionResult CheckOut(Booking booking)
    {
        _unitOfWork.Booking.UpdateStatus(booking.Id, SD.StatusCompleted, booking.VillaNumber);
        _unitOfWork.Save();
        TempData["success"] = "Booking updated successfully.";
        return RedirectToAction(nameof(BookingDetails), new { BookingId = booking.Id });
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin)]
    public IActionResult CancelBooking(Booking booking)
    {
        _unitOfWork.Booking.UpdateStatus(booking.Id, SD.StatusCanceled, 0);
        _unitOfWork.Save();
        TempData["success"] = "Booking cancelled successfully.";
        return RedirectToAction(nameof(BookingDetails), new { BookingId = booking.Id });
    }

    private List<int> AssignAvailableVillaNumberByVilla(int villaId)
    {
        List<int> availableVillaNumbers = new();
        var villaNumbers = _unitOfWork.VillaNumber.GetAll(e => e.VillaId == villaId);
        var checkedInVilla = _unitOfWork.Booking.GetAll(q => q.VillaId == villaId && q.Status == SD.StatusCheckedIn)
            .Select(w => w.VillaNumber);

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

        objBookings = _unitOfWork.Booking.GetAll(filter: null, includeProperties: "Villa,User");
        if (!string.IsNullOrEmpty(status))
        {
            objBookings = objBookings.Where(q => q.Status.ToLower().Equals(status.ToLower()));
        }
        return Json(new { data = objBookings });
    }

    #endregion
}
