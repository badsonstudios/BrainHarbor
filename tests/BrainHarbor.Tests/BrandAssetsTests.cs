using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// The logo kit (docs/design/entry-hub-handoff/brand/README.md), wired into the
/// shell. Two things worth a test: a brand path that 404s is invisible — the
/// page still renders, just without a logo or a favicon — and the alt text is
/// an accessibility rule the kit states explicitly ("Brain Harbor", never
/// "logo"), which is exactly the sort of thing a later edit quietly undoes.
/// </summary>
[Collection("Database")]
public class BrandAssetsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public BrandAssetsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));
    }

    [Fact]
    [Trait("Category", "Database")]
    public async Task TheHeaderShowsTheLockupWithTheBrandNameAsItsAltText()
    {
        var html = await _factory.CreateClient().GetStringAsync("/");

        // The path carries a content fingerprint (.NET rewrites ~/ asset URLs
        // for cache-busting), so match around it rather than on the bare name.
        Assert.Matches(@"/img/brand/lockup-no-tagline(\.[a-z0-9]+)?\.svg", html);
        Assert.Contains("alt=\"Brain Harbor\"", html, StringComparison.Ordinal);

        // "logo" as alt text describes the medium, not the thing — a screen
        // reader should hear the name a sighted reader reads.
        Assert.DoesNotContain("alt=\"logo\"", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Database")]
    public async Task TheIconsAndManifestAreDeclared()
    {
        var html = await _factory.CreateClient().GetStringAsync("/");

        Assert.Contains("/img/brand/favicon.svg", html, StringComparison.Ordinal);
        Assert.Contains("/img/brand/favicon-32.png", html, StringComparison.Ordinal);
        Assert.Contains("rel=\"apple-touch-icon\"", html, StringComparison.Ordinal);
        Assert.Contains("/site.webmanifest", html, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Category", "Database")]
    public async Task ASharedLinkUnfurlsWithAnAbsoluteImageUrl()
    {
        // og:image does not accept a relative path — a bare "/img/..." is
        // silently ignored and the link unfurls as a grey box.
        var html = await _factory.CreateClient().GetStringAsync("/");

        var og = Regex.Match(html, @"<meta property=""og:image"" content=""([^""]+)""");
        Assert.True(og.Success, "no og:image tag");
        Assert.StartsWith("http", og.Groups[1].Value, StringComparison.Ordinal);
        Assert.EndsWith("/img/brand/lockup-1600.png", og.Groups[1].Value, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Category", "Database")]
    public async Task EveryBrandAssetTheShellAsksForIsActuallyServed()
    {
        // The failure this catches: a renamed or mistyped file. The page still
        // renders perfectly, so nothing looks wrong until someone notices the
        // header is empty.
        var client = _factory.CreateClient();
        var html = await client.GetStringAsync("/");

        var referenced = Regex.Matches(html, @"/img/brand/[A-Za-z0-9._-]+")
            .Select(m => m.Value)
            .Append("/site.webmanifest")
            .Distinct(StringComparer.Ordinal)
            .ToList();

        Assert.NotEmpty(referenced);

        foreach (var path in referenced)
        {
            var response = await client.GetAsync(path);
            Assert.True(response.IsSuccessStatusCode, $"{path} returned {(int)response.StatusCode}");
        }
    }

    [Fact]
    [Trait("Category", "Database")]
    public async Task TheManifestPointsAtIconsThatExist()
    {
        var client = _factory.CreateClient();
        var manifest = await client.GetStringAsync("/site.webmanifest");

        foreach (Match icon in Regex.Matches(manifest, @"""src""\s*:\s*""([^""]+)"""))
        {
            var response = await client.GetAsync(icon.Groups[1].Value);
            Assert.True(response.IsSuccessStatusCode,
                $"manifest icon {icon.Groups[1].Value} returned {(int)response.StatusCode}");
        }
    }

    /// <summary>
    /// The home-screen icons must have no TRANSPARENT PIXELS. iOS paints alpha
    /// in an apple-touch-icon black, so a rounded transparent icon shows black
    /// wedges in the corners; Android maskable icons expect opaque full-bleed
    /// art. The brand README calls this out, and it is invisible until it ships
    /// to somebody's phone.
    ///
    /// These files DO carry an alpha channel (PNG colour type 6), so the
    /// channel's presence proves nothing either way — the pixels have to be
    /// read. Only the top scanline is decoded: rounding shows at the corners,
    /// row 0 holds two of them, and row 0 is the one row that needs no
    /// previous-row state to unfilter.
    /// </summary>
    [Theory]
    [InlineData("apple-touch-icon-180.png")]
    [InlineData("icon-192.png")]
    [InlineData("icon-512.png")]
    public void TheHomeScreenIconsHaveNoTransparentPixelsAlongTheTopEdge(string file)
    {
        var path = Path.Combine(
            RepoRoot(), "src", "BrainHarbor.Web", "wwwroot", "img", "brand", file);
        Assert.True(File.Exists(path), $"{path} is missing");

        var (width, alphas) = DecodeFirstRowAlpha(path);

        Assert.Equal(width, alphas.Count);
        var transparent = alphas.Count(a => a != 255);
        Assert.True(transparent == 0,
            $"{file}: {transparent} of {width} pixels on the top edge are not fully opaque — " +
            "iOS will paint those corners black");
    }

    /// <summary>
    /// Minimal PNG reader for the one case these icons are in: 8-bit RGBA,
    /// non-interlaced. Returns the alpha byte of every pixel in row 0.
    /// </summary>
    private static (int Width, List<byte> Alpha) DecodeFirstRowAlpha(string path)
    {
        var bytes = File.ReadAllBytes(path);

        int ReadInt(int at) => (bytes[at] << 24) | (bytes[at + 1] << 16) | (bytes[at + 2] << 8) | bytes[at + 3];

        var width = ReadInt(16);
        Assert.Equal(8, bytes[24]);                          // bit depth
        Assert.Equal(6, bytes[25]);                          // RGBA
        Assert.Equal(0, bytes[28]);                          // not interlaced

        // Concatenate the IDAT chunks, then inflate.
        var compressed = new MemoryStream();
        var offset = 8;
        while (offset + 8 <= bytes.Length)
        {
            var length = ReadInt(offset);
            var type = System.Text.Encoding.ASCII.GetString(bytes, offset + 4, 4);
            if (type == "IDAT")
            {
                compressed.Write(bytes, offset + 8, length);
            }
            if (type == "IEND")
            {
                break;
            }
            offset += 12 + length;                           // length + type + data + CRC
        }

        compressed.Position = 0;
        using var inflate = new System.IO.Compression.ZLibStream(
            compressed, System.IO.Compression.CompressionMode.Decompress);

        const int bytesPerPixel = 4;
        var stride = width * bytesPerPixel;
        var row = new byte[stride];
        var filter = inflate.ReadByte();
        inflate.ReadExactly(row);

        // Unfilter row 0. Up/Paeth reference the previous row, which is all
        // zeroes here, so both reduce to their left-pixel terms.
        for (var i = 0; i < stride; i++)
        {
            var left = i >= bytesPerPixel ? row[i - bytesPerPixel] : 0;
            row[i] = filter switch
            {
                0 or 2 => row[i],                            // None, Up (prior row = 0)
                1 or 4 => (byte)(row[i] + left),             // Sub, Paeth (prior row = 0)
                3 => (byte)(row[i] + left / 2),              // Average
                _ => throw new InvalidOperationException($"unexpected PNG filter {filter}"),
            };
        }

        var alpha = new List<byte>(width);
        for (var x = 0; x < width; x++)
        {
            alpha.Add(row[(x * bytesPerPixel) + 3]);
        }
        return (width, alpha);
    }

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "BrainHarbor.slnx")))
        {
            dir = dir.Parent!;
        }
        return dir?.FullName ?? throw new InvalidOperationException("repo root not found");
    }
}
