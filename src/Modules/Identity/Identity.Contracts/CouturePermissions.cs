namespace Couture.Identity.Contracts;

public static class CouturePermissions
{
    // Orders
    public const string OrdersCreate = "Permissions.Orders.Create";
    public const string OrdersView = "Permissions.Orders.View";
    public const string OrdersViewOwn = "Permissions.Orders.ViewOwn";
    public const string OrdersUpdate = "Permissions.Orders.Update";
    public const string OrdersChangeStatus = "Permissions.Orders.ChangeStatus";
    public const string OrdersChangeStatusOwn = "Permissions.Orders.ChangeStatusOwn";
    public const string OrdersDeliver = "Permissions.Orders.Deliver";

    // Clients
    public const string ClientsCreate = "Permissions.Clients.Create";
    public const string ClientsView = "Permissions.Clients.View";
    public const string ClientsUpdate = "Permissions.Clients.Update";

    // Finance
    public const string FinanceRecord = "Permissions.Finance.Record";
    public const string FinanceView = "Permissions.Finance.View";

    // Dashboard
    public const string DashboardView = "Permissions.Dashboard.View";
    public const string DashboardViewFinance = "Permissions.Dashboard.ViewFinance";

    // Notifications
    public const string NotificationsView = "Permissions.Notifications.View";
    public const string NotificationsConfigure = "Permissions.Notifications.Configure";

    // Users
    public const string UsersManage = "Permissions.Users.Manage";

    // Settings
    public const string SettingsManage = "Permissions.Settings.Manage";
}
