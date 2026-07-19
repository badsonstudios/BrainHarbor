using BrainHarbor.Pipeline.Sources;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-204: the hard-rule pre-filter. Its bias must stay toward KEEPING —
/// a wrongly-dropped study is invisible to patients forever, while a
/// wrongly-kept one costs a few tokens and gets filtered downstream.
/// </summary>
public class PubMedPreFilterTests
{
    [Theory]
    [InlineData("Vorasidenib in IDH-mutant low-grade glioma")]
    [InlineData("A phase 3 trial in glioblastoma")]
    [InlineData("Meningioma recurrence after resection")]
    [InlineData("Outcomes in diffuse midline glioma")]
    [InlineData("Temozolomide dosing schedules")]
    [InlineData("Tumor treating fields in newly diagnosed patients")]
    [InlineData("Quality of life in neuro-oncology patients")]
    [InlineData("Pediatric medulloblastoma survival trends")]
    public void KeepsBrainTumorResearch(string title)
    {
        Assert.False(PubMedPreFilter.ShouldExclude(title));
    }

    [Theory]
    [InlineData("Breast cancer brain metastases respond to a new drug")]
    [InlineData("Melanoma brain metastases and immunotherapy")]
    [InlineData("Lung cancer with intracranial tumor spread")]
    [InlineData("Brain metastasis rates after adjuvant therapy")]
    public void KeepsOtherCancersWhenTheyReachTheBrain(string title)
    {
        // The single most important keep-case: these patients are our audience.
        Assert.False(PubMedPreFilter.ShouldExclude(title));
    }

    [Theory]
    [InlineData("Glioblastomas resistant to temozolomide")]
    [InlineData("Meningiomas in older adults")]
    [InlineData("Brain tumors in adolescents")]
    [InlineData("Gliomatosis cerebri imaging")]
    public void PluralAndSuffixedFormsStillMatchTheKeepList(string title)
    {
        // A trailing \b in the keep-list regex silently failed on every plural.
        Assert.False(PubMedPreFilter.ShouldExclude(title));
    }

    [Theory]
    [InlineData("Adjuvant chemotherapy in early breast cancer")]
    [InlineData("Screening for colorectal cancer in adults over 45")]
    [InlineData("Prostate cancer active surveillance outcomes")]
    public void DropsWrongOrganCancers(string title)
    {
        Assert.True(PubMedPreFilter.ShouldExclude(title));
    }

    [Theory]
    [InlineData("Amyloid clearance in Alzheimer disease")]
    [InlineData("Deep brain stimulation for Parkinson disease")]
    [InlineData("Thrombectomy after acute ischemic stroke")]
    [InlineData("Multiple sclerosis relapse rates on new therapy")]
    public void DropsUnrelatedNeurology(string title)
    {
        Assert.True(PubMedPreFilter.ShouldExclude(title));
    }

    [Theory]
    [InlineData("Erratum: Vorasidenib in IDH-mutant glioma")]
    [InlineData("Author Correction: A trial in glioblastoma")]
    [InlineData("Retraction: early results in meningioma")]
    [InlineData("Corrigendum — glioma treatment patterns")]
    [InlineData("Editorial board")]
    [InlineData("Index")]
    [InlineData("Table of contents")]
    public void DropsNoticesAndFrontMatterEvenWhenOnTopic(string title)
    {
        Assert.True(PubMedPreFilter.ShouldExclude(title));
    }

    // ---------- regressions: titles that were WRONGLY DROPPED ----------

    [Theory]
    // The notice rule fired on ordinary titles that merely start with these words.
    [InlineData("Response to bevacizumab in recurrent high-grade glioma")]
    [InlineData("Withdrawal of antiepileptic drugs after brain tumour surgery")]
    [InlineData("Correction of hyponatremia in neurosurgical patients with brain tumours")]
    // Multi-word keep terms assumed a literal space.
    [InlineData("Anxiety and depression in brain-tumor caregivers")]
    [InlineData("Tumor-treating fields in newly diagnosed glioblastoma")]
    // Vocabulary the audience actually uses was missing from the keep list.
    [InlineData("Breast cancer brain mets: outcomes with tucatinib")]
    [InlineData("Trastuzumab deruxtecan in HER2+ breast cancer with CNS involvement")]
    [InlineData("Intracranial response in patients with breast cancer and CNS disease")]
    [InlineData("Leptomeningeal carcinomatosis from breast cancer")]
    [InlineData("Brain cancer risk among survivors of breast cancer")]
    [InlineData("Depression in survivors of childhood CNS malignancy")]
    [InlineData("Cervical spinal cord tumour resection outcomes")]
    [InlineData("IDH-mutated astrocytoma treated with vorasidenib")]
    // Over-broad neurology rules ate late-effects research.
    [InlineData("Risk of stroke after cranial irradiation in childhood cancer survivors")]
    [InlineData("Dementia and cognitive decline after whole-brain radiotherapy")]
    public void RegressionKeepsResearchThatWasPreviouslyDropped(string title)
    {
        Assert.False(PubMedPreFilter.ShouldExclude(title),
            $"'{title}' is relevant to this audience and must not be dropped");
    }

    [Fact]
    public void AnAbstractMentioningABrainTumorRescuesAnAmbiguousTitle()
    {
        const string title = "A new immunotherapy approach";
        const string abstractText =
            "We tested the approach in patients with recurrent glioblastoma.";

        Assert.True(PubMedPreFilter.ShouldExclude(title, "Patients with metastatic breast cancer were enrolled."));
        Assert.False(PubMedPreFilter.ShouldExclude(title, abstractText));
    }

    [Fact]
    public void AmbiguousTitlesAreKeptWhenNoRuleFires()
    {
        // Keep-biased: no evidence to drop means keep.
        Assert.False(PubMedPreFilter.ShouldExclude("A novel therapeutic approach in oncology"));
        Assert.False(PubMedPreFilter.ShouldExclude("Patient-reported outcomes after radiotherapy"));
    }

    [Fact]
    public void EmptyTitleIsDropped()
    {
        Assert.True(PubMedPreFilter.ShouldExclude(""));
        Assert.True(PubMedPreFilter.ShouldExclude("   "));
    }

    [Fact]
    public void MatchingIsCaseInsensitive()
    {
        Assert.False(PubMedPreFilter.ShouldExclude("GLIOBLASTOMA outcomes"));
        Assert.True(PubMedPreFilter.ShouldExclude("ERRATUM: something"));
    }
}
