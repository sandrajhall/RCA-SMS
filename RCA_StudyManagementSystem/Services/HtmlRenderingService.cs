using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;

namespace RCA_StudyManagementSystem.Services
{

    public interface IHtmlRenderingService
    {
        Task<string> RenderComponentToHtml<TComponent>(Dictionary<string, object> parameters) where TComponent : IComponent;
    }

    public class HtmlRenderingService : IHtmlRenderingService
    {
        private readonly HtmlRenderer _renderer;

        public HtmlRenderingService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            _renderer = new HtmlRenderer(serviceProvider, loggerFactory);
        }

        public async Task<string> RenderComponentToHtml<TComponent>(Dictionary<string, object> parameters) where TComponent : IComponent
        {
            var html = await _renderer.Dispatcher.InvokeAsync(async () =>
            {
                var parameterView = ParameterView.FromDictionary(parameters);
                var output = await _renderer.RenderComponentAsync<TComponent>(parameterView);
                return output.ToHtmlString();
            });

            return html;
        }
    }
}
