namespace WristMatch.Infrastructure;

public sealed class AdminMenuPermission
{
    private AdminMenuPermission() { }

    public AdminMenuPermission(string roleName, string menuKey, bool canView, bool canCreate, bool canUpdate, bool canDelete)
    {
        Id = Guid.NewGuid();
        RoleName = roleName;
        MenuKey = menuKey;
        CanView = canView;
        CanCreate = canCreate;
        CanUpdate = canUpdate;
        CanDelete = canDelete;
    }

    public Guid Id { get; private set; }
    public string RoleName { get; private set; } = string.Empty;
    public string MenuKey { get; private set; } = string.Empty;
    public bool CanView { get; private set; }
    public bool CanCreate { get; private set; }
    public bool CanUpdate { get; private set; }
    public bool CanDelete { get; private set; }

    public void Update(bool canView, bool canCreate, bool canUpdate, bool canDelete)
    {
        CanView = canView;
        CanCreate = canCreate;
        CanUpdate = canUpdate;
        CanDelete = canDelete;
    }
}
