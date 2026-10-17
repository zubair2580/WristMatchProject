namespace WristMatch.Application;

public sealed record LookMatchScore(string Concern, string Region, double? RawScore, int? UiScore, double? Score);
public sealed record LookMatchProduct(Guid Id, string Name, string Slug, string Colour, string Material, string ImageUrl, decimal Price, string Currency, int MatchScore, string Reason);
public sealed record LookMatchResult(
    string ProviderMode,
    string Undertone,
    IReadOnlyList<string> PreferredMetals,
    IReadOnlyList<string> PreferredColours,
    IReadOnlyList<LookMatchScore> Scores,
    IReadOnlyList<LookMatchProduct> Recommendations,
    string Summary);

public interface ILookMatchService
{
    Task<LookMatchResult> AnalyzeAsync(Guid sessionId, Stream facePhoto, string contentType, long byteSize, bool consent, string consentVersion, CancellationToken cancellationToken);
}
