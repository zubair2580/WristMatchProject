import { ArrowUpRight, Boxes, PackageCheck, Receipt, ShieldCheck, ShoppingBag, Truck, Users, Wallet } from 'lucide-react'
import { useQuery } from '@tanstack/react-query'
import { apiUrl } from '../store/api'
import { useAdmin } from './AdminGuard'

type DataPoint = { label: string; value: number }
type Commerce = {
  periodDays: number
  salesTotal: number
  revenueTotal: number
  outstandingAr: number
  openOrders: number
  issuedInvoices: number
  hasSales: boolean
  hasRevenue: boolean
  salesByDay: DataPoint[]
  revenueByDay: DataPoint[]
}
type Overview = {
  activeProducts: number
  readyProducts: number
  adminUsers: number
  customers: number
  suppliers: number
  averageProductPrice: number
  priceBands: DataPoint[]
  materials: DataPoint[]
  providerMode: string
  commerce: Commerce
}

export default function AdminDashboard() {
  const admin = useAdmin()
  const { data, isPending } = useQuery<Overview>({
    queryKey: ['admin-overview'],
    queryFn: async () => {
      const response = await fetch(`${apiUrl}/admin/overview`, { credentials: 'include' })
      if (!response.ok) throw new Error('Unable to load overview')
      return response.json()
    },
  })

  const commerce = data?.commerce
  const maxBand = Math.max(1, ...(data?.priceBands.map(item => item.value) ?? [1]))
  const materialTotal = Math.max(1, data?.materials.reduce((sum, item) => sum + item.value, 0) ?? 1)
  const first = ((data?.materials[0]?.value ?? 0) / materialTotal) * 100
  const second = first + ((data?.materials[1]?.value ?? 0) / materialTotal) * 100
  const donut = `conic-gradient(#b78c50 0 ${first}%, #6f8975 ${first}% ${second}%, #4d6170 ${second}% 100%)`
  const hour = new Date().getHours()
  const greeting = hour < 12 ? 'Good morning' : hour < 18 ? 'Good afternoon' : 'Good evening'

  return (
    <div>
      <div className="page-heading">
        <div>
          <p className="eyebrow">OPERATIONS</p>
          <h1>{greeting}, {admin.displayName.split(' ')[0]}.</h1>
          <p>Live catalogue health plus sales and collections from posted orders and payments.</p>
        </div>
      </div>

      <section className="metric-grid dashboard-metrics">
        <div className="metric-card">
          <div className="metric-icon gold"><ShoppingBag size={19} /></div>
          <span>Sales ({commerce?.periodDays ?? 14}d)</span>
          <strong>{commerce ? formatMoney(commerce.salesTotal) : '—'}</strong>
          <small>Confirmed and completed orders</small>
        </div>
        <div className="metric-card">
          <div className="metric-icon green"><Wallet size={19} /></div>
          <span>Collected ({commerce?.periodDays ?? 14}d)</span>
          <strong>{commerce ? formatMoney(commerce.revenueTotal) : '—'}</strong>
          <small>Confirmed payment receipts</small>
        </div>
        <div className="metric-card">
          <div className="metric-icon blue"><Receipt size={19} /></div>
          <span>Outstanding AR</span>
          <strong>{commerce ? formatMoney(commerce.outstandingAr) : '—'}</strong>
          <small>{commerce ? `${commerce.issuedInvoices} issued/paid invoices` : 'Loading'}</small>
        </div>
        <div className="metric-card">
          <div className="metric-icon charcoal"><PackageCheck size={19} /></div>
          <span>Open orders</span>
          <strong>{commerce?.openOrders ?? '—'}</strong>
          <small>Confirmed or processing</small>
        </div>
      </section>

      <div className="dashboard-chart-grid">
        <section className="panel chart-panel">
          <div className="panel-heading">
            <div>
              <p className="eyebrow">SALES TREND</p>
              <h2>Order value · last {commerce?.periodDays ?? 14} days</h2>
            </div>
            <a href="/admin/orders" className="text-link">View orders <ArrowUpRight size={15} /></a>
          </div>
          <TrendChart points={commerce?.salesByDay} empty={!commerce?.hasSales} emptyLabel="No posted sales in this period yet." loading={isPending} />
        </section>
        <section className="panel chart-panel">
          <div className="panel-heading">
            <div>
              <p className="eyebrow">REVENUE</p>
              <h2>Payments collected</h2>
            </div>
            <a href="/admin/invoices" className="text-link">View invoices <ArrowUpRight size={15} /></a>
          </div>
          <TrendChart points={commerce?.revenueByDay} empty={!commerce?.hasRevenue} emptyLabel="No confirmed payments in this period yet." loading={isPending} accent="green" />
        </section>
      </div>

      <section className="metric-grid dashboard-metrics">
        <div className="metric-card">
          <div className="metric-icon gold"><PackageCheck size={19} /></div>
          <span>Active products</span>
          <strong>{data?.activeProducts ?? '—'}</strong>
          <small>{data ? `${data.readyProducts} ready for try-on` : 'Loading'}</small>
        </div>
        <div className="metric-card">
          <div className="metric-icon green"><Users size={19} /></div>
          <span>Customers</span>
          <strong>{data?.customers ?? '—'}</strong>
          <small>Guest and registered profiles</small>
        </div>
        <div className="metric-card">
          <div className="metric-icon blue"><Truck size={19} /></div>
          <span>Suppliers</span>
          <strong>{data?.suppliers ?? '—'}</strong>
          <small>Active supply partners</small>
        </div>
        <div className="metric-card">
          <div className="metric-icon charcoal"><Boxes size={19} /></div>
          <span>Average list price</span>
          <strong>{data ? formatMoney(data.averageProductPrice) : '—'}</strong>
          <small>Across active catalogue products</small>
        </div>
      </section>

      <div className="dashboard-chart-grid">
        <section className="panel chart-panel">
          <div className="panel-heading">
            <div>
              <p className="eyebrow">PRICE DISTRIBUTION</p>
              <h2>Catalogue by price band</h2>
            </div>
            <a href="/admin/products" className="text-link">View products <ArrowUpRight size={15} /></a>
          </div>
          <div className="bar-chart">
            {data?.priceBands.map(item => (
              <div className="bar-column" key={item.label}>
                <div className="bar-value">{item.value}</div>
                <div className="bar-track"><div style={{ height: `${Math.max(8, item.value / maxBand * 100)}%` }} /></div>
                <span>{item.label}</span>
              </div>
            ))}
          </div>
          {isPending && <div className="chart-loading">Loading live catalogue data...</div>}
        </section>
        <section className="panel chart-panel">
          <div className="panel-heading">
            <div>
              <p className="eyebrow">MATERIAL MIX</p>
              <h2>Product composition</h2>
            </div>
          </div>
          <div className="donut-wrap">
            <div className="donut-chart" style={{ background: donut }}>
              <div><strong>{data?.activeProducts ?? 0}</strong><span>products</span></div>
            </div>
            <div className="donut-legend">
              {data?.materials.map((item, index) => (
                <div key={item.label}><i className={`legend-${index % 3}`} /><span>{item.label}</span><strong>{item.value}</strong></div>
              ))}
            </div>
          </div>
        </section>
      </div>

      <div className="admin-grid-two dashboard-bottom">
        <section className="panel">
          <div className="panel-heading">
            <div>
              <p className="eyebrow">SYSTEM STATUS</p>
              <h2>Operational readiness</h2>
            </div>
          </div>
          <div className="health-row"><span className="health-status"><i /> Database connection</span><strong>Healthy</strong></div>
          <div className="health-row"><span className="health-status"><i /> Public catalogue API</span><strong>Healthy</strong></div>
          <div className="health-row"><span className="health-status pending"><i /> Try-on provider</span><strong>{data?.providerMode ?? 'Loading'}</strong></div>
        </section>
        <section className="panel dark-panel">
          <ShieldCheck size={22} />
          <p className="eyebrow">DATA INTEGRITY</p>
          <h2>Charts use live posted records.</h2>
          <p>Sales and revenue series come from confirmed orders and payments in the database. Empty charts mean nothing was posted in the selected window — not missing widgets.</p>
        </section>
      </div>
    </div>
  )
}

function TrendChart({ points, empty, emptyLabel, loading, accent = 'gold' }: { points?: DataPoint[]; empty?: boolean; emptyLabel: string; loading?: boolean; accent?: 'gold' | 'green' }) {
  const max = Math.max(1, ...(points?.map(item => item.value) ?? [1]))
  return (
    <div className="trend-chart-wrap">
      {!empty && (
        <div className={`bar-chart trend-chart accent-${accent}`}>
          {(points ?? []).map(item => (
            <div className="bar-column" key={item.label}>
              <div className="bar-value">{item.value > 0 ? compactMoney(item.value) : ''}</div>
              <div className="bar-track"><div style={{ height: `${item.value > 0 ? Math.max(8, item.value / max * 100) : 2}%` }} /></div>
              <span>{item.label}</span>
            </div>
          ))}
        </div>
      )}
      {empty && !loading && (
        <div className="chart-empty">
          <div className="module-empty-mark">W</div>
          <h2>Nothing posted yet</h2>
          <p>{emptyLabel}</p>
        </div>
      )}
      {loading && <div className="chart-loading">Loading commerce data...</div>}
    </div>
  )
}

function formatMoney(value: number) {
  return `PKR ${Math.round(value).toLocaleString()}`
}

function compactMoney(value: number) {
  if (value >= 100000) return `${Math.round(value / 1000)}k`
  if (value >= 1000) return `${(value / 1000).toFixed(value >= 10000 ? 0 : 1)}k`
  return String(Math.round(value))
}
