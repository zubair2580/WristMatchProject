import { ArrowLeft, Save } from 'lucide-react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { useState } from 'react'
import { apiUrl } from '../store/api'
import { adminMutation } from '../store/adminApi'

type Permission = { id: string; roleName: string; menuKey: string; canView: boolean; canCreate: boolean; canUpdate: boolean; canDelete: boolean }
const menus = ['overview', 'products', 'categories', 'customers', 'orders', 'invoices', 'stock', 'warehouses', 'suppliers', 'purchases', 'accounts', 'reports', 'tryons', 'users']

export default function AdminPermissionsPage() {
  const client = useQueryClient()
  const [role, setRole] = useState('SuperAdmin')
  const [menu, setMenu] = useState('overview')
  const [saving, setSaving] = useState(false)
  const query = useQuery<Permission[]>({ queryKey: ['admin-permissions'], queryFn: async () => { const response = await fetch(`${apiUrl}/admin/menu-permissions`, { credentials: 'include' }); if (!response.ok) throw new Error('Unable to load permissions'); return response.json() } })
  const roles = [...new Set(query.data?.map(item => item.roleName) ?? [])]
  const current = query.data?.find(item => item.roleName === role && item.menuKey === menu)
  const update = async (key: keyof Pick<Permission, 'canView' | 'canCreate' | 'canUpdate' | 'canDelete'>, value: boolean) => {
    if (!current) return
    setSaving(true)
    await adminMutation('/admin/menu-permissions', 'PUT', JSON.stringify({ roleName: role, menuKey: menu, canView: key === 'canView' ? value : current.canView, canCreate: key === 'canCreate' ? value : current.canCreate, canUpdate: key === 'canUpdate' ? value : current.canUpdate, canDelete: key === 'canDelete' ? value : current.canDelete }), { 'Content-Type': 'application/json' })
    await client.invalidateQueries({ queryKey: ['admin-permissions'] })
    setSaving(false)
  }
  return <div><div className="page-heading"><div><Link className="back-link" to="/admin/users"><ArrowLeft size={15} /> Back to user management</Link><p className="eyebrow">ACCESS CONTROL</p><h1>Menu permissions</h1><p>Configure which roles can view and mutate each admin module.</p></div></div><section className="panel permission-panel"><div className="permission-selects"><label>Role<select value={role} onChange={event => setRole(event.target.value)}>{roles.map(item => <option key={item}>{item}</option>)}</select></label><label>Menu<select value={menu} onChange={event => setMenu(event.target.value)}>{menus.map(item => <option key={item}>{item}</option>)}</select></label></div><div className="permission-grid">{(['canView', 'canCreate', 'canUpdate', 'canDelete'] as const).map(key => <label key={key}><input type="checkbox" checked={Boolean(current?.[key])} disabled={!current || saving} onChange={event => update(key, event.target.checked)} /><span>{key.replace('can', '')}</span></label>)}</div><div className="permission-note"><Save size={16} /> Changes save immediately to the Identity permission database.</div></section></div>
}
