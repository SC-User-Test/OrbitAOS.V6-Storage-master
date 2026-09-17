// OrbitAOS .NET 8 - Site JavaScript
// Migrated from ASP.NET MVC on .NET Framework to ASP.NET Core MVC on .NET 8

// Add any site-wide JavaScript here.
// Example: auto-dismiss alerts after 5 seconds
document.addEventListener('DOMContentLoaded', function () {
    var alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(function (alert) {
        setTimeout(function () {
            var bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000);
    });
});
