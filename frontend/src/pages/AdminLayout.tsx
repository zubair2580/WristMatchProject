import { BarChart3, Boxes, ChevronDown, CircleDollarSign, CircleUserRound, ClipboardList, KeyRound, LayoutDashboard, LogOut, ReceiptText, ShoppingCart, Sparkles, Tags, Truck, Users, Warehouse, type LucideIcon } from 'lucide-react'
import { NavLink, Navigate, Outlet, useLocation, useNavigate } from 'react-router-dom'
import { apiUrl } from '../store/api'
import { useAdmin } from './AdminGuard'

const groups: { label: string; items: { to: string; label: string; icon: LucideIcon; menuKey: string; end?: boolean }[] }[] = [
  { label: 'Workspace', items: [{ to: '/admin', label: 'Overview', icon: LayoutDashboard, menuKey: 'overview', end: true }] },
  { label: 'Commerce', items: [{ to: '/admin/products', label: 'Products', icon: Boxes, menuKey: 'products' }, { to: '/admin/categories', label: 'Categories', icon: Tags, menuKey: 'categories' }, { to: '/admin/customers', label: 'Customers', icon: Users, menuKey: 'customers' }, { to: '/admin/orders', label: 'Orders', icon: ShoppingCart, menuKey: 'orders' }, { to: '/admin/invoices', label: 'Invoices', icon: ReceiptText, menuKey: 'invoices' }] },
  { label: 'Inventory', items: [{ to: '/admin/stock', label: 'Stock', icon: Warehouse, menuKey: 'stock' }, { to: '/admin/warehouses', label: 'Warehouses', icon: Warehouse, menuKey: 'warehouses' }, { to: '/admin/suppliers', label: 'Suppliers', icon: Truck, menuKey: 'suppliers' }, { to: '/admin/purchases', label: 'Purchases', icon: ClipboardList, menuKey: 'purchases' }] },
  { label: 'Finance', items: [{ to: '/admin/accounts', label: 'Accounts', icon: CircleDollarSign, menuKey: 'accounts' }, { to: '/admin/reports', label: 'Reports', icon: BarChart3, menuKey: 'reports' }] },
  { label: 'Operations', items: [{ to: '/admin/tryons', label: 'Try-on jobs', icon: Sparkles, menuKey: 'tryons' }, { to: '/admin/users', label: 'User management', icon: CircleUserRound, menuKey: 'users' }, { to: '/admin/permissions', label: 'Permissions', icon: KeyRound, menuKey: 'permissions' }] },
]

function pathMenuKey(pathname: string) {
  if (pathname === '/admin') return 'overview'
  const segment = pathname.replace(/^\/admin\/?/, '').split('/')[0]
  return segment || 'overview'
}

export default function AdminLayout() {
  const location = useLocation()
  const navigate = useNavigate()
  const admin = useAdmin()
  const canView = (menuKey: string) => {
    if (admin.roles.includes('SuperAdmin')) return true
    if (menuKey === 'permissions') return Boolean(admin.menus.users?.canView || admin.menus.permissions?.canView)
    return Boolean(admin.menus[menuKey]?.canView)
  }
  const visibleGroups = groups
    .map(group => ({ ...group, items: group.items.filter(item => canView(item.menuKey)) }))
    .filter(group => group.items.length > 0)
  const current = groups.flatMap(group => group.items).find(item => item.to === location.pathname)?.label ?? 'Operations'
  const activeKey = pathMenuKey(location.pathname)
  const allowed = canView(activeKey)
  const logout = async () => {
    await fetch(`${apiUrl}/admin/logout`, { method: 'POST', credentials: 'include' })
    navigate('/admin/login', { replace: true })
  }
  if (!allowed) return <Navigate to="/admin" replace />
  return (
    <div className="admin-shell">
      <aside className="admin-sidebar">
        <a className="admin-brand" href="/admin"><span>W</span><div>WRISTMATCH<small>COMMERCE ERP</small></div></a>
        <nav className="admin-nav admin-nav-scroll">
          {visibleGroups.map(group => (
            <div className="admin-nav-group" key={group.label}>
              <div className="admin-nav-label">{group.label}</div>
              {group.items.map(({ to, label, icon: Icon, end }) => (
                <NavLink key={to} to={to} end={end} className={({ isActive }) => (isActive ? 'active' : '')}>
                  <Icon size={17} /><span>{label}</span>
                </NavLink>
              ))}
            </div>
          ))}
        </nav>
        <div className="sidebar-bottom">
          <NavLink to="/" className="back-to-store">View storefront <ChevronDown size={15} /></NavLink>
          <button className="admin-logout" onClick={logout}><LogOut size={17} /> Sign out</button>
        </div>
      </aside>
      <div className="admin-main">
        <header className="admin-topbar">
          <div><span className="admin-context">Workspace /</span><strong> {current}</strong></div>
          <div className="admin-profile">
            <span className="online-dot" />
            <div><strong>{admin.displayName}</strong><small>{admin.email}</small></div>
            <CircleUserRound size={27} />
          </div>
        </header>
        <main className="admin-content"><Outlet /></main>
      </div>
    </div>
  )
}
