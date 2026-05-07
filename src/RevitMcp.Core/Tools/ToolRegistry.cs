namespace RevitMcp.Core.Tools;

public sealed class ToolRegistry
{
    private readonly Dictionary<string, IRevitMcpTool> _tools;

    public ToolRegistry(IEnumerable<IRevitMcpTool> tools)
    {
        _tools = new Dictionary<string, IRevitMcpTool>(StringComparer.Ordinal);

        foreach (var tool in tools)
        {
            if (!_tools.TryAdd(tool.Name, tool))
            {
                throw new InvalidOperationException($"Tool '{tool.Name}' is already registered.");
            }
        }
    }

    public IReadOnlyCollection<IRevitMcpTool> List() => _tools.Values.ToArray();

    public IRevitMcpTool Get(string name)
    {
        return _tools.TryGetValue(name, out var tool)
            ? tool
            : throw new InvalidOperationException($"Tool '{name}' is not registered.");
    }

    public bool TryGet(string name, out IRevitMcpTool tool)
    {
        return _tools.TryGetValue(name, out tool!);
    }

    public static IReadOnlyCollection<IRevitMcpTool> CreateDefaultTools()
    {
        return new IRevitMcpTool[]
        {
            new DocumentInfoTool(),
            new SelectionGetTool(),
            new ElementsFindTool(),
            new ElementParametersGetTool(),
            new ElementParameterSetTool(),
            new WallCreateLineTool()
        };
    }
}
