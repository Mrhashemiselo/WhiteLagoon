﻿namespace WhiteLagoon.Application.Common.DTOs;

public class RedialBarChartDto
{
    public decimal TotalCount { get; set; }
    public decimal CountInCurrentMonth { get; set; }
    public bool HasRatioIncreased { get; set; }
    public int[] Series { get; set; }
}
