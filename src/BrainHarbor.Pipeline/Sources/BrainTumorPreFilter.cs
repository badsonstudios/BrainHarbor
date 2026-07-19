using System.Text.RegularExpressions;

namespace BrainHarbor.Pipeline.Sources;

/// <summary>
/// WI-204: cheap hard rules that drop obvious non-matches BEFORE Claude sees
/// them (PLAN.md §5). This exists to save tokens and reduce noise, not to make
/// editorial calls — the real relevance decision is the M3 classifier and the
/// human review gate.
///
/// **The bias is toward KEEPING.** A wrongly-dropped study is invisible to
/// patients forever; a wrongly-kept one costs a few tokens and gets filtered
/// downstream. Two rules of thumb learned the hard way here:
///
///   * Never end a keep-list alternative with \b — these are word PREFIXES,
///     so plurals and suffixes ("metastases", "tumors", "IDH-mutated") must
///     still match.
///   * Never assume a literal space — "brain-tumor" and "brain tumour" are
///     both common, so multi-word terms use [\s\-]+.
///
/// Both mistakes silently dropped real brain-tumor research before they were
/// caught. Any new rule needs a regression test with a real-looking title.
/// </summary>
public static partial class BrainTumorPreFilter
{
    /// <summary>
    /// If any of these appear, the item stays. Deliberately broad: it covers
    /// the tumor types, the vocabulary of brain involvement from other
    /// cancers ("brain mets", "CNS disease", "leptomeningeal"), and the
    /// treatment/marker terms that only show up in this field.
    /// </summary>
    [GeneratedRegex(
        // tumor types
        @"\b(brain[\s\-]+(tumou?r|neoplasm|cancer|met|mets|metasta|lesion)|" +
        @"glioma|glioblastoma|gliosarcoma|gbm|astrocytoma|oligodendroglioma|" +
        @"meningioma|medulloblastoma|ependymoma|craniopharyngioma|" +
        @"schwannoma|chordoma|hemangioblastoma|pineoblastoma|germinoma|atrt|" +
        @"acoustic neuroma|neurofibromatosis|dipg|" +
        @"diffuse midline glioma|pituitary[\s\-]+(tumou?r|adenoma)|pitnet|" +
        // CNS phrasing, incl. how other cancers describe brain involvement
        @"cns[\s\-]+(tumou?r|neoplasm|lymphoma|involvement|disease|metasta|malignan|progression)|" +
        @"central nervous system[\s\-]+(tumou?r|neoplasm|malignan|metasta)|" +
        @"intracranial[\s\-]+(tumou?r|neoplasm|metasta|response|disease|progression)|" +
        @"leptomeningeal|spinal cord[\s\-]+(tumou?r|neoplasm)|" +
        @"neuro-?oncolog|" +
        // markers and treatments specific to this field
        @"idh-?(mutant|mutat|wildtype|wild-type)|mgmt|h3\s*k27|1p/19q|" +
        @"temozolomide|vorasidenib|lomustine|craniotomy|craniospinal|" +
        @"tumou?r[\s\-]+treating fields|whole[\s\-]+brain (radiat|radiother))\w*",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex BrainTumorTerms();

    /// <summary>
    /// Wrong-organ cancers. Only decisive when NO brain-tumor term is present,
    /// because "breast cancer brain metastases" is very much our subject.
    /// </summary>
    [GeneratedRegex(
        @"\b(breast|prostate|colorectal|colon|pancreatic|hepatocellular|gastric|" +
        @"ovarian|bladder|renal cell|thyroid|melanoma|leukemi|myeloma)\w*.{0,40}?" +
        @"\b(cancer|carcinoma|tumou?r|neoplasm)\w*",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex WrongOrganCancer();

    /// <summary>
    /// Non-tumor neurology. Kept narrow on purpose: broad terms like "stroke",
    /// "depression" and "dementia" were dropping legitimate late-effects
    /// research (cognitive decline after whole-brain radiotherapy, stroke risk
    /// after cranial irradiation), which is exactly what this audience reads.
    /// </summary>
    [GeneratedRegex(
        @"\b(alzheimer|parkinson|multiple sclerosis|amyotrophic lateral|" +
        @"huntington|migraine|schizophren|autism|" +
        @"ischemic stroke|thrombectomy|thrombolysis|" +
        @"traumatic brain injur|cerebral aneurysm)\w*",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex UnrelatedNeurology();

    /// <summary>
    /// Publication notices — nothing for a patient to read. Requires the
    /// notice punctuation ("Erratum:", "Retraction —") so ordinary titles
    /// beginning with these words survive. "Response to" and "Withdrawal"
    /// are NOT here: they open real papers ("Response to bevacizumab in
    /// recurrent high-grade glioma", "Withdrawal of antiepileptic drugs
    /// after brain tumour surgery").
    /// </summary>
    [GeneratedRegex(
        @"^\s*(erratum|corrigendum|retraction|retracted|" +
        @"(author|publisher) correction|expression of concern)\s*[:.–—\-]",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PublicationNotice();

    /// <summary>Front-matter with no article behind it at all.</summary>
    [GeneratedRegex(
        @"^\s*(editorial board|table of contents|contents|index|" +
        @"acknowledgment|acknowledgement|in this issue|masthead|" +
        @"information for authors|issue information)\s*[:.]?\s*$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex FrontMatter();

    /// <summary>
    /// True when the item should never reach the classifier. Callers pass the
    /// title and (optionally) the abstract.
    /// </summary>
    public static bool ShouldExclude(string title, string? rawSummary = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return true;
        }

        // Notices and front matter are junk regardless of subject — these are
        // the only rules the keep-list can't override.
        if (PublicationNotice().IsMatch(title) || FrontMatter().IsMatch(title))
        {
            return true;
        }

        var text = rawSummary is null ? title : $"{title}\n{rawSummary}";

        // Any brain-tumor term wins. This escape hatch is what makes the two
        // subject rules below safe to apply at all.
        if (BrainTumorTerms().IsMatch(text))
        {
            return false;
        }

        return WrongOrganCancer().IsMatch(text) || UnrelatedNeurology().IsMatch(text);
    }
}
