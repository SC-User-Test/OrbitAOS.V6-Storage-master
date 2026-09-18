namespace OrbitAOS.V6.Models;

/// <summary>
/// View model for the error page, carrying the request trace identifier.
/// </summary>
public class ErrorViewModel
{
    /// <summary>Gets or sets the current request ID for diagnostic purposes.</summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Gets a value indicating whether the request ID should be displayed.
    /// Returns <c>true</c> when <see cref="RequestId"/> is non-empty.
    /// </summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
