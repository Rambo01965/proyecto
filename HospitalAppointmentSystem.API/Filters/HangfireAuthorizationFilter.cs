using Hangfire.Dashboard;

namespace HospitalAppointmentSystem.API.Filters;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // In development, allow all access
        // In production, you should implement proper authorization
        var httpContext = context.GetHttpContext();
        
        // For development purposes, allow all access
        // TODO: Implement proper authorization in production
        return true;
        
        // Example of how to restrict access:
        // return httpContext.User.Identity.IsAuthenticated && 
        //        httpContext.User.IsInRole("Admin");
    }
}
