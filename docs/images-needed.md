# Images Needed — the per-page slot inventory (WI-562)

> **This file chooses no pictures. It is the list.** For every curated page it says
> how many figures the page wants, which heading each one goes after, which of the
> four kinds it is, what it should show, and one sentence of draft alt text.
> Sourcing the pictures is the next job; nothing here is committed to `wwwroot`.

**SHIPPED SO FAR: 15 of the 148 (WI-572, 2026-10-09)** — `dia-brain-regions` and the
fourteen other files of the location family, on 33 of the 464 slots. They are one
drawing: the variants are derived from the master, so the entries below describe
fifteen files and one piece of artwork. See `wwwroot/img/figures/README.md` before
editing any of them, and content-pipeline **§12.35** for what the first real picture
changed about this file. **Two of the drafts below were reworded by that item** (the
master's caption promised a report's words the master does not print, and the sellar
alt text described a seat where the drawing shades the gland), and **15 drafts across
the catalogue carried a British form the corpus's own list gates** — *labelled* six
times, *colour* six, and one each of *grey*, *licence* and *a drip* — which nothing
here was checking, because the gates were the 6.0 grade and §3b's figure rules. All 15
are fixed and the corpus's own American-forms list is read over every draft now. Five
more lines were reworded in the same pass and were NOT gate failures: *tablets* to
*pills* and *drips* to *infusions*, for consistency with the corpus, plus the two
rewordings above. **The gate is on the drafts only, and that is
deliberate:** the `Shows` and `Getting it` lines are notes to whoever sources the
picture, and this file's own prose says *licence* thirty-nine times while writing about
licensing. The rule is about the words that get pasted onto a page, which are the alt
text and the caption.

Measured against the live corpus on **2026-10-09**, at commit `3e9c0c6`.
The mechanism these slots are written against shipped at WI-561:
`docs/content-pipeline.md` §3b (what an author writes) and §12.33 (the ruling),
plus `src/BrainHarbor.Web/wwwroot/img/figures/README.md` for the rules that fail the build.

---

## 1. The number, and why it is not the one the ticket was written against

The density rule is **one figure per ~500 words of body text, minimum one per page**.
Applied to the corpus as it stands that is **461 slots across 50 reader-facing pages**.
This inventory lists **464**, because the rule is a floor and a few pages earn one more.

WI-562's own notes expected about thirty pictures, and §12.33 says "~30 real
pictures" in as many words. Both numbers are right. The ticket anticipated the
corpus growing — *"a tumor hub is ~180 words today and becomes a 17-section hub at
WI-513, and the rule moves it from one slot to four without anyone editing this
ticket"* — and then it grew harder than that: a hub is not four slots now, it is
eleven to fifteen. **Nothing is wrong with the rule. The corpus is 17x the size the
table in the ticket was measured on** — 217,148 words of reader-facing body text against the
13,133 that table adds up to.

### A slot is not a file, and that is what makes this finite

464 slots resolve to **148 distinct figures**, because the corpus is heavily templated:
the 23 tumor hubs share §12.3's seventeen sections, the test and treatment pages
repeat "what happens, step by step" and "what to ask your team", and eight shared
blocks are composed into pages dozens of times over. One picture of two people talking
at a kitchen table discharges **37 slots**, on its own.

### The count only comes out at 461 if you count the words a reader reads

The eight shared blocks (§3a) are composed into the page at render time. The raw
`.md` files total 351 slots; with the blocks composed in it is 461. The
`caregiver` block alone puts 413 words on 37 pages, and `mechanism` puts 854 on 18.
**Counting the files rather than the pages would have left the longest unbroken
stretches of text on the site uncovered** — which is the exact defect this item exists
to fix, since those stretches are identical on every page that includes them.

**And §3b says a shared block may not carry an image.** So the words a block
contributes demand a slot the block itself can never host: the slot is hosted by each
*including page*, at the directive line, declared in that page's own front matter. That
is not a workaround. It is what makes a shared figure correct rather than a compromise —
**the same words get the same picture**, and a credit still travels with the file on
every page that shows it.

### A shared photograph is honest. A shared scan of a diagnosis is a false claim.

An empty waiting room is an empty waiting room on all 50 pages. **A scan of a
diagnosis is not.** An MRI of a glioblastoma on the meningioma page says *this is what
yours looks like*, and it is wrong. So the 23 `pd-scan-*` members are one per hub,
each sourced and licence-checked on its own, and the inventory uses no member twice.

**Reuse is free for everything else, including the other kind-4 pictures** — the ones
that illustrate a MECHANISM rather than a diagnosis. `pd-hydrocephalus-ct` is big fluid
spaces, which is the thing a shunt exists for, and the same picture is true on
`/treatments/shunts` and on `/where-your-tumor-is`; neither page claims it is the
reader's own scan. `ImagesNeededInventoryTests` therefore checks this on the **kind**
with those pictures on a named exemption list carrying its reason, rather than on the
`pd-scan-` name: a rule enforced on a filename is a rule a rename walks out of.

### No figure goes inside an `:::outlook` gate

§3b permits one there. This inventory places none, on any page. The outlook
section's subject is how long people live, and the only thing a picture there could
illustrate is a prognosis — which is the one claim the whole anti-hype apparatus exists
to keep off these pages. Every tumor hub's *"What might happen over time"* is
deliberately empty of slots, and a future page must not quietly acquire one.

### Five pages get no slots, and that is a ruling rather than an omission

"Minimum one per page" is a rule about pages a reader reads **for care**. It is not a
rule about the terms of service. These five are listed in §7 with the reason each one
is out, so the next person does not re-open it — and the guard in
`ImagesNeededInventoryTests` holds the set closed: a NEW page cannot join it by being
forgotten.

---

## 2. The four kinds, and the two that are work rather than shopping

| Kind | What it is | Figures |
|---|---|---|
| 1 | Sourceable photo | **37** |
| 2 | Mock document | **15** |
| 3 | Simple diagram | **64** |
| 4 | Public-domain medical imagery | **32** |

**79 of the 148 figures are a DRAW or a BUILD** (kinds 3 and 2), not a search. That is the
number worth knowing before starting: the acceptance asks for these to be called out
separately *"because they are work rather than shopping and Dan should not discover
that halfway through"*. Every catalogue entry below opens its **Getting it** line with
`DRAW.` or `BUILD.` when it is one of those.

**No AI-generated imagery**, on any slot, consistent with the standing rule on feed
cards. Nothing can check that one for you.

---

## 3. Sourcing order — three waves

The waves are a reading of value per hour, not a priority on the pages. Wave 1 is what
makes the site visibly different; wave 3 is the long tail of per-tumor scans.

### Wave 1 — 49 figures, 316 slots

The figures that carry the most slots or the most meaning. The shared-block figures are all here: one drawing lands on up to 37 pages at once.

| Figure | Kind | Slots it fills |
|---|---|---|
| `ph-two-people-talking` | 1 | 37 |
| `dia-escalation-card` | 3 | 30 |
| `dia-followup-timeline` | 3 | 24 |
| `dia-treatment-path` | 3 | 24 |
| `doc-pathology-parts` | 2 | 23 |
| `dia-grade-ladder` | 3 | 22 |
| `dia-symptom-map` | 3 | 22 |
| `ph-recovery-bed` | 1 | 19 |
| `dia-mechanism-pressure` | 3 | 18 |
| `dia-brain-regions` | 3 | 8 |
| `ph-car-keys` | 1 | 8 |
| `dia-side-effect-timeline` | 3 | 5 |
| `doc-preop-sheet` | 2 | 5 |
| `dia-region-cerebellum` | 3 | 4 |
| `dia-scan-side-by-side` | 3 | 4 |
| `doc-medicines-list` | 2 | 4 |
| `ph-pill-box` | 1 | 4 |
| `dia-marker-meaning` | 3 | 3 |
| `dia-region-brainstem` | 3 | 3 |
| `dia-region-sellar` | 3 | 3 |
| `dia-tumor-or-medicine` | 3 | 3 |
| `dia-wait-timeline` | 3 | 3 |
| `ph-diary` | 1 | 3 |
| `ph-kitchen-table-papers` | 1 | 3 |
| `ph-mask` | 1 | 3 |
| `dia-region-frontal` | 3 | 2 |
| `dia-tissue-to-slide` | 3 | 2 |
| `doc-molecular-panel` | 2 | 2 |
| `doc-seizure-diary` | 2 | 2 |
| `ph-cannula` | 1 | 2 |
| `ph-microscope` | 1 | 2 |
| `ph-waiting-room` | 1 | 2 |
| `dia-chemo-cycle` | 3 | 1 |
| `dia-craniotomy-steps` | 3 | 1 |
| `dia-csf-shunt` | 3 | 1 |
| `dia-fractions-calendar` | 3 | 1 |
| `dia-report-layers` | 3 | 1 |
| `dia-seizure-do-not` | 3 | 1 |
| `dia-seizure-steps` | 3 | 1 |
| `dia-steroid-swelling` | 3 | 1 |
| `dia-visual-field` | 3 | 1 |
| `doc-addendum` | 2 | 1 |
| `doc-steroid-taper` | 2 | 1 |
| `ph-head-coil` | 1 | 1 |
| `ph-infusion-chair` | 1 | 1 |
| `ph-linac` | 1 | 1 |
| `ph-mri-scanner` | 1 | 1 |
| `ph-phone-note` | 1 | 1 |
| `ph-specimen-pot` | 1 | 1 |

### Wave 2 — 58 figures, 105 slots

Everything that makes a specific page work, once the shared set exists.

| Figure | Kind | Slots it fills |
|---|---|---|
| `dia-recurrence-options` | 3 | 17 |
| `doc-questions-sheet` | 2 | 8 |
| `dia-csf-spread` | 3 | 5 |
| `dia-tumor-board` | 3 | 5 |
| `dia-region-ventricles` | 3 | 3 |
| `ph-hospital-bag` | 1 | 3 |
| `ph-theatre` | 1 | 3 |
| `dia-biopsy-route` | 3 | 2 |
| `dia-crosswalk-names` | 3 | 2 |
| `dia-looks-worse-two-causes` | 3 | 2 |
| `dia-region-skull-base` | 3 | 2 |
| `dia-your-team` | 3 | 2 |
| `doc-implant-card` | 2 | 2 |
| `pd-hydrocephalus-ct` | 4 | 2 |
| `ph-blister-pack` | 1 | 2 |
| `ph-call-button` | 1 | 2 |
| `ph-walk-outdoors` | 1 | 2 |
| `dia-awake-mapping` | 3 | 1 |
| `dia-bbb` | 3 | 1 |
| `dia-cause-known-unknown` | 3 | 1 |
| `dia-extent-of-resection` | 3 | 1 |
| `dia-placebo-add-on` | 3 | 1 |
| `dia-proton-stop` | 3 | 1 |
| `dia-radiation-margin` | 3 | 1 |
| `dia-region-hearing-nerve` | 3 | 1 |
| `dia-region-meninges` | 3 | 1 |
| `dia-region-names` | 3 | 1 |
| `dia-region-occipital` | 3 | 1 |
| `dia-region-parietal` | 3 | 1 |
| `dia-region-pineal` | 3 | 1 |
| `dia-region-spinal-cord` | 3 | 1 |
| `dia-region-temporal` | 3 | 1 |
| `dia-scan-types` | 3 | 1 |
| `dia-seizure-threshold` | 3 | 1 |
| `dia-srs-vs` | 3 | 1 |
| `dia-target-lock` | 3 | 1 |
| `dia-trial-phases` | 3 | 1 |
| `dia-ttf-arrays` | 3 | 1 |
| `dia-tumor-vs-inherited` | 3 | 1 |
| `dia-two-tests-compared` | 3 | 1 |
| `dia-watch-cadence` | 3 | 1 |
| `doc-blood-count-sheet` | 2 | 1 |
| `doc-consent-first-page` | 2 | 1 |
| `doc-insurance-denial` | 2 | 1 |
| `doc-radiation-schedule` | 2 | 1 |
| `doc-radiology-report` | 2 | 1 |
| `pd-ct-head` | 4 | 1 |
| `pd-flair-swelling` | 4 | 1 |
| `pd-immunostain` | 4 | 1 |
| `ph-bath-shower` | 1 | 1 |
| `ph-ct-scanner` | 1 | 1 |
| `ph-head-dressing` | 1 | 1 |
| `ph-pencil-puzzle` | 1 | 1 |
| `ph-pharmacy-label` | 1 | 1 |
| `ph-reflex-hammer` | 1 | 1 |
| `ph-scan-console` | 1 | 1 |
| `ph-srs-frame` | 1 | 1 |
| `ph-walking-rail` | 1 | 1 |

### Wave 3 — 41 figures, 43 slots

The per-tumor scans and the long tail. Each needs its own licence check, so this is the slowest wave per picture and the easiest to get wrong.

| Figure | Kind | Slots it fills |
|---|---|---|
| `dia-posterior-fossa` | 3 | 3 |
| `dia-endoscopic-route` | 3 | 1 |
| `dia-fmri-task` | 3 | 1 |
| `dia-wafer-in-cavity` | 3 | 1 |
| `dia-wbrt-field` | 3 | 1 |
| `doc-neuropsych-summary` | 2 | 1 |
| `pd-angiogram` | 4 | 1 |
| `pd-dti-tracts` | 4 | 1 |
| `pd-mrs-graph` | 4 | 1 |
| `pd-perfusion-map` | 4 | 1 |
| `pd-pet-brain` | 4 | 1 |
| `pd-scan-acoustic-neuroma` | 4 | 1 |
| `pd-scan-all-brain-tumors` | 4 | 1 |
| `pd-scan-astrocytoma` | 4 | 1 |
| `pd-scan-atrt` | 4 | 1 |
| `pd-scan-brain-metastases` | 4 | 1 |
| `pd-scan-chordoma` | 4 | 1 |
| `pd-scan-cns-germ-cell-tumor` | 4 | 1 |
| `pd-scan-cns-lymphoma` | 4 | 1 |
| `pd-scan-craniopharyngioma` | 4 | 1 |
| `pd-scan-diffuse-midline-glioma` | 4 | 1 |
| `pd-scan-dipg` | 4 | 1 |
| `pd-scan-ependymoma` | 4 | 1 |
| `pd-scan-glioblastoma` | 4 | 1 |
| `pd-scan-glioma` | 4 | 1 |
| `pd-scan-hemangioblastoma` | 4 | 1 |
| `pd-scan-high-grade-glioma` | 4 | 1 |
| `pd-scan-low-grade-glioma` | 4 | 1 |
| `pd-scan-medulloblastoma` | 4 | 1 |
| `pd-scan-meningioma` | 4 | 1 |
| `pd-scan-oligodendroglioma` | 4 | 1 |
| `pd-scan-pediatric-brain-tumor` | 4 | 1 |
| `pd-scan-pituitary-tumor` | 4 | 1 |
| `pd-scan-spinal-cord-tumor` | 4 | 1 |
| `ph-asleep-chair` | 1 | 1 |
| `ph-clippers` | 1 | 1 |
| `ph-clock-water` | 1 | 1 |
| `ph-hat-scarf` | 1 | 1 |
| `ph-naming-cards` | 1 | 1 |
| `ph-proton-gantry` | 1 | 1 |
| `ph-shoulder-bag` | 1 | 1 |

---

## 4. Before any picture is committed

Everything in this list is checked mechanically once the file lands, so none of it is
advice (§3b, and `wwwroot/img/figures/README.md`):

- The file is **served from this site**, under `wwwroot/img/figures/`, named for its
  figure id above. A remote `src` fails.
- One line of Markdown, **alone in its own paragraph**, at the position this inventory
  gives: `![alt](/img/figures/id.svg "caption")`. Both are graded at 6.0 like every
  other sentence on the page, and they may not be the same words.
- **Write the caption in single quotes.** A double quote inside it ends the title early,
  the line stops being an image, and a reader gets raw Markdown (§12.33, finding 3).
- The page's front matter declares `credit`, `license`, `license_url`, `width` and
  `height` for that `src`. `source_url` is the only optional field.
- **The declared `width`/`height` are compared against the file itself.** They are what
  holds the space open for a lazy-loaded figure.
- **Never an NCI embedded image**, and nothing shipped with the AHFS or MedlinePlus drug
  monographs. This is now enforced **in both directions** — on the image's `source_url`
  host *and* on its `credit` string — so the thing most likely to be forgotten when
  somebody reaches for an existing brain diagram fails the build. cancer.gov's **text**
  is cited all over this corpus; only its **pictures** are barred.
- A figure must be a **top-level block**. Not a bullet, a quote or a table cell.
- A page may not show the same file twice, and a file no page declares fails the suite.

---

## 5. The figure catalogue

The draft alt text and caption are a **default per file**, written once here because they
describe the file. §3b lets a page vary them, and a caption often should: the same
drawing beside different prose can mean something slightly different. The drafts are
graded at 6.0 by `ImagesNeededInventoryTests` so the accessibility and reading-level
requirements are not an afterthought at paste time.

### Kind 1 — Sourceable photo (37)

Free stock or public domain. Dan can search for these directly, and they are the only kind that is genuinely shopping.

#### `ph-asleep-chair` — kind 1, wave 3

- **Shows:** Someone asleep in an armchair in daylight.
- **Draft alt text:** A person asleep in an armchair with daylight in the room.
- **Draft caption:** Sleeping in the day for weeks is a known effect, not a bad sign on its own.
- **Getting it:** Free stock. A whole-room shot reads better than a close-up of a face.
- **Lands at:** `wwwroot/img/figures/ph-asleep-chair.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/radiation-therapy`

#### `ph-bath-shower` — kind 1, wave 2

- **Shows:** A shower head in a tiled shower.
- **Draft alt text:** A shower head in a tiled shower, with the water running.
- **Draft caption:** A shower is safer than a bath. The danger is water you could fall into.
- **Getting it:** Free stock.
- **Lands at:** `wwwroot/img/figures/ph-bath-shower.svg` (or `.png` / `.jpg`) — **used by:** `/seizures/living-with`

#### `ph-blister-pack` — kind 1, wave 2

- **Shows:** A half-used blister pack of tablets.
- **Draft alt text:** A blister pack of pills with some of the pockets empty.
- **Draft caption:** Pills come in packs like this, so you can see what you have taken.
- **Getting it:** Free stock.
- **Lands at:** `wwwroot/img/figures/ph-blister-pack.svg` (or `.png` / `.jpg`) — **used by:** `/tests/getting-ready-for-surgery`, `/treatments/chemotherapy`

#### `ph-call-button` — kind 1, wave 2

- **Shows:** A hand holding a call button on a cord.
- **Draft alt text:** A hand holding a small button on a cord.
- **Draft caption:** You hold the button the whole time. Press it and they stop.
- **Getting it:** Free stock.
- **Lands at:** `wwwroot/img/figures/ph-call-button.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/radiation-therapy`, `/treatments/stereotactic-radiosurgery`

#### `ph-cannula` — kind 1, wave 1

- **Shows:** A thin tube taped into the back of a hand.
- **Draft alt text:** A thin tube held to the back of a hand with tape.
- **Draft caption:** The dye goes in through a small tube like this one.
- **Getting it:** Free stock. Plenty of clean examples exist. Nothing gory.
- **Lands at:** `wwwroot/img/figures/ph-cannula.svg` (or `.png` / `.jpg`) — **used by:** `/tests/ct-scan`, `/tests/mri`

#### `ph-car-keys` — kind 1, wave 1

- **Shows:** Car keys and a driving licence on a table.
- **Draft alt text:** Car keys and a driver's license lying on a table.
- **Draft caption:** Driving is the rule people hear about last and mind about most.
- **Getting it:** Free stock. A generic or blanked licence. Never a real one.
- **Lands at:** `wwwroot/img/figures/ph-car-keys.svg` (or `.png` / `.jpg`) — **used by:** **8 pages**

#### `ph-clippers` — kind 1, wave 3

- **Shows:** Hair clippers and a towel on a bathroom shelf.
- **Draft alt text:** Hair clippers and a folded towel on a shelf.
- **Draft caption:** The head has to stay shaved for the pads to stick. Most people do it at home.
- **Getting it:** Free stock.
- **Lands at:** `wwwroot/img/figures/ph-clippers.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/tumor-treating-fields`

#### `ph-clock-water` — kind 1, wave 3

- **Shows:** A glass of water next to a clock.
- **Draft alt text:** A glass of water on a table beside a clock.
- **Draft caption:** Water is allowed later than food. Ask for your own two times and write them down.
- **Getting it:** Free stock.
- **Lands at:** `wwwroot/img/figures/ph-clock-water.svg` (or `.png` / `.jpg`) — **used by:** `/tests/getting-ready-for-surgery`

#### `ph-ct-scanner` — kind 1, wave 2

- **Shows:** A CT scanner: a ring, shorter and thicker than an MRI, with a bed.
- **Draft alt text:** A short, thick ring with a bed sliding through the middle.
- **Draft caption:** A CT scanner. It is quicker than an MRI, and the ring is much shorter.
- **Getting it:** Free stock or Wikimedia.
- **Lands at:** `wwwroot/img/figures/ph-ct-scanner.svg` (or `.png` / `.jpg`) — **used by:** `/tests/ct-scan`

#### `ph-diary` — kind 1, wave 1

- **Shows:** A notebook and a pen, open on a table.
- **Draft alt text:** An open notebook with a pen resting on the page.
- **Draft caption:** Writing it down the same day beats trying to remember it weeks later.
- **Getting it:** Free stock.
- **Lands at:** `wwwroot/img/figures/ph-diary.svg` (or `.png` / `.jpg`) — **used by:** `/tests/follow-up-scans`, `/treatments/stereotactic-radiosurgery`, `/treatments/watch-and-wait`

#### `ph-hat-scarf` — kind 1, wave 3

- **Shows:** A soft hat and a scarf on a table.
- **Draft alt text:** A soft hat and a scarf folded on a table.
- **Draft caption:** A hat covers the pads when you would rather not explain them.
- **Getting it:** Free stock.
- **Lands at:** `wwwroot/img/figures/ph-hat-scarf.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/tumor-treating-fields`

#### `ph-head-coil` — kind 1, wave 1

- **Shows:** A head coil — the open cage that sits around your head — on the scanner bed.
- **Draft alt text:** An open plastic cage on a bed, shaped to fit around a head.
- **Draft caption:** The cage around your head is part of the camera. It does not touch you.
- **Getting it:** Free stock or Wikimedia.
- **Lands at:** `wwwroot/img/figures/ph-head-coil.svg` (or `.png` / `.jpg`) — **used by:** `/tests/mri`

#### `ph-head-dressing` — kind 1, wave 2

- **Shows:** The back of someone's head with a dressing taped over one side.
- **Draft alt text:** The back of a head with a white dressing taped on one side.
- **Draft caption:** The dressing is small. Only a strip of hair is shaved, not all of it.
- **Getting it:** Free stock is thin here, so this may have to be staged. No wound and no blood in the shot.
- **Lands at:** `wwwroot/img/figures/ph-head-dressing.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/craniotomy`

#### `ph-hospital-bag` — kind 1, wave 2

- **Shows:** A small packed bag by a front door.
- **Draft alt text:** A small packed bag standing by a front door.
- **Draft caption:** Pack small. A bag you can carry beats one somebody has to fetch.
- **Getting it:** Free stock.
- **Lands at:** `wwwroot/img/figures/ph-hospital-bag.svg` (or `.png` / `.jpg`) — **used by:** `/tests/getting-ready-for-surgery`, `/treatments/proton-therapy`, `/treatments/radiation-therapy`

#### `ph-infusion-chair` — kind 1, wave 1

- **Shows:** A chair in a day unit with a drip stand beside it.
- **Draft alt text:** A padded chair with an IV pole next to it.
- **Draft caption:** Many infusions are given in a chair like this, and you go home the same day.
- **Getting it:** Free stock.
- **Lands at:** `wwwroot/img/figures/ph-infusion-chair.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/chemotherapy`

#### `ph-kitchen-table-papers` — kind 1, wave 1

- **Shows:** Hands holding hospital letters at a kitchen table.
- **Draft alt text:** Hands holding a few printed letters at a kitchen table.
- **Draft caption:** Keep your own copy of everything. You will be asked for it more than once.
- **Getting it:** Free stock. No readable text in the letters.
- **Lands at:** `wwwroot/img/figures/ph-kitchen-table-papers.svg` (or `.png` / `.jpg`) — **used by:** `/start`, `/tests/pathology-report`, `/treatments/clinical-trials`

#### `ph-linac` — kind 1, wave 1

- **Shows:** A radiation treatment room: the machine arm over a flat bed.
- **Draft alt text:** A large arm above a flat bed in a bare room.
- **Draft caption:** The machine moves around you. You lie still and feel nothing.
- **Getting it:** Free stock or Wikimedia. Pick an empty room. A patient in the shot dates the picture fast.
- **Lands at:** `wwwroot/img/figures/ph-linac.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/radiation-therapy`

#### `ph-mask` — kind 1, wave 1

- **Shows:** A clear mesh radiation mask on a table, moulded to the shape of a face.
- **Draft alt text:** A clear mesh mask on a table, moulded to a face shape.
- **Draft caption:** The mask holds your head in the same place each day. It is made to fit only you.
- **Getting it:** Free stock or Wikimedia. The mask alone, on a table, is kinder than one on a face.
- **Lands at:** `wwwroot/img/figures/ph-mask.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/proton-therapy`, `/treatments/radiation-therapy`, `/treatments/stereotactic-radiosurgery`

#### `ph-microscope` — kind 1, wave 1

- **Shows:** A microscope with a glass slide under it.
- **Draft alt text:** A microscope on a bench, with a glass slide under the lens.
- **Draft caption:** Someone has to look at your tissue down a microscope. That takes days, not minutes.
- **Getting it:** Free stock or Wikimedia.
- **Lands at:** `wwwroot/img/figures/ph-microscope.svg` (or `.png` / `.jpg`) — **used by:** `/tests/biopsy`, `/tumors/all-brain-tumors`

#### `ph-mri-scanner` — kind 1, wave 1

- **Shows:** An MRI scanner in an empty scan room, seen from the side.
- **Draft alt text:** A wide white ring with a narrow bed in front of it.
- **Draft caption:** An MRI scanner. The bed slides into the ring, and the ring makes the pictures.
- **Getting it:** Free stock or Wikimedia. Hospital-room stock is easy to find. Make sure no patient face is in it.
- **Lands at:** `wwwroot/img/figures/ph-mri-scanner.svg` (or `.png` / `.jpg`) — **used by:** `/tests/mri`

#### `ph-naming-cards` — kind 1, wave 3

- **Shows:** Picture-naming cards laid out on a table.
- **Draft alt text:** A row of simple picture cards laid out on a table.
- **Draft caption:** While you are awake you name pictures out loud. That is how they find the speech area.
- **Getting it:** Free stock is thin, but flash cards are easy to photograph.
- **Lands at:** `wwwroot/img/figures/ph-naming-cards.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/awake-craniotomy`

#### `ph-pencil-puzzle` — kind 1, wave 2

- **Shows:** Pencil-and-paper puzzles, cards and a stopwatch on a table.
- **Draft alt text:** Paper puzzles, cards and a stopwatch laid out on a table.
- **Draft caption:** Memory testing is paper, cards and talking. There is no needle in it.
- **Getting it:** Free stock.
- **Lands at:** `wwwroot/img/figures/ph-pencil-puzzle.svg` (or `.png` / `.jpg`) — **used by:** `/tests/neuro-exam-and-memory-testing`

#### `ph-pharmacy-label` — kind 1, wave 2

- **Shows:** A medicine box with a pharmacy label on it, no brand showing.
- **Draft alt text:** A medicine box with a printed label on the front.
- **Draft caption:** The label says how much to take and how often. Keep it with the box.
- **Getting it:** Free stock, or photograph one with the name blanked out. Never a real patient name.
- **Lands at:** `wwwroot/img/figures/ph-pharmacy-label.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/anti-seizure-medicines`

#### `ph-phone-note` — kind 1, wave 1

- **Shows:** A phone next to a card with a ward number written on it.
- **Draft alt text:** A phone on a table next to a card with a number on it.
- **Draft caption:** Put the number where you can find it at three in the morning.
- **Getting it:** Free stock. No real phone number visible.
- **Lands at:** `wwwroot/img/figures/ph-phone-note.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/anti-seizure-medicines`

#### `ph-pill-box` — kind 1, wave 1

- **Shows:** A weekly pill box with the days of the week marked.
- **Draft alt text:** A plastic box with one small lid for each day of the week.
- **Draft caption:** A box like this is the easiest way to see if you took today's dose.
- **Getting it:** Free stock.
- **Lands at:** `wwwroot/img/figures/ph-pill-box.svg` (or `.png` / `.jpg`) — **used by:** **4 pages**

#### `ph-proton-gantry` — kind 1, wave 3

- **Shows:** A proton treatment room, which is much bigger than a normal radiation room.
- **Draft alt text:** A very large round machine wall behind a treatment bed.
- **Draft caption:** A proton room is far bigger, because the machine behind the wall is huge.
- **Getting it:** Free stock or Wikimedia.
- **Lands at:** `wwwroot/img/figures/ph-proton-gantry.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/proton-therapy`

#### `ph-recovery-bed` — kind 1, wave 1

- **Shows:** A hospital bed by a window on a quiet ward.
- **Draft alt text:** An empty hospital bed beside a window on a ward.
- **Draft caption:** Most people wake up on a ward like this one, with checks every hour at first.
- **Getting it:** Free stock.
- **Lands at:** `wwwroot/img/figures/ph-recovery-bed.svg` (or `.png` / `.jpg`) — **used by:** **19 pages**

#### `ph-reflex-hammer` — kind 1, wave 2

- **Shows:** A reflex hammer and a tuning fork on a desk.
- **Draft alt text:** A small rubber hammer and a metal tuning fork on a desk.
- **Draft caption:** The tools are this simple. The test is what the doctor sees you do.
- **Getting it:** Free stock.
- **Lands at:** `wwwroot/img/figures/ph-reflex-hammer.svg` (or `.png` / `.jpg`) — **used by:** `/tests/neuro-exam-and-memory-testing`

#### `ph-scan-console` — kind 1, wave 2

- **Shows:** A radiographer at a console, looking through the window into the scan room.
- **Draft alt text:** Two people at a desk of screens, looking through a window.
- **Draft caption:** Someone watches you the whole time, from the room next door.
- **Getting it:** Free stock. Avoid shots that look like a control centre. This one should look calm.
- **Lands at:** `wwwroot/img/figures/ph-scan-console.svg` (or `.png` / `.jpg`) — **used by:** `/tests/ct-scan`

#### `ph-shoulder-bag` — kind 1, wave 3

- **Shows:** A shoulder bag with a strap, of the size that carries a battery.
- **Draft alt text:** A plain shoulder bag with a wide strap.
- **Draft caption:** The battery and the box go in a bag you carry all day.
- **Getting it:** Free stock.
- **Lands at:** `wwwroot/img/figures/ph-shoulder-bag.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/tumor-treating-fields`

#### `ph-specimen-pot` — kind 1, wave 1

- **Shows:** A gloved hand holding a small clear specimen pot.
- **Draft alt text:** A gloved hand holding a small clear pot with a lid.
- **Draft caption:** Your tissue leaves the operating room in a small pot like this one.
- **Getting it:** Free stock. Nothing recognisable inside the pot.
- **Lands at:** `wwwroot/img/figures/ph-specimen-pot.svg` (or `.png` / `.jpg`) — **used by:** `/tests/waiting-for-results`

#### `ph-srs-frame` — kind 1, wave 2

- **Shows:** A radiosurgery machine with its helmet, or a head frame on a stand.
- **Draft alt text:** A domed machine with a helmet at one end.
- **Draft caption:** One long session, with the beams aimed from many sides at once.
- **Getting it:** Free stock or Wikimedia. Keep brand names out of the shot where you can.
- **Lands at:** `wwwroot/img/figures/ph-srs-frame.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/stereotactic-radiosurgery`

#### `ph-theatre` — kind 1, wave 2

- **Shows:** An empty operating room with the lights over the table.
- **Draft alt text:** An empty operating room. Bright lights hang over the table.
- **Draft caption:** The room is full of people and machines. Each one has a job.
- **Getting it:** Free stock or Wikimedia.
- **Lands at:** `wwwroot/img/figures/ph-theatre.svg` (or `.png` / `.jpg`) — **used by:** `/tests/biopsy`, `/treatments/awake-craniotomy`, `/treatments/craniotomy`

#### `ph-two-people-talking` — kind 1, wave 1

- **Shows:** Two people sitting at a table, one talking and one listening.
- **Draft alt text:** Two people at a table. One is talking and one is listening.
- **Draft caption:** The person with you hears things you will not. Let them write it down.
- **Getting it:** Free stock. Everyday clothes, home or cafe, not a clinic. THIS ONE SHOWS ON MORE PAGES THAN ANY OTHER FIGURE HERE — see the used-by line — so pick a warm, plain, un-staged shot and expect to look at a lot of them before one is good enough to repeat that often.
- **Lands at:** `wwwroot/img/figures/ph-two-people-talking.svg` (or `.png` / `.jpg`) — **used by:** **37 pages**

#### `ph-waiting-room` — kind 1, wave 1

- **Shows:** An empty hospital waiting room, chairs in rows, daylight.
- **Draft alt text:** An empty waiting room with chairs in rows and a window.
- **Draft caption:** Most of the wait is not spent here. It is spent at home, and that is the hard part.
- **Getting it:** Free stock.
- **Lands at:** `wwwroot/img/figures/ph-waiting-room.svg` (or `.png` / `.jpg`) — **used by:** `/tests/follow-up-scans`, `/tests/waiting-for-results`

#### `ph-walk-outdoors` — kind 1, wave 2

- **Shows:** A path through a park, with a person walking away from the camera.
- **Draft alt text:** A path through trees, with someone walking away in the distance.
- **Draft caption:** A short walk most days does more for low mood than waiting for it to lift.
- **Getting it:** Free stock.
- **Lands at:** `wwwroot/img/figures/ph-walk-outdoors.svg` (or `.png` / `.jpg`) — **used by:** `/seizures/living-with`, `/treatments/watch-and-wait`

#### `ph-walking-rail` — kind 1, wave 2

- **Shows:** A hand on a rail in a physio room.
- **Draft alt text:** A hand holding a rail in a room with mats and steps.
- **Draft caption:** Weak legs are worked on with rails and short walks, not with rest.
- **Getting it:** Free stock.
- **Lands at:** `wwwroot/img/figures/ph-walking-rail.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/steroids`

### Kind 2 — Mock document (15)

A made-up example with the parts labelled. **Never a real report** — a real one carries PHI, and no stock site has one. Somebody has to build each of these.

#### `doc-addendum` — kind 2, wave 1

- **Shows:** A first report and the later addendum beside it, with the changed line marked.
- **Draft alt text:** Two made-up reports side by side. One line is marked as changed.
- **Draft caption:** A second report is normal. It means a slower test has come back.
- **Getting it:** BUILD. Made-up text only.
- **Lands at:** `wwwroot/img/figures/doc-addendum.svg` (or `.png` / `.jpg`) — **used by:** `/tests/waiting-for-results`

#### `doc-blood-count-sheet` — kind 2, wave 2

- **Shows:** A made-up blood count result with the low line marked and the range beside it.
- **Draft alt text:** A made-up blood result with one low line marked and a range beside it.
- **Draft caption:** The number they watch most is the one that fights infection.
- **Getting it:** BUILD.
- **Lands at:** `wwwroot/img/figures/doc-blood-count-sheet.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/chemotherapy`

#### `doc-consent-first-page` — kind 2, wave 2

- **Shows:** The first page of a made-up trial consent form, with its parts labelled.
- **Draft alt text:** The first page of a made-up consent form, with its parts labeled.
- **Draft caption:** You can take this home and read it with someone. Nobody can rush you.
- **Getting it:** BUILD.
- **Lands at:** `wwwroot/img/figures/doc-consent-first-page.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/clinical-trials`

#### `doc-implant-card` — kind 2, wave 2

- **Shows:** A made-up implant or device card, with the model and the MRI line labelled.
- **Draft alt text:** A made-up wallet card with the device name and an MRI line on it.
- **Draft caption:** Carry the card. The scan team needs the exact name, not a description.
- **Getting it:** BUILD. No real brand or model number.
- **Lands at:** `wwwroot/img/figures/doc-implant-card.svg` (or `.png` / `.jpg`) — **used by:** `/tests/mri`, `/treatments/shunts`

#### `doc-insurance-denial` — kind 2, wave 2

- **Shows:** A made-up denial letter with the reason and the appeal deadline ringed.
- **Draft alt text:** A made-up letter with a reason and a date ringed in pen.
- **Draft caption:** The date is the part to find first. An appeal has a clock on it.
- **Getting it:** BUILD. US insurer wording, no real company name or logo.
- **Lands at:** `wwwroot/img/figures/doc-insurance-denial.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/proton-therapy`

#### `doc-medicines-list` — kind 2, wave 1

- **Shows:** A made-up medicines list, with the vitamins and herbal part filled in too.
- **Draft alt text:** A made-up list of medicines. There is a space for vitamins too.
- **Draft caption:** Put it all on the list. The things you buy count too.
- **Getting it:** BUILD.
- **Lands at:** `wwwroot/img/figures/doc-medicines-list.svg` (or `.png` / `.jpg`) — **used by:** **4 pages**

#### `doc-molecular-panel` — kind 2, wave 1

- **Shows:** A made-up gene results page, each line labelled with what it is for.
- **Draft alt text:** A made-up gene results page with a label on each line.
- **Draft caption:** Gene results come on a page like this, often weeks after the first report.
- **Getting it:** BUILD. Made-up gene names and values only — never a real patient's panel.
- **Lands at:** `wwwroot/img/figures/doc-molecular-panel.svg` (or `.png` / `.jpg`) — **used by:** `/tests/molecular-markers`, `/treatments/targeted-therapy`

#### `doc-neuropsych-summary` — kind 2, wave 3

- **Shows:** A made-up memory-test summary, with the scores and the advice part labelled.
- **Draft alt text:** A made-up test summary. The scores and the advice are marked.
- **Draft caption:** The scores are not the point. What they tell you to change is the point.
- **Getting it:** BUILD.
- **Lands at:** `wwwroot/img/figures/doc-neuropsych-summary.svg` (or `.png` / `.jpg`) — **used by:** `/tests/neuro-exam-and-memory-testing`

#### `doc-pathology-parts` — kind 2, wave 1

- **Shows:** A made-up pathology report with its nine parts called out, one label each.
- **Draft alt text:** A made-up lab report with nine labels pointing at its parts.
- **Draft caption:** Your own report has these same parts, in roughly this order.
- **Getting it:** BUILD. Never a real report: a real one carries PHI, and no stock site has one. THE SINGLE HIGHEST-VALUE PICTURE ON THE SITE — it is this page's whole subject.
- **Lands at:** `wwwroot/img/figures/doc-pathology-parts.svg` (or `.png` / `.jpg`) — **used by:** **23 pages**

#### `doc-preop-sheet` — kind 2, wave 1

- **Shows:** A made-up pre-op sheet with two columns: medicines to keep, medicines to stop.
- **Draft alt text:** A made-up sheet with two columns, one for keep and one for stop.
- **Draft caption:** Ask for yours in writing. The two lists are not the same for everyone.
- **Getting it:** BUILD.
- **Lands at:** `wwwroot/img/figures/doc-preop-sheet.svg` (or `.png` / `.jpg`) — **used by:** **5 pages**

#### `doc-questions-sheet` — kind 2, wave 2

- **Shows:** A printed question list with blank lines to write the answers on.
- **Draft alt text:** A printed sheet of questions with blank lines under each one.
- **Draft caption:** Take the page in with you. The answers are worth more than the questions.
- **Getting it:** BUILD. This is also the printable we already ask readers to take to appointments.
- **Lands at:** `wwwroot/img/figures/doc-questions-sheet.svg` (or `.png` / `.jpg`) — **used by:** **8 pages**

#### `doc-radiation-schedule` — kind 2, wave 2

- **Shows:** A made-up treatment schedule: the same time each weekday for six weeks.
- **Draft alt text:** A made-up schedule sheet with the same time each weekday for weeks.
- **Draft caption:** You get the whole run of dates at the start. Most are the same slot each day.
- **Getting it:** BUILD.
- **Lands at:** `wwwroot/img/figures/doc-radiation-schedule.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/radiation-therapy`

#### `doc-radiology-report` — kind 2, wave 2

- **Shows:** A made-up MRI report, with its sections labelled and the measurement line marked.
- **Draft alt text:** A made-up scan report with its sections labeled down the side.
- **Draft caption:** A scan report has a shape too. The last part is the part your team acts on.
- **Getting it:** BUILD.
- **Lands at:** `wwwroot/img/figures/doc-radiology-report.svg` (or `.png` / `.jpg`) — **used by:** `/tests/follow-up-scans`

#### `doc-seizure-diary` — kind 2, wave 1

- **Shows:** A filled-in seizure diary page: date, time, what happened, how long.
- **Draft alt text:** A diary page filled in by hand, with a date and time on each line.
- **Draft caption:** This is the thing that changes a dose. A doctor cannot see what you do not write.
- **Getting it:** BUILD. Handwriting is fine and reads warmer than a form.
- **Lands at:** `wwwroot/img/figures/doc-seizure-diary.svg` (or `.png` / `.jpg`) — **used by:** `/seizures/living-with`, `/seizures/what-to-do`

#### `doc-steroid-taper` — kind 2, wave 1

- **Shows:** A made-up tapering card: the dose going down a step at a time, by date.
- **Draft alt text:** A made-up card showing a dose going down step by step, with dates.
- **Draft caption:** A taper is written down for you. Coming off faster than this can make you ill.
- **Getting it:** BUILD.
- **Lands at:** `wwwroot/img/figures/doc-steroid-taper.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/steroids`

### Kind 3 — Simple diagram (64)

A line drawing we make. The highest explanatory value on the site, and a design job rather than a search.

The 13 `dia-region-*` entries are **one drawing**: `dia-brain-regions` is the master
and each variant is the same file with one area shaded. That is WI-572's diagram, and
it is the single most load-bearing drawing on the site.

**SHIPPED (WI-572).** The master, the 13 variants and `dia-region-names` are committed
and live on 33 slots. **Four placements carry a caption of their own** rather than the
draft below, because the page's own text gives more than one place and a picture under
"where does it grow" is an answer to that question: `/tumors/atrt` ("About half of
these tumors start in the shaded part, low at the back."), `/tumors/chordoma`,
`/tumors/ependymoma` and `/tumors/cns-germ-cell-tumor`. §5 licenses that, the alt text
is pinned to the draft and the caption is not, and the four are named here so the next
paste does not flatten them back. They are DERIVED from the master rather than drawn one at a time,
and a test rebuilds every one of them — so a variant is never edited on its own. A
variant labels exactly **one** region, because the page showing it names that region
and not the other eight: a label is a claim the text has to make too (§12.35).

#### `dia-awake-mapping` — kind 3, wave 2

- **Shows:** The mapping step: the surgeon tests a spot, you speak, the map gets marked.
- **Draft alt text:** A brain surface marked with small numbered tags.
- **Draft caption:** They test each spot before they take anything. Your talking is the test.
- **Getting it:** DRAW.
- **Lands at:** `wwwroot/img/figures/dia-awake-mapping.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/awake-craniotomy`

#### `dia-bbb` — kind 3, wave 2

- **Shows:** A wall of cells with most drugs blocked and a few getting through.
- **Draft alt text:** A wall of cells with most drugs stopped and a few slipping past.
- **Draft caption:** The brain has a wall around it. Most drugs cannot get through it.
- **Getting it:** DRAW.
- **Lands at:** `wwwroot/img/figures/dia-bbb.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/chemotherapy`

#### `dia-biopsy-route` — kind 3, wave 2

- **Shows:** A needle path to a deep spot, through a small hole, with the frame holding it.
- **Draft alt text:** A needle on a straight path to a deep spot inside the brain.
- **Draft caption:** A needle goes in through a small hole. It does not take the tumor out.
- **Getting it:** DRAW.
- **Lands at:** `wwwroot/img/figures/dia-biopsy-route.svg` (or `.png` / `.jpg`) — **used by:** `/tests/biopsy`, `/tumors/all-brain-tumors`

#### `dia-brain-regions` — kind 3, wave 1

- **Shows:** The master brain map: a side view with every region this site names labelled in plain words.
- **Draft alt text:** A side view of the brain with each part labeled in plain words.
- **Draft caption:** The parts of the brain, in the words this site uses.
- **Getting it:** DRAW. This is WI-572's diagram and the master file every dia-region-* variant comes from. DO NOT take an existing brain diagram from cancer.gov: NCI embedded images are BANNED (PLAN.md §5) and the build now fails on the credit as well as the URL.
- **Lands at:** `wwwroot/img/figures/dia-brain-regions.svg` (or `.png` / `.jpg`) — **used by:** **8 pages**

#### `dia-cause-known-unknown` — kind 3, wave 2

- **Shows:** Two columns: the few things known to raise the risk, and the long list that does not.
- **Draft alt text:** Two columns. A short list on the left and a much longer one on the right.
- **Draft caption:** The short list is what is known. Almost everything people blame is on the right.
- **Getting it:** DRAW. Carries the shared causes block. Keep it factual — this is the self-blame page, so no reassuring language that the text does not already carry.
- **Lands at:** `wwwroot/img/figures/dia-cause-known-unknown.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/all-brain-tumors`

#### `dia-chemo-cycle` — kind 3, wave 1

- **Shows:** A cycle: days on, days off, repeat — drawn as a strip, not a wheel.
- **Draft alt text:** A strip of days, with the treatment days marked and the rest blank.
- **Draft caption:** A cycle is the days on plus the days off. The days off are part of it.
- **Getting it:** DRAW. A strip rather than a circle: a wheel reads as endless.
- **Lands at:** `wwwroot/img/figures/dia-chemo-cycle.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/chemotherapy`

#### `dia-craniotomy-steps` — kind 3, wave 1

- **Shows:** Open, remove, close — three boxes, with the bone put back in the third.
- **Draft alt text:** Three boxes in a row: open the bone, take the tumor, put the bone back.
- **Draft caption:** The bone comes out and goes back in the same operation.
- **Getting it:** DRAW.
- **Lands at:** `wwwroot/img/figures/dia-craniotomy-steps.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/craniotomy`

#### `dia-crosswalk-names` — kind 3, wave 2

- **Shows:** Old tumor names on the left, the name used now on the right, arrows between.
- **Draft alt text:** Old tumor names on the left, the name used now on the right.
- **Draft caption:** Names changed in 2021. An older letter may use the one on the left.
- **Getting it:** DRAW. Carries the shared crosswalk block, which is the content most likely to need correcting later — so the picture has to be as easy to edit as the text.
- **Lands at:** `wwwroot/img/figures/dia-crosswalk-names.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/chordoma`, `/tumors/meningioma`

#### `dia-csf-shunt` — kind 3, wave 1

- **Shows:** Where the fluid is made, where it is stuck, and where a shunt sends it.
- **Draft alt text:** A tube running from inside the head, down the neck, to the belly.
- **Draft caption:** A shunt moves the fluid somewhere your body can take it away.
- **Getting it:** DRAW.
- **Lands at:** `wwwroot/img/figures/dia-csf-shunt.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/shunts`

#### `dia-csf-spread` — kind 3, wave 2

- **Shows:** The fluid path from the brain down the spine, and how cells can travel in it.
- **Draft alt text:** The fluid path running from the brain down the spine.
- **Draft caption:** The fluid runs all the way down. That is why the spine is scanned too.
- **Getting it:** DRAW. Carries the shared spinal-cord block.
- **Lands at:** `wwwroot/img/figures/dia-csf-spread.svg` (or `.png` / `.jpg`) — **used by:** **5 pages**

#### `dia-endoscopic-route` — kind 3, wave 3

- **Shows:** The route through the nose to the base of the brain, with no cut on the head.
- **Draft alt text:** A route through the nose to the base of the brain.
- **Draft caption:** Some tumors are reached through the nose, with no cut on your head.
- **Getting it:** DRAW.
- **Lands at:** `wwwroot/img/figures/dia-endoscopic-route.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/craniotomy`

#### `dia-escalation-card` — kind 3, wave 1

- **Shows:** Three columns: call an ambulance now, call your team today, mention at the next visit.
- **Draft alt text:** Three columns, from call an ambulance now to mention it next visit.
- **Draft caption:** Keep this where someone else can find it, not only you.
- **Getting it:** DRAW. Carries the shared escalation block. The wording must match the block exactly — a picture that disagrees with the text beside it is worse than no picture.
- **Lands at:** `wwwroot/img/figures/dia-escalation-card.svg` (or `.png` / `.jpg`) — **used by:** **30 pages**

#### `dia-extent-of-resection` — kind 3, wave 2

- **Shows:** The same tumor three times: all out, most out, some out — with the words used for each.
- **Draft alt text:** The same tumor three times, with less of it left each time.
- **Draft caption:** These three words mean three different amounts. Ask which one yours was.
- **Getting it:** DRAW.
- **Lands at:** `wwwroot/img/figures/dia-extent-of-resection.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/craniotomy`

#### `dia-fmri-task` — kind 3, wave 3

- **Shows:** In the scanner: a screen, a button, and the task you are given.
- **Draft alt text:** A screen and a hand-held button, with a simple word task on the screen.
- **Draft caption:** You do a small job while they scan, so they can see which part lights up.
- **Getting it:** DRAW.
- **Lands at:** `wwwroot/img/figures/dia-fmri-task.svg` (or `.png` / `.jpg`) — **used by:** `/tests/planning-scans`

#### `dia-followup-timeline` — kind 3, wave 1

- **Shows:** A line of scan dates: close together at first, further apart later.
- **Draft alt text:** A line of scan dates. They are close at first and wider apart later.
- **Draft caption:** Scans start close together and spread out. Spreading out is good news.
- **Getting it:** DRAW.
- **Lands at:** `wwwroot/img/figures/dia-followup-timeline.svg` (or `.png` / `.jpg`) — **used by:** **24 pages**

#### `dia-fractions-calendar` — kind 3, wave 1

- **Shows:** Six weeks of weekdays, each marked, with the weekends blank.
- **Draft alt text:** Six weeks of boxes, with the weekdays marked and weekends blank.
- **Draft caption:** Small doses every weekday do less harm than a few big ones.
- **Getting it:** DRAW.
- **Lands at:** `wwwroot/img/figures/dia-fractions-calendar.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/radiation-therapy`

#### `dia-grade-ladder` — kind 3, wave 1

- **Shows:** Grade 1 to grade 4 as four steps, with no gaps and no colour coding of hope.
- **Draft alt text:** Four steps in a row, marked grade 1 up to grade 4.
- **Draft caption:** The grade says how fast it is likely to grow. It is not a stage.
- **Getting it:** DRAW. No red/green. A colour scale here reads as a verdict.
- **Lands at:** `wwwroot/img/figures/dia-grade-ladder.svg` (or `.png` / `.jpg`) — **used by:** **22 pages**

#### `dia-looks-worse-two-causes` — kind 3, wave 2

- **Shows:** A brighter area with two possible causes: growth, or the treatment working.
- **Draft alt text:** A brighter area on a scan, with two arrows: growth, or treatment.
- **Draft caption:** A scan looking worse has two possible causes, and one of them is treatment.
- **Getting it:** DRAW.
- **Lands at:** `wwwroot/img/figures/dia-looks-worse-two-causes.svg` (or `.png` / `.jpg`) — **used by:** `/tests/follow-up-scans`, `/treatments/watch-and-wait`

#### `dia-marker-meaning` — kind 3, wave 1

- **Shows:** Each marker on the left, and what it changes about treatment on the right.
- **Draft alt text:** Markers listed on the left, and what each one changes on the right.
- **Draft caption:** A marker is only worth knowing because of what it changes.
- **Getting it:** DRAW.
- **Lands at:** `wwwroot/img/figures/dia-marker-meaning.svg` (or `.png` / `.jpg`) — **used by:** `/tests/molecular-markers`, `/treatments/chemotherapy`, `/treatments/targeted-therapy`

#### `dia-mechanism-pressure` — kind 3, wave 1

- **Shows:** A growing lump inside a closed box, and what has to give way.
- **Draft alt text:** A lump growing inside a closed box, pressing on what is around it.
- **Draft caption:** The skull cannot stretch. That is why a small lump can cause big symptoms.
- **Getting it:** DRAW. This one carries the shared mechanism block, which is 864 words on 19 pages.
- **Lands at:** `wwwroot/img/figures/dia-mechanism-pressure.svg` (or `.png` / `.jpg`) — **used by:** **18 pages**

#### `dia-placebo-add-on` — kind 3, wave 2

- **Shows:** Standard care in both arms, with the new drug added to one of them.
- **Draft alt text:** Two arms side by side. Both get standard care, one also gets the new drug.
- **Draft caption:** Nobody is left with nothing. The new drug is added on top.
- **Getting it:** DRAW. The single most reassuring fact on the page, and the one people do not believe.
- **Lands at:** `wwwroot/img/figures/dia-placebo-add-on.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/clinical-trials`

#### `dia-posterior-fossa` — kind 3, wave 3

- **Shows:** The back and bottom of the brain, and what can stop working after surgery there.
- **Draft alt text:** The back and bottom of the brain, with the speech and balance parts marked.
- **Draft caption:** Surgery at the back can stop speech for weeks. It comes back.
- **Getting it:** DRAW. Carries the shared posterior-fossa-syndrome block.
- **Lands at:** `wwwroot/img/figures/dia-posterior-fossa.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/atrt`, `/tumors/ependymoma`, `/tumors/pediatric-brain-tumor`

#### `dia-proton-stop` — kind 3, wave 2

- **Shows:** A photon beam carrying on through, and a proton beam stopping at the target.
- **Draft alt text:** Two beams into a head. One carries on through and one stops inside.
- **Draft caption:** A proton beam stops. That is the whole difference, and it matters most in children.
- **Getting it:** DRAW.
- **Lands at:** `wwwroot/img/figures/dia-proton-stop.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/proton-therapy`

#### `dia-radiation-margin` — kind 3, wave 2

- **Shows:** The tumor, the margin around it, and the healthy brain outside both.
- **Draft alt text:** A lump with a ring drawn around it, and healthy brain outside the ring.
- **Draft caption:** The beam covers a little more than the tumor. That margin is deliberate.
- **Getting it:** DRAW.
- **Lands at:** `wwwroot/img/figures/dia-radiation-margin.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/radiation-therapy`

#### `dia-recurrence-options` — kind 3, wave 2

- **Shows:** If it comes back: the four doors, as boxes, with trials as one of them.
- **Draft alt text:** Four boxes, one for each choice if the tumor comes back.
- **Draft caption:** There is usually more than one door, and a trial is one of them.
- **Getting it:** DRAW.
- **Lands at:** `wwwroot/img/figures/dia-recurrence-options.svg` (or `.png` / `.jpg`) — **used by:** **17 pages**

#### `dia-region-brainstem` — kind 3, wave 1

- **Shows:** The master map with the stalk the brain sits on shaded.
- **Draft alt text:** The brain map with the stalk at the base shaded.
- **Draft caption:** The shaded part is the stalk the brain sits on.
- **Getting it:** DRAW. A shaded variant of dia-brain-regions, from the same master file — not a new drawing. NEVER an NCI brain diagram (PLAN.md §5): the build fails on the credit as well as the URL.
- **Lands at:** `wwwroot/img/figures/dia-region-brainstem.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/diffuse-midline-glioma`, `/tumors/dipg`, `/where-your-tumor-is`

#### `dia-region-cerebellum` — kind 3, wave 1

- **Shows:** The master map with low at the back, under everything else shaded.
- **Draft alt text:** The brain map with the low back part shaded.
- **Draft caption:** The shaded part is low at the back, under everything else.
- **Getting it:** DRAW. A shaded variant of dia-brain-regions, from the same master file — not a new drawing. NEVER an NCI brain diagram (PLAN.md §5): the build fails on the credit as well as the URL.
- **Lands at:** `wwwroot/img/figures/dia-region-cerebellum.svg` (or `.png` / `.jpg`) — **used by:** **4 pages**

#### `dia-region-frontal` — kind 3, wave 1

- **Shows:** The master map with the front of the brain shaded.
- **Draft alt text:** The brain map with the front shaded.
- **Draft caption:** The shaded part is the front of the brain.
- **Getting it:** DRAW. A shaded variant of dia-brain-regions, from the same master file — not a new drawing. NEVER an NCI brain diagram (PLAN.md §5): the build fails on the credit as well as the URL.
- **Lands at:** `wwwroot/img/figures/dia-region-frontal.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/oligodendroglioma`, `/where-your-tumor-is`

#### `dia-region-hearing-nerve` — kind 3, wave 2

- **Shows:** The master map with the nerve behind the ear shaded.
- **Draft alt text:** The brain map with the nerve behind the ear shaded.
- **Draft caption:** The shaded part is the nerve behind the ear.
- **Getting it:** DRAW. A shaded variant of dia-brain-regions, from the same master file — not a new drawing. NEVER an NCI brain diagram (PLAN.md §5): the build fails on the credit as well as the URL.
- **Lands at:** `wwwroot/img/figures/dia-region-hearing-nerve.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/acoustic-neuroma`

#### `dia-region-meninges` — kind 3, wave 2

- **Shows:** The master map with the coverings over the brain shaded.
- **Draft alt text:** The brain map with the thin covering layers shaded.
- **Draft caption:** The shaded part is the coverings over the brain.
- **Getting it:** DRAW. A shaded variant of dia-brain-regions, from the same master file — not a new drawing. NEVER an NCI brain diagram (PLAN.md §5): the build fails on the credit as well as the URL.
- **Lands at:** `wwwroot/img/figures/dia-region-meninges.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/meningioma`

#### `dia-region-names` — kind 3, wave 2

- **Shows:** The master map again, labelled with the report's words instead of the plain ones.
- **Draft alt text:** The same brain map, labeled with the words a report uses.
- **Draft caption:** The same places, in the words your report is written in.
- **Getting it:** DRAW. A second labelling of dia-brain-regions, not a second drawing.
- **Lands at:** `wwwroot/img/figures/dia-region-names.svg` (or `.png` / `.jpg`) — **used by:** `/where-your-tumor-is`

#### `dia-region-occipital` — kind 3, wave 2

- **Shows:** The master map with the back of the brain shaded.
- **Draft alt text:** The brain map with the back shaded.
- **Draft caption:** The shaded part is the back of the brain.
- **Getting it:** DRAW. A shaded variant of dia-brain-regions, from the same master file — not a new drawing. NEVER an NCI brain diagram (PLAN.md §5): the build fails on the credit as well as the URL.
- **Lands at:** `wwwroot/img/figures/dia-region-occipital.svg` (or `.png` / `.jpg`) — **used by:** `/where-your-tumor-is`

#### `dia-region-parietal` — kind 3, wave 2

- **Shows:** The master map with the upper back part of the brain shaded.
- **Draft alt text:** The brain map with the upper back part shaded.
- **Draft caption:** The shaded part is the upper back part of the brain.
- **Getting it:** DRAW. A shaded variant of dia-brain-regions, from the same master file — not a new drawing. NEVER an NCI brain diagram (PLAN.md §5): the build fails on the credit as well as the URL.
- **Lands at:** `wwwroot/img/figures/dia-region-parietal.svg` (or `.png` / `.jpg`) — **used by:** `/where-your-tumor-is`

#### `dia-region-pineal` — kind 3, wave 2

- **Shows:** The master map with the small gland deep in the middle shaded.
- **Draft alt text:** The brain map with the small gland deep in the middle shaded.
- **Draft caption:** The shaded part is the small gland deep in the middle.
- **Getting it:** DRAW. A shaded variant of dia-brain-regions, from the same master file — not a new drawing. NEVER an NCI brain diagram (PLAN.md §5): the build fails on the credit as well as the URL.
- **Lands at:** `wwwroot/img/figures/dia-region-pineal.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/cns-germ-cell-tumor`

#### `dia-region-sellar` — kind 3, wave 1

- **Shows:** The master map with behind the nose, at the base of the brain shaded.
- **Draft alt text:** The brain map with the small gland behind the nose shaded.
- **Draft caption:** The shaded part is behind the nose, at the base of the brain.
- **Getting it:** DRAW. A shaded variant of dia-brain-regions, from the same master file — not a new drawing. NEVER an NCI brain diagram (PLAN.md §5): the build fails on the credit as well as the URL.
- **Lands at:** `wwwroot/img/figures/dia-region-sellar.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/craniopharyngioma`, `/tumors/pituitary-tumor`, `/where-your-tumor-is`

#### `dia-region-skull-base` — kind 3, wave 2

- **Shows:** The master map with the floor of the skull shaded.
- **Draft alt text:** The brain map with the floor of the skull shaded.
- **Draft caption:** The shaded part is the floor of the skull.
- **Getting it:** DRAW. A shaded variant of dia-brain-regions, from the same master file — not a new drawing. NEVER an NCI brain diagram (PLAN.md §5): the build fails on the credit as well as the URL.
- **Lands at:** `wwwroot/img/figures/dia-region-skull-base.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/chordoma`, `/where-your-tumor-is`

#### `dia-region-spinal-cord` — kind 3, wave 2

- **Shows:** The master map with the cord running down the back shaded.
- **Draft alt text:** The brain map with the cord running down the back shaded.
- **Draft caption:** The shaded part is the cord running down the back.
- **Getting it:** DRAW. A shaded variant of dia-brain-regions, from the same master file — not a new drawing. NEVER an NCI brain diagram (PLAN.md §5): the build fails on the credit as well as the URL.
- **Lands at:** `wwwroot/img/figures/dia-region-spinal-cord.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/spinal-cord-tumor`

#### `dia-region-temporal` — kind 3, wave 2

- **Shows:** The master map with the side of the brain, near the ear shaded.
- **Draft alt text:** The brain map with the side, near the ear, shaded.
- **Draft caption:** The shaded part is the side of the brain, near the ear.
- **Getting it:** DRAW. A shaded variant of dia-brain-regions, from the same master file — not a new drawing. NEVER an NCI brain diagram (PLAN.md §5): the build fails on the credit as well as the URL.
- **Lands at:** `wwwroot/img/figures/dia-region-temporal.svg` (or `.png` / `.jpg`) — **used by:** `/where-your-tumor-is`

#### `dia-region-ventricles` — kind 3, wave 2

- **Shows:** The master map with deep in the middle, where the fluid runs shaded.
- **Draft alt text:** The brain map with the deep fluid spaces shaded.
- **Draft caption:** The shaded part is deep in the middle, where the fluid runs.
- **Getting it:** DRAW. A shaded variant of dia-brain-regions, from the same master file — not a new drawing. NEVER an NCI brain diagram (PLAN.md §5): the build fails on the credit as well as the URL.
- **Lands at:** `wwwroot/img/figures/dia-region-ventricles.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/cns-lymphoma`, `/tumors/ependymoma`, `/where-your-tumor-is`

#### `dia-report-layers` — kind 3, wave 1

- **Shows:** The report as stacked bands: the answer on top, the evidence underneath.
- **Draft alt text:** Stacked bands, with the answer on top and the evidence under it.
- **Draft caption:** The answer sits at the top. Everything under it shows how they got there.
- **Getting it:** DRAW.
- **Lands at:** `wwwroot/img/figures/dia-report-layers.svg` (or `.png` / `.jpg`) — **used by:** `/tests/pathology-report`

#### `dia-scan-side-by-side` — kind 3, wave 1

- **Shows:** The same slice of the same brain on two dates, with the measuring line on both.
- **Draft alt text:** The same slice of a brain on two dates, measured the same way both times.
- **Draft caption:** They compare the same slice, measured the same way. That is what change means here.
- **Getting it:** DRAW. Must be a drawing, not two real scans: two real scans of two people is a false claim.
- **Lands at:** `wwwroot/img/figures/dia-scan-side-by-side.svg` (or `.png` / `.jpg`) — **used by:** **4 pages**

#### `dia-scan-types` — kind 3, wave 2

- **Shows:** Four extra scans, each with the one thing it adds.
- **Draft alt text:** Four scan names, each with the one thing it adds beside it.
- **Draft caption:** Each extra scan answers one question the normal one cannot.
- **Getting it:** DRAW.
- **Lands at:** `wwwroot/img/figures/dia-scan-types.svg` (or `.png` / `.jpg`) — **used by:** `/tests/planning-scans`

#### `dia-seizure-do-not` — kind 3, wave 1

- **Shows:** The three things not to do, each crossed out.
- **Draft alt text:** Three things crossed out: holding them down, the mouth, and food or drink.
- **Draft caption:** These three do harm. Nothing goes in the mouth, ever.
- **Getting it:** DRAW.
- **Lands at:** `wwwroot/img/figures/dia-seizure-do-not.svg` (or `.png` / `.jpg`) — **used by:** `/seizures/what-to-do`

#### `dia-seizure-steps` — kind 3, wave 1

- **Shows:** What to do, in order, as four boxes: time it, cushion the head, turn them, stay.
- **Draft alt text:** Four boxes in order: time it, cushion the head, turn them, stay.
- **Draft caption:** Do these four things, in this order. Most seizures stop on their own.
- **Getting it:** DRAW. Must say the same thing as the text beside it, word for word.
- **Lands at:** `wwwroot/img/figures/dia-seizure-steps.svg` (or `.png` / `.jpg`) — **used by:** `/seizures/what-to-do`

#### `dia-seizure-threshold` — kind 3, wave 2

- **Shows:** A spark spreading across the brain, and the medicine raising the bar it has to clear.
- **Draft alt text:** A spark spreading across the brain, with a bar in its way.
- **Draft caption:** The medicine makes a seizure harder to start. It does not touch the tumor.
- **Getting it:** DRAW.
- **Lands at:** `wwwroot/img/figures/dia-seizure-threshold.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/anti-seizure-medicines`

#### `dia-side-effect-timeline` — kind 3, wave 1

- **Shows:** Three bands: during treatment, the weeks after, and much later.
- **Draft alt text:** Three bands on one line: during, the weeks after, and much later.
- **Draft caption:** Some effects come at once, some weeks later, and a few much later.
- **Getting it:** DRAW.
- **Lands at:** `wwwroot/img/figures/dia-side-effect-timeline.svg` (or `.png` / `.jpg`) — **used by:** **5 pages**

#### `dia-srs-vs` — kind 3, wave 2

- **Shows:** Many weak beams from many sides, meeting at one point.
- **Draft alt text:** Many thin beams from all sides, meeting at one small point.
- **Draft caption:** Each beam is weak. Only where they cross is the dose high.
- **Getting it:** DRAW.
- **Lands at:** `wwwroot/img/figures/dia-srs-vs.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/stereotactic-radiosurgery`

#### `dia-steroid-swelling` — kind 3, wave 1

- **Shows:** The same tumor twice: swelling around it, then less swelling.
- **Draft alt text:** The same tumor twice, with less swelling around it the second time.
- **Draft caption:** The steroid works on the swelling, not on the tumor.
- **Getting it:** DRAW. The most-misunderstood point on the page, so the picture has to carry it alone.
- **Lands at:** `wwwroot/img/figures/dia-steroid-swelling.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/steroids`

#### `dia-symptom-map` — kind 3, wave 1

- **Shows:** Which part of the brain, next to which symptom, as a two-column map.
- **Draft alt text:** A brain map on the left, and the symptom each part causes on the right.
- **Draft caption:** Your symptoms point at a place. That is how they are read.
- **Getting it:** DRAW. Built from dia-brain-regions so the two agree.
- **Lands at:** `wwwroot/img/figures/dia-symptom-map.svg` (or `.png` / `.jpg`) — **used by:** **22 pages**

#### `dia-target-lock` — kind 3, wave 2

- **Shows:** One drug fitting one change, and the same drug not fitting another.
- **Draft alt text:** A drug shape fitting one slot, and not fitting the one beside it.
- **Draft caption:** A targeted drug only works if your tumor has the change it is built for.
- **Getting it:** DRAW.
- **Lands at:** `wwwroot/img/figures/dia-target-lock.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/targeted-therapy`

#### `dia-tissue-to-slide` — kind 3, wave 1

- **Shows:** Tissue, fixative, wax block, slice, slide, microscope — five boxes joined by arrows.
- **Draft alt text:** Five boxes joined by arrows, from a piece of tissue to a glass slide.
- **Draft caption:** This is why it takes days. Each step has to finish before the next one starts.
- **Getting it:** DRAW. This is WI-507's nine numbered paragraphs as one picture.
- **Lands at:** `wwwroot/img/figures/dia-tissue-to-slide.svg` (or `.png` / `.jpg`) — **used by:** `/tests/waiting-for-results`, `/tumors/all-brain-tumors`

#### `dia-treatment-path` — kind 3, wave 1

- **Shows:** Surgery, radiation and medicine as three boxes, with the branches that skip one.
- **Draft alt text:** Three boxes joined by arrows, with side paths that skip a box.
- **Draft caption:** Most plans use some of these, in some order. Very few use all three.
- **Getting it:** DRAW. Show the skip paths. A straight line reads as a schedule everybody is on.
- **Lands at:** `wwwroot/img/figures/dia-treatment-path.svg` (or `.png` / `.jpg`) — **used by:** **24 pages**

#### `dia-trial-phases` — kind 3, wave 2

- **Shows:** Phase 1, 2 and 3 as three steps, with what each one is asking.
- **Draft alt text:** Three steps, each with the question that phase is asking.
- **Draft caption:** Each phase asks a different question. Only the last one compares treatments.
- **Getting it:** DRAW.
- **Lands at:** `wwwroot/img/figures/dia-trial-phases.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/clinical-trials`

#### `dia-ttf-arrays` — kind 3, wave 2

- **Shows:** The pads on a shaved scalp, the leads, and the battery in its bag.
- **Draft alt text:** Pads on a shaved head, with leads running to a battery in a bag.
- **Draft caption:** Pads on the scalp, wires to a battery, worn most of the day.
- **Getting it:** DRAW. A drawing, not a product photo: the device has one maker and its photos are theirs.
- **Lands at:** `wwwroot/img/figures/dia-ttf-arrays.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/tumor-treating-fields`

#### `dia-tumor-board` — kind 3, wave 2

- **Shows:** A table with labelled seats: who is in the meeting about your case.
- **Draft alt text:** A table with labeled seats, one for each kind of doctor.
- **Draft caption:** A room of people reads your case together. You are not in the room.
- **Getting it:** DRAW. Carries the shared tumor-board block.
- **Lands at:** `wwwroot/img/figures/dia-tumor-board.svg` (or `.png` / `.jpg`) — **used by:** **5 pages**

#### `dia-tumor-or-medicine` — kind 3, wave 1

- **Shows:** Two columns of the same symptom list: what the tumor does, what the medicine does.
- **Draft alt text:** The same symptoms in two columns, one for the tumor and one for the medicine.
- **Draft caption:** The same symptom can come from either. Which one it is changes what you do.
- **Getting it:** DRAW. Reused on four pages that each ask this question in their own words.
- **Lands at:** `wwwroot/img/figures/dia-tumor-or-medicine.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/anti-seizure-medicines`, `/treatments/craniotomy`, `/treatments/radiation-therapy`

#### `dia-tumor-vs-inherited` — kind 3, wave 2

- **Shows:** Two bodies: a change only in the tumor, and a change in every cell.
- **Draft alt text:** Two outlines of a person. One has a marked lump, the other is marked all over.
- **Draft caption:** Nearly always the change is only in the tumor, so it is not passed on.
- **Getting it:** DRAW.
- **Lands at:** `wwwroot/img/figures/dia-tumor-vs-inherited.svg` (or `.png` / `.jpg`) — **used by:** `/tests/molecular-markers`

#### `dia-two-tests-compared` — kind 3, wave 2

- **Shows:** Two columns: what the neuro exam looks at, and what memory testing looks at.
- **Draft alt text:** Two columns, one for each test, with what each one looks at.
- **Draft caption:** Two tests, not one. They look at different things.
- **Getting it:** DRAW.
- **Lands at:** `wwwroot/img/figures/dia-two-tests-compared.svg` (or `.png` / `.jpg`) — **used by:** `/tests/neuro-exam-and-memory-testing`

#### `dia-visual-field` — kind 3, wave 1

- **Shows:** The same street scene twice: whole, and with one half missing.
- **Draft alt text:** A street scene twice. The second one is missing one half.
- **Draft caption:** This is what losing half your field looks like. It is why driving has to stop.
- **Getting it:** DRAW. WI-573's finding as a picture, and the most concrete thing on the page.
- **Lands at:** `wwwroot/img/figures/dia-visual-field.svg` (or `.png` / `.jpg`) — **used by:** `/where-your-tumor-is`

#### `dia-wafer-in-cavity` — kind 3, wave 3

- **Shows:** Thin wafers left in the space where the tumor was, dissolving in place.
- **Draft alt text:** Thin discs left in the space where a tumor was taken out.
- **Draft caption:** They go in during the operation. They melt where they sit.
- **Getting it:** DRAW.
- **Lands at:** `wwwroot/img/figures/dia-wafer-in-cavity.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/chemotherapy`

#### `dia-wait-timeline` — kind 3, wave 1

- **Shows:** One line, three arrivals: the quick answer, the microscope answer, the gene results.
- **Draft alt text:** One line with three points on it, days apart, then weeks apart.
- **Draft caption:** Three answers arrive at three different times. The last one can change the name.
- **Getting it:** DRAW.
- **Lands at:** `wwwroot/img/figures/dia-wait-timeline.svg` (or `.png` / `.jpg`) — **used by:** `/tests/molecular-markers`, `/tests/waiting-for-results`, `/tumors/all-brain-tumors`

#### `dia-watch-cadence` — kind 3, wave 2

- **Shows:** Scan, wait, scan, wait — and what would change the plan.
- **Draft alt text:** Scans on a line with gaps between, and a branch where the plan changes.
- **Draft caption:** Watching is a plan. The branch is what they are watching for.
- **Getting it:** DRAW.
- **Lands at:** `wwwroot/img/figures/dia-watch-cadence.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/watch-and-wait`

#### `dia-wbrt-field` — kind 3, wave 3

- **Shows:** One small target shaded, next to the whole head shaded, side by side.
- **Draft alt text:** Two heads side by side. One has a small shaded spot, one is shaded all over.
- **Draft caption:** This way treats all of it. The spots nobody can see count too.
- **Getting it:** DRAW.
- **Lands at:** `wwwroot/img/figures/dia-wbrt-field.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/radiation-therapy`

#### `dia-your-team` — kind 3, wave 2

- **Shows:** Labelled circles: who is on your team and what each one is for.
- **Draft alt text:** Labeled circles, one for each person on your team.
- **Draft caption:** Many people, each with one job. The one to ask first is your nurse.
- **Getting it:** DRAW.
- **Lands at:** `wwwroot/img/figures/dia-your-team.svg` (or `.png` / `.jpg`) — **used by:** `/tests/getting-ready-for-surgery`, `/treatments/clinical-trials`

### Kind 4 — Public-domain medical imagery (32)

Real scans and slides from Wikimedia Commons or NIH open sets. **Each licence checked individually**, and **never an NCI embedded image** (PLAN.md §5).

Every one of these needs its licence read individually, and the 23 `pd-scan-*`
members are one per hub by design (§1 above).

#### `pd-angiogram` — kind 4, wave 3

- **Shows:** The blood vessels of the brain, shown as a branching tree.
- **Draft alt text:** A branching tree of blood vessels, white on black.
- **Draft caption:** This one maps the blood supply, so nothing important is cut.
- **Getting it:** Wikimedia Commons or an NIH open set. Licence checked per picture. Never NCI.
- **Lands at:** `wwwroot/img/figures/pd-angiogram.svg` (or `.png` / `.jpg`) — **used by:** `/tests/planning-scans`

#### `pd-ct-head` — kind 4, wave 2

- **Shows:** A plain CT of the head.
- **Draft alt text:** A gray cross-section of a head, taken by a CT scanner.
- **Draft caption:** A CT is quick and good at bleeding and bone. It shows less detail than an MRI.
- **Getting it:** Wikimedia Commons or an NIH open set. CHECK THE LICENCE ONE PICTURE AT A TIME. NEVER an NCI embedded image, and nothing shipped with the AHFS or MedlinePlus monographs.
- **Lands at:** `wwwroot/img/figures/pd-ct-head.svg` (or `.png` / `.jpg`) — **used by:** `/tests/ct-scan`

#### `pd-dti-tracts` — kind 4, wave 3

- **Shows:** A tractography picture: the wiring of the brain drawn as coloured strands.
- **Draft alt text:** Colored strands running through a brain, like bundles of wire.
- **Draft caption:** The colors are bundles of wiring. The surgeon plans a route around them.
- **Getting it:** Wikimedia Commons or an NIH open set. Licence checked per picture. Never NCI.
- **Lands at:** `wwwroot/img/figures/pd-dti-tracts.svg` (or `.png` / `.jpg`) — **used by:** `/tests/planning-scans`

#### `pd-flair-swelling` — kind 4, wave 2

- **Shows:** An MRI showing the bright swelling around a tumor.
- **Draft alt text:** An MRI slice with a bright cloudy area around a darker lump.
- **Draft caption:** The bright cloud is swelling, not tumor. Steroids work on the cloud.
- **Getting it:** Wikimedia Commons or an NIH open set. Licence checked per picture. Never NCI.
- **Lands at:** `wwwroot/img/figures/pd-flair-swelling.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/steroids`

#### `pd-hydrocephalus-ct` — kind 4, wave 2

- **Shows:** A scan with the fluid spaces much larger than normal.
- **Draft alt text:** A scan of a head with two large dark spaces in the middle.
- **Draft caption:** When the fluid cannot drain, the spaces swell and the pressure rises.
- **Getting it:** Wikimedia Commons or an NIH open set. Licence checked per picture. Never NCI.
- **Lands at:** `wwwroot/img/figures/pd-hydrocephalus-ct.svg` (or `.png` / `.jpg`) — **used by:** `/treatments/shunts`, `/where-your-tumor-is`

#### `pd-immunostain` — kind 4, wave 2

- **Shows:** A stained slide where some cells have gone brown and others have not.
- **Draft alt text:** A slide of cells where some have turned brown and others have not.
- **Draft caption:** A stain makes one protein show up. Which cells turn color is the answer.
- **Getting it:** Wikimedia Commons has good open pathology. Licence checked per picture. Never NCI.
- **Lands at:** `wwwroot/img/figures/pd-immunostain.svg` (or `.png` / `.jpg`) — **used by:** `/tests/molecular-markers`

#### `pd-mrs-graph` — kind 4, wave 3

- **Shows:** A spectroscopy trace: peaks along a line, with the chemicals named.
- **Draft alt text:** A line with peaks along it, each peak named for a chemical.
- **Draft caption:** This one is a graph, not a picture of the brain. The peaks are chemicals.
- **Getting it:** Wikimedia Commons. Licence checked per picture. Never NCI.
- **Lands at:** `wwwroot/img/figures/pd-mrs-graph.svg` (or `.png` / `.jpg`) — **used by:** `/tests/planning-scans`

#### `pd-perfusion-map` — kind 4, wave 3

- **Shows:** A perfusion map: blood flow shown as colour over a scan slice.
- **Draft alt text:** A scan slice with color over it, showing how much blood flows where.
- **Draft caption:** Color here means blood flow. Busy areas can mean an active tumor.
- **Getting it:** Wikimedia Commons or an NIH open set. Licence checked per picture. Never NCI.
- **Lands at:** `wwwroot/img/figures/pd-perfusion-map.svg` (or `.png` / `.jpg`) — **used by:** `/tests/planning-scans`

#### `pd-pet-brain` — kind 4, wave 3

- **Shows:** A PET scan of a head, with the bright area where the tracer gathered.
- **Draft alt text:** A blurry colored head scan with one bright patch in it.
- **Draft caption:** A PET shows what is busy rather than what is where.
- **Getting it:** Wikimedia Commons or an NIH open set. Licence checked per picture. Never NCI.
- **Lands at:** `wwwroot/img/figures/pd-pet-brain.svg` (or `.png` / `.jpg`) — **used by:** `/tests/planning-scans`

#### `pd-scan-acoustic-neuroma` — kind 4, wave 3

- **Shows:** A real scan of this tumor, from an open-licensed set.
- **Draft alt text:** An MRI slice with a small round lump beside the inner ear.
- **Draft caption:** One example, not yours. Tumors of the same name do not all look alike.
- **Getting it:** Wikimedia Commons, an NIH open set, or an open-access case report. CHECK THE LICENCE ONE PICTURE AT A TIME. NEVER an NCI embedded image. ONE PER HUB AND NEVER REUSED: a scan of a different tumor on this page would be a false claim about the reader's own.
- **Lands at:** `wwwroot/img/figures/pd-scan-acoustic-neuroma.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/acoustic-neuroma`

#### `pd-scan-all-brain-tumors` — kind 4, wave 3

- **Shows:** A real scan of this tumor, from an open-licensed set.
- **Draft alt text:** An MRI slice with a spot marked, and no name given for it.
- **Draft caption:** One example, not yours. Tumors of the same name do not all look alike.
- **Getting it:** Wikimedia Commons, an NIH open set, or an open-access case report. CHECK THE LICENCE ONE PICTURE AT A TIME. NEVER an NCI embedded image. ONE PER HUB AND NEVER REUSED: a scan of a different tumor on this page would be a false claim about the reader's own.
- **Lands at:** `wwwroot/img/figures/pd-scan-all-brain-tumors.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/all-brain-tumors`

#### `pd-scan-astrocytoma` — kind 4, wave 3

- **Shows:** A real scan of this tumor, from an open-licensed set.
- **Draft alt text:** An MRI slice with a pale area that has no clear edge.
- **Draft caption:** One example, not yours. Tumors of the same name do not all look alike.
- **Getting it:** Wikimedia Commons, an NIH open set, or an open-access case report. CHECK THE LICENCE ONE PICTURE AT A TIME. NEVER an NCI embedded image. ONE PER HUB AND NEVER REUSED: a scan of a different tumor on this page would be a false claim about the reader's own.
- **Lands at:** `wwwroot/img/figures/pd-scan-astrocytoma.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/astrocytoma`

#### `pd-scan-atrt` — kind 4, wave 3

- **Shows:** A real scan of this tumor, from an open-licensed set.
- **Draft alt text:** An MRI slice of a young child's head with a large lump at the back.
- **Draft caption:** One example, not yours. Tumors of the same name do not all look alike.
- **Getting it:** Wikimedia Commons, an NIH open set, or an open-access case report. CHECK THE LICENCE ONE PICTURE AT A TIME. NEVER an NCI embedded image. ONE PER HUB AND NEVER REUSED: a scan of a different tumor on this page would be a false claim about the reader's own.
- **Lands at:** `wwwroot/img/figures/pd-scan-atrt.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/atrt`

#### `pd-scan-brain-metastases` — kind 4, wave 3

- **Shows:** A real scan of this tumor, from an open-licensed set.
- **Draft alt text:** An MRI slice with several small bright spots, not one lump.
- **Draft caption:** One example, not yours. Tumors of the same name do not all look alike.
- **Getting it:** Wikimedia Commons, an NIH open set, or an open-access case report. CHECK THE LICENCE ONE PICTURE AT A TIME. NEVER an NCI embedded image. ONE PER HUB AND NEVER REUSED: a scan of a different tumor on this page would be a false claim about the reader's own.
- **Lands at:** `wwwroot/img/figures/pd-scan-brain-metastases.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/brain-metastases`

#### `pd-scan-chordoma` — kind 4, wave 3

- **Shows:** A real scan of this tumor, from an open-licensed set.
- **Draft alt text:** An MRI slice with a lump in the bone at the base of the skull.
- **Draft caption:** One example, not yours. Tumors of the same name do not all look alike.
- **Getting it:** Wikimedia Commons, an NIH open set, or an open-access case report. CHECK THE LICENCE ONE PICTURE AT A TIME. NEVER an NCI embedded image. ONE PER HUB AND NEVER REUSED: a scan of a different tumor on this page would be a false claim about the reader's own.
- **Lands at:** `wwwroot/img/figures/pd-scan-chordoma.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/chordoma`

#### `pd-scan-cns-germ-cell-tumor` — kind 4, wave 3

- **Shows:** A real scan of this tumor, from an open-licensed set.
- **Draft alt text:** An MRI slice with a lump deep in the middle of the brain.
- **Draft caption:** One example, not yours. Tumors of the same name do not all look alike.
- **Getting it:** Wikimedia Commons, an NIH open set, or an open-access case report. CHECK THE LICENCE ONE PICTURE AT A TIME. NEVER an NCI embedded image. ONE PER HUB AND NEVER REUSED: a scan of a different tumor on this page would be a false claim about the reader's own.
- **Lands at:** `wwwroot/img/figures/pd-scan-cns-germ-cell-tumor.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/cns-germ-cell-tumor`

#### `pd-scan-cns-lymphoma` — kind 4, wave 3

- **Shows:** A real scan of this tumor, from an open-licensed set.
- **Draft alt text:** An MRI slice with a bright lump next to the deep fluid spaces.
- **Draft caption:** One example, not yours. Tumors of the same name do not all look alike.
- **Getting it:** Wikimedia Commons, an NIH open set, or an open-access case report. CHECK THE LICENCE ONE PICTURE AT A TIME. NEVER an NCI embedded image. ONE PER HUB AND NEVER REUSED: a scan of a different tumor on this page would be a false claim about the reader's own.
- **Lands at:** `wwwroot/img/figures/pd-scan-cns-lymphoma.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/cns-lymphoma`

#### `pd-scan-craniopharyngioma` — kind 4, wave 3

- **Shows:** A real scan of this tumor, from an open-licensed set.
- **Draft alt text:** An MRI slice with a lump just above the small bony seat.
- **Draft caption:** One example, not yours. Tumors of the same name do not all look alike.
- **Getting it:** Wikimedia Commons, an NIH open set, or an open-access case report. CHECK THE LICENCE ONE PICTURE AT A TIME. NEVER an NCI embedded image. ONE PER HUB AND NEVER REUSED: a scan of a different tumor on this page would be a false claim about the reader's own.
- **Lands at:** `wwwroot/img/figures/pd-scan-craniopharyngioma.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/craniopharyngioma`

#### `pd-scan-diffuse-midline-glioma` — kind 4, wave 3

- **Shows:** A real scan of this tumor, from an open-licensed set.
- **Draft alt text:** An MRI slice with a swollen stalk at the base of the brain.
- **Draft caption:** One example, not yours. Tumors of the same name do not all look alike.
- **Getting it:** Wikimedia Commons, an NIH open set, or an open-access case report. CHECK THE LICENCE ONE PICTURE AT A TIME. NEVER an NCI embedded image. ONE PER HUB AND NEVER REUSED: a scan of a different tumor on this page would be a false claim about the reader's own.
- **Lands at:** `wwwroot/img/figures/pd-scan-diffuse-midline-glioma.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/diffuse-midline-glioma`

#### `pd-scan-dipg` — kind 4, wave 3

- **Shows:** A real scan of this tumor, from an open-licensed set.
- **Draft alt text:** An MRI slice with the middle of the brain stem swollen and pale.
- **Draft caption:** One example, not yours. Tumors of the same name do not all look alike.
- **Getting it:** Wikimedia Commons, an NIH open set, or an open-access case report. CHECK THE LICENCE ONE PICTURE AT A TIME. NEVER an NCI embedded image. ONE PER HUB AND NEVER REUSED: a scan of a different tumor on this page would be a false claim about the reader's own.
- **Lands at:** `wwwroot/img/figures/pd-scan-dipg.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/dipg`

#### `pd-scan-ependymoma` — kind 4, wave 3

- **Shows:** A real scan of this tumor, from an open-licensed set.
- **Draft alt text:** An MRI slice with a lump sitting inside a fluid space.
- **Draft caption:** One example, not yours. Tumors of the same name do not all look alike.
- **Getting it:** Wikimedia Commons, an NIH open set, or an open-access case report. CHECK THE LICENCE ONE PICTURE AT A TIME. NEVER an NCI embedded image. ONE PER HUB AND NEVER REUSED: a scan of a different tumor on this page would be a false claim about the reader's own.
- **Lands at:** `wwwroot/img/figures/pd-scan-ependymoma.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/ependymoma`

#### `pd-scan-glioblastoma` — kind 4, wave 3

- **Shows:** A real scan of this tumor, from an open-licensed set.
- **Draft alt text:** An MRI slice with a bright ring around a dark middle.
- **Draft caption:** One example, not yours. Tumors of the same name do not all look alike.
- **Getting it:** Wikimedia Commons, an NIH open set, or an open-access case report. CHECK THE LICENCE ONE PICTURE AT A TIME. NEVER an NCI embedded image. ONE PER HUB AND NEVER REUSED: a scan of a different tumor on this page would be a false claim about the reader's own.
- **Lands at:** `wwwroot/img/figures/pd-scan-glioblastoma.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/glioblastoma`

#### `pd-scan-glioma` — kind 4, wave 3

- **Shows:** A real scan of this tumor, from an open-licensed set.
- **Draft alt text:** An MRI slice with a pale area spreading into the brain around it.
- **Draft caption:** One example, not yours. Tumors of the same name do not all look alike.
- **Getting it:** Wikimedia Commons, an NIH open set, or an open-access case report. CHECK THE LICENCE ONE PICTURE AT A TIME. NEVER an NCI embedded image. ONE PER HUB AND NEVER REUSED: a scan of a different tumor on this page would be a false claim about the reader's own.
- **Lands at:** `wwwroot/img/figures/pd-scan-glioma.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/glioma`

#### `pd-scan-hemangioblastoma` — kind 4, wave 3

- **Shows:** A real scan of this tumor, from an open-licensed set.
- **Draft alt text:** An MRI slice with a small bright lump on the wall of a fluid pocket.
- **Draft caption:** One example, not yours. Tumors of the same name do not all look alike.
- **Getting it:** Wikimedia Commons, an NIH open set, or an open-access case report. CHECK THE LICENCE ONE PICTURE AT A TIME. NEVER an NCI embedded image. ONE PER HUB AND NEVER REUSED: a scan of a different tumor on this page would be a false claim about the reader's own.
- **Lands at:** `wwwroot/img/figures/pd-scan-hemangioblastoma.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/hemangioblastoma`

#### `pd-scan-high-grade-glioma` — kind 4, wave 3

- **Shows:** A real scan of this tumor, from an open-licensed set.
- **Draft alt text:** An MRI slice with a bright lump and a dark middle.
- **Draft caption:** One example, not yours. Tumors of the same name do not all look alike.
- **Getting it:** Wikimedia Commons, an NIH open set, or an open-access case report. CHECK THE LICENCE ONE PICTURE AT A TIME. NEVER an NCI embedded image. ONE PER HUB AND NEVER REUSED: a scan of a different tumor on this page would be a false claim about the reader's own.
- **Lands at:** `wwwroot/img/figures/pd-scan-high-grade-glioma.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/high-grade-glioma`

#### `pd-scan-low-grade-glioma` — kind 4, wave 3

- **Shows:** A real scan of this tumor, from an open-licensed set.
- **Draft alt text:** An MRI slice with a pale area and no bright ring.
- **Draft caption:** One example, not yours. Tumors of the same name do not all look alike.
- **Getting it:** Wikimedia Commons, an NIH open set, or an open-access case report. CHECK THE LICENCE ONE PICTURE AT A TIME. NEVER an NCI embedded image. ONE PER HUB AND NEVER REUSED: a scan of a different tumor on this page would be a false claim about the reader's own.
- **Lands at:** `wwwroot/img/figures/pd-scan-low-grade-glioma.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/low-grade-glioma`

#### `pd-scan-medulloblastoma` — kind 4, wave 3

- **Shows:** A real scan of this tumor, from an open-licensed set.
- **Draft alt text:** An MRI slice of a child's head with a lump low at the back.
- **Draft caption:** One example, not yours. Tumors of the same name do not all look alike.
- **Getting it:** Wikimedia Commons, an NIH open set, or an open-access case report. CHECK THE LICENCE ONE PICTURE AT A TIME. NEVER an NCI embedded image. ONE PER HUB AND NEVER REUSED: a scan of a different tumor on this page would be a false claim about the reader's own.
- **Lands at:** `wwwroot/img/figures/pd-scan-medulloblastoma.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/medulloblastoma`

#### `pd-scan-meningioma` — kind 4, wave 3

- **Shows:** A real scan of this tumor, from an open-licensed set.
- **Draft alt text:** An MRI slice with a smooth lump sitting against the inside of the skull.
- **Draft caption:** One example, not yours. Tumors of the same name do not all look alike.
- **Getting it:** Wikimedia Commons, an NIH open set, or an open-access case report. CHECK THE LICENCE ONE PICTURE AT A TIME. NEVER an NCI embedded image. ONE PER HUB AND NEVER REUSED: a scan of a different tumor on this page would be a false claim about the reader's own.
- **Lands at:** `wwwroot/img/figures/pd-scan-meningioma.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/meningioma`

#### `pd-scan-oligodendroglioma` — kind 4, wave 3

- **Shows:** A real scan of this tumor, from an open-licensed set.
- **Draft alt text:** An MRI slice with a pale area in the front of the brain.
- **Draft caption:** One example, not yours. Tumors of the same name do not all look alike.
- **Getting it:** Wikimedia Commons, an NIH open set, or an open-access case report. CHECK THE LICENCE ONE PICTURE AT A TIME. NEVER an NCI embedded image. ONE PER HUB AND NEVER REUSED: a scan of a different tumor on this page would be a false claim about the reader's own.
- **Lands at:** `wwwroot/img/figures/pd-scan-oligodendroglioma.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/oligodendroglioma`

#### `pd-scan-pediatric-brain-tumor` — kind 4, wave 3

- **Shows:** A real scan of this tumor, from an open-licensed set.
- **Draft alt text:** An MRI slice of a child's head with a lump marked on it.
- **Draft caption:** One example, not yours. Tumors of the same name do not all look alike.
- **Getting it:** Wikimedia Commons, an NIH open set, or an open-access case report. CHECK THE LICENCE ONE PICTURE AT A TIME. NEVER an NCI embedded image. ONE PER HUB AND NEVER REUSED: a scan of a different tumor on this page would be a false claim about the reader's own.
- **Lands at:** `wwwroot/img/figures/pd-scan-pediatric-brain-tumor.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/pediatric-brain-tumor`

#### `pd-scan-pituitary-tumor` — kind 4, wave 3

- **Shows:** A real scan of this tumor, from an open-licensed set.
- **Draft alt text:** An MRI slice from the front, with a lump in the small bony seat.
- **Draft caption:** One example, not yours. Tumors of the same name do not all look alike.
- **Getting it:** Wikimedia Commons, an NIH open set, or an open-access case report. CHECK THE LICENCE ONE PICTURE AT A TIME. NEVER an NCI embedded image. ONE PER HUB AND NEVER REUSED: a scan of a different tumor on this page would be a false claim about the reader's own.
- **Lands at:** `wwwroot/img/figures/pd-scan-pituitary-tumor.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/pituitary-tumor`

#### `pd-scan-spinal-cord-tumor` — kind 4, wave 3

- **Shows:** A real scan of this tumor, from an open-licensed set.
- **Draft alt text:** An MRI of a spine from the side, with a lump inside the cord.
- **Draft caption:** One example, not yours. Tumors of the same name do not all look alike.
- **Getting it:** Wikimedia Commons, an NIH open set, or an open-access case report. CHECK THE LICENCE ONE PICTURE AT A TIME. NEVER an NCI embedded image. ONE PER HUB AND NEVER REUSED: a scan of a different tumor on this page would be a false claim about the reader's own.
- **Lands at:** `wwwroot/img/figures/pd-scan-spinal-cord-tumor.svg` (or `.png` / `.jpg`) — **used by:** `/tumors/spinal-cord-tumor`

---

## 6. The per-page slots

`Goes after` is the heading the figure follows, exactly as the page writes it today, or
a `[BLOCK]` directive line. Every one is checked against the live page by
`ImagesNeededInventoryTests`, so a renamed heading fails the build rather than leaving a
stale position in this file.

### `/seizures/living-with`

1814 words, 4 required, 4 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | Water | 1 | `ph-bath-shower` |
| 2 | Keeping the things you like doing | 1 | `ph-walk-outdoors` |
| 3 | The diary is the thing that changes treatment | 2 | `doc-seizure-diary` |
| 4 | Driving | 1 | `ph-car-keys` |

### `/seizures/what-to-do`

1035 words, 3 required, 3 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | Do this now | 3 | `dia-seizure-steps` |
| 2 | Do not | 3 | `dia-seizure-do-not` |
| 3 | What to write down | 2 | `doc-seizure-diary` |

### `/start`

835 words, 2 required, 2 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | Get emergency help right away if you notice any of these signs | 3 | `dia-escalation-card` |
| 2 | What helps in the first week | 1 | `ph-kitchen-table-papers` |

### `/tests/biopsy`

3353 words, 7 required, 7 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | What is a biopsy? | 3 | `dia-biopsy-route` |
| 2 | What happens, step by step | 1 | `ph-theatre` |
| 3 | What can go wrong? | 1 | `ph-recovery-bed` |
| 4 | Is there a way to find out without one? | 3 | `dia-scan-side-by-side` |
| 5 | What you need first, and what to bring | 2 | `doc-preop-sheet` |
| 6 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |
| 7 | Who reads it, and how do I get the result? | 1 | `ph-microscope` |

### `/tests/ct-scan`

2235 words, 5 required, 5 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | What is a CT scan? | 1 | `ph-ct-scanner` |
| 2 | Why a CT and not an MRI? | 4 | `pd-ct-head` |
| 3 | What happens, step by step | 1 | `ph-scan-console` |
| 4 | The dye they may put in your arm | 1 | `ph-cannula` |
| 5 | What to tell them before the day | 2 | `doc-medicines-list` |

### `/tests/follow-up-scans`

3050 words, 7 required, 7 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | What is a follow-up scan? | 3 | `dia-followup-timeline` |
| 2 | What happens to the pictures | 3 | `dia-scan-side-by-side` |
| 3 | The waiting, and what actually helps | 1 | `ph-waiting-room` |
| 4 | What the words on the report mean | 2 | `doc-radiology-report` |
| 5 | What if the scan looks worse? | 3 | `dia-looks-worse-two-causes` |
| 6 | What you can do around a scan | 1 | `ph-diary` |
| 7 | Who reads it, and how do I get the result? | 3 | `dia-tumor-board` |

### `/tests/getting-ready-for-surgery`

3096 words, 7 required, 7 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | What happens, step by step | 2 | `doc-preop-sheet` |
| 2 | When do I stop eating and drinking? | 1 | `ph-clock-water` |
| 3 | Which of my medicines do I keep taking? | 2 | `doc-medicines-list` |
| 4 | Blood thinners: do not stop one on your own | 1 | `ph-blister-pack` |
| 5 | What to bring, and what to leave at home | 1 | `ph-hospital-bag` |
| 6 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |
| 7 | Who reads all of this, and what if something is not right? | 3 | `dia-your-team` |

### `/tests/molecular-markers`

2432 words, 5 required, 5 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | What these tests are | 3 | `dia-marker-meaning` |
| 2 | Are these results passed on to my children? | 3 | `dia-tumor-vs-inherited` |
| 3 | Where these words sit on your report | 2 | `doc-molecular-panel` |
| 4 | The words, one by one | 4 | `pd-immunostain` |
| 5 | How long do these take? | 3 | `dia-wait-timeline` |

### `/tests/mri`

2378 words, 5 required, 5 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | What is an MRI? | 1 | `ph-mri-scanner` |
| 2 | What happens, step by step | 1 | `ph-head-coil` |
| 3 | The dye: what people ask about it | 1 | `ph-cannula` |
| 4 | Metal, implants and your device card | 2 | `doc-implant-card` |
| 5 | `[TUMOR-BOARD]` | 3 | `dia-tumor-board` |

### `/tests/neuro-exam-and-memory-testing`

2427 words, 5 required, 5 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | Two different things, often in the same conversation | 3 | `dia-two-tests-compared` |
| 2 | What happens, step by step | 1 | `ph-reflex-hammer` |
| 3 | Am I being judged? | 1 | `ph-pencil-puzzle` |
| 4 | What the report is used for | 2 | `doc-neuropsych-summary` |
| 5 | What to sort out beforehand | 2 | `doc-questions-sheet` |

### `/tests/pathology-report`

1978 words, 4 required, 4 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | Where the answer sits on the page | 3 | `dia-report-layers` |
| 2 | Your report, part by part | 2 | `doc-pathology-parts` |
| 3 | What the grade means | 3 | `dia-grade-ladder` |
| 4 | What you can do with your copy | 1 | `ph-kitchen-table-papers` |

### `/tests/planning-scans`

3287 words, 7 required, 7 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | What are these extra scans? | 3 | `dia-scan-types` |
| 2 | The scan where you have a job: functional MRI | 3 | `dia-fmri-task` |
| 3 | The map of the wiring: DTI | 4 | `pd-dti-tracts` |
| 4 | The chemistry graph: MR spectroscopy | 4 | `pd-mrs-graph` |
| 5 | The blood supply scan: perfusion | 4 | `pd-perfusion-map` |
| 6 | The tracer scan: PET | 4 | `pd-pet-brain` |
| 7 | Pictures of the blood vessels | 4 | `pd-angiogram` |

### `/tests/waiting-for-results`

2659 words, 6 required, 6 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | What is being done with your tissue? | 1 | `ph-specimen-pot` |
| 2 | What happens to your tissue, step by step | 3 | `dia-tissue-to-slide` |
| 3 | How long does it take? | 3 | `dia-wait-timeline` |
| 4 | What the wait is like | 1 | `ph-waiting-room` |
| 5 | Why is there a second report? | 2 | `doc-addendum` |
| 6 | `[TUMOR-BOARD]` | 3 | `dia-tumor-board` |

### `/treatments/anti-seizure-medicines`

4159 words, 9 required, 9 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | What is it, and what is it doing? | 3 | `dia-seizure-threshold` |
| 2 | What happens, step by step | 1 | `ph-pill-box` |
| 3 | How long will I be on it? | 1 | `ph-pharmacy-label` |
| 4 | The change in mood and temper | 3 | `dia-tumor-or-medicine` |
| 5 | Thoughts of harming yourself | 1 | `ph-phone-note` |
| 6 | Tell them about everything else you take | 2 | `doc-medicines-list` |
| 7 | Can I drive? | 1 | `ph-car-keys` |
| 8 | When to call, and what about | 3 | `dia-escalation-card` |
| 9 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |

### `/treatments/awake-craniotomy`

3350 words, 7 required, 7 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | What is an awake craniotomy? | 3 | `dia-awake-mapping` |
| 2 | Before the day | 2 | `doc-preop-sheet` |
| 3 | On the day | 1 | `ph-theatre` |
| 4 | Your job while you are awake | 1 | `ph-naming-cards` |
| 5 | What can go wrong | 1 | `ph-recovery-bed` |
| 6 | What you need first | 2 | `doc-questions-sheet` |
| 7 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |

### `/treatments/chemotherapy`

4356 words, 9 required, 9 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | What is chemotherapy for a brain tumor? | 3 | `dia-bbb` |
| 2 | Temozolomide | 1 | `ph-blister-pack` |
| 3 | PCV | 3 | `dia-chemo-cycle` |
| 4 | Carmustine wafers | 3 | `dia-wafer-in-cavity` |
| 5 | Which tumors usually get which drug | 3 | `dia-marker-meaning` |
| 6 | Your blood counts, and the fever rule | 2 | `doc-blood-count-sheet` |
| 7 | What does a cycle feel like? | 1 | `ph-infusion-chair` |
| 8 | Feeling sick, and the medicine for it | 1 | `ph-pill-box` |
| 9 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |

### `/treatments/clinical-trials`

2988 words, 6 required, 7 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | What is it, and what is it not? | 3 | `dia-trial-phases` |
| 2 | When should I ask? | 3 | `dia-treatment-path` |
| 3 | What happens, step by step | 2 | `doc-consent-first-page` |
| 4 | Will I be given a sugar pill? | 3 | `dia-placebo-add-on` |
| 5 | Who is looking out for me? | 3 | `dia-your-team` |
| 6 | What will it cost me? | 1 | `ph-kitchen-table-papers` |
| 7 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |

### `/treatments/craniotomy`

5304 words, 11 required, 11 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | What is a craniotomy? | 3 | `dia-craniotomy-steps` |
| 2 | Before the day | 2 | `doc-preop-sheet` |
| 3 | The day | 1 | `ph-theatre` |
| 4 | Waking up | 1 | `ph-recovery-bed` |
| 5 | The days after | 1 | `ph-head-dressing` |
| 6 | "How much did you get out?" | 3 | `dia-extent-of-resection` |
| 7 | What can go wrong | 3 | `dia-escalation-card` |
| 8 | Is this the tumor, or is it the medicine? | 3 | `dia-tumor-or-medicine` |
| 9 | Is there a way to do this without opening the skull? | 3 | `dia-endoscopic-route` |
| 10 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |
| 11 | `[TUMOR-BOARD]` | 3 | `dia-tumor-board` |

### `/treatments/proton-therapy`

3120 words, 7 required, 7 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | What is proton therapy? | 3 | `dia-proton-stop` |
| 2 | What happens, step by step | 1 | `ph-mask` |
| 3 | Is it the same to be in? | 1 | `ph-proton-gantry` |
| 4 | Where are the centers, and what would getting there mean? | 1 | `ph-hospital-bag` |
| 5 | What if my insurance says no? | 2 | `doc-insurance-denial` |
| 6 | What you need first | 2 | `doc-questions-sheet` |
| 7 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |

### `/treatments/radiation-therapy`

6128 words, 13 required, 13 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | What is radiation therapy? | 3 | `dia-radiation-margin` |
| 2 | Why am I having it? | 3 | `dia-treatment-path` |
| 3 | The first appointment | 2 | `doc-radiation-schedule` |
| 4 | The planning visit, and the mask | 1 | `ph-mask` |
| 5 | A treatment day | 1 | `ph-linac` |
| 6 | Why every day, and why so many? | 3 | `dia-fractions-calendar` |
| 7 | If the mask frightens you | 1 | `ph-call-button` |
| 8 | Side effects during treatment, and in the weeks after | 3 | `dia-side-effect-timeline` |
| 9 | Is this the tumor, or is it the medicine? | 3 | `dia-tumor-or-medicine` |
| 10 | Somnolence syndrome | 1 | `ph-asleep-chair` |
| 11 | Whole-brain radiation | 3 | `dia-wbrt-field` |
| 12 | What you need first, and what to bring | 1 | `ph-hospital-bag` |
| 13 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |

### `/treatments/shunts`

2308 words, 5 required, 5 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | What is it? | 3 | `dia-csf-shunt` |
| 2 | Why would I need one? | 4 | `pd-hydrocephalus-ct` |
| 3 | Signs a shunt is not working | 3 | `dia-escalation-card` |
| 4 | Living with a shunt | 2 | `doc-implant-card` |
| 5 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |

### `/treatments/stereotactic-radiosurgery`

4157 words, 9 required, 9 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | What is it? | 1 | `ph-srs-frame` |
| 2 | Why this and not an operation, or weeks of radiation? | 3 | `dia-srs-vs` |
| 3 | What happens, step by step | 1 | `ph-mask` |
| 4 | What does it feel like? | 1 | `ph-call-button` |
| 5 | The wait, and what to do with it | 1 | `ph-diary` |
| 6 | Side effects that can come later | 3 | `dia-side-effect-timeline` |
| 7 | `[ESCALATION]` | 3 | `dia-escalation-card` |
| 8 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |
| 9 | Who reads it, and how do I get the result? | 3 | `dia-followup-timeline` |

### `/treatments/steroids`

4214 words, 9 required, 9 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | What is it, and what is it doing? | 3 | `dia-steroid-swelling` |
| 2 | Why am I taking one? | 4 | `pd-flair-swelling` |
| 3 | What happens, step by step | 1 | `ph-pill-box` |
| 4 | Why can I not just stop taking it? | 2 | `doc-steroid-taper` |
| 5 | The weakness in your legs and arms | 1 | `ph-walking-rail` |
| 6 | The rest of the side effects | 3 | `dia-side-effect-timeline` |
| 7 | Why does my team ask about my steroid dose before a scan? | 2 | `doc-medicines-list` |
| 8 | When to call, and what about | 3 | `dia-escalation-card` |
| 9 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |

### `/treatments/targeted-therapy`

3611 words, 8 required, 8 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | What is a targeted drug? | 3 | `dia-target-lock` |
| 2 | Why does my tumor's report decide this? | 2 | `doc-molecular-panel` |
| 3 | Which drug goes with which result? | 3 | `dia-marker-meaning` |
| 4 | What happens, step by step | 1 | `ph-pill-box` |
| 5 | What would it mean to say this worked? | 3 | `dia-scan-side-by-side` |
| 6 | Side effects | 3 | `dia-side-effect-timeline` |
| 7 | If you are having surgery, bevacizumab has its own rule | 2 | `doc-preop-sheet` |
| 8 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |

### `/treatments/tumor-treating-fields`

2726 words, 6 required, 6 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | What is it? | 3 | `dia-ttf-arrays` |
| 2 | What happens, step by step | 1 | `ph-clippers` |
| 3 | Carrying it, sleeping, and washing | 1 | `ph-shoulder-bag` |
| 4 | The shaved head, and other people | 1 | `ph-hat-scarf` |
| 5 | Your scalp, and other side effects | 3 | `dia-side-effect-timeline` |
| 6 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |

### `/treatments/watch-and-wait`

2372 words, 5 required, 5 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | What is watch and wait? | 3 | `dia-watch-cadence` |
| 2 | What happens, step by step | 1 | `ph-diary` |
| 3 | Worry and low mood, and what you can do | 1 | `ph-walk-outdoors` |
| 4 | What if it grows while we are watching? | 3 | `dia-scan-side-by-side` |
| 5 | How can they know what it is without a sample? | 3 | `dia-looks-worse-two-causes` |

### `/tumors/acoustic-neuroma`

5675 words, 12 required, 12 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | Is it cancer? What does its grade mean? | 3 | `dia-grade-ladder` |
| 2 | Where does it grow, and why does it cause these symptoms? | 3 | `dia-region-hearing-nerve` |
| 3 | `[MECHANISM]` | 3 | `dia-mechanism-pressure` |
| 4 | What symptoms does it cause? | 3 | `dia-symptom-map` |
| 5 | `[ESCALATION]` | 3 | `dia-escalation-card` |
| 6 | How do doctors find out it is this? | 4 | `pd-scan-acoustic-neuroma` |
| 7 | What do the words on my report mean? | 2 | `doc-pathology-parts` |
| 8 | How is it usually treated? | 3 | `dia-treatment-path` |
| 9 | What is treatment actually like, and what is normal afterwards? | 1 | `ph-recovery-bed` |
| 10 | Follow-up scans, and what to do while you wait | 3 | `dia-followup-timeline` |
| 11 | If it comes back | 3 | `dia-recurrence-options` |
| 12 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |

### `/tumors/all-brain-tumors`

5663 words, 12 required, 12 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | What have you actually been told? | 4 | `pd-scan-all-brain-tumors` |
| 2 | Why the scan cannot say on its own | 3 | `dia-tissue-to-slide` |
| 3 | `[MECHANISM]` | 3 | `dia-mechanism-pressure` |
| 4 | `[ESCALATION]` | 3 | `dia-escalation-card` |
| 5 | Will I need a piece of it taken? | 3 | `dia-biopsy-route` |
| 6 | How long until it has a name? | 3 | `dia-wait-timeline` |
| 7 | Why nobody has given you a stage | 3 | `dia-grade-ladder` |
| 8 | `[TUMOR-BOARD]` | 3 | `dia-tumor-board` |
| 9 | Asking somebody else to look | 1 | `ph-microscope` |
| 10 | `[CAUSES]` | 3 | `dia-cause-known-unknown` |
| 11 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |
| 12 | What to ask your team | 2 | `doc-questions-sheet` |

### `/tumors/astrocytoma`

6010 words, 13 required, 13 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | Is it cancer? What does its grade mean? | 3 | `dia-grade-ladder` |
| 2 | Where does it grow, and why does it cause these symptoms? | 3 | `dia-brain-regions` |
| 3 | `[MECHANISM]` | 3 | `dia-mechanism-pressure` |
| 4 | What symptoms does it cause? | 3 | `dia-symptom-map` |
| 5 | `[ESCALATION]` | 3 | `dia-escalation-card` |
| 6 | How do doctors find out it is this? | 4 | `pd-scan-astrocytoma` |
| 7 | What do the words on my report mean? | 2 | `doc-pathology-parts` |
| 8 | How is it usually treated? | 3 | `dia-treatment-path` |
| 9 | What is treatment actually like, and what is normal afterwards? | 1 | `ph-recovery-bed` |
| 10 | Everyday life: work, driving, seizures and tiredness | 1 | `ph-car-keys` |
| 11 | Follow-up scans, and what to do while you wait | 3 | `dia-followup-timeline` |
| 12 | If it comes back, or changes | 3 | `dia-recurrence-options` |
| 13 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |

### `/tumors/atrt`

5937 words, 12 required, 12 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | Is it cancer? What does its grade mean? | 3 | `dia-grade-ladder` |
| 2 | Where does it grow, and why does it cause these symptoms? | 3 | `dia-region-cerebellum` |
| 3 | What symptoms does it cause in a baby or young child? | 3 | `dia-symptom-map` |
| 4 | `[ESCALATION]` | 3 | `dia-escalation-card` |
| 5 | `[SPINAL-CORD]` | 3 | `dia-csf-spread` |
| 6 | How do doctors find out it is this? | 4 | `pd-scan-atrt` |
| 7 | What do the words on my child's report mean? | 2 | `doc-pathology-parts` |
| 8 | How is it usually treated? | 3 | `dia-treatment-path` |
| 9 | `[POSTERIOR-FOSSA-SYNDROME]` | 3 | `dia-posterior-fossa` |
| 10 | Follow-up scans, and what to do while you wait | 3 | `dia-followup-timeline` |
| 11 | If it comes back, or changes | 3 | `dia-recurrence-options` |
| 12 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |

### `/tumors/brain-metastases`

6089 words, 13 required, 13 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | Where does it grow, and why does it cause these symptoms? | 3 | `dia-brain-regions` |
| 2 | `[MECHANISM]` | 3 | `dia-mechanism-pressure` |
| 3 | What symptoms does it cause? | 3 | `dia-symptom-map` |
| 4 | `[ESCALATION]` | 3 | `dia-escalation-card` |
| 5 | How do doctors find out it is this? | 4 | `pd-scan-brain-metastases` |
| 6 | What do the words on my report mean? | 2 | `doc-pathology-parts` |
| 7 | How is it usually treated? | 3 | `dia-treatment-path` |
| 8 | What is treatment actually like, and what is normal afterwards? | 1 | `ph-recovery-bed` |
| 9 | Everyday life: work, driving, seizures and tiredness | 1 | `ph-car-keys` |
| 10 | Follow-up scans, and what to do while you wait | 3 | `dia-followup-timeline` |
| 11 | If it comes back, or changes | 3 | `dia-recurrence-options` |
| 12 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |
| 13 | What to ask your team | 2 | `doc-questions-sheet` |

### `/tumors/chordoma`

6542 words, 14 required, 14 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | Is it cancer? What does its grade mean? | 3 | `dia-grade-ladder` |
| 2 | Where does it grow, and why does it cause these symptoms? | 3 | `dia-region-skull-base` |
| 3 | What symptoms does it cause? | 3 | `dia-symptom-map` |
| 4 | `[ESCALATION]` | 3 | `dia-escalation-card` |
| 5 | How do doctors find out it is this? | 4 | `pd-scan-chordoma` |
| 6 | What do the words on my report mean? | 2 | `doc-pathology-parts` |
| 7 | `[CROSSWALK]` | 3 | `dia-crosswalk-names` |
| 8 | How is it usually treated? | 3 | `dia-treatment-path` |
| 9 | What is treatment actually like, and what is normal afterwards? | 1 | `ph-recovery-bed` |
| 10 | Everyday life | 1 | `ph-car-keys` |
| 11 | Follow-up scans, and what to do while you wait | 3 | `dia-followup-timeline` |
| 12 | If it comes back | 3 | `dia-recurrence-options` |
| 13 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |
| 14 | What to ask your team | 2 | `doc-questions-sheet` |

### `/tumors/cns-germ-cell-tumor`

7129 words, 15 required, 15 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | Is it cancer? What does its grade mean? | 3 | `dia-grade-ladder` |
| 2 | Where does it grow, and why does it cause these symptoms? | 3 | `dia-region-pineal` |
| 3 | `[MECHANISM]` | 3 | `dia-mechanism-pressure` |
| 4 | What symptoms does it cause? | 3 | `dia-symptom-map` |
| 5 | `[ESCALATION]` | 3 | `dia-escalation-card` |
| 6 | `[SPINAL-CORD]` | 3 | `dia-csf-spread` |
| 7 | How do doctors find out it is this? | 4 | `pd-scan-cns-germ-cell-tumor` |
| 8 | What do the words on my report mean? | 2 | `doc-pathology-parts` |
| 9 | How is it usually treated? | 3 | `dia-treatment-path` |
| 10 | What is treatment actually like, and what is normal afterwards? | 1 | `ph-recovery-bed` |
| 11 | Everyday life | 1 | `ph-car-keys` |
| 12 | Follow-up scans, and what to do while you wait | 3 | `dia-followup-timeline` |
| 13 | If it comes back | 3 | `dia-recurrence-options` |
| 14 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |
| 15 | What to ask your team | 2 | `doc-questions-sheet` |

### `/tumors/cns-lymphoma`

6232 words, 13 required, 13 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | Is it cancer? What does its grade mean? | 3 | `dia-grade-ladder` |
| 2 | Where does it grow, and why does it cause these symptoms? | 3 | `dia-region-ventricles` |
| 3 | `[MECHANISM]` | 3 | `dia-mechanism-pressure` |
| 4 | What symptoms does it cause? | 3 | `dia-symptom-map` |
| 5 | `[ESCALATION]` | 3 | `dia-escalation-card` |
| 6 | How do doctors find out it is this? | 4 | `pd-scan-cns-lymphoma` |
| 7 | What do the words on my report mean? | 2 | `doc-pathology-parts` |
| 8 | How is it usually treated? | 3 | `dia-treatment-path` |
| 9 | What is treatment actually like, and what is normal afterwards? | 1 | `ph-recovery-bed` |
| 10 | Everyday life | 1 | `ph-car-keys` |
| 11 | Follow-up scans, and what to do while you wait | 3 | `dia-followup-timeline` |
| 12 | If it comes back | 3 | `dia-recurrence-options` |
| 13 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |

### `/tumors/craniopharyngioma`

5632 words, 12 required, 12 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | Is it cancer? What does its grade mean? | 3 | `dia-grade-ladder` |
| 2 | Where does it grow, and why does it cause these symptoms? | 3 | `dia-region-sellar` |
| 3 | `[MECHANISM]` | 3 | `dia-mechanism-pressure` |
| 4 | What symptoms does it cause? | 3 | `dia-symptom-map` |
| 5 | `[ESCALATION]` | 3 | `dia-escalation-card` |
| 6 | How do doctors find out it is this? | 4 | `pd-scan-craniopharyngioma` |
| 7 | What do the words on my report mean? | 2 | `doc-pathology-parts` |
| 8 | How is it usually treated? | 3 | `dia-treatment-path` |
| 9 | What is treatment actually like, and what is normal afterwards? | 1 | `ph-recovery-bed` |
| 10 | Follow-up scans, and what to do while you wait | 3 | `dia-followup-timeline` |
| 11 | If it comes back | 3 | `dia-recurrence-options` |
| 12 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |

### `/tumors/diffuse-midline-glioma`

4541 words, 10 required, 10 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | Is it cancer? What does its grade mean? | 3 | `dia-grade-ladder` |
| 2 | Where does it grow, and why does it cause these symptoms? | 3 | `dia-region-brainstem` |
| 3 | `[MECHANISM]` | 3 | `dia-mechanism-pressure` |
| 4 | What symptoms does it cause? | 3 | `dia-symptom-map` |
| 5 | `[ESCALATION]` | 3 | `dia-escalation-card` |
| 6 | How do doctors find out it is this? | 4 | `pd-scan-diffuse-midline-glioma` |
| 7 | What do the words on my report mean? | 2 | `doc-pathology-parts` |
| 8 | How is it usually treated? | 3 | `dia-treatment-path` |
| 9 | Follow-up scans, and what to do while you wait | 3 | `dia-followup-timeline` |
| 10 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |

### `/tumors/dipg`

4537 words, 10 required, 10 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | Is it cancer? What does its grade mean? | 3 | `dia-grade-ladder` |
| 2 | Where does it grow, and why does it cause these symptoms? | 3 | `dia-region-brainstem` |
| 3 | `[MECHANISM]` | 3 | `dia-mechanism-pressure` |
| 4 | What symptoms does it cause? | 3 | `dia-symptom-map` |
| 5 | `[ESCALATION]` | 3 | `dia-escalation-card` |
| 6 | How do doctors find out it is this? | 4 | `pd-scan-dipg` |
| 7 | What do the words on my report mean? | 2 | `doc-pathology-parts` |
| 8 | How is it usually treated? | 3 | `dia-treatment-path` |
| 9 | Follow-up scans, and what to do while you wait | 3 | `dia-followup-timeline` |
| 10 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |

### `/tumors/ependymoma`

5628 words, 12 required, 12 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | Is it cancer? What does its grade mean? | 3 | `dia-grade-ladder` |
| 2 | Where does it grow, and why does it cause these symptoms? | 3 | `dia-region-ventricles` |
| 3 | `[MECHANISM]` | 3 | `dia-mechanism-pressure` |
| 4 | What symptoms does it cause? | 3 | `dia-symptom-map` |
| 5 | `[ESCALATION]` | 3 | `dia-escalation-card` |
| 6 | `[SPINAL-CORD]` | 3 | `dia-csf-spread` |
| 7 | How do doctors find out it is this? | 4 | `pd-scan-ependymoma` |
| 8 | What do the words on my report mean? | 2 | `doc-pathology-parts` |
| 9 | How is it usually treated? | 3 | `dia-treatment-path` |
| 10 | `[POSTERIOR-FOSSA-SYNDROME]` | 3 | `dia-posterior-fossa` |
| 11 | Follow-up scans, and what to do while you wait | 3 | `dia-followup-timeline` |
| 12 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |

### `/tumors/glioblastoma`

5736 words, 12 required, 12 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | Is it cancer? What does its grade mean? | 3 | `dia-grade-ladder` |
| 2 | Where does it grow, and why does it cause these symptoms? | 3 | `dia-brain-regions` |
| 3 | `[MECHANISM]` | 3 | `dia-mechanism-pressure` |
| 4 | What symptoms does it cause? | 3 | `dia-symptom-map` |
| 5 | `[ESCALATION]` | 3 | `dia-escalation-card` |
| 6 | How do doctors find out it is this? | 4 | `pd-scan-glioblastoma` |
| 7 | What do the words on my report mean? | 2 | `doc-pathology-parts` |
| 8 | How is it usually treated? | 3 | `dia-treatment-path` |
| 9 | What is treatment actually like, and what is normal afterwards? | 1 | `ph-recovery-bed` |
| 10 | Follow-up scans, and what to do while you wait | 3 | `dia-followup-timeline` |
| 11 | If it comes back, or changes | 3 | `dia-recurrence-options` |
| 12 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |

### `/tumors/glioma`

5567 words, 12 required, 12 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | Is it cancer? What does its grade mean? | 3 | `dia-grade-ladder` |
| 2 | Where does it grow, and why does it cause these symptoms? | 3 | `dia-brain-regions` |
| 3 | `[MECHANISM]` | 3 | `dia-mechanism-pressure` |
| 4 | What symptoms does it cause? | 3 | `dia-symptom-map` |
| 5 | `[ESCALATION]` | 3 | `dia-escalation-card` |
| 6 | How do doctors find out it is this? | 4 | `pd-scan-glioma` |
| 7 | What do the words on my report mean? | 2 | `doc-pathology-parts` |
| 8 | How is it usually treated? | 3 | `dia-treatment-path` |
| 9 | What is treatment actually like, and what is normal afterwards? | 1 | `ph-recovery-bed` |
| 10 | Follow-up scans, and what to do while you wait | 3 | `dia-followup-timeline` |
| 11 | If it comes back, or changes | 3 | `dia-recurrence-options` |
| 12 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |

### `/tumors/hemangioblastoma`

5642 words, 12 required, 12 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | Is it cancer? What does its grade mean? | 3 | `dia-grade-ladder` |
| 2 | Where does it grow, and why does it cause these symptoms? | 3 | `dia-region-cerebellum` |
| 3 | `[MECHANISM]` | 3 | `dia-mechanism-pressure` |
| 4 | What symptoms does it cause? | 3 | `dia-symptom-map` |
| 5 | `[ESCALATION]` | 3 | `dia-escalation-card` |
| 6 | How do doctors find out it is this? | 4 | `pd-scan-hemangioblastoma` |
| 7 | What do the words on my report mean? | 2 | `doc-pathology-parts` |
| 8 | How is it usually treated? | 3 | `dia-treatment-path` |
| 9 | What is treatment actually like, and what is normal afterwards? | 1 | `ph-recovery-bed` |
| 10 | Follow-up scans, and what to do while you wait | 3 | `dia-followup-timeline` |
| 11 | If it comes back | 3 | `dia-recurrence-options` |
| 12 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |

### `/tumors/high-grade-glioma`

5902 words, 12 required, 12 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | Is it cancer? What does its grade mean? | 3 | `dia-grade-ladder` |
| 2 | Where does it grow, and why does it cause these symptoms? | 3 | `dia-brain-regions` |
| 3 | `[MECHANISM]` | 3 | `dia-mechanism-pressure` |
| 4 | What symptoms does it cause? | 3 | `dia-symptom-map` |
| 5 | `[ESCALATION]` | 3 | `dia-escalation-card` |
| 6 | How do doctors find out it is this? | 4 | `pd-scan-high-grade-glioma` |
| 7 | What do the words on my report mean? | 2 | `doc-pathology-parts` |
| 8 | How is it usually treated? | 3 | `dia-treatment-path` |
| 9 | What is treatment actually like, and what is normal afterwards? | 1 | `ph-recovery-bed` |
| 10 | Follow-up scans, and what to do while you wait | 3 | `dia-followup-timeline` |
| 11 | If it comes back, or changes | 3 | `dia-recurrence-options` |
| 12 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |

### `/tumors/low-grade-glioma`

5241 words, 11 required, 11 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | Is it cancer? What does the grade mean? | 3 | `dia-grade-ladder` |
| 2 | Where does it grow, and why does it cause these symptoms? | 3 | `dia-brain-regions` |
| 3 | What symptoms does it cause? | 3 | `dia-symptom-map` |
| 4 | `[ESCALATION]` | 3 | `dia-escalation-card` |
| 5 | How do doctors find out it is this? | 4 | `pd-scan-low-grade-glioma` |
| 6 | What do the words on my report mean? | 2 | `doc-pathology-parts` |
| 7 | How is it usually treated? | 3 | `dia-treatment-path` |
| 8 | What is treatment actually like, and what is normal afterwards? | 1 | `ph-recovery-bed` |
| 9 | Follow-up scans, and what to do while you wait | 3 | `dia-followup-timeline` |
| 10 | If it comes back, or changes | 3 | `dia-recurrence-options` |
| 11 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |

### `/tumors/medulloblastoma`

5223 words, 11 required, 11 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | Is it cancer? What does its grade mean? | 3 | `dia-grade-ladder` |
| 2 | Where does it grow, and why does it cause these symptoms? | 3 | `dia-region-cerebellum` |
| 3 | `[MECHANISM]` | 3 | `dia-mechanism-pressure` |
| 4 | What symptoms does it cause? | 3 | `dia-symptom-map` |
| 5 | `[ESCALATION]` | 3 | `dia-escalation-card` |
| 6 | `[SPINAL-CORD]` | 3 | `dia-csf-spread` |
| 7 | How do doctors find out it is this? | 4 | `pd-scan-medulloblastoma` |
| 8 | What do the words on my report mean? | 2 | `doc-pathology-parts` |
| 9 | How is it usually treated? | 3 | `dia-treatment-path` |
| 10 | Follow-up scans, and what to do while you wait | 3 | `dia-followup-timeline` |
| 11 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |

### `/tumors/meningioma`

7172 words, 15 required, 15 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | Is it cancer? What does its grade mean? | 3 | `dia-grade-ladder` |
| 2 | Where does it grow, and why does it cause these symptoms? | 3 | `dia-region-meninges` |
| 3 | `[MECHANISM]` | 3 | `dia-mechanism-pressure` |
| 4 | What symptoms does it cause? | 3 | `dia-symptom-map` |
| 5 | `[ESCALATION]` | 3 | `dia-escalation-card` |
| 6 | How do doctors find out it is this? | 4 | `pd-scan-meningioma` |
| 7 | What do the words on my report mean? | 2 | `doc-pathology-parts` |
| 8 | `[CROSSWALK]` | 3 | `dia-crosswalk-names` |
| 9 | How is it usually treated? | 3 | `dia-treatment-path` |
| 10 | What is treatment actually like, and what is normal afterwards? | 1 | `ph-recovery-bed` |
| 11 | Everyday life: work, driving, seizures and tiredness | 1 | `ph-car-keys` |
| 12 | Follow-up scans, and what to do while you wait | 3 | `dia-followup-timeline` |
| 13 | If it comes back, or changes | 3 | `dia-recurrence-options` |
| 14 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |
| 15 | What to ask your team | 2 | `doc-questions-sheet` |

### `/tumors/oligodendroglioma`

5883 words, 12 required, 12 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | Is it cancer? What does its grade mean? | 3 | `dia-grade-ladder` |
| 2 | Where does it grow, and why does it cause these symptoms? | 3 | `dia-region-frontal` |
| 3 | `[MECHANISM]` | 3 | `dia-mechanism-pressure` |
| 4 | What symptoms does it cause? | 3 | `dia-symptom-map` |
| 5 | `[ESCALATION]` | 3 | `dia-escalation-card` |
| 6 | How do doctors find out it is this? | 4 | `pd-scan-oligodendroglioma` |
| 7 | What do the words on my report mean? | 2 | `doc-pathology-parts` |
| 8 | How is it usually treated? | 3 | `dia-treatment-path` |
| 9 | What is treatment actually like, and what is normal afterwards? | 1 | `ph-recovery-bed` |
| 10 | Follow-up scans, and what to do while you wait | 3 | `dia-followup-timeline` |
| 11 | If it comes back, or changes | 3 | `dia-recurrence-options` |
| 12 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |

### `/tumors/pediatric-brain-tumor`

6372 words, 13 required, 13 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | Where it grows, and why that causes symptoms | 3 | `dia-brain-regions` |
| 2 | `[MECHANISM]` | 3 | `dia-mechanism-pressure` |
| 3 | What symptoms look like in a child | 3 | `dia-symptom-map` |
| 4 | `[ESCALATION]` | 3 | `dia-escalation-card` |
| 5 | `[SPINAL-CORD]` | 3 | `dia-csf-spread` |
| 6 | How doctors find out | 4 | `pd-scan-pediatric-brain-tumor` |
| 7 | What the words on the report mean | 2 | `doc-pathology-parts` |
| 8 | How treatment is different for a child | 3 | `dia-treatment-path` |
| 9 | What is treatment actually like, and what is normal afterwards? | 1 | `ph-recovery-bed` |
| 10 | `[POSTERIOR-FOSSA-SYNDROME]` | 3 | `dia-posterior-fossa` |
| 11 | Follow-up scans, and the check-ups that come with them | 3 | `dia-followup-timeline` |
| 12 | If it comes back | 3 | `dia-recurrence-options` |
| 13 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |

### `/tumors/pituitary-tumor`

4199 words, 9 required, 9 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | Is it cancer? What does its grade mean? | 3 | `dia-grade-ladder` |
| 2 | Where does it grow, and why does it cause these symptoms? | 3 | `dia-region-sellar` |
| 3 | What symptoms does it cause? | 3 | `dia-symptom-map` |
| 4 | `[ESCALATION]` | 3 | `dia-escalation-card` |
| 5 | How do doctors find out it is this? | 4 | `pd-scan-pituitary-tumor` |
| 6 | What do the words on my report mean? | 2 | `doc-pathology-parts` |
| 7 | How is it usually treated? | 3 | `dia-treatment-path` |
| 8 | Follow-up scans, and what to do while you wait | 3 | `dia-followup-timeline` |
| 9 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |

### `/tumors/spinal-cord-tumor`

5278 words, 11 required, 11 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | Is it cancer? What does its grade mean? | 3 | `dia-grade-ladder` |
| 2 | Where does it grow, and why does it cause these symptoms? | 3 | `dia-region-spinal-cord` |
| 3 | What symptoms does it cause? | 3 | `dia-symptom-map` |
| 4 | `[ESCALATION]` | 3 | `dia-escalation-card` |
| 5 | How do doctors find out it is this? | 4 | `pd-scan-spinal-cord-tumor` |
| 6 | What do the words on my report mean? | 2 | `doc-pathology-parts` |
| 7 | How is it usually treated? | 3 | `dia-treatment-path` |
| 8 | What is treatment actually like, and what is normal afterwards? | 1 | `ph-recovery-bed` |
| 9 | Follow-up scans, and what to do while you wait | 3 | `dia-followup-timeline` |
| 10 | If it comes back, or changes | 3 | `dia-recurrence-options` |
| 11 | `[CAREGIVER]` | 1 | `ph-two-people-talking` |

### `/where-your-tumor-is`

5946 words, 12 required, 14 listed.

| # | Goes after | Kind | Figure |
|---|---|---|---|
| 1 | What "where it is" tells you, and what it does not | 3 | `dia-brain-regions` |
| 2 | The regions, in plain words and in your report's words | 3 | `dia-region-names` |
| 3 | The front of your brain | 3 | `dia-region-frontal` |
| 4 | The side of your brain, near your ear | 3 | `dia-region-temporal` |
| 5 | The upper back part of your brain | 3 | `dia-region-parietal` |
| 6 | The back of your brain | 3 | `dia-region-occipital` |
| 7 | Low at the back, under everything else | 3 | `dia-region-cerebellum` |
| 8 | The stalk the brain sits on | 3 | `dia-region-brainstem` |
| 9 | Deep in the middle, where the fluid runs | 3 | `dia-region-ventricles` |
| 10 | The floor of the skull | 3 | `dia-region-skull-base` |
| 11 | Behind your nose, at the base of your brain | 3 | `dia-region-sellar` |
| 12 | When where it sits makes it urgent: the fluid | 4 | `pd-hydrocephalus-ct` |
| 13 | `[ESCALATION]` | 3 | `dia-escalation-card` |
| 14 | When what changed is your sight, and what that means for driving | 3 | `dia-visual-field` |

---

## 7. The pages with no slots

| Page | Words | Why none |
|---|---|---|
| `/about` | 244 | who runs the site and why. No care content, and a picture here is decoration. |
| `/digest` | 139 | a sign-up page. One form, and nothing on it to illustrate. |
| `/how-we-write` | 539 | the editorial rules. A reader who gets here wants the rules, not a photograph. |
| `/privacy` | 413 | a legal page. A picture on it reads as reassurance the text has not earned. |
| `/terms` | 282 | a legal page, same reason. |

A page added to the corpus is **not** in this set. It gets slots, and the guard fails
until this file gives it some.

