using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace Minglouguli.DocView;

public class UseDocViewUIOption
{
    public Action<RouteHandlerBuilder> RouteHandler { get; set; }
}

public static class DocViewUIExtensions
{
    public static WebApplication UseDocViewUI(this WebApplication app,Action<UseDocViewUIOption> action=null)
    {
        var opt = new UseDocViewUIOption();
       var routeHandler= app.MapGet("/docview/api/entityjson", async ([FromServices] DocViewService docViewService) =>
        {
            return await docViewService.GetEntityJsonData();
        });
        action?.Invoke(opt);
        if (opt.RouteHandler != null)
        {
            opt.RouteHandler.Invoke(routeHandler);
        }

        var fileServerOptions = new FileServerOptions
        {
            EnableDirectoryBrowsing = false,
            EnableDefaultFiles = true,
            RequestPath = "/docview"
        };
        fileServerOptions.StaticFileOptions.FileProvider = new PhysicalFileProvider(Path.Combine(AppContext.BaseDirectory, "DocViewUI"));
        fileServerOptions.DefaultFilesOptions.DefaultFileNames = new List<string> { "index.html" };


        //var rewriteOptions = new RewriteOptions()
        // .AddRewrite(@"/docview/entity", "/docview", true)
        //;
        //app.UseRewriter(rewriteOptions);

        app.UseFileServer(fileServerOptions);


        return app;
    }
}
