using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minglouguli.DocView;
public static class DocViewServiceExtensions
{
    public static IServiceCollection AddDocView(this IServiceCollection services,Action<DocViewServiceOptions> options=null)
    {
        var opt = new DocViewServiceOptions();
        options?.Invoke(opt);

        services.AddSingleton(opt);

        services.AddSingleton<DocViewService>();
        return services;
    }
}
