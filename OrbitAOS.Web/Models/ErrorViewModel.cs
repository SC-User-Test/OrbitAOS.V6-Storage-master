namespace OrbitAOS.Web.Models
{
    /// <summary>
    /// View model for the Error page.
    /// Migrated from ASP.NET MVC 5 to ASP.NET Core MVC on .NET 8.
    /// Nullable reference types enabled (net8.0 default).
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>Gets or sets the request identifier for diagnostics.</summary>
        public string? RequestId { get; set; }

        /// <summary>Gets a value indicating whether the request ID should be displayed.</summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
