namespace eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.PresentationDTO;

/// <summary>
/// Represents attribution details for an icon asset displayed within the application UI.
/// </summary>
public sealed record IconCredit
{
    /// <summary>
    /// Gets or sets the name of the icon author or creator.
    /// </summary>
    public string IconCreator { get; set; } = string.Empty;

    /// <summary>
    /// Gets the image resource source path or URI for the icon.
    /// </summary>
    public string IconImageSource { get; init; } = string.Empty;

    /// <summary>
    /// Gets the external web hyperlink URL pointing to the icon's source page.
    /// </summary>
    public string IconHyperLink { get; init; } = string.Empty;

    /// <summary>
    /// Gets the user-facing text content displayed for the attribution hyperlink.
    /// </summary>
    public string IconHyperLinkContent { get; init; } = string.Empty;
}
