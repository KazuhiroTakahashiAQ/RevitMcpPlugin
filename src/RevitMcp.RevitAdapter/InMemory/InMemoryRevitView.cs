using RevitMcp.Core.Revit;

namespace RevitMcp.RevitAdapter.InMemory;

public sealed class InMemoryRevitView : IRevitView
{
    public InMemoryRevitView(int id, string name, string viewType)
    {
        Id = id;
        Name = name;
        ViewType = viewType;
    }

    public int Id { get; }

    public string Name { get; }

    public string ViewType { get; }
}
