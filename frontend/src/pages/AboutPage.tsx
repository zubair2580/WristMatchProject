import { ArrowRight } from 'lucide-react'
import { Link } from 'react-router-dom'

const pillars = [
  { title: 'Considered curation', copy: 'A small collection chosen for design, versatility, and the rituals of everyday wear.' },
  { title: 'Private previews', copy: 'Upload once, preview privately, and decide with more certainty before you buy.' },
  { title: 'Human support', copy: 'WhatsApp, phone, and our studio guide are here when a recommendation needs a person.' },
]

export default function AboutPage() {
  return (
    <main className="about-page">
      <section className="about-hero">
        <p className="eyebrow">ABOUT WRISTMATCH</p>
        <h1>Less guessing.<br /><em>More knowing.</em></h1>
      </section>

      <section className="about-story">
        <div>
          <p className="eyebrow">OUR STORY</p>
          <h2>Built in Karachi for considered choices</h2>
        </div>
        <div>
          <p>WristMatch began with a simple frustration: product photographs show a watch, but they rarely help you imagine it as yours. We built a quieter storefront and a private try-on studio so decisions feel calmer, clearer, and more personal.</p>
          <p>Every piece in the collection is selected for proportion, material quality, and the way it moves from day to evening — not for noise.</p>
          <Link className="primary-button" to="/catalog">Explore the collection <ArrowRight size={17} /></Link>
        </div>
      </section>

      <section className="about-pillars">
        {pillars.map(item => (
          <article key={item.title}>
            <h3>{item.title}</h3>
            <p>{item.copy}</p>
          </article>
        ))}
      </section>

      <section className="about-cta">
        <div>
          <p className="eyebrow">VISIT OR WRITE</p>
          <h2>We would love to hear from you</h2>
        </div>
        <Link className="text-link" to="/contact">Contact us <ArrowRight size={15} /></Link>
      </section>
    </main>
  )
}
