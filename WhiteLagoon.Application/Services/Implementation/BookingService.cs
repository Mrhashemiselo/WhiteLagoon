using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utilities;
using WhiteLagoon.Application.Services.Interface;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Implementation;
public class BookingService(IUnitOfWork unitOfWork) : IBookingService
{
    public void CreateBooking(Booking booking)
    {
        unitOfWork.Booking.Add(booking);
        unitOfWork.Save();
    }

    public IEnumerable<Booking> GetAllBookings(string userId = "", string? statusFilterList = "")
    {
        IEnumerable<string> statusList = statusFilterList.ToLower().Split(",");
        if (!string.IsNullOrEmpty(statusFilterList) && !string.IsNullOrEmpty(userId))
        {
            return unitOfWork.Booking.GetAll(t => statusList.Contains(t.Status.ToLower()) &&
            t.UserId == userId, includeProperties: "User,Villa");
        }
        else
        {
            if (!string.IsNullOrEmpty(statusFilterList))
            {
                return unitOfWork.Booking.GetAll(a => statusList.Contains(a.Status.ToLower()),
                    includeProperties: "User,Villa");
            }
            if (!string.IsNullOrEmpty(userId))
            {
                return unitOfWork.Booking.GetAll(a => a.UserId == userId, includeProperties: "User,Villa");
            }
        }
        return unitOfWork.Booking.GetAll(includeProperties: "User,Villa");
    }

    public Booking GetBookingById(int bookingId)
    {
        return unitOfWork.Booking.Get(s => s.Id == bookingId, includeProperties: "User,Villa");
    }

    public IEnumerable<int> GetCheckedInVillaNumbers(int villaId)
    {
        return unitOfWork.Booking.GetAll(u => u.VillaId == villaId && u.Status == SD.StatusCheckedIn)
            .Select(a => a.VillaNumber); ;
    }

    public void UpdateStatus(int bookingId, string bookingStatus, int villaNumber = 0)
    {
        var bookingFromDb = unitOfWork.Booking
            .Get(w => w.Id == bookingId, tracked: true);
        if (bookingFromDb != null)
        {
            bookingFromDb.Status = bookingStatus;
            if (bookingStatus == SD.StatusCheckedIn)
            {
                bookingFromDb.VillaNumber = villaNumber;
                bookingFromDb.ActualCheckInDate = DateTime.Now;
            }
            if (bookingStatus == SD.StatusCompleted)
            {
                bookingFromDb.ActualCheckOutDate = DateTime.Now;
            }
        }
    }

    public void UpdateStripePaymentId(int bookingId, string sessionId, string paymentIntentId)
    {
        var bookingFromDb = unitOfWork.Booking
            .Get(a => a.Id == bookingId, tracked: true);
        if (bookingFromDb != null)
        {
            if (!string.IsNullOrEmpty(sessionId))
            {
                bookingFromDb.StripeSessionId = sessionId;
            }
            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                bookingFromDb.StripePaymentIntentId = paymentIntentId;
                bookingFromDb.PaymentDate = DateTime.Now;
                bookingFromDb.IsPaymentSuccessful = true;
            }
        }
        unitOfWork.Save();
    }
}
