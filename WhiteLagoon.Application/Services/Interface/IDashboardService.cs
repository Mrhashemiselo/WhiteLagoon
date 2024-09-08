using WhiteLagoon.Application.Common.DTOs;

namespace WhiteLagoon.Application.Services.Interface;
public interface IDashboardService
{
    Task<RedialBarChartDto> GetTotalBookingRedialChartDate();
    Task<RedialBarChartDto> GetRegisterUserChartDate();
    Task<RedialBarChartDto> GetRevenueChartDate();
    Task<PieChartDto> GetBookingPieChartDate();
    Task<LineChartDto> GetMemberAndBookingLineChartDate();
}
