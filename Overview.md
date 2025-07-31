### Project Description — Prism AI: Personal Experience Curator  

**What it does**  
Prism AI turns any theme, mood, or “I-feel-like…” prompt into a fully-fledged, cross-domain experience in seconds. Powered by Qloo’s Taste AI and an **LLM that is profile-aware**, it aligns a user’s age, gender, and saved interests with the hidden affinities Qloo uncovers, then assembles a cohesive set of recommendations—movies, music, food, destinations, activities, and more—presented on an interactive dashboard. Whether someone wants an *“Urban Family Exploration”* weekend or an *“Ultimate Science-Fiction Watchathon”*, Prism AI hand-picks items that naturally belong together, explains *why* they fit, and lets the user swap any card for instant alternatives.

**Key features & functionality**

| Area | Highlights |
|------|------------|
| **LLM-Driven Qloo Orchestration** | The LLM both **generates** the appropriate multi-endpoint Qloo requests (tailoring parameters to the user’s profile) **and** **interprets** the complex responses into friendly, theme-coherent cards—essential for wrangling Qloo’s flexible but intricate API at scale. |
| **Cross-Domain Curation** | Fetches high-affinity picks in multiple categories (movie, restaurant, playlist, activity, etc.) from Qloo in parallel, proving the API’s unique multi-domain reach. |
| **Free-Form or Quick Themes** | Users may type any theme (“cozy mystery-movie night”) *or* tap pre-made chips with popular options (divided by category) for speed. |
| **Narrative Itinerary Assembly** | The LLM stitches Qloo results into a contextual itinerary—complete with friendly explanations that cite taste links (“Jazz + diners are loved by mystery-film fans…”). |
| **Interactive Cards** | Each recommendation appears as a card with an image, quick blurb, “Learn More,” and “Find Alternative” buttons so users can personalize on the fly. |
| **Learn More Deep Dives** | “Learn More” triggers LLM-guided web, YouTube, or Google Books searches (based on item type) and returns an annotated, citation-rich summary—so users can explore context without leaving Prism. |
| **Built-in Maps & Directions** | For *place* and *destination* cards, a Google Maps overlay opens with one click, preloaded with live directions from the user’s current location. |
| **Explain-Why Toggle** | A subtle “?” icon expands a plain-language rationale grounded in Qloo affinity scores, boosting transparency and trust. |
| **Chat-with-Prism Agent** | The same LLM powers a chat mode that can refine or extend the plan—e.g., “Make dinner vegetarian.” “Add something outdoorsy.” or “Extend the theme to the whole weekend.” |
| **Privacy & Speed** | Profile data stays local; anchor preferences and demographics are stored only in browser storage so returning visitors get faster suggestions without sharing personal info. |

**Why it matters**  
Recommendation engines usually solve *one* domain. Prism AI demonstrates how an LLM-guided orchestration of Qloo’s cultural graph can elevate the experience from “choose a movie” to “design my whole evening,” making taste intelligence feel like a real-world concierge. By letting the LLM handle both sides—query crafting *and* response interpretation—it showcases a powerful pattern for harnessing complex, flexible APIs in user-centric products.
