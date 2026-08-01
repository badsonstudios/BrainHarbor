using System.Globalization;
using BrainHarbor.Web.Models;
using Microsoft.AspNetCore.Html;

namespace BrainHarbor.Web.Feed;

/// <summary>
/// The card hero: a vetted photo backdrop (chosen by content — see
/// <see cref="CardImages"/>) with the item's readiness score laid over it as a
/// dial. The photo is picked from a small human-reviewed pool, never generated,
/// so it can't invent a misleading medical image; the dial sits on a dark disc
/// so its number is legible on any photo. Decorative — rendered aria-hidden.
/// </summary>
public static class CardArt
{
    public static IHtmlContent Hero(FeedCard card, string? imageUrl)
    {
        var bg = imageUrl is not null
            ? $"background-image:url('{imageUrl}')"
            : "background:linear-gradient(135deg,hsl(205,30%,90%),hsl(190,28%,94%))";

        var dial = card.Readiness is { } score ? Dial(score) : "";

        return new HtmlString(
            $"<div class=\"card-hero\" style=\"{bg}\"><span class=\"card-hero__scrim\"></span>{dial}</div>");
    }

    private static string Dial(int score)
    {
        const double r = 46, circ = 2 * Math.PI * r;
        var offset = (circ * (1 - Math.Clamp(score, 1, 10) / 10.0)).ToString("0.0", CultureInfo.InvariantCulture);
        var arc = score >= 7 ? "#5ec8bb" : score >= 4 ? "#7fb4e6" : "#e6b566";
        var c = circ.ToString("0.0", CultureInfo.InvariantCulture);

        return "<svg class=\"card-hero__dial\" viewBox=\"0 0 120 120\" aria-hidden=\"true\" role=\"presentation\">" +
               "<circle cx=\"60\" cy=\"60\" r=\"54\" fill=\"rgba(14,28,42,0.42)\"/>" +
               "<circle cx=\"60\" cy=\"60\" r=\"46\" fill=\"none\" stroke=\"rgba(255,255,255,0.32)\" stroke-width=\"11\"/>" +
               $"<circle cx=\"60\" cy=\"60\" r=\"46\" fill=\"none\" stroke=\"{arc}\" stroke-width=\"11\" " +
               $"stroke-linecap=\"round\" stroke-dasharray=\"{c}\" stroke-dashoffset=\"{offset}\" transform=\"rotate(-90 60 60)\"/>" +
               $"<text x=\"60\" y=\"62\" text-anchor=\"middle\" class=\"card-hero__num\">{score}</text>" +
               "<text x=\"60\" y=\"80\" text-anchor=\"middle\" class=\"card-hero__lbl\">of 10 ready</text>" +
               "</svg>";
    }
}
