# Recorded ClinicalTrials.gov v2 responses (WI-402)

Captured verbatim from the live registry so the fetcher is tested against the
shape the API actually returns, not one we imagined. (The WI-205 lesson: a
fetcher can pass hand-written fixtures and still be wrong about the real API.)

**One edit is made to each recording:** the per-site `contacts`,
`centralContacts` and `overallOfficials` arrays are deleted. They carry real
investigators' names, direct phone numbers and email addresses, and this
repository is public. The mapper never reads those fields, so removing them
costs no fidelity.

| File | What it is |
|---|---|
| `studies-page1.json` | First page of recruiting glioblastoma trials, oldest update first |
| `studies-page2.json` | The genuine continuation of page 1, fetched with its `nextPageToken` |
| `studies-closed.json` | Completed trials — the records that refresh the cache but earn no feed item |
| `studies-empty.json` | A query with no matches, i.e. what the end of paging looks like |
