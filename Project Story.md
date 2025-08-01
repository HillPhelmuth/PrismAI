## Project Story

### Inspiration  
We love tools that make “What should we do tonight?” effortless. But most recommendation engines only solve **one** slice of life—pick a movie, find a restaurant, choose a playlist. When we discovered how Qloo’s Taste AI bridges domains (film → food → music → travel), we imagined a true **cultural concierge** that stitches everything together. Add an LLM’s knack for both creating structure from natural language and creating narrative and from structured inputs, and Prism AI was born: an app that translates any vibe into a ready-to-go experience.

### What We Learned  
* **Taste graphs are powerful, but complex.** Digging into Qloo’s flexible Insights endpoints taught us how affinities jump across seemingly unrelated domains.  
* **LLMs excel as “API whisperers.”** Prompt engineering let the model both craft Qloo queries and unpack the JSON responses into human-friendly cards.  
* **Great UX matters.** Users loved instant “Learn More” deep dives and one-tap Google Maps directions—features that turn static lists into actionable plans.

### How We Built It  
* **Stack:** C# / Blazor (Server + WASM) frontend, Semantic Kernel-driven LLM agents, Azure Functions proxy for Qloo & Google APIs.
* **Created Qloo Taste AI Api Parameter Models:** Modeled all parameters (Filters, Signals Outputs) to create LLM friendly schemas for Tool Call paramters and Structured Ouputs.  
* **Profile-Aware Agent:** The primary agent ingests age, gender, interests, theme, and timeframe, then generates multi-endpoint Qloo calls and assembles results into an itinerary.  
* **Enrichment Agents:** On “Learn More,” a second agent launches Web, YouTube, or Google Books searches and produces an annotated summary with inline citations.  
* **Maps Integration:** For place/destination cards, we feed lat/lng into the Google Maps JavaScript API, auto-centering on the user’s geolocation with live directions.  
* **Local Storage & Return Visits:** Anchor preferences and demographics live in browser storage, delivering instant recall with zero sign-up friction.  
* **UI Polish:** A few custom SVG icons, a whole lot of custom css, dynamic chips for quick themes, and an explain-why toggle that reveals the Qloo affinity reasoning.

### Challenges  
* **API Complexity:** Qloo’s richness and flexibility meant dozens of optional parameters and near infinite paramter combinations. We iterated prompts so the LLM could decide when to add filters like `location`, `tags` or `audience` or signals like `tags` or `demographics` automatically.  
* **Latency vs. Quality:** Calling multiple endpoints per domain risked slow responses. We parallelized requests and cached common anchors to keep the experience snappy.  
* **Citations & Hallucination:** For “Learn More,” we enforced structured JSON from the agent and validated each URL before display to avoid broken links or fabricated facts.  

### Accomplishments We’re Proud Of 
 
* **End-to-End LLM Orchestration:** A single prompt pipeline drives both Qloo query generation *and* response interpretation—_very_ little manual glue code required.  
* **True Cross-Domain Experiences:** Movies, meals, music, books, and destinations all pull from the same taste graph, proving the breadth of Qloo’s API.  
* **Actionability Over Lists:** “Learn More” summaries with citations and one-click Google Maps directions transform recommendations into immediate next steps.  

### What's Next for Prism AI - Your Personal Experience Curator

* **Collaborative Planning:** Enable shared boards so friends can co-edit an itinerary in real time.  
* **Calendar & Ticketing Hooks:** One-tap export to Google Calendar, OpenTable, or ticketing sites to complete the booking loop.  
* **Taste Graph Learning:** Fine-tune a lightweight on-device model to adapt suggestions based on past accept/reject actions.  
* **PWA & Mobile-First UI:** Package as an installable Progressive Web App for offline access and push notifications.  
* **Open-Source Core:** Release the agent orchestration layer so other devs can plug in their own APIs and front-ends.

### Outcome  
Prism AI now converts any prompt—from *“Retro 80s date night”* to *“Nature-loving staycation”*—into a beautifully packaged plan, complete with context, directions, and swap-anytime flexibility. The project showcases how LLM reasoning + Qloo’s cross-domain affinities can elevate recommendations from single-item lists to holistic experiences.
