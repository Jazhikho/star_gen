# Source review procedure

Standard steps when adding or reviewing a scientific source for StarGen. Keeps [AnnotatedBibliography.md](AnnotatedBibliography.md) and [ToReview.md](ToReview.md) consistent and ensures we record whether each paper **supports** current implementation, **requires changes**, or offers **fidelity improvements**.

---

## When a new source is introduced

1. **Identify the source**  
   Add it to [ToReview.md](ToReview.md) in the appropriate topic section (stellar IMF, solar neighborhood density, exoplanet demographics, orbital stability, or a new section). Include: title, authors, year, link (arXiv/DOI), short “Covers” note from the abstract, and “Abstract only” or “Full text available” as applicable.

2. **Obtain full text**  
   When you have PDF or HTML, extract plain text (e.g. `pdftotext paper.pdf`) or paste from an HTML version. Do **not** commit binary PDFs to the repo.

3. **Create the Texts/ copy**  
   Save as `Sources/Texts/AuthorYear.txt` (e.g. `Li2023.txt`, `Bovy2017.txt`). Use first author surname and publication year; if duplicate year, add disambiguator (e.g. `Chabrier2003a.txt`).

4. **Abridge if needed**  
   Long papers may be abridged. See “What to include from the paper” below. **Every abridged file must have an abridgement note at the top** stating that it is an abridgement and that the abridgement does not misrepresent the paper.

5. **Update AnnotatedBibliography.md**  
   Add the source in the relevant topic table(s). For each entry provide:
   - **Citation:** Full APA 7 reference and link to `Texts/AuthorYear.txt` if full text is in repo.
   - **Summary:** What the paper gives (numbers, equations, conclusions) in one or two sentences.
   - **Supports / requires changes / fidelity:**  
     - **Supports:** Current StarGen code or benchmarks are consistent with the paper; no change required.  
     - **Requires changes:** The paper contradicts or narrows our assumptions; describe what should be altered.  
     - **Fidelity:** Paper supports current use but offers ways to increase accuracy (e.g. metallicity-dependent IMF, scale heights). Note concrete options and where they could be used (e.g. “calibration mode”, “documentation”).
   - **Used in:** Code paths or docs that rely on or could cite this source (e.g. `ScientificBenchmarks.gd`, `SubSectorGenerator.gd`).

6. **Update ToReview.md**  
   For the same paper, change the Note column to **Reviewed**, add the path to `Texts/AuthorYear.txt`, and point to the relevant section(s) in AnnotatedBibliography.

7. **Update ProjectStructure.md**  
   If you added a new topic section or new file under `Sources/`, list it in [Docs/ProjectStructure.md](../Docs/ProjectStructure.md) under the `Sources/` tree.

---

## What to include from the paper (Texts/ copy)

- **Always include (verbatim or near-verbatim):**
  - Title, authors, year, venue/arXiv.
  - Full abstract.
  - All key quantitative results: central values, uncertainties, units (e.g. 0.040 ± 0.002 M☉/pc³, α = 1.9–2.5, dα/d[M/H] ≈ 0.5).
  - Any equations we might cite or implement (e.g. dn/dM, stability criteria).
  - Main conclusions that affect StarGen (e.g. “IMF varies with metallicity”, “scale height 50–150 pc”).

- **Summarize rather than transcribe:**
  - Lengthy methodology (e.g. selection function derivation, MCMC details).
  - Extended literature review.
  - Full figure captions (keep the scientific content; omit boilerplate).
  - Long reference lists (one-line summary, e.g. “Refs 1–28 main text; 29–59 methods”).

- **Abridgement note (required for abridged files):**  
  At the top of the file, state that the document is an abridgement of the published work, that certain sections are summarized, and that the abridgement is intended not to misrepresent the paper. Point readers to the journal article or arXiv for full methodology, figures, and tables.

---

## Consistency and fidelity check

For each reviewed source, answer:

1. **Does the paper support what we currently do?**  
   If yes, say so in the bibliography (“Supports: …”). If no, note what conflicts and what would need to change (“Requires changes: …”).

2. **Is there room to increase fidelity without breaking current behavior?**  
   Examples: document a number we don’t yet use (e.g. mass density 0.040 M☉/pc³), add an optional calibration (e.g. metallicity-dependent M-dwarf fraction), or cite the paper in code comments. Record these under “Fidelity” in the bibliography.

3. **Where is this (or could this be) used in the project?**  
   Fill the “Used in” column so we can trace assumptions to code and docs.

---

## Citation style

Use **APA 7 author-date** as in [AnnotatedBibliography.md](AnnotatedBibliography.md): in-text `(Author, Year)`; reference list “Author, A. A. (Year). *Title*. *Journal*, *Volume*(Issue), pages. URL.”

---

## File checklist (new source)

- [ ] Entry in [ToReview.md](ToReview.md) (or already present).
- [ ] Plain-text copy in `Sources/Texts/AuthorYear.txt`; if abridged, abridgement note at top.
- [ ] Row(s) in [AnnotatedBibliography.md](AnnotatedBibliography.md) with citation, summary, Supports/Requires/Fidelity, and Used in.
- [ ] ToReview.md note set to “Reviewed” with link to Texts/ and AnnotatedBibliography section.
- [ ] [Docs/ProjectStructure.md](../Docs/ProjectStructure.md) updated if Sources/ structure changed.
- [ ] Changelog in AnnotatedBibliography updated (optional but recommended).
