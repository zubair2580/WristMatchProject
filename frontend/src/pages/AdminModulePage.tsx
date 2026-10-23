import { Search } from 'lucide-react'
import { useMemo, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { useParams } from 'react-router-dom'
import { apiUrl } from '../store/api'

const modules: Record<string, { title: string; eyebrow: string; description: string; columns: string[]; state: string }> = {
  orders: { title: 'Orders', eyebrow: 'SALES OPERATIONS', description: 'Checkout orders, payment state, and fulfilment readiness.', columns: ['Order', 'Customer', 'Placed', 'Total', 'Payment', 'Fulfilment'], state: 'Live' },
  invoices: { title: 'Invoices', eyebrow: 'ACCOUNTS RECEIVABLE', description: 'Sales invoices and outstanding balances.', columns: ['Invoice', 'Customer', 'Issued', 'Due', 'Total', 'Balance', 'Status'], state: 'Live' },
  stock: { title: 'Stock', eyebrow: 'INVENTORY CONTROL', description: 'Warehouse balances and stock movements.', columns: ['SKU', 'Product', 'Warehouse', 'On hand', 'Reserved', 'Available', 'Average cost'], state: 'Live' },
  purchases: { title: 'Purchases', eyebrow: 'PROCUREMENT', description: 'Purchase orders linked to suppliers and warehouses.', columns: ['Purchase order', 'Supplier', 'Warehouse', 'Expected', 'Total', 'Status'], state: 'Live' },
  accounts: { title: 'Chart of accounts', eyebrow: 'GENERAL LEDGER', description: 'Company chart of accounts.', columns: ['Code', 'Account', 'Type', 'Normal balance', 'Current balance', 'Status'], state: 'Live' },
  reports: { title: 'Reports', eyebrow: 'BUSINESS INTELLIGENCE', description: 'Operational and financial report catalogue.', columns: ['Report', 'Module', 'Owner', 'Last run', 'Format', 'Status'], state: 'Live' },
  tryons: { title: 'Try-on jobs', eyebrow: 'STUDIO OPERATIONS', description: 'Virtual try-on jobs: queued, processing, succeeded, and failed.', columns: ['Job', 'Product', 'Created', 'State', 'Provider task', 'Duration'], state: 'Live' },
}

export default function AdminModulePage() {
  const { module = '' } = useParams()
  const definition = modules[module] ?? modules.orders
  const liveModules = Object.keys(modules)
  const [search, setSearch] = useState('')
  const query = useQuery<{ items: Record<string, unknown>[]; total: number }>({
    queryKey: ['demo-module', module, search],
    enabled: liveModules.includes(module),
    queryFn: async () => {
      const endpoint = module === 'tryons'
        ? `${apiUrl}/admin/tryon-jobs?search=${encodeURIComponent(search)}&page=1&pageSize=100`
        : `${apiUrl}/admin/demo-modules/${module}`
      const response = await fetch(endpoint, { credentials: 'include' })
      if (!response.ok) throw new Error('Unable to load module records')
      const payload = await response.json()
      if (module === 'tryons') {
        return {
          total: payload.total,
          items: (payload.items as Record<string, unknown>[]).map(item => ({
            Job: String(item.job ?? item.Job ?? item.id).slice(0, 8),
            Product: item.product ?? item.Product,
            Created: item.created ?? item.Created,
            State: item.state ?? item.State,
            'Provider task': item.providerTask ?? item.ProviderTask,
            Duration: item.duration ?? item.Duration ?? '—',
          })),
        }
      }
      return payload
    },
  })
  const rows = useMemo(() => {
    const items = query.data?.items ?? []
    if (module === 'tryons') return items
    const term = search.trim().toLowerCase()
    if (!term) return items
    return items.filter(row => definition.columns.some(column => String(row[column] ?? '').toLowerCase().includes(term)))
  }, [query.data?.items, search, definition.columns, module])

  return (
    <div>
      <div className="page-heading">
        <div>
          <p className="eyebrow">{definition.eyebrow}</p>
          <h1>{definition.title}</h1>
          <p>{definition.description}</p>
        </div>
        <div className="module-state"><span className="module-state-dot" /><strong>Read-only</strong><span>{definition.state}</span></div>
      </div>
      <section className="panel table-panel">
        <div className="table-toolbar">
          <div className="search-box">
            <Search size={17} />
            <input value={search} onChange={event => setSearch(event.target.value)} placeholder={`Search ${definition.title.toLowerCase()}`} />
          </div>
        </div>
        <div className="table-scroll">
          <table>
            <thead>
              <tr>
                <th>Sr #</th>
                {definition.columns.map(column => <th key={column}>{column}</th>)}
              </tr>
            </thead>
            <tbody>
              {rows.map((row, index) => (
                <tr key={index}>
                  <td>{index + 1}</td>
                  {definition.columns.map(column => <td key={column}>{formatValue(row[column])}</td>)}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        {query.isPending && <div className="module-empty"><h2>Loading live records...</h2></div>}
        {!query.isPending && rows.length === 0 && (
          <div className="module-empty">
            <div className="module-empty-mark">W</div>
            <h2>{search ? 'No matching records' : 'No records to display'}</h2>
            <p>{search ? 'Try a different search term.' : 'There are no records in this local testing database for this module.'}</p>
          </div>
        )}
        <div className="table-pagination">
          <span>{rows.length} of {query.data?.total ?? 0} records</span>
          <div><span>Page 1 of 1</span></div>
        </div>
      </section>
    </div>
  )
}

function formatValue(value: unknown) {
  if (value === null || value === undefined) return 'Not provided'
  if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}T\d{2}:/.test(value)) return new Date(value).toLocaleDateString()
  if (typeof value === 'number') return value.toLocaleString()
  return String(value)
}
