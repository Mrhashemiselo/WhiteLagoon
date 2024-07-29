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
}
