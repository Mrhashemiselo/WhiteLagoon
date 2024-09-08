namespace WhiteLagoon.Application.Common.DTOs;

public class LineChartDto
{
    public List<ChartData> Series { get; set; }
    public string[] Categories { get; set; }
}
