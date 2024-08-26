using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Common.Utilities;
public class SD //StaticDetail
{
    public const string Role_Customer = "Customer";
    public const string Role_Admin = "Admin";

    public const string StatusPending = "Pending"; //when user click on checkout now
    public const string StatusApproved = "Approved"; //when payment is successful
    public const string StatusCheckedIn = "CheckedIn";
    public const string StatusCompleted = "Completed"; //checkout
    public const string StatusCanceled = "Canceled";
    public const string StatusRefunded = "Refunded";

    public static int VillaRoomsAvailable_Count(int villaId,
        List<VillaNumber> villaNumberList, DateOnly checkInDate, int nights,
        List<Booking> bookings)
    {
        List<int> bookingInDate = new();
        int finalAvailableRoomForAllNights = int.MaxValue;
        var roomInVilla = villaNumberList
            .Where(x => x.VillaId == villaId)
            .Count();
        for (int i = 0; i < nights; i++)
        {
            var villasBooked = bookings
                .Where(a =>
                a.CheckInDate <= checkInDate.AddDays(i) &&
                a.CheckOutDate > checkInDate.AddDays(i) &&
                a.VillaId == villaId);

            foreach (var booking in villasBooked)
            {
                if (!bookingInDate.Contains(booking.Id))
                {
                    bookingInDate.Add(booking.Id);
                }
            }

            var totalAvailableRooms = roomInVilla - bookingInDate.Count;
            if (totalAvailableRooms == 0)
            {
                return 0;
            }
            else
            {
                if (finalAvailableRoomForAllNights > totalAvailableRooms)
                {
                    finalAvailableRoomForAllNights = totalAvailableRooms;
                }
            }
        }
        return finalAvailableRoomForAllNights;
    }
}
