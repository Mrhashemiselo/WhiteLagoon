﻿namespace WhiteLagoon.Web.ViewModels;

public class RedialBarChartViewModel
{
    public decimal TotalCount { get; set; }
    public decimal CountInCurrentMonth { get; set; }
    public bool HasRatioIncreased { get; set; }
    public int[] Series { get; set; }
}
