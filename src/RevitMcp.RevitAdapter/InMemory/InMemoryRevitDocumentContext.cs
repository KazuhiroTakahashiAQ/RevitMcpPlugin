using RevitMcp.Core.Revit;

namespace RevitMcp.RevitAdapter.InMemory;

public sealed class InMemoryRevitDocumentContext : IRevitDocumentContext
{
    private readonly Dictionary<int, InMemoryRevitElement> _elements;
    private readonly List<int> _selectedElementIds = new();
    private int _nextElementId;

    public InMemoryRevitDocumentContext(
        string title,
        string path,
        bool isFamilyDocument,
        bool isModified,
        bool isWorkshared,
        InMemoryRevitView activeView,
        IEnumerable<InMemoryRevitElement>? elements = null)
    {
        Title = title;
        Path = path;
        IsFamilyDocument = isFamilyDocument;
        IsModified = isModified;
        IsWorkshared = isWorkshared;
        ActiveView = activeView;
        _elements = (elements ?? Array.Empty<InMemoryRevitElement>()).ToDictionary(element => element.Id);
        _nextElementId = _elements.Count == 0 ? 1 : _elements.Keys.Max() + 1;
    }

    public string Title { get; }

    public string Path { get; }

    public bool IsFamilyDocument { get; }

    public bool IsModified { get; set; }

    public bool IsWorkshared { get; }

    public IRevitView ActiveView { get; }

    public IReadOnlyCollection<IRevitElement> GetSelectedElements()
    {
        return _selectedElementIds
            .Select(id => _elements[id])
            .Cast<IRevitElement>()
            .ToArray();
    }

    public IReadOnlyCollection<IRevitElement> FindElements(RevitElementQuery query)
    {
        IEnumerable<InMemoryRevitElement> elements = _elements.Values;

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            elements = elements.Where(element => string.Equals(element.Category, query.Category, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.NameContains))
        {
            elements = elements.Where(element => element.Name.Contains(query.NameContains, StringComparison.OrdinalIgnoreCase));
        }

        if (query.ParameterEquals is not null)
        {
            elements = elements.Where(element =>
            {
                var parameter = element.GetParameter(query.ParameterEquals.Name);
                return parameter?.Value?.ToString() == query.ParameterEquals.Value;
            });
        }

        return elements.Take(query.Limit).Cast<IRevitElement>().ToArray();
    }

    public IRevitElement? GetElement(int elementId)
    {
        return _elements.GetValueOrDefault(elementId);
    }

    public IRevitElement CreateWallLine(RevitLineWallDefinition definition)
    {
        var element = new InMemoryRevitElement(
            id: _nextElementId++,
            uniqueId: Guid.NewGuid().ToString("N"),
            category: "OST_Walls",
            name: definition.WallTypeName ?? "Basic Wall",
            parameters: new[]
            {
                new InMemoryRevitParameter("Level", ParameterStorageType.String, false, definition.LevelName),
                new InMemoryRevitParameter("Height", ParameterStorageType.Double, false, definition.HeightMm)
            });

        _elements[element.Id] = element;
        IsModified = true;
        return element;
    }

    public void Select(params int[] elementIds)
    {
        _selectedElementIds.Clear();
        _selectedElementIds.AddRange(elementIds);
    }
}
