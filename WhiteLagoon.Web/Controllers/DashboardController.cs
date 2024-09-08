using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Services.Interface;

namespace WhiteLagoon.Web.Controllers;
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> GetTotalBookingRedialChartData()
    {
        return Json(await _dashboardService.GetTotalBookingRedialChartDate());
    }

    public async Task<IActionResult> GetRegisteredUserChartData()
    {
        return Json(await _dashboardService.GetRegisterUserChartDate());
    }

    public async Task<IActionResult> GetRevenueChartData()
    {
        return Json(await _dashboardService.GetRevenueChartDate());
    }

    public async Task<IActionResult> GetBookingPieChartData()
    {
        return Json(await _dashboardService.GetBookingPieChartDate());
    }

    public async Task<IActionResult> GetMemberAndBookingLineChartData()
    {
        return Json(await _dashboardService.GetMemberAndBookingLineChartDate());
    }


}
