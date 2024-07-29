using Dto.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utilities;
using WhiteLagoon.Domain.Entities;
using ZarinPal.Class;

namespace WhiteLagoon.Web.Controllers;
public class BookingController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private Payment _payment;
    private Authority _authority;
    private Transactions _transactions;

    public BookingController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        var expose = new Expose();
        _payment = expose.CreatePayment();
        _authority = expose.CreateAuthority();
        _transactions = expose.CreateTransactions();
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
            CheckoutDate = checkInDate.AddDays(nights),
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
    public async Task<IActionResult> FinalizeBooking(Booking booking)
    {
        var villa = _unitOfWork.Villa.Get(a => a.Id == booking.VillaId);
        booking.TotalCost = villa.Price * booking.Nights;
        booking.Status = SD.StatusPending;
        booking.BookingDate = DateTime.Now;
        _unitOfWork.Booking.Add(booking);
        _unitOfWork.Save();
        TempData["Amount"] = booking.TotalCost.ToString();
        var domain = Request.Scheme + "://" + Request.Host.Value + "/";
        var req = await _payment.Request(new DtoRequest()
        {
            Amount = (int)(booking.TotalCost),
            CallbackUrl = domain + $"booking/BookingConfirmation?bookingId={booking.Id}.html",
            Description = "Villa rent payment",
            Email = booking.Email,
            Mobile = booking.Phone,
            MerchantId = "aaaaaaaaaassssssssssddddddddddwqerty"
        },
        Payment.Mode.sandbox
        );

        return Redirect($"https://sandbox.zarinpal.com/pg/StartPay/{req.Authority}");
    }

    [Authorize]
    public async Task<IActionResult> BookingConfirmation(string authority, string status)
    {
        int amount = int.Parse(TempData["Amount"].ToString());
        var verification = await _payment.Verification(new DtoVerification()
        {
            Amount = amount,
            Authority = authority,
            MerchantId = "aaaaaaaaaassssssssssddddddddddwqerty"
        },
        Payment.Mode.sandbox);

        if (status == "OK")
        {
            return View();
        }
        else
        {
            TempData["resultStatus"] = $"error, Code: {verification.Status.ToString()}";
            return View("PaymentFailed");
        }
    }


    #region Need VPN(StripePayment)
    /// A VPN must be used
    //[Authorize]
    //[HttpPost]
    //public IActionResult FinalizeBooking(Booking booking)
    //{
    //    var villa = unitOfWork.Villa.Get(w => w.Id == booking.VillaId);
    //    booking.TotalCost = villa.Price * booking.Nights;
    //    booking.Status = SD.StatusPending;
    //    booking.BookingDate = DateTime.Now;
    //    unitOfWork.Booking.Add(booking);
    //    unitOfWork.Save();



    //var domain = Request.Scheme + "://" + Request.Host.Value + "/";
    //var options = new SessionCreateOptions
    //{
    //    LineItems = new List<SessionLineItemOptions>(),
    //    Mode = "payment",
    //    SuccessUrl = domain + $"booking/BookingConfirmation?bookingId={booking.Id}.html",
    //    CancelUrl = domain + $"booking/FinalizeBooking?villaId={booking.VillaId}&checkInDate={booking.CheckInDate}&nights={booking.Nights}.html"
    //};

    //options.LineItems.Add(new SessionLineItemOptions
    //{
    //    PriceData = new SessionLineItemPriceDataOptions()
    //    {
    //        UnitAmount = (long)(booking.TotalCost * 100),
    //        Currency = "usd",
    //        ProductData = new SessionLineItemPriceDataProductDataOptions()
    //        {
    //            Name = villa.Name
    //        }
    //    },
    //    Quantity = 1
    //});

    //var service = new SessionService();
    //Session session = service.Create(options);
    //unitOfWork.Booking.UpdateStripePaymentId(booking.Id, session.Id, session.PaymentIntentId);
    //unitOfWork.Save();
    //Response.Headers.Add("Location", session.Url);
    //return new StatusCodeResult(303);
    //}

    //[Authorize]
    //public IActionResult BookingConfirmation(int bookingId)
    //{
    //var bookingFromDb = unitOfWork.Booking.Get(a => a.Id == bookingId
    //    , includeProperties: "User,Villa");

    //if (bookingFromDb.Status == SD.StatusPending)
    //{
    //    var service = new SessionService();
    //    Session session = service.Get(bookingFromDb.StripeSessionId);
    //    if (session.PaymentStatus == "paid")
    //    {
    //        unitOfWork.Booking.UpdateStatus(bookingFromDb.Id, SD.StatusApproved);
    //        unitOfWork.Booking.UpdateStripePaymentId(bookingFromDb.Id, session.Id, session.PaymentIntentId);
    //        unitOfWork.Save();
    //    }
    //}

    //return View(bookingId);
    //}
    #endregion

    #region API calls
    [HttpGet]
    [Authorize]
    public IActionResult GetAll()
    {
        IEnumerable<Booking> objBookings;

        if (User.IsInRole(SD.Role_Admin))
        {
            objBookings = _unitOfWork.Booking.GetAll(includeProperties: "User,Villa");

        }
        else
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            objBookings = _unitOfWork.Booking
                .GetAll(q => q.UserId == userId, includeProperties: "User,Villa");
        }
        return Json(new { data = objBookings });

    }
    #endregion
}
