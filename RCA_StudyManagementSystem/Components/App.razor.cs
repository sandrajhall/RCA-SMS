using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

public partial class App
{
    [Inject] private IHttpContextAccessor HttpContextAccessor { get; set; } = default!;

    private string BaseHref => (HttpContextAccessor.HttpContext?.Request.PathBase.Value ?? "") + "/";
}
