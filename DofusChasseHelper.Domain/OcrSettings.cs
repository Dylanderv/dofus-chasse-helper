namespace DofusChasseHelper.Domain;

public record OcrSettings(
    bool IsHuntBoxInTopLeftCorner,
    IReadOnlyCollection<string> HeaderTexts,
    IReadOnlyCollection<string> FooterTexts,
    IReadOnlyCollection<string> CurrentTexts,
    IReadOnlyCollection<string> StartTexts,
    IReadOnlyCollection<string> CurrentPositionTexts
);
