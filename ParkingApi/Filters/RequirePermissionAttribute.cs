using System;

namespace ParkingApi.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class RequirePermissionAttribute : Attribute
{
    public string Slug { get; }

    public RequirePermissionAttribute(string slug)
    {
        Slug = slug;
    }
}
