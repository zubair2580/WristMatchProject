import { ArrowLeft, ShieldCheck } from 'lucide-react'
import { Link } from 'react-router-dom'

export default function PrivacyPage() {
  return <main className="privacy-page"><Link className="back-link" to="/"><ArrowLeft size={15} /> Back to WristMatch</Link><p className="eyebrow">YOUR PRIVACY</p><h1>Private by<br /><em>design.</em></h1><div className="privacy-content"><ShieldCheck size={25} /><div><h2>What happens to your photo?</h2><p>Photos are uploaded only after you give explicit consent. They are normalized, stored privately, and linked to an opaque guest session rather than a shopper account.</p><h2>How long is it kept?</h2><p>Private photos and generated results expire after the configured retention period. Deleting a photo immediately blocks local access; physical cleanup completes through the background cleanup process.</p><h2>What the preview means</h2><p>The result is a virtual try-on preview. It is not a measurement, guarantee of physical fit, proof of authenticity, or live AR tracking.</p></div></div></main>
}
