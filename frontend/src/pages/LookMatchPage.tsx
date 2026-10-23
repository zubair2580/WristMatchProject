import { ArrowRight, Camera, Check, Sparkles, Upload } from 'lucide-react'
import { useEffect, useMemo, useRef, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { apiUrl } from '../store/api'
import { colourHex } from '../store/productExtras'
import { useStore } from '../store/StoreContext'

type LookMatchScore = { concern: string; region: string; rawScore?: number | null; uiScore?: number | null; score?: number | null }
type LookMatchProduct = { id: string; name: string; slug: string; colour: string; material: string; imageUrl: string; price: number; currency: string; matchScore: number; reason: string }
type LookMatchResult = {
  providerMode: string
  undertone: string
  preferredMetals: string[]
  preferredColours: string[]
  scores: LookMatchScore[]
  recommendations: LookMatchProduct[]
  summary: string
}

export default function LookMatchPage() {
  const inputRef = useRef<HTMLInputElement>(null)
  const navigate = useNavigate()
  const { addToCart, selection } = useStore()
  const [file, setFile] = useState<File | null>(null)
  const [consent, setConsent] = useState(false)
  const [analyzing, setAnalyzing] = useState(false)
  const [error, setError] = useState('')
  const [result, setResult] = useState<LookMatchResult | null>(null)
  const localPreview = useMemo(() => file ? URL.createObjectURL(file) : null, [file])
  useEffect(() => () => { if (localPreview) URL.revokeObjectURL(localPreview) }, [localPreview])

  const status = useQuery({
    queryKey: ['youcam-status'],
    queryFn: async () => {
      const response = await fetch(`${apiUrl}/look-match/status`)
      if (!response.ok) throw new Error('status')
      return response.json() as Promise<{ youCamConfigured: boolean; tryOnProviderMode: string }>
    },
  })

  const analyze = async () => {
    if (!file || !consent) { setError('Choose a face selfie and confirm consent.'); return }
    setAnalyzing(true)
    setError('')
    try {
      const session = await fetch(`${apiUrl}/sessions`, { method: 'POST', credentials: 'include' })
      if (!session.ok) throw new Error('A private session could not be created.')
      const body = new FormData()
      body.append('file', file)
      body.append('consent', 'true')
      body.append('consentVersion', '2026-10-11')
      const response = await fetch(`${apiUrl}/look-match`, { method: 'POST', credentials: 'include', body })
      if (!response.ok) {
        const problem = await response.json().catch(() => null) as { detail?: string } | null
        throw new Error(problem?.detail ?? 'Look Match could not analyse this selfie.')
      }
      setResult(await response.json() as LookMatchResult)
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Look Match failed.')
    } finally {
      setAnalyzing(false)
    }
  }

  const tryMatched = (productId: string) => {
    if (!selection.includes(productId)) addToCart(productId)
    navigate(`/studio?product=${productId}`)
  }

  return (
    <main className="look-match-page">
      <section className="look-match-hero">
        <div>
          <p className="eyebrow">YOUCAM SKIN AI · LOOK MATCH</p>
          <h1>Metal that flatters <em>your skin.</em></h1>
          <p>Upload a face selfie. YouCam Skin Analysis plus WristMatch undertone mapping ranks watches by metal and colour harmony — then jump straight into Watch VTO.</p>
        </div>
        <div className={`look-match-mode ${status.data?.youCamConfigured ? 'live' : 'demo'}`}>
          <Sparkles size={16} />
          <span>{status.data?.youCamConfigured ? 'Skin analysis connected' : 'Undertone matching is ready — connect YouCam for full skin scores'}</span>
        </div>
      </section>

      <div className="look-match-grid">
        <section className="look-match-upload">
          {localPreview ? <img src={localPreview} alt="Face selfie preview" /> : (
            <div className="look-match-empty">
              <Camera size={28} />
              <h2>Face selfie</h2>
              <p>Even light · face forward · no heavy filters</p>
            </div>
          )}
          <input ref={inputRef} type="file" accept="image/jpeg,image/png" hidden onChange={event => { setResult(null); setFile(event.target.files?.[0] ?? null); setError('') }} />
          <div className="upload-actions">
            <button className="outline-button" type="button" onClick={() => inputRef.current?.click()}><Camera size={16} /> Choose selfie</button>
            <button className="primary-button compact-button" type="button" disabled={!file || !consent || analyzing} onClick={analyze}>
              <Upload size={16} /> {analyzing ? 'Analysing...' : 'Run Look Match'}
            </button>
          </div>
          {file && (
            <label className="consent-card">
              <input type="checkbox" checked={consent} onChange={event => setConsent(event.target.checked)} />
              <span><strong>I consent to private Skin Analysis.</strong> Photo is session-scoped and expires automatically. <Link to="/privacy">Privacy</Link></span>
            </label>
          )}
          {error && <div className="studio-error">{error}</div>}
        </section>

        <aside className="look-match-side">
          {!result && (
            <div className="look-match-guide">
              <p className="eyebrow">HOW IT WORKS</p>
              <div><Check size={16} /><span>YouCam Skin Analysis scores texture, pores, wrinkles, and acne</span></div>
              <div><Check size={16} /><span>Local undertone estimate maps warm / cool / neutral</span></div>
              <div><Check size={16} /><span>Catalogue ranked for metal and strap colour harmony</span></div>
              <div><Check size={16} /><span>One tap opens Studio for YouCam Watch Virtual Try-On</span></div>
            </div>
          )}
          {result && (
            <div className="look-match-result">
              <p className="eyebrow">YOUR PROFILE · {result.providerMode.toUpperCase()}</p>
              <h2>{result.undertone} undertone</h2>
              <p>{result.summary}</p>
              <div className="look-match-tags">
                {result.preferredMetals.map(item => <span key={item}>{item}</span>)}
              </div>
              {result.scores.length > 0 && (
                <div className="look-match-scores">
                  {result.scores.slice(0, 6).map(item => (
                    <div key={`${item.concern}-${item.region}`}>
                      <strong>{item.concern}</strong>
                      <span>{item.region}</span>
                      <em>{item.uiScore ?? item.score ?? '—'}</em>
                    </div>
                  ))}
                </div>
              )}
            </div>
          )}
        </aside>
      </div>

      {result && (
        <section className="look-match-recs">
          <div className="look-match-recs-head">
            <div>
              <p className="eyebrow">RECOMMENDED WATCHES</p>
              <h2>Try these on your wrist</h2>
            </div>
            <Link className="text-link" to="/studio">Open Studio <ArrowRight size={15} /></Link>
          </div>
          <div className="look-match-cards">
            {result.recommendations.map(item => (
              <article key={item.id}>
                <img src={item.imageUrl} alt={item.name} />
                <div>
                  <strong>{item.name}</strong>
                  <small><i style={{ background: colourHex(item.colour) }} />{item.colour} · {item.material}</small>
                  <p>{item.reason}</p>
                  <div className="look-match-card-actions">
                    <span className="match-score">{item.matchScore}% match</span>
                    <button type="button" className="primary-button compact-button" onClick={() => tryMatched(item.id)}>Try on</button>
                  </div>
                </div>
              </article>
            ))}
          </div>
        </section>
      )}
    </main>
  )
}
