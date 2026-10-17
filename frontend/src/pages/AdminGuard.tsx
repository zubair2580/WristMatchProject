import { useQuery } from '@tanstack/react-query'
import { createContext, useContext } from 'react'
import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { apiUrl } from '../store/api'

export type MenuAccess = { canView: boolean; canCreate: boolean; canUpdate: boolean; canDelete: boolean }
export type AdminUser = { id: string; displayName: string; email: string; roles: string[]; menus: Record<string, MenuAccess> }
const AdminContext = createContext<AdminUser | null>(null)

export function useAdmin() {
  const user = useContext(AdminContext)
  if (!user) throw new Error('Admin authentication is required')
  return user
}

export function useCanView(menuKey: string) {
  const admin = useAdmin()
  if (admin.roles.includes('SuperAdmin')) return true
  return Boolean(admin.menus[menuKey]?.canView)
}

export default function AdminGuard() {
  const location = useLocation()
  const { data, isPending, isError } = useQuery<AdminUser>({
    queryKey: ['admin-session'],
    retry: false,
    queryFn: async () => {
      const response = await fetch(`${apiUrl}/admin/me`, { credentials: 'include' })
      if (!response.ok) throw new Error('Unauthenticated')
      return response.json()
    },
  })
  if (isPending) return <div className="admin-auth-loading"><div className="admin-auth-mark">W</div><span>Securing your workspace...</span></div>
  if (isError || !data) return <Navigate to="/admin/login" state={{ from: location.pathname }} replace />
  return <AdminContext.Provider value={data}><Outlet /></AdminContext.Provider>
}
