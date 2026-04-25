using Microsoft.Extensions.Options;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliTools.App;

public class Application(IOptions<ApplicationOptions> options) : IApplication
{
    public string AppName { get; } = options.Value.AppName;
}
