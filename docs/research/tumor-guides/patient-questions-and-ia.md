# Patient Questions & Information Architecture — Research for the BrainHarbor Curated Pages

**Prepared:** 2026-08-30
**Scope:** Non-clinical. What to answer, in what order, in what words.
**Audience for this doc:** the team writing ~40 curated pages (24 tumor types + treatment library + tests/workup library).

Every substantive claim below carries a URL. Where a claim rests on a single study, the
population is named, because several of the best studies are in narrow populations
(meningioma-only, TYA survivors, advanced cancer generally) and should not be over-generalised.

---

## 0. Method and honest limits

We deliberately did not scrape patient forums (ToS + third-party health information).
Instead this report draws on the published literature that *studied* those forums and
populations: forum content analyses, qualitative interview studies, unmet-needs surveys,
national guideline evidence reviews, and search-log analyses.

Three limits worth stating up front:

1. **The forum-analysis literature is thin.** The main direct study (PMC12119380) analysed
   383 posts across four forums. It is indicative, not representative.
   https://pmc.ncbi.nlm.nih.gov/articles/PMC12119380/
2. **Search-log studies are mostly not brain-tumor-specific.** The best ones cover cancer
   generally or chemotherapy specifically. https://pmc.ncbi.nlm.nih.gov/articles/PMC1550578/ ,
   https://pmc.ncbi.nlm.nih.gov/articles/PMC11395687/
3. **The published "questions to ask" lists are clinician-authored**, not patient-derived.
   They are still useful as a checklist, but they are what professionals think patients
   should ask, not what patients actually ask. The qualitative studies are the corrective.

---

# PART 1 — What patients and caregivers actually ask

## 1.1 The evidence base

| Source | Population / method | Why it matters here |
|---|---|---|
| "We are a club none of us wanted to join" — forum thematic analysis, 2025 | 383 posts (80 initial + 303 replies) from ABTA, Mayo Clinic Brain Tumor Connect, Macmillan Brain Cancer Forum, ACS Cancer Survivors Network, collected Sept 2024 | The closest thing to forum mining that we can legitimately cite. https://pmc.ncbi.nlm.nih.gov/articles/PMC12119380/ |
| Caring for the brain tumor patient: family caregiver burden and unmet needs | 25 family caregivers, interviews, phased by illness stage | Best phase-by-phase map of caregiver information needs. https://pmc.ncbi.nlm.nih.gov/articles/PMC2600839/ |
| Unmet needs of patients with intracranial meningioma | Qualitative, meningioma only | The only study that squarely names the "what is normal in recovery?" gap. https://pmc.ncbi.nlm.nih.gov/articles/PMC7318855/ |
| NICE evidence review: care needs of people with brain tumours | Guideline evidence review, high-quality qualitative evidence | Authoritative statement that information timing must be flexible. https://www.ncbi.nlm.nih.gov/books/NBK570056/ |
| Information and support needs of patients diagnosed with High Grade Glioma | Qualitative, HGG | Four themes incl. prognostic uncertainty + need for individualised information. https://pubmed.ncbi.nlm.nih.gov/19762197/ |
| "Do I want to know it all?" | 25 patients (10 F / 15 M), newly diagnosed grade II–IV glioma, interviews | Proves preferences are heterogeneous — "one size does not fit all". https://www.ncbi.nlm.nih.gov/pmc/articles/PMC8062391/ |
| Assessing patient and caregiver needs and challenges in information and symptom management: primary brain tumors | Mixed-methods | Names 60+ distinct symptoms/events and the attribution problem. https://pmc.ncbi.nlm.nih.gov/articles/PMC5333312/ |
| Coping with Glioblastoma: prognostic communication & understanding | 17 patient/caregiver/oncologist triads, recurrent GBM | Quantifies the recall gap between what clinicians say and what patients hear. https://pmc.ncbi.nlm.nih.gov/articles/PMC10022487/ |
| Long-term unmet supportive care needs of TYA childhood brain tumour survivors and caregivers | Cross-sectional survey | Ranked unmet needs, psychological domain on top. https://www.ncbi.nlm.nih.gov/pmc/articles/PMC8795012/ |
| Searching for cancer information on the internet: natural language queries | 76k+ queries | How people actually phrase things. https://pmc.ncbi.nlm.nih.gov/articles/PMC1550578/ |
| Thoughts, needs and fears of chemotherapy patients via Google search behaviour | 1,461 German search terms | Ranked chemo concerns; evidence of terminology confusion. https://pmc.ncbi.nlm.nih.gov/articles/PMC11395687/ |
| Prevalence and severity of scanxiety in advanced cancers | Multicentre survey, n=222 | Quantifies the scan-wait window. https://link.springer.com/article/10.1007/s00520-021-06454-9 |
| Patient and caregiver return to work after a primary brain tumor | Neuro-Oncology Practice | Work/driving/finance evidence. https://academic.oup.com/nop/article/10/6/565/7226298 |

## 1.2 Themed list of real questions and worries

Grouped as they should appear on the site. Wording below is either quoted from the
literature or a faithful paraphrase of a documented concern; the citation follows each group.

### Theme A — "What actually is this thing?" (identity and label)

- What is the name of my tumour type? What is its grade? Where is it and how big is it?
  https://braintumor.org/brain-tumors/diagnosis-treatment/diagnosis/key-questions/
- Is it cancer or not? Caregivers explicitly struggled with this: *"I know part is fast
  growing and part less risk, but I don't know how much is more risk."*
  https://pmc.ncbi.nlm.nih.gov/articles/PMC2600839/
- What mutations are present? What did the methylation analysis show? Will you test for
  mutations and methylation? https://braintumor.org/brain-tumors/diagnosis-treatment/diagnosis/key-questions/
- Is there anything else that could be causing these symptoms?
  https://braintumor.org/brain-tumors/diagnosis-treatment/diagnosis/key-questions/
- Why did this happen to me? (Cause/risk queries are consistently a *small* share of cancer
  search volume — under 2% — so do not over-build this section.)
  https://pmc.ncbi.nlm.nih.gov/articles/PMC1550578/

**Design implication:** the "what is it" answer must be first and must explicitly resolve
benign/malignant and grade, because that is the confusion that recurs in the caregiver data.

### Theme B — Treatment: the three big ones, in the order people discuss them

Forum discussion split almost evenly across chemotherapy (26.4%), surgery (25.8%) and
radiation (25.2%); gliomas were 57.5% of tumour-type mentions.
https://pmc.ncbi.nlm.nih.gov/articles/PMC12119380/

Surgery questions (clinician-curated lists converge strongly here):
- Why do I need surgery? What type — craniotomy, awake craniotomy, minimally invasive?
- How soon? What are the risks if I delay?
- What is your experience with my tumour type? How many a year do you do?
- How long is the operation? How long in hospital? How long to recover? When can I go back
  to normal activities?
- What changes should I expect afterwards — independence, work, driving?
- Will I need rehab, speech therapy, physio?
- What happens to my tumour tissue afterwards? Can I get access to it later?
- What happens if I don't have surgery?
  https://www.cancer.gov/rare-brain-spine-tumor/living/questions/neurosurgeon

Chemotherapy questions — search-behaviour ranking of what people actually look up:
side effects 32%, tumour-type-specific chemo info 21%, general information about the
process/success rates/regimens 20%, specific medications 12%, prognosis 6%. Within side
effects, hair loss was 51% of queries and skin problems 9%.
https://pmc.ncbi.nlm.nih.gov/articles/PMC11395687/

The same study found people conflate **adjuvant vs neoadjuvant** and **chemo vs
immunotherapy** — i.e. they search for them together as if interchangeable. That is a
direct, evidenced brief for a jargon block.
https://pmc.ncbi.nlm.nih.gov/articles/PMC11395687/

Radiation/general treatment sequencing questions:
- If I need more than one treatment, in what order?
- For how many weeks, how many sessions a week?
- Are your recommendations the standard guideline? If not, why?
- Are there clinical trials? Will this treatment stop me joining a trial later?
  https://www.cancer.gov/rare-brain-spine-tumor/living/questions/recurrence

### Theme C — Side effects, symptoms, and "is this normal?"

- Patients and caregivers reported **over 60 different symptoms, side effects and health
  events**, and the hardest part was *attribution*: is this the tumour, the progression, or
  the drug? https://pmc.ncbi.nlm.nih.gov/articles/PMC5333312/
- Many did not recognise neurological symptoms — e.g. mistaking a seizure for a stroke.
  https://pmc.ncbi.nlm.nih.gov/articles/PMC5333312/
- Caregivers noticed cognitive and personality changes *before* patients did.
  https://pmc.ncbi.nlm.nih.gov/articles/PMC5333312/
- "What should I be watching for that means I call for help right now?"
  https://braintumor.org/brain-tumors/diagnosis-treatment/diagnosis/key-questions/
- "What symptoms do I need to report, and what is the best way to report them?" and "How do
  I contact your team at night or at the weekend?"
  https://www.cancer.gov/rare-brain-spine-tumor/living/questions/recurrence
- Caregivers wanted to be told about expected neurocognitive change, depression, personality
  change and impulsive/aggressive behaviour **before it happened**, plus when to seek help.
  https://pmc.ncbi.nlm.nih.gov/articles/PMC2600839/
- One participant on being under-briefed about drugs and treatments: *"you feel like you are
  jumping out of an airplane without a parachute."* https://pmc.ncbi.nlm.nih.gov/articles/PMC5333312/

### Theme D — Recovery: "what is normal after?"

This is the single most under-served theme in the literature.

- Meningioma patients reported uncertainty about the recovery timeline, what post-op
  symptoms are normal, and how much physical activity is safe. One said *"I feel like not
  knowing what's gonna happen in your recovery process… the real fight started after I woke
  up."* https://pmc.ncbi.nlm.nih.gov/articles/PMC7318855/
- They also described relying on the internet and being frightened by worst-case scenarios,
  and feeling "forgotten" after treatment finished.
  https://pmc.ncbi.nlm.nih.gov/articles/PMC7318855/
- Pre-treatment patients specifically said they wanted to be better informed about recovery
  time and side effects. https://pmc.ncbi.nlm.nih.gov/articles/PMC5333312/

### Theme E — Prognosis, fear and uncertainty

- Fear, uncertainty and "scanxiety" were among the dominant emotional themes in the forum
  data, co-occurring with information seeking about chemo, imaging and surgery.
  https://pmc.ncbi.nlm.nih.gov/articles/PMC12119380/
- "Uncertainty about the future" ranked as the second most common unmet need (50.7%) behind
  anxiety (60.3%) in TYA brain tumour survivors; "feeling down and depressed" was third
  (48.5%), with a mean of 9.4 unmet needs per person. (Population caveat: teenage/young
  adult survivors of childhood tumours.) https://www.ncbi.nlm.nih.gov/pmc/articles/PMC8795012/
- Adult brain tumour patients most frequently reported needing support for fatigue,
  uncertainty about the future, and not being able to do the things they used to do.
  https://pubmed.ncbi.nlm.nih.gov/18329220/
- HGG patients' four themes included *feelings of uncertainty around prognosis and quality
  of life* and *communication with health professionals around prognostic uncertainty*.
  https://pubmed.ncbi.nlm.nih.gov/19762197/
- Scanxiety: in 222 people with advanced cancer, mean severity 6/10, peaking in the window
  between scan and result; 70% waited more than two days for results. Scanxiety was worse in
  people diagnosed within the past year — i.e. exactly our audience.
  https://link.springer.com/article/10.1007/s00520-021-06454-9
- Recurrence-specific questions people ask: is it in the same place? is it the same type and
  grade? has it spread? what is the chance it comes back again?
  https://www.cancer.gov/rare-brain-spine-tumor/living/questions/recurrence

**Critically — not everyone wants the prognosis.** In 25 newly diagnosed glioma patients,
some wanted full honest information to make autonomous choices, some wanted general
information without details, and *some wanted no bad news at all, only positive information*.
The authors' conclusion is blunt: "Not all patients want to know it all, one size does not
fit all." https://www.ncbi.nlm.nih.gov/pmc/articles/PMC8062391/
NICE's committee reached the same operational conclusion: "Some people would prefer to
receive more information earlier in their treatment pathway and some later."
https://www.ncbi.nlm.nih.gov/books/NBK570056/

### Theme F — Everyday life, money, work, driving

Practical and logistical challenges, especially financial strain, *dominated* the forum
discussions. Quoted examples: *"Medicare will not pay for rehab and cancer treatments at the
same time. It's been 3 months."* and *"She lost her car because she has been out of work
because of this."* https://pmc.ncbi.nlm.nih.gov/articles/PMC12119380/

- All 25 interviewed caregiver families reported financial problems; several relied on
  charity for basic needs. https://pmc.ncbi.nlm.nih.gov/articles/PMC2600839/
- Meningioma patients: 2–3 hour one-way commutes to specialist centres, hospital parking
  costs, out-of-pocket costs for medication/equipment/rehab, driving restrictions destroying
  independence, employers without disability cover.
  https://pmc.ncbi.nlm.nih.gov/articles/PMC7318855/
- Return to work: problems with concentration, spatial awareness and processing speed were
  common; some patients stopped working immediately after a first seizure, especially if the
  job involved driving or machinery. Caregivers' own work was heavily affected.
  https://academic.oup.com/nop/article/10/6/565/7226298
- Insurance is a whole question category of its own — NBTS lists fourteen insurance
  questions. https://braintumor.org/brain-tumors/diagnosis-treatment/diagnosis/key-questions/
- Will treatment affect fertility? Should I see a fertility specialist first?
  https://www.uclahealth.org/cancer/cancer-services/brain-tumor/patient-resources/questions-ask-your-doctor
- Cancer Research UK treats **driving** as a first-class section on the tumour page itself,
  which is a good precedent. https://www.cancerresearchuk.org/about-cancer/brain-tumours/types/glioblastoma

### Theme G — Caregiver-only questions (the biggest unmet need)

The forum study's sharpest finding: **care coordination and advocacy concerns were expressed
almost exclusively by informal caregivers, not patients.**
https://pmc.ncbi.nlm.nih.gov/articles/PMC12119380/

Caregiver questions and needs, by phase:
- *At diagnosis:* understand the diagnosis, tumour type, location and prognosis. All 25
  caregivers felt "overwhelmed and in shock" despite the physician having explained things.
- *Chronic phase:* what neurocognitive and personality changes are coming, how to manage
  depression, aggression and impulsivity, and when to call for help.
- *End of life:* symptom management during decline, what the final weeks look like, hospice.
  Six caregivers regretted not accepting hospice sooner.
- *Throughout:* they performed nursing tasks (dressing changes, medications) with **no formal
  instruction**; they feared "offending the physician by asking too many questions"; they
  did not know what to tell the children; and they were socially isolated — *"Most people…
  will walk away to avoid saying something wrong."*
  https://pmc.ncbi.nlm.nih.gov/articles/PMC2600839/
- Caregivers are frequently the better information processor when the patient has cognitive
  impairment, and should be explicitly addressed.
  https://pmc.ncbi.nlm.nih.gov/articles/PMC10022487/
- Caregiver needs change with the trajectory: treatment-related support at diagnosis,
  psychosocial/supportive needs after discharge home, emotional needs at end of life.
  https://www.ncbi.nlm.nih.gov/pmc/articles/PMC8267327/

### Theme H — Talking to the care team (a meta-need)

- A persistent gap between what patients need and what providers communicate. One patient:
  *"I don't know how to bring up these topics with my doctor — the idea of trying to tell the
  people who know me is scarier."* https://pmc.ncbi.nlm.nih.gov/articles/PMC12119380/
- Second opinions and struggling to interpret complex medical information were recurrent.
  https://pmc.ncbi.nlm.nih.gov/articles/PMC12119380/
- Objective evidence of the gap: in recurrent GBM triads, only 17.6% of patients recalled
  discussing prognosis at recurrence disclosure vs 58.8% of oncologists; only 41.2% of
  patients had ever discussed prognosis vs 76.5% of oncologists. Only 3 of 17 triads agreed
  across all three topics. https://pmc.ncbi.nlm.nih.gov/articles/PMC10022487/
- NICE: people wanted a named professional responsible for coordinating health and social
  care support. https://www.ncbi.nlm.nih.gov/books/NBK570056/
- Practical advice repeatedly given: write questions down beforehand, bring a notepad, bring
  someone. https://www.cancer.gov/rare-brain-spine-tumor/living/questions/neurosurgeon

### Theme I — Family and social

- Family functioned as both support and stressor. *"He is pushing me away and I don't
  understand why. I feel like my marriage isn't going to survive this."*
  https://pmc.ncbi.nlm.nih.gov/articles/PMC12119380/
- Isolation persists even with family nearby: *"There is a very, very isolating part of
  disease and recovery that nobody can help you with."*
  https://pmc.ncbi.nlm.nih.gov/articles/PMC7318855/
- Peer connection was the most emphasised finding of the forum study: *"We are a club none of
  us wanted to join, but I am so grateful for every member here."*
  https://pmc.ncbi.nlm.nih.gov/articles/PMC12119380/

### 1.3 Cross-cutting findings about *how* information should arrive

1. **Shock blocks comprehension.** The shock of diagnosis plus symptoms impairs the ability
   to take in information and to express needs. https://pubmed.ncbi.nlm.nih.gov/29603812/
2. **Cognitive impairment is the norm, not the exception.** Prevalence of cognitive
   impairment in adult WHO grade 1–3 glioma is estimated at 27–83%; up to ~90% of
   glioblastoma patients show cognitive decline before treatment.
   https://link.springer.com/article/10.1007/s11060-016-2152-7 ,
   https://www.ncbi.nlm.nih.gov/pmc/articles/PMC11940939/
3. **Needs change over time and must be re-offered**, not delivered once.
   https://www.ncbi.nlm.nih.gov/books/NBK570056/ , https://pubmed.ncbi.nlm.nih.gov/19762197/
4. **People search in natural-language sentences and misspell heavily** ("prostrate",
   "chemothereapy"). Headings phrased as questions and generous synonym coverage matter.
   https://pmc.ncbi.nlm.nih.gov/articles/PMC1550578/
5. **Symptom-type queries outnumber treatment queries** for most cancers.
   https://pmc.ncbi.nlm.nih.gov/articles/PMC1550578/

---

# PART 2 — How comparable organisations structure this content

## 2.1 Per-organisation IA

### Cancer Research UK — the best-structured comparator
Hub order: what is a brain tumour → cancerous or non-cancerous → types → how common →
symptoms / risks and causes → diagnosis and tests → **grades** → treatment and research →
survival → living with.
https://www.cancerresearchuk.org/about-cancer/brain-tumours

Individual tumour page (glioblastoma) order: What are glioblastomas? → What does IDH
wildtype mean? → Grades → How common → Symptoms → What tests will I have? → Treatment
(on-page summary per modality, each linking to the central treatment page) → Research and
clinical trials (link) → **Driving** (link) → Follow up (link) → Coping (link).
https://www.cancerresearchuk.org/about-cancer/brain-tumours/types/glioblastoma

- **Model: hybrid.** Treatment is centralised; the tumour page carries a short
  contextualising summary and links out. This is the pattern closest to our three layers.
- **Does well:** grade explained in behavioural terms, not survival terms ("The cells look
  very much like normal cells… usually slow growing"); driving treated as first-class;
  the molecular label given its own heading right after "what is it".
  https://www.cancerresearchuk.org/about-cancer/brain-tumours/grades
- **Does badly:** "How common are they?" sits above "Symptoms" — epidemiology before the
  reader's own body. Survival page publishes figures. Reading level well above 6th grade.
  Nothing for caregivers as a distinct reader.

### Macmillan
Hub order: what is a primary brain tumour → symptoms → causes → diagnosing → grading →
treatment → after treatment → how we can help.
https://www.macmillan.org.uk/cancer-information-and-support/brain-tumour

Tumour-family page (glioma): What is a glioma? → Astrocytoma → Oligodendroglioma →
Ependymoma → Getting support → About our information → How we can help. **No treatment
content on the page at all**; treatment lives entirely in the central section. No summary
box at the top. https://www.macmillan.org.uk/cancer-information-and-support/brain-tumour/glioma

- **Model: fully centralised treatment.**
- **Does well:** "About our information" provenance block; a strong "how we can help"
  support call-to-action at the end of every page.
- **Does badly:** the tumour page is a taxonomy stub. A reader who lands on "glioma" gets a
  list of other tumour names and no answer to "what happens to me now?" No summary at top —
  a direct PEMAT failure. https://www.ahrq.gov/health-literacy/patient-education/pemat-p.html

### The Brain Tumour Charity
IA is split into `/brain-tumour-diagnosis-treatment/` (with sub-trees for
*how brain tumours are diagnosed* → biology, grading, types; and *treating brain tumours* →
adult treatments, child treatments) and `/living-with-a-brain-tumour/`.
https://www.thebraintumourcharity.org/brain-tumour-diagnosis-treatment/treating-brain-tumours/ ,
https://www.thebraintumourcharity.org/brain-tumour-diagnosis-treatment/types-of-brain-tumour-adult/ ,
https://www.thebraintumourcharity.org/living-with-a-brain-tumour/get-support/

- **Model: centralised treatment, separated by adult vs child.**
- **Does well:** a "Step by Step" interactive guide sequenced by *stage of the journey*
  rather than by medical topic — the only comparator that organises around time-since-
  diagnosis. A dedicated "Questions to ask your doctor" page.
  https://www.thebraintumourcharity.org/brain-tumour-diagnosis-treatment/navigating-the-healthcare-system/questions-to-ask/
- **Does badly:** deep URL nesting (four levels) makes orientation hard for an impaired
  reader; the questions page is a destination rather than an in-context block.

### ABTA
IA: `/about-brain-tumors/` → `brain-tumor-types/`, `brain-tumor-diagnosis/` (incl.
signs & symptoms), `brain-tumor-treatment/`, `brain-tumor-education/` (FAQs), plus a
"Newly Diagnosed Toolkit" and a separate `mindmatters` blog carrying the "Top 10 questions
to ask" content. https://www.abta.org/about-brain-tumors/ ,
https://www.abta.org/about-brain-tumors/brain-tumor-types/ ,
https://www.abta.org/about-brain-tumors/brain-tumor-diagnosis/brain-tumor-signs-symptoms/ ,
https://www.abta.org/mindmatters/brain-tumor-101/top-10-questions-to-ask/

- **Model: centralised treatment; tumour pages are descriptive.**
- **Does well:** symptoms explained by tumour *location* (frontal lobe, parietal), which
  answers "why is this happening to me" rather than just listing symptoms; explicitly frames
  symptom content as preparation — "understanding what might occur… may help you better
  prepare". https://www.abta.org/about-brain-tumors/brain-tumor-diagnosis/brain-tumor-signs-symptoms/
- **Does badly:** the most useful patient-facing artefact (top 10 questions) is filed under
  a *blog*, not under the tumour pages. Content is split across `/about-brain-tumors/` and
  `/mindmatters/` with no obvious relationship.

### NCI / cancer.gov (PDQ)
Hub splits adult vs childhood, then one integrated page per tumour type covering symptoms,
risk factors, diagnosis and treatment together, plus separate Drugs Approved, Clinical
Trials, and Statistics sections. https://www.cancer.gov/types/brain

PDQ patient summary order: General Information → **Stages** → Treatment Option Overview →
Treatment by Type of Tumor → Spinal Cord Tumors → Recurrent → Metastatic → To Learn More.
https://www.ncbi.nlm.nih.gov/books/NBK66023/

- **Model: duplicated.** Treatment is re-stated per tumour type inside the PDQ summaries.
- **Does well:** exhaustive, versioned, and authoritative; clear "to learn more" endings.
- **Does badly:** structured like a monograph. "Stages" comes second, before the reader has
  any grip on what the disease is. Nothing about driving, work, money, or caregivers. Very
  long single pages.

### NCI-CONNECT (Rare Brain and Spine Tumor Network) — the most interesting outlier
Top-level: Tumor Types / **Living with a Tumor** / Refer and Participate / Blog / About.
"Living with a Tumor" branches by *journey stage*: newly diagnosed, symptom management,
recurrence — plus standalone question pages (Questions to Ask Your Neurosurgeon, Questions
to Ask about Recurrence). https://www.cancer.gov/rare-brain-spine-tumor/ ,
https://www.cancer.gov/rare-brain-spine-tumor/living/newly-diagnosed ,
https://www.cancer.gov/rare-brain-spine-tumor/living/questions/neurosurgeon

Tumour page (gliosarcoma) headings, all phrased as questions: What Are the Grades? → What Do
They Look Like on an MRI? → What Causes Them? → Where Do They Form? → Do They Spread? → What
Are the Symptoms? → Who Is Diagnosed With One? → What Is the Prognosis? → What Are the
Treatment Options? https://www.cancer.gov/rare-brain-spine-tumor/tumors/gliosarcoma

- **Does well:** every heading is a question — the single best IA idea on any of these sites,
  and it matches how people type queries. https://pmc.ncbi.nlm.nih.gov/articles/PMC1550578/
  Audience segmentation (patients vs providers) and journey-stage organisation are also right.
- **Does badly:** the *order* is wrong for a frightened newcomer — MRI appearance and
  aetiology precede symptoms, and prognosis precedes treatment. Treatment gets the least
  detail, symptoms are an unelaborated bullet list.

### Mayo Clinic
Fixed four-tab template per condition: Symptoms & causes → Diagnosis & treatment → Doctors &
departments → Care at Mayo Clinic, with "When to see a doctor" embedded inside Symptoms.
https://www.mayoclinic.org/diseases-conditions/glioma/symptoms-causes/syc-20350251

- **Model: duplicated per condition, template-driven.**
- **Does well:** ruthless consistency; "When to see a doctor" as a named, findable block.
- **Does badly:** the last two tabs are institutional marketing. No caregiver lane, no
  practical-life content, no questions block.

### Johns Hopkins (Comprehensive Brain Tumor Center)
Organised by *service line* — Patient Resources, Appointments & Referrals, specialty centers
(e.g. Glioma Center) — not by patient question.
https://www.hopkinsmedicine.org/brain-tumor ,
https://www.hopkinsmedicine.org/brain-tumor/patient-resources

- **Does badly, for our purposes:** it is a referral funnel. Education is outsourced (it
  points patients at ABTA's mentor programme). Confirms the general finding that academic
  centre sites are the worst readability performers.
  https://academic.oup.com/nop/article/12/5/901/8124733

### NHS.uk — the strongest plain-language exemplar
Order: What is it? → Symptoms → Causes → Tests and next steps → Treatment → Help and
support. Each is a separate short page rather than one long page.
https://www.nhs.uk/conditions/malignant-brain-tumour/

- **Does well:** the shortest, plainest prose of any comparator; "Tests and **next steps**"
  is better framing than "Diagnosis" because it answers what happens *to you*; the page
  opens with a single-sentence definition.
- **Does badly:** thin. No grade explanation, no molecular content, nothing per tumour type,
  nothing for caregivers.

Also worth noting as a tone exemplar: **brainstrust** (UK) built its offer around decoding
specialist language and a physical "brain box" toolkit, explicitly framed around managing a
"complicated and confusing journey". https://brainstrust.org.uk/about-brainstrust/our-story/

## 2.2 Duplicate vs centralise — the tally

| Organisation | Treatment content |
|---|---|
| Cancer Research UK | **Hybrid** — short contextual summary on the tumour page + link to canonical page |
| Macmillan | Centralised (tumour page carries none) |
| The Brain Tumour Charity | Centralised |
| ABTA | Centralised |
| NHS | Centralised (separate linked page) |
| NCI PDQ | Duplicated per tumour type |
| NCI-CONNECT | Duplicated (short) per tumour type |
| Mayo | Duplicated per condition |
| Johns Hopkins | Service-line, not patient-education |

**The hybrid model wins**, and CRUK is the only one executing it. Full centralisation
(Macmillan) leaves the tumour page useless; full duplication (PDQ, Mayo) creates maintenance
debt and inconsistency, and for us would multiply the reading-level QA burden by 24.

## 2.3 Gap analysis — where BrainHarbor's three-layer structure can genuinely win

1. **Nobody is readable.** Across 91 US brain tumour centres and 8 patient organisations,
   mean Flesch-Kincaid grade level was 11, fewer than 10% of centre sites were readable at
   8th grade, and **no patient organisation met the 8th-grade target**.
   https://academic.oup.com/nop/article/12/5/901/8124733
   An earlier analysis of 180 documents from 50 tertiary academic centres found median FKGL
   12.5 (Flesch Reading Ease 38.2), with treatment content (FKGL 12.9) harder than diagnostic
   content (11.6). https://pubmed.ncbi.nlm.nih.gov/31785430/
   For brain-tumour *prognosis* material specifically, neither existing web pages nor
   ChatGPT output met the AMA 6th-grade or NIH 8th-grade benchmarks.
   https://pubmed.ncbi.nlm.nih.gov/40543265/
   **A rigorously 6th-grade brain-tumour library would be, on the published evidence, the
   first of its kind.** That is the headline gap.

2. **Caregivers have no lane.** Care coordination and advocacy are the concerns raised almost
   exclusively by caregivers (https://pmc.ncbi.nlm.nih.gov/articles/PMC12119380/), and
   caregivers perform untrained nursing tasks and fear asking questions
   (https://pmc.ncbi.nlm.nih.gov/articles/PMC2600839/). No comparator puts caregiver-specific
   content *inside* the tumour page. Every one of them treats caregivers as a separate
   "support" silo.

3. **Nobody decodes the pathology report.** CRUK explains one marker on one page ("What does
   IDH wildtype mean?"); NCI-CONNECT explains grade and MRI appearance. There is no
   systematic, per-tumour "what the words on your report mean" block anywhere — despite
   documented confusion between adjuvant/neoadjuvant and chemo/immunotherapy in real search
   data. https://pmc.ncbi.nlm.nih.gov/articles/PMC11395687/

4. **"What is normal after treatment?" is missing.** The literature names it explicitly as a
   gap (https://pmc.ncbi.nlm.nih.gov/articles/PMC7318855/) and no comparator's tumour page
   answers it. CRUK gets closest via a "Follow up" link.

5. **Prognosis is handled as a binary.** Comparators either publish survival figures
   (https://www.cancerresearchuk.org/about-cancer/brain-tumours/survival) or omit prognosis
   entirely. Nobody teaches the *concepts* — grade, median, five-year rate — so the reader can
   interpret numbers they will inevitably meet elsewhere. And nobody offers progressive
   disclosure despite the evidence that a subset of patients actively does not want to know.
   https://www.ncbi.nlm.nih.gov/pmc/articles/PMC8062391/

6. **Money, work, driving and logistics are buried** — even though financial strain
   *dominated* the real forum discussion. https://pmc.ncbi.nlm.nih.gov/articles/PMC12119380/
   CRUK's inclusion of "Driving" on the tumour page is the only counterexample.

7. **Questions blocks are not at the point of need.** NCI, NBTS, ABTA and TBTC all have good
   question lists — filed as separate destination pages or blog posts. PEMAT's actionability
   dimension requires the material to "clearly identify at least one action the user can
   take"; a contextual questions block is that action.
   https://www.ahrq.gov/health-literacy/patient-education/pemat-p.html

8. **Cultural sensitivity gap.** 48% of brain tumour centres and 63% of patient organisations
   met recommended cultural-sensitivity scores; **no** organisation addressed cultural
   perceptions of cancer. https://academic.oup.com/nop/article/12/5/901/8124733

9. **Quality signalling.** No website met all four JAMA benchmark criteria (authorship,
   attribution, disclosure, currency). Patient organisations scored better than hospitals on
   DISCERN (57.13 vs 46.48). Publishing sources + last-reviewed on every page is cheap
   differentiation. https://academic.oup.com/nop/article/12/5/901/8124733

---

# PART 3 — Plain language and readability practice

## 3.1 The standards, and what they actually require

**Reading level targets.** NIH and AMA guidance is that patient education material be written
at 4th–6th grade. https://academic.oup.com/nop/article/12/5/901/8124733

**Evidence the target is not arbitrary.** In an RCT of 85 diabetic patients randomised to read
foot-care material at grade 6 vs grade 9 or grade 6 vs grade 11 (SMOG), mean CLOZE
comprehension was better for the grade-6 material than for either the grade-9 or grade-11
material. Overland JE et al., *Diabetic Medicine* 1993;10(9):847–50.
https://onlinelibrary.wiley.com/doi/abs/10.1111/j.1464-5491.1993.tb00178.x

**Who we are writing for.** In the 2003 US National Assessment of Adult Literacy, 12% of
adults were Proficient in health literacy, 53% Intermediate, 22% Basic and 14% Below Basic —
i.e. 36% at Basic or below. https://nces.ed.gov/naal/health_results.asp
Layer on top of that: cognitive impairment in 27–83% of adult grade 1–3 glioma patients and
up to ~90% of glioblastoma patients pre-treatment
(https://link.springer.com/article/10.1007/s11060-016-2152-7 ,
https://www.ncbi.nlm.nih.gov/pmc/articles/PMC11940939/), plus the documented fact that the
shock of diagnosis itself degrades comprehension
(https://pubmed.ncbi.nlm.nih.gov/29603812/). The 6th-grade rule is the floor, not a stretch.

**CDC Clear Communication Index.** 4 open-ended introductory questions + 20 scored items
across seven areas: main message and call to action; language; information design; state of
the science; behavioural recommendations; numbers; risk. Items score 0 or 1, converted to a
/100 scale. **90 or higher is passing.** Parts B–D (behavioural recommendations, numbers,
risk) may be N/A for a given material.
https://www.cdc.gov/ccindex/index.html , https://www.cdc.gov/ccindex/tool/how-to-use.html
Practical consequence for us: because we publish no figures, Part C (numbers) will largely
score N/A — but Part D (risk) will not, and the "state of the science" items demand that we
say what is known, what is not, and who says so.

**PEMAT-P.** 24 items — 17 understandability, 7 actionability — grouped as Content, Word
Choice & Style, Use of Numbers, Organization, Layout & Design, Use of Visual Aids, and
Actionability. Scored separately for understandability and actionability.
https://www.ahrq.gov/health-literacy/patient-education/pemat.html ,
https://www.ahrq.gov/health-literacy/patient-education/pemat-p.html
The items that should become hard rules in our page template:
- 1. Purpose completely evident (a title/upfront text that says at a glance what this is).
- 2. No content that distracts from the purpose.
- 3. Common, everyday language.
- 4. Medical terms used only to familiarise the audience with them; when used, defined.
- 5. Active voice.
- 8. Information broken into short "chunks".
- 9. Informative headers on every section.
- 10. Logical sequence.
- **11. The material provides a summary.**
- 12. Visual cues (bold, boxes, bullets) draw attention to key points.
- 20. Clearly identifies at least one action the user can take.
- 21. Addresses the user directly when describing actions.
- 22. Breaks actions into manageable, explicit steps.

**CDC "Simply Put".** Concrete numeric rules: use one- and two-syllable words where you can;
keep most sentences to **8–10 words**; limit paragraphs to **3–5 sentences**; present one
complete idea per page or two facing pages; use headings and sub-headings to chunk; define
jargon first and then explain it; write as if talking to a friend; test for readability.
https://stacks.cdc.gov/view/cdc/11938

**NIH/NCI "Clear & Simple".** Three low-literacy criteria: sentences under **15 words**,
active voice, and minimising words of more than 2–3 syllables. Organise content to answer the
reader's questions; use "you" and other pronouns.
https://files.eric.ed.gov/fulltext/ED381691.pdf

**How people read a web page.** Users read roughly 20–28% of the words on a page and scan in
an F-shaped pattern; the recommended response is scannable text, highlighted keywords, one
idea per paragraph, inverted-pyramid structure (conclusion first), and half the word count of
conventional writing. https://www.nngroup.com/articles/how-users-read-on-the-web/ ,
https://www.nngroup.com/articles/f-shaped-pattern-reading-web-content-discovered/

**Accessibility for impaired readers.** The aphasia-friendly literature identifies both
content and design characteristics as determinants of access: font size, line spacing,
document length, and inclusion of graphics; simplified language plus pictures to support text.
https://pubmed.ncbi.nlm.nih.gov/21682542/ ,
https://acnr.co.uk/articles/making-information-accessible-for-people-with-aphasia/
This is directly applicable — brain tumour patients frequently have language and processing
deficits.

## 3.2 Concrete techniques for the hard concepts

### General moves that preserve accuracy at 6th grade

1. **Say the outcome, then name the word.** Do not lead with the term. Explain what happens
   in plain words, then give the medical name in a short trailing sentence so the reader can
   match it to what their team says. This satisfies PEMAT item 4 without dumbing down.
   https://www.ahrq.gov/health-literacy/patient-education/pemat-p.html
   - "The surgeon opens a small window in the skull and takes out as much of the tumour as is
     safe. Then they close the window. **Doctors call this a craniotomy.**"
2. **Split the compound sentence into a sequence.** Accuracy usually survives; length dies.
   - Before: "Maximal safe resection followed by adjuvant chemoradiotherapy is the standard of
     care." (grade ~17)
   - After: "Most people have surgery first. The surgeon takes out as much of the tumour as is
     safe. After that, most people have radiation and chemotherapy. This order is the usual
     plan." (grade ~4)
3. **Replace nominalisations with verbs.** "resection" → "taking it out"; "administration" →
   "you get"; "monitoring" → "we check".
4. **Use concrete, sensory description for procedures.** What you will see, feel, hear, and
   how long it lasts. This also does the anxiety work: it converts an unknown into a
   sequence. It maps to the meningioma finding that people wanted to know what recovery would
   actually be like. https://pmc.ncbi.nlm.nih.gov/articles/PMC7318855/
5. **One idea per sentence, one question per heading.** Headings as questions match how people
   type (https://pmc.ncbi.nlm.nih.gov/articles/PMC1550578/) and are already proven in
   NCI-CONNECT's template (https://www.cancer.gov/rare-brain-spine-tumor/tumors/gliosarcoma).
6. **Answer in the first sentence under the heading** (inverted pyramid), because most readers
   will not reach the second paragraph. https://www.nngroup.com/articles/how-users-read-on-the-web/
7. **Comparisons, not metaphors that overclaim.** The genetics plain-language work used
   "blueprint or recipe" for DNA and "building blocks" style comparisons, developed *with*
   low-literacy patients and validated by them. Borrow the method, not just the words.
   https://pmc.ncbi.nlm.nih.gov/articles/PMC8062319/
8. **Keep the qualifier, shorten it.** Accuracy loss usually happens when a hedge gets cut.
   Do not cut it — compress it. "may" / "often" / "usually" / "not for everyone" are all
   one-syllable-ish and 6th-grade safe. "Most people have…" is both accurate and short.

### Surgery

- Lead with purpose, not technique: why an operation is being considered at all (take it out /
  take a piece to test it / relieve pressure). NCI's question list starts with exactly this —
  "Why do I need surgery?" https://www.cancer.gov/rare-brain-spine-tumor/living/questions/neurosurgeon
- Replace "gross total resection" / "debulking" with "taking out all of the tumour the
  surgeon can see" and "taking out as much as is safe".
- Explain "awake craniotomy" by purpose: "You are awake for part of it. That way the team can
  check your speech and movement while they work. It sounds frightening. It is done to protect
  the parts of your brain you use every day."
- Always include the timeline block (how long the operation, how long in hospital, how long to
  recover, when normal activities resume) — those are four of the most consistently asked
  questions across every published list.
  https://www.cancer.gov/rare-brain-spine-tumor/living/questions/neurosurgeon
- Include "what happens if I don't have surgery?" — it is on NCI's list and is the question
  people are least likely to ask out loud.
  https://www.cancer.gov/rare-brain-spine-tumor/living/questions/recurrence

### Radiation

- Lead with the experience: "You lie still on a table. A machine moves around you. It does not
  touch you. You do not feel the radiation. Each visit is short. Most people come back for
  several weeks."
- Then the mechanism in one line: "Radiation damages tumour cells so they cannot keep growing."
- Then the schedule, because "for how many weeks / how many per week" is a named question.
  https://www.uclahealth.org/cancer/cancer-services/brain-tumor/patient-resources/questions-ask-your-doctor
- Then side effects — with timing, because the attribution problem is documented: people
  cannot tell tumour from treatment. https://pmc.ncbi.nlm.nih.gov/articles/PMC5333312/
  "Tiredness is common. It often builds up over the weeks and can last a while after you
  finish. That is expected. It does not mean the tumour is growing."
- A mask is worth a plain sentence of its own; Macmillan treats "making a radiotherapy mask"
  as a standalone topic, which tells you people find it distressing.
  https://be.macmillan.org.uk/be/s-519-brain-and-spinal-tumours.aspx

### Molecular markers / grade

- Frame the whole category first, once, in the treatment/tests library, and reuse:
  "After surgery or a biopsy, the lab looks at the tumour cells. It also tests the tumour's
  genes. Genes are the instructions inside cells. The lab looks for changes in those
  instructions. These changes are like labels on your tumour. The labels help your team pick
  the treatment most likely to work. **The labels are about the tumour, not about you.** You
  did not do anything to cause them, and you cannot pass them on to your children."
  (The "change or mistake in a gene" phrasing and the not-inherited clarification both come
  from validated plain-language work: https://pmc.ncbi.nlm.nih.gov/articles/PMC8062319/ ;
  the inherited-vs-tumour distinction is the standardised terminology adopted across 40+
  cancer organisations: https://www.cancersupportcommunity.org/blog/precision-medicine-plain-language-lexicon )
- Then per marker, three short lines: what it is, what your team does with it, what it does
  **not** mean. Keep the third line — it is the anti-hype guardrail.
- For grade, copy CRUK's approach exactly: describe the *appearance and behaviour* of cells,
  never survival. "Grade 1: The cells look very much like normal cells. They are usually slow
  growing and less likely to spread… Grade 4: The tumour cells look very abnormal. These
  tumours grow quickly and often come back after treatment."
  https://www.cancerresearchuk.org/about-cancer/brain-tumours/grades
  Then add the sentence CRUK implies but does not say plainly: **"Grade is a description of
  the cells. It is not a countdown."**
- Explicitly disambiguate the two terms the search data shows people conflate:
  adjuvant/neoadjuvant ("before" vs "after" the main treatment) and chemotherapy vs
  immunotherapy. https://pmc.ncbi.nlm.nih.gov/articles/PMC11395687/

## 3.3 Framing uncertainty and frightening information

The scoping review of practice recommendations for communicating uncertainty found 13
recommendations across three goals — preparing for uncertainty, informing about uncertainty,
and helping people cope with it. Only 13 of 47 publications offered empirically supported
recommendations, so treat these as expert consensus rather than proven fact.
https://pmc.ncbi.nlm.nih.gov/articles/PMC8369117/

Adaptable to written content:

1. **Warn before you disclose.** Signal that a section contains hard information *before* the
   reader is inside it, so they can choose. This is the written analogue of "warn patients
   beforehand" and it operationalises the "not everyone wants to know" finding.
   https://pmc.ncbi.nlm.nih.gov/articles/PMC8369117/ ,
   https://www.ncbi.nlm.nih.gov/pmc/articles/PMC8062391/
2. **Prefer implicit to explicit uncertainty wording where possible.** Some people respond
   better to "it could be" than to "we don't know".
   https://pmc.ncbi.nlm.nih.gov/articles/PMC8369117/
3. **Use best case / worst case / most likely.** This is the Schwarze/Taylor Best Case/Worst
   Case framework: present a *choice between treatments*, describe **outcomes** rather than a
   list of discrete procedural risks, and describe how a person might actually experience
   each outcome. After training, shared decision-making improved on an objective measure.
   https://jamanetwork.com/journals/jamasurgery/fullarticle/2599170 , https://hipxchange.org/toolkit/bcwc/
   Cancer patients prefer prognostic information framed this way.
   https://pmc.ncbi.nlm.nih.gov/articles/PMC8369117/
4. **Give control back.** Highly anxious readers benefit disproportionately when the material
   provides a sense of control to counterbalance the uncertainty: name what *is* known, what
   happens next, what the reader can do today, and when they will know more.
   https://pmc.ncbi.nlm.nih.gov/articles/PMC8369117/
5. **Alternate hard information with something solid.** Uncertain news followed by concrete,
   reassuring, actionable content. Noted in the review, with the honest caveat that the
   evidence for it is thin. https://pmc.ncbi.nlm.nih.gov/articles/PMC8369117/
6. **Do not be afraid that stating uncertainty will frighten people.** A web-based RCT with
   1,727 adults varied the degree, type and number of sources of uncertainty in a written
   research summary and found minimal effects on understanding, trust, anxiety or intended
   decisions; only stacking three sources of uncertainty at once measurably reduced perceived
   benefit (47.2% vs 56.3%). Practical read: **state uncertainty plainly, but do not pile up
   three hedges in one paragraph.** https://pmc.ncbi.nlm.nih.gov/articles/PMC7445603/
7. **Normalise, then instruct.** For scanxiety specifically — it is common, peaks in the wait
   between scan and result, and is worse in the first year after diagnosis. Say so, then give
   the reader something to do with the wait.
   https://link.springer.com/article/10.1007/s00520-021-06454-9
8. **Never leave a frightening statement as the last thing on the screen.** Follow it with a
   next step or a place to get help. This is PEMAT actionability plus the "provide control"
   recommendation, combined.

---

# PART 4 — Explaining statistics without publishing numbers

## 4.1 The canonical argument

Stephen Jay Gould, "The Median Isn't the Message", written after his 1982 mesothelioma
diagnosis with a median survival of 8 months. The core insight: the median is a midpoint, not
a sentence, and it conceals the right tail — "The distribution of variation had to be
right-skewed… the upper half can extend out for years." Reprinted by the AMA *Journal of
Ethics* in 2013 and revisited in *JCO* in 2020.
https://journalofethics.ama-assn.org/article/message-isnt-mean-we-may-think/2013-01 ,
https://ascopubs.org/doi/10.1200/JCO.19.03078

## 4.2 The clinical-communication literature around it

**Kirkebøen, "The median isn't the message": how to communicate the uncertainties of survival
prognoses to cancer patients in a realistic and hopeful way**, *European Journal of Cancer
Care* 2019. https://pmc.ncbi.nlm.nih.gov/articles/PMC9285825/ ,
https://onlinelibrary.wiley.com/doi/10.1111/ecc.13056

Its three-part framework:
1. Establish that you are describing a **group**, not making an individual prediction.
2. Present the **distribution**, not just the midpoint — and state the median positively
   ("half will live *longer* than…") rather than negatively.
3. Show and name the **right skew** — that the tail extends a long way — and say so
   explicitly.

Its experimental finding: explaining the right-skewed distribution increased hopefulness
*and* produced a more realistic understanding of the variation, simultaneously. It also
reports that 85% of patients wanted to know the longest survival time with treatment, and
that patients can understand and accept that a prognosis is uncertain.
https://pmc.ncbi.nlm.nih.gov/articles/PMC9285825/

**Supporting evidence from the wider prognosis literature:**
- All patients want honesty, and the vast majority want some broad indication of prognosis —
  but preferences for *quantitative* information vary widely.
  https://www.annalsofoncology.org/article/S0923-7534(19)47676-X/fulltext
- Nine "best" approaches identified: honesty and clarity, leaving room for hope, personalising
  language, empowering the patient and family, anticipatory guidance, setting the scene.
  https://www.annalsofoncology.org/article/S0923-7534(19)47676-X/fulltext
- In malignant glioma specifically, accurate prognostic awareness ranged from 25% to 100%
  across studies, and there is a subset of patients who do not want accurate prognostic
  information. https://link.springer.com/article/10.1007/s11060-014-1487-1
- First communication of prognosis in HGG requires careful negotiation because people cannot
  process detailed prognostic information while in shock; information should be individualised.
  https://link.springer.com/article/10.1007/s11060-010-0495-z
- A recent framework proposes **staged disclosure**: incurability and likely neurocognitive
  decline discussed at the first encounter, but life-expectancy estimates deferred to a later
  visit. https://link.springer.com/article/10.1007/s11060-025-05356-8

**This is the strongest possible support for our decision.** A website cannot negotiate, cannot
check readiness, and cannot stage disclosure across visits. Publishing a median survival
figure on a page a newly diagnosed person will find at 2am is precisely the thing every
strand of this literature says not to do. Teaching the *concept* so they can interpret the
number when their team gives it to them in context is the defensible substitute.

## 4.3 The best existing plain-language wordings to adapt

NCI:
> "The estimate of how the disease will go for you is called prognosis."
> "Doctors estimate prognosis by using statistics that researchers have collected over many
> years about people with the same type of cancer."
> "Because statistics are based on large groups of people, they cannot be used to predict
> exactly what will happen to you. Everyone is different."
> "A prognosis is an educated guess. Your doctor cannot be certain how it will go for you."
https://www.cancer.gov/about-cancer/diagnosis-staging/prognosis

Cancer Research UK:
> "No one can tell you exactly how long you will live."
> "The terms 1 year survival and 5 year survival don't mean that you will only live for 1 or
> 5 years."
> "These are general statistics based on large groups of people. Remember, they can't tell
> you what will happen in your individual case."
https://www.cancerresearchuk.org/about-cancer/brain-tumours/survival

American Cancer Society (the staleness caveat — the most under-used and most useful one):
> "People now being diagnosed… may have a better outlook than these numbers show. Treatments
> improve over time, and these numbers are based on people who were diagnosed and treated at
> least five years earlier."
https://www.cancer.org/cancer/types/pancreatic-neuroendocrine-tumor/detection-diagnosis-staging/survival-rates.html

The five-year benchmark is a researchers' comparison convention, not a prediction horizon.
https://www.curetoday.com/view/what-five-years-really-means

## 4.4 Draft wording for BrainHarbor (no figures)

**Framing block — "Why we don't put numbers here"**
> You will find survival numbers on other sites. We don't put them on ours.
> Here is why. The numbers come from large groups of people. They were treated years ago.
> They were older or younger than you. Their tumours were not your tumour.
> A number from that group cannot tell you what will happen to you.
> Your team knows your scans, your test results and your health. Ask them. This page is here
> to help you understand what they say.

**"What grade means"**
> Grade is about how the tumour cells look under a microscope.
> Low grade means the cells look close to normal. They usually grow slowly.
> High grade means the cells look very different from normal. They usually grow faster.
> Grade helps your team choose treatment.
> Grade is a description of the cells. **It is not a countdown.**
(Cell-appearance framing from https://www.cancerresearchuk.org/about-cancer/brain-tumours/grades )

**"What median survival means"**
> Sometimes doctors line up how long a large group of people lived, from shortest to longest.
> The middle of that line is called the median.
> Half of that group lived less time than the middle. Half lived **longer**.
> Some people at the far end of the line lived much, much longer.
> The line is a picture of a group. It is not a prediction about you.
(Positive framing of the median and explicit right-tail from
https://pmc.ncbi.nlm.nih.gov/articles/PMC9285825/ )

**"What a five-year survival rate means"**
> A five-year survival rate counts how many people in a study were still alive five years
> after they found out they had the tumour.
> It does **not** mean people live five years. Many live longer.
> Researchers picked five years so they could compare studies. It is a measuring stick, not a
> deadline.
> These counts also come from people treated years ago. Treatments have changed since then.
(Benchmark-not-prediction from https://www.curetoday.com/view/what-five-years-really-means ;
staleness caveat from https://www.cancer.org/cancer/types/pancreatic-neuroendocrine-tumor/detection-diagnosis-staging/survival-rates.html )

**"If you want to know more" — the ask-your-team bridge**
> Some people want to know the numbers. Some people don't. Both are okay.
> If you do want to know, these are good ways to ask:
> - "What does this mean for someone like me?"
> - "What is the best case? What is the worst case? What is most likely?"
> - "How sure are you about that?"
> - "How much detail do you want to give me, and how much do I want to hear?"
> You can change your mind later. You can ask again.
(Best/worst/most likely from https://jamanetwork.com/journals/jamasurgery/fullarticle/2599170 ;
individualised, re-offered disclosure from https://www.ncbi.nlm.nih.gov/books/NBK570056/ and
https://www.ncbi.nlm.nih.gov/pmc/articles/PMC8062391/ )

**Reader-choice gate (progressive disclosure)**
> The next part is about outlook. Some people want to read it. Some people would rather not.
> You can skip it and come back another day. Nothing else on this page depends on it.
(Warn-before-disclosing from https://pmc.ncbi.nlm.nih.gov/articles/PMC8369117/ )

---

# PART 5 — Recommended standard section order for a tumor page

Justification is per line. The design principle throughout: **answer first, epidemiology
never, prognosis by consent, action at the end of every scary block.**

| # | Section (heading as a question) | Why here |
|---|---|---|
| 0 | **Summary box: "The short version"** — 3–5 short sentences | PEMAT items 1 and 11 (purpose evident, provides a summary); inverted pyramid; readers consume ~20–28% of the page. https://www.ahrq.gov/health-literacy/patient-education/pemat-p.html , https://www.nngroup.com/articles/how-users-read-on-the-web/ |
| 1 | **What is a [tumor]?** | The single most-asked identity question; every comparator opens here. https://www.cancerresearchuk.org/about-cancer/brain-tumours/types/glioblastoma |
| 2 | **Is it cancer? What does its grade mean?** | The documented core confusion in caregiver interviews; resolve benign/malignant and grade immediately, in cell-behaviour terms not survival terms. https://pmc.ncbi.nlm.nih.gov/articles/PMC2600839/ , https://www.cancerresearchuk.org/about-cancer/brain-tumours/grades |
| 3 | **Where does it grow, and why does it cause these symptoms?** | Location→symptom mapping answers "why is this happening to me", which listing symptoms alone does not. ABTA's strongest move. https://www.abta.org/about-brain-tumors/brain-tumor-diagnosis/brain-tumor-signs-symptoms/ |
| 4 | **What symptoms does it cause?** + **When should I call for help right now?** | Symptom queries outnumber treatment queries; people fail to recognise neurological symptoms; "what do I call about" is an explicitly listed patient question. https://pmc.ncbi.nlm.nih.gov/articles/PMC1550578/ , https://pmc.ncbi.nlm.nih.gov/articles/PMC5333312/ , https://braintumor.org/brain-tumors/diagnosis-treatment/diagnosis/key-questions/ |
| 5 | **How do doctors find out it is this?** → links to tests library | "Tests and next steps" framing (NHS) beats "Diagnosis"; imaging is one of the three highest-volume forum topics. https://www.nhs.uk/conditions/malignant-brain-tumour/ , https://pmc.ncbi.nlm.nih.gov/articles/PMC12119380/ |
| 6 | **What do the words on my report mean?** (tumor name, grade, molecular markers) | The clearest content gap across all comparators; documented terminology confusion in real search logs. https://pmc.ncbi.nlm.nih.gov/articles/PMC11395687/ , https://www.cancersupportcommunity.org/blog/precision-medicine-plain-language-lexicon |
| 7 | **How is it usually treated?** — short contextual summary per modality, each linking to the treatment library | The CRUK hybrid model; chemo/surgery/radiation are ~78% of forum treatment discussion combined. https://www.cancerresearchuk.org/about-cancer/brain-tumours/types/glioblastoma , https://pmc.ncbi.nlm.nih.gov/articles/PMC12119380/ |
| 8 | **What is treatment actually like, and what is normal afterwards?** | The loudest gap in the qualitative literature — "the real fight started after I woke up". Also fixes the symptom-attribution problem. https://pmc.ncbi.nlm.nih.gov/articles/PMC7318855/ , https://pmc.ncbi.nlm.nih.gov/articles/PMC5333312/ |
| 9 | **Everyday life: driving, work, money, seizures, tiredness, memory** | Financial and logistical strain *dominated* real forum discussion; driving/work loss are concrete and immediate. CRUK is the only comparator that puts driving on the tumour page. https://pmc.ncbi.nlm.nih.gov/articles/PMC12119380/ , https://academic.oup.com/nop/article/10/6/565/7226298 , https://www.cancerresearchuk.org/about-cancer/brain-tumours/types/glioblastoma |
| 10 | **Follow-up scans, and what to do while you wait** | Scanxiety is common, severe (mean 6/10), peaks in the wait, and is worse in the first year. https://link.springer.com/article/10.1007/s00520-021-06454-9 |
| 11 | **If it comes back** | A distinct, named question set; NCI-CONNECT treats recurrence as its own journey stage. https://www.cancer.gov/rare-brain-spine-tumor/living/questions/recurrence |
| 12 | **Outlook — behind a reader-choice gate, no numbers** | Not everyone wants to know; shock impairs processing; staged disclosure is the emerging clinical standard. Putting it here means the reader has already got everything actionable before meeting anything frightening. https://www.ncbi.nlm.nih.gov/pmc/articles/PMC8062391/ , https://link.springer.com/article/10.1007/s11060-025-05356-8 |
| 13 | **For the person caring for someone with this** | Care coordination and advocacy are raised almost exclusively by caregivers, and no comparator gives them a lane inside the tumour page. https://pmc.ncbi.nlm.nih.gov/articles/PMC12119380/ , https://pmc.ncbi.nlm.nih.gov/articles/PMC2600839/ |
| 14 | **Questions to ask your team** (printable/copyable) | PEMAT actionability requires a concrete action; every major org has good lists but files them away from the point of need. https://www.ahrq.gov/health-literacy/patient-education/pemat-p.html , https://www.cancer.gov/rare-brain-spine-tumor/living/questions/neurosurgeon |
| 15 | **Where to get support** | Peer connection was the most emphasised finding of the forum study; isolation is documented. https://pmc.ncbi.nlm.nih.gov/articles/PMC12119380/ , https://pmc.ncbi.nlm.nih.gov/articles/PMC7318855/ |
| 16 | **Where this came from / last reviewed** | No website in the national evaluation met all four JAMA benchmark criteria; provenance is cheap differentiation. https://academic.oup.com/nop/article/12/5/901/8124733 |

### Deliberate omissions and moves

- **"How common is it?" is cut** (or demoted to a single clause inside section 1). CRUK, NHS
  and NCI-CONNECT all place epidemiology high. For a frightened newcomer it is filler
  competing for the ~20–28% of the page they will actually read.
  https://www.nngroup.com/articles/how-users-read-on-the-web/
- **"What causes it?" is demoted or cut.** Cause/risk queries were under 2% of cancer search
  volume, and for brain tumours the honest answer is usually "we don't know", which is a bad
  thing to put in a reader's first three screens. https://pmc.ncbi.nlm.nih.gov/articles/PMC1550578/
- **"What does it look like on an MRI?" is cut from the patient page** (NCI-CONNECT includes
  it second) — it serves clinicians, not patients.
  https://www.cancer.gov/rare-brain-spine-tumor/tumors/gliosarcoma
- **Prognosis moves from position 8 (NCI-CONNECT) to position 12, behind a gate.**
  https://www.cancer.gov/rare-brain-spine-tumor/tumors/gliosarcoma vs
  https://www.ncbi.nlm.nih.gov/pmc/articles/PMC8062391/

---

# PART 6 — Recommendations for this project

1. **Adopt the hybrid three-layer model explicitly.** Tumor page carries a 3–6 sentence
   contextual summary per treatment modality ("for this tumor, surgery usually means…") and
   links to the canonical treatment page. Only CRUK does this; nobody does it at 6th grade.
2. **Make every section heading a question.** Steal NCI-CONNECT's template, fix its order.
3. **Ship a per-tumor "what the words on your report mean" block.** This is the clearest
   unoccupied gap and it is cheap for us because we already version prompt templates.
4. **Give caregivers a named block on every tumor page**, not a separate silo — targeted at
   coordination, advocacy, untrained nursing tasks, and permission to ask questions.
5. **Gate the outlook section.** A reader-initiated expand, with a warning line above it.
   Publish no figures; publish the concepts and the ask-your-team bridge.
6. **Add a "call right now if…" block to every tumor page**, adjacent to symptoms.
7. **Score pages against PEMAT-P and the CDC Index in CI**, not just Flesch-Kincaid.
   Readability formulas do not measure structure, and the two things the comparators fail on
   are structure and actionability, not only word length.
   https://www.ahrq.gov/health-literacy/patient-education/pemat.html , https://www.cdc.gov/ccindex/index.html
8. **Adopt the hard numeric writing rules** as lint targets: sentences 8–15 words, paragraphs
   3–5 sentences, one idea per paragraph, active voice, define-then-name for every medical
   term. https://stacks.cdc.gov/view/cdc/11938 , https://files.eric.ed.gov/fulltext/ED381691.pdf
9. **Never end a section on a frightening sentence.** Enforce it as a review rule: every
   block that names a risk ends with a next step, a question to ask, or a place to get help.
   https://pmc.ncbi.nlm.nih.gov/articles/PMC8369117/
10. **Publish sources and a last-reviewed date on every page** — no comparator site met all
    four JAMA benchmark criteria. https://academic.oup.com/nop/article/12/5/901/8124733

---

# Sources

**Patient and caregiver needs**
- https://pmc.ncbi.nlm.nih.gov/articles/PMC12119380/
- https://pmc.ncbi.nlm.nih.gov/articles/PMC2600839/
- https://pubmed.ncbi.nlm.nih.gov/17993635/
- https://pmc.ncbi.nlm.nih.gov/articles/PMC7318855/
- https://academic.oup.com/nop/article/7/2/228/5610505
- https://www.ncbi.nlm.nih.gov/books/NBK570056/
- https://pubmed.ncbi.nlm.nih.gov/19762197/
- https://www.sciencedirect.com/science/article/abs/pii/S0738399109004017
- https://www.ncbi.nlm.nih.gov/pmc/articles/PMC8062391/
- https://pubmed.ncbi.nlm.nih.gov/33125538/
- https://pmc.ncbi.nlm.nih.gov/articles/PMC5333312/
- https://www.ncbi.nlm.nih.gov/pmc/articles/PMC8267327/
- https://www.ncbi.nlm.nih.gov/pmc/articles/PMC8795012/
- https://pubmed.ncbi.nlm.nih.gov/18329220/
- https://pubmed.ncbi.nlm.nih.gov/29603812/
- https://www.ncbi.nlm.nih.gov/pmc/articles/PMC12124322/
- https://academic.oup.com/nop/article/10/6/565/7226298
- https://link.springer.com/article/10.1007/s00520-021-06454-9
- https://pubmed.ncbi.nlm.nih.gov/34076779/
- https://link.springer.com/article/10.1007/s11060-016-2152-7
- https://www.ncbi.nlm.nih.gov/pmc/articles/PMC11940939/

**Search / information-seeking behaviour**
- https://pmc.ncbi.nlm.nih.gov/articles/PMC1550578/
- https://pmc.ncbi.nlm.nih.gov/articles/PMC11395687/
- https://pmc.ncbi.nlm.nih.gov/articles/PMC6588432/

**Published "questions to ask" lists**
- https://www.cancer.gov/rare-brain-spine-tumor/living/questions/neurosurgeon
- https://www.cancer.gov/rare-brain-spine-tumor/living/questions/recurrence
- https://braintumor.org/brain-tumors/diagnosis-treatment/diagnosis/key-questions/
- https://www.abta.org/mindmatters/brain-tumor-101/top-10-questions-to-ask/
- https://www.thebraintumourcharity.org/brain-tumour-diagnosis-treatment/navigating-the-healthcare-system/questions-to-ask/
- https://www.uclahealth.org/cancer/cancer-services/brain-tumor/patient-resources/questions-ask-your-doctor
- https://www.moffitt.org/cancers/brain-cancer/faqs/questions-to-ask-your-brain-cancer-specialist/
- https://www.roswellpark.org/cancertalk/202405/5-questions-ask-your-brain-surgeon

**Comparable site information architecture**
- https://www.cancer.gov/types/brain
- https://www.ncbi.nlm.nih.gov/books/NBK66023/
- https://www.cancer.gov/rare-brain-spine-tumor/
- https://www.cancer.gov/rare-brain-spine-tumor/tumors
- https://www.cancer.gov/rare-brain-spine-tumor/tumors/gliosarcoma
- https://www.cancer.gov/rare-brain-spine-tumor/living/newly-diagnosed
- https://www.cancerresearchuk.org/about-cancer/brain-tumours
- https://www.cancerresearchuk.org/about-cancer/brain-tumours/types/glioblastoma
- https://www.cancerresearchuk.org/about-cancer/brain-tumours/grades
- https://www.cancerresearchuk.org/about-cancer/brain-tumours/survival
- https://www.macmillan.org.uk/cancer-information-and-support/brain-tumour
- https://www.macmillan.org.uk/cancer-information-and-support/brain-tumour/glioma
- https://be.macmillan.org.uk/be/s-519-brain-and-spinal-tumours.aspx
- https://www.thebraintumourcharity.org/brain-tumour-diagnosis-treatment/treating-brain-tumours/
- https://www.thebraintumourcharity.org/brain-tumour-diagnosis-treatment/types-of-brain-tumour-adult/
- https://www.thebraintumourcharity.org/brain-tumour-diagnosis-treatment/how-brain-tumours-are-diagnosed/how-brain-tumours-are-graded/
- https://www.thebraintumourcharity.org/living-with-a-brain-tumour/get-support/
- https://www.abta.org/about-brain-tumors/
- https://www.abta.org/about-brain-tumors/brain-tumor-types/
- https://www.abta.org/about-brain-tumors/brain-tumor-diagnosis/brain-tumor-signs-symptoms/
- https://www.abta.org/about-brain-tumors/brain-tumor-education/
- https://braintumor.org/brain-tumors/about-brain-tumors/brain-tumor-types/
- https://www.mayoclinic.org/diseases-conditions/glioma/symptoms-causes/syc-20350251
- https://www.hopkinsmedicine.org/brain-tumor
- https://www.hopkinsmedicine.org/brain-tumor/patient-resources
- https://www.nhs.uk/conditions/malignant-brain-tumour/
- https://brainstrust.org.uk/about-brainstrust/our-story/
- https://brainstrust.org.uk/brain-tumour-support/resources/downloads/

**Readability and plain language**
- https://academic.oup.com/nop/article/12/5/901/8124733
- https://pubmed.ncbi.nlm.nih.gov/31785430/
- https://pubmed.ncbi.nlm.nih.gov/40543265/
- https://www.jmir.org/2025/1/e69955
- https://onlinelibrary.wiley.com/doi/abs/10.1111/j.1464-5491.1993.tb00178.x
- https://nces.ed.gov/naal/health_results.asp
- https://pmc.ncbi.nlm.nih.gov/articles/PMC2668931/
- https://www.cdc.gov/ccindex/index.html
- https://www.cdc.gov/ccindex/tool/how-to-use.html
- https://www.cdc.gov/ccindex/pdf/full-index-score-sheet.pdf
- https://www.ahrq.gov/health-literacy/patient-education/pemat.html
- https://www.ahrq.gov/health-literacy/patient-education/pemat-p.html
- https://www.ahrq.gov/sites/default/files/publications/files/pemat_guide.pdf
- https://stacks.cdc.gov/view/cdc/11938
- https://files.eric.ed.gov/fulltext/ED381691.pdf
- https://www.nngroup.com/articles/how-users-read-on-the-web/
- https://www.nngroup.com/articles/f-shaped-pattern-reading-web-content-discovered/
- https://pubmed.ncbi.nlm.nih.gov/21682542/
- https://acnr.co.uk/articles/making-information-accessible-for-people-with-aphasia/
- https://pmc.ncbi.nlm.nih.gov/articles/PMC8062319/
- https://www.cancersupportcommunity.org/blog/precision-medicine-plain-language-lexicon
- https://pmc.ncbi.nlm.nih.gov/articles/PMC3514986/

**Prognosis, uncertainty and statistics communication**
- https://pmc.ncbi.nlm.nih.gov/articles/PMC9285825/
- https://onlinelibrary.wiley.com/doi/10.1111/ecc.13056
- https://journalofethics.ama-assn.org/article/message-isnt-mean-we-may-think/2013-01
- https://ascopubs.org/doi/10.1200/JCO.19.03078
- https://www.annalsofoncology.org/article/S0923-7534(19)47676-X/fulltext
- https://pubmed.ncbi.nlm.nih.gov/18952746/
- https://link.springer.com/article/10.1007/s11060-014-1487-1
- https://link.springer.com/article/10.1007/s11060-010-0495-z
- https://link.springer.com/article/10.1007/s11060-025-05356-8
- https://pmc.ncbi.nlm.nih.gov/articles/PMC10022487/
- https://pmc.ncbi.nlm.nih.gov/articles/PMC8369117/
- https://pmc.ncbi.nlm.nih.gov/articles/PMC7445603/
- https://jamanetwork.com/journals/jamasurgery/fullarticle/2599170
- https://hipxchange.org/toolkit/bcwc/
- https://www.ncbi.nlm.nih.gov/pmc/articles/PMC5479749/
- https://www.cancer.gov/about-cancer/diagnosis-staging/prognosis
- https://www.cancer.org/cancer/types/pancreatic-neuroendocrine-tumor/detection-diagnosis-staging/survival-rates.html
- https://www.curetoday.com/view/what-five-years-really-means
- https://my.clevelandclinic.org/health/articles/cancer-survival-rate
- https://www.ncbi.nlm.nih.gov/pmc/articles/PMC7181418/
