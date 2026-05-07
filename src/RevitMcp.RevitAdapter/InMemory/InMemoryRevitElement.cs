using RevitMcp.Core.Revit;

namespace RevitMcp.RevitAdapter.InMemory;

public sealed class InMemoryRevitElement : IRevitElement
{
    private readonly Dictionary<string, InMemoryRevitParameter> _parameters;

    public InMemoryRevitElement(
        int id,
        string uniqueId,
        string category,
        string name,
        IEnumerable<InMemoryRevitParameter>? parameters = null)
    {
        Id = id;
        UniqueId = uniqueId;
        Category = category;
        Name = name;
        _parameters = (parameters ?? Array.Empty<InMemoryRevitParameter>())
            .ToDictionary(parameter => parameter.Name, StringComparer.OrdinalIgnoreCase);
    }

    public int Id { get; }

    public string UniqueId { get; }

    public string Category { get; }

    public string Name { get; }

    public IReadOnlyCollection<IRevitParameter> GetParameters()
    {
        return _parameters.Values.Cast<IRevitParameter>().ToArray();
    }

    public IRevitParameter? GetParameter(string name)
    {
        return _parameters.GetValueOrDefault(name);
    }
}
