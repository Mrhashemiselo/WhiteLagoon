using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utilities;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers;
public class DashboardController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    static int previousMonth = DateTime.Now.Month == 1 ? 12 : DateTime.Now.Month - 1;
    readonly DateTime previousMonthStartDate = new(DateTime.Now.Year, previousMonth, 1);
    readonly DateTime currentMonthStartDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);
    public DashboardController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> GetTotalBookingRedialChartData()
    {
        var totalBookings = _unitOfWork.Booking.GetAll(t =>
        t.Status != SD.StatusPending || t.Status == SD.StatusCanceled);

        var countByCurrentMonth = totalBookings.Count(s =>
        s.BookingDate >= currentMonthStartDate && s.BookingDate <= DateTime.Now);

        var countByPreviousMonth = totalBookings.Count(s =>
        s.BookingDate >= previousMonthStartDate && s.BookingDate <= currentMonthStartDate);

        return Json(GetRedialCartDataModel(
            totalBookings.Count(),
            countByCurrentMonth,
            countByPreviousMonth));
    }

    public async Task<IActionResult> GetRegisteredUserChartData()
    {
        var totalUsers = _unitOfWork.User.GetAll();

        var countByCurrentMonth = totalUsers.Count(s =>
        s.CreatedAt >= currentMonthStartDate && s.CreatedAt <= DateTime.Now);

        var countByPreviousMonth = totalUsers.Count(s =>
        s.CreatedAt >= previousMonthStartDate && s.CreatedAt <= currentMonthStartDate);

        return Json(GetRedialCartDataModel(
            totalUsers.Count(),
            countByCurrentMonth,
            countByPreviousMonth));
    }

    public async Task<IActionResult> GetRevenueChartData()
    {
        var totalBookings = _unitOfWork.Booking.GetAll(u =>
         u.Status != SD.StatusPending || u.Status == SD.StatusCanceled);

        var totalRevenue = (int)totalBookings.Sum(s => s.TotalCost);

        var countByCurrentMonth = totalBookings
            .Where(s => s.BookingDate >= currentMonthStartDate &&
            s.BookingDate <= DateTime.Now)
            .Sum(s => s.TotalCost);

        var countByPreviousMonth = totalBookings
            .Where(s => s.BookingDate >= previousMonthStartDate &&
            s.BookingDate <= currentMonthStartDate)
            .Sum(s => s.TotalCost);

        return Json(GetRedialCartDataModel(
            totalRevenue,
            countByCurrentMonth,
            countByPreviousMonth));
    }

    public async Task<IActionResult> GetBookingPieChartData()
    {
        var totalBookings = _unitOfWork.Booking.GetAll(t => t.BookingDate >= DateTime.Now.AddDays(-30) &&
        (t.Status != SD.StatusPending || t.Status == SD.StatusCanceled));

        var customerWithOneBooking = totalBookings
            .GroupBy(a => a.UserId)
            .Where(s => s.Count() == 1)
            .Select(d => d.Key)
            .ToList();

        int bookingsByNewCustomer = customerWithOneBooking.Count();
        int bookingByReturnCustomer = totalBookings.Count() - bookingsByNewCustomer;

        PieChartViewModel pieChartVM = new()
        {
            Labels = ["New Customer Bookings", "Returning Customer Bookings"],
            Series = [bookingsByNewCustomer, bookingByReturnCustomer]
        };

        return Json(pieChartVM);
    }

    public async Task<IActionResult> GetMemberAndBookingLineChartData()
    {
        var bookingData = _unitOfWork.Booking.GetAll(w => w.BookingDate >= DateTime.Now.AddDays(-30) &&
        w.BookingDate.Date <= DateTime.Now)
            .GroupBy(d => d.BookingDate)
            .Select(u => new
            {
                DateTime = u.Key,
                NewBookingCount = u.Count()
            });

        var customerData = _unitOfWork.User.GetAll(w => w.CreatedAt >= DateTime.Now.AddDays(-30) &&
        w.CreatedAt.Date <= DateTime.Now)
            .GroupBy(d => d.CreatedAt.Date)
            .Select(u => new
            {
                DateTime = u.Key,
                NewCustomerCount = u.Count()
            });

        var leftJoin = bookingData
            .GroupJoin(customerData, booking => booking.DateTime, customer => customer.DateTime,
            (booking, customer) => new
            {
                booking.DateTime,
                booking.NewBookingCount,
                NewCustomerCount = customer
                .Select(s => s.NewCustomerCount)
                .FirstOrDefault()
            });

        var rightJoin = customerData
            .GroupJoin(bookingData, customer => customer.DateTime, booking => booking.DateTime,
            (customer, booking) => new
            {
                customer.DateTime,
                NewBookingCount = bookingData
                .Select(a => a.NewBookingCount)
                .FirstOrDefault(),
                customer.NewCustomerCount
            });

        var mergedDate = leftJoin
            .Union(rightJoin)
            .OrderBy(s => s.DateTime)
            .ToList();

        var newBookingData = mergedDate
            .Select(x => x.NewBookingCount)
            .ToArray();
        var newCustomerData = mergedDate
            .Select(x => x.NewCustomerCount)
            .ToArray();
        var categories = mergedDate.Select(a => a.DateTime.ToString("MM/dd/yyyy"))
            .ToArray();

        List<ChartData> chartDataList = new()
        {
            new ChartData()
            {
                Name = "New Bookings",
                Data = newBookingData
            },
            new ChartData()
            {
                 Name = "New Members",
                Data = newCustomerData
            }
        };

        LineChartViewModel lineChartVM = new()
        {
            Categories = categories,
            Series = chartDataList
        };

        return Json(lineChartVM);
    }

    private static RedialBarChartViewModel GetRedialCartDataModel(int totalCount, double currentMonthCount, double prevMonthCount)
    {
        RedialBarChartViewModel redialBarChartVM = new();

        int increaseDecreaseRatio = 100;

        if (prevMonthCount != 0)
        {
            increaseDecreaseRatio = Convert.ToInt32((currentMonthCount - prevMonthCount) / prevMonthCount * 100);
        }

        redialBarChartVM.TotalCount = totalCount;
        redialBarChartVM.CountInCurrentMonth = (int)currentMonthCount;
        redialBarChartVM.HasRatioIncreased = currentMonthCount > prevMonthCount;
        redialBarChartVM.Series = [increaseDecreaseRatio];

        return redialBarChartVM;
    }
}
