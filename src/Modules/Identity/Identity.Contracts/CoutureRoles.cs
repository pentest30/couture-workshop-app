namespace Couture.Identity.Contracts;

public static class CoutureRoles
{
    public const string Manager = "Manager";
    public const string Tailor = "Tailor";
    public const string Embroiderer = "Embroiderer";
    public const string Beader = "Beader";
    public const string Cashier = "Cashier";

    public static readonly IReadOnlyDictionary<string, string[]> RolePermissions = new Dictionary<string, string[]>
    {
        [Manager] =
        [
            CouturePermissions.OrdersCreate, CouturePermissions.OrdersView, CouturePermissions.OrdersUpdate,
            CouturePermissions.OrdersChangeStatus, CouturePermissions.OrdersDeliver,
            CouturePermissions.ClientsCreate, CouturePermissions.ClientsView, CouturePermissions.ClientsUpdate,
            CouturePermissions.FinanceRecord, CouturePermissions.FinanceView,
            CouturePermissions.DashboardView, CouturePermissions.DashboardViewFinance,
            CouturePermissions.NotificationsView, CouturePermissions.NotificationsConfigure,
            CouturePermissions.UsersManage, CouturePermissions.SettingsManage,
        ],
        [Tailor] =
        [
            CouturePermissions.OrdersCreate, CouturePermissions.OrdersViewOwn, CouturePermissions.OrdersChangeStatusOwn,
            CouturePermissions.ClientsCreate, CouturePermissions.ClientsView,
            CouturePermissions.NotificationsView,
        ],
        [Embroiderer] =
        [
            CouturePermissions.OrdersViewOwn, CouturePermissions.OrdersChangeStatusOwn,
            CouturePermissions.NotificationsView,
        ],
        [Beader] =
        [
            CouturePermissions.OrdersViewOwn, CouturePermissions.OrdersChangeStatusOwn,
            CouturePermissions.NotificationsView,
        ],
        [Cashier] =
        [
            CouturePermissions.OrdersView, CouturePermissions.OrdersDeliver,
            CouturePermissions.ClientsCreate, CouturePermissions.ClientsView,
            CouturePermissions.FinanceRecord, CouturePermissions.FinanceView,
            CouturePermissions.DashboardViewFinance,
            CouturePermissions.NotificationsView,
        ],
    };
}
