namespace OrbitAOS.V6.Web.Models;

/// <summary>
/// View model for displaying error information on the error page.
/// </summary>
public class ErrorViewModel
{
    /// <summary>Gets or sets the request identifier for tracing purposes.</summary>
    public string? RequestId { get; set; }

    /// <summary>Gets a value indicating whether the request ID should be displayed.</summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
