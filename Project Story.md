## Project Story

### Inspiration  
We love tools that make “What should we do tonight?” effortless. But most recommendation engines only solve **one** slice of life—pick a movie, find a restaurant, choose a playlist. When we discovered how Qloo’s Taste AI bridges domains (film → food → music → travel), we imagined a true **cultural concierge** that stitches everything together. Add an LLM’s knack for both creating structure from natural language and creating narrative and from structured inputs, and Prism AI was born: an app that translates any vibe into a ready-to-go experience.

### What We Learned  
* **Taste graphs are powerful, but complex.** Digging into Qloo’s flexible Insights endpoints taught us how affinities jump across seemingly unrelated domains.  
* **LLMs excel as “API whisperers.”** Prompt engineering let the model both craft Qloo queries and unpack the JSON responses into human-friendly cards.  
* **Great UX matters.** Users loved instant “Learn More” deep dives and one-tap Google Maps directions—features that turn static lists into actionable plans.

### How We Built It  
* **Stack:** C# / Blazor (Server + WASM) frontend, Semantic Kernel-driven LLM agents, Azure Functions proxy for Qloo & Google APIs.  
* **Profile-Aware Agent:** The primary agent ingests age, gender, interests, theme, and timeframe, then generates multi-endpoint Qloo calls and assembles results into an itinerary.  
* **Enrichment Agent:** On “Learn More,” a second agent launches Web, YouTube, or Google Books searches and produces an annotated summary with inline citations.  
* **Maps Integration:** For place/destination cards, we feed lat/lng into the Google Maps JavaScript API, auto-centering on the user’s geolocation with live directions.  
* **Local Storage & Return Visits:** Anchor preferences and demographics live in browser storage, delivering instant recall with zero sign-up friction.  
* **UI Polish:** A custom SVG icon set, a whole lot of custom css, dynamic chips for quick themes, and an explain-why toggle that reveals the Qloo affinity reasoning.

### Challenges  
* **API Complexity:** Qloo’s richness and flexibility meant dozens of optional parameters and near infinite paramter combinations. We iterated prompts so the LLM could decide when to add filters like `location`, `tags` or `audience` or signals like `tags` or `demographics` automatically.  
* **Latency vs. Quality:** Calling multiple endpoints per domain risked slow responses. We parallelized requests and cached common anchors to keep the experience snappy.  
* **Citations & Hallucination:** For “Learn More,” we enforced structured JSON from the agent and validated each URL before display to avoid broken links or fabricated facts.  

### Outcome  
Prism AI now converts any prompt—from *“Retro 80s date night”* to *“Nature-loving staycation”*—into a beautifully packaged plan, complete with context, directions, and swap-anytime flexibility. The project showcases how LLM reasoning + Qloo’s cross-domain affinities can elevate recommendations from single-item lists to holistic experiences.
