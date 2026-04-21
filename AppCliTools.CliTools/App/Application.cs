using Microsoft.Extensions.Options;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliTools.App;

public class Application : IApplication
{
    public Application(IOptions<ApplicationOptions> options)
    {
        AppName = options.Value.AppName;
    }

    public string AppName { get; }
}
