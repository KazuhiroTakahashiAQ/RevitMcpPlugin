using System.Text.Json;
using RevitMcp.Core.Revit;

namespace RevitMcp.RevitAdapter.InMemory;

public static class InMemorySampleProjectFactory
{
    public static InMemoryRevitDocumentContext CreateDefault()
    {
        var wall = new InMemoryRevitElement(
            id: 1001,
            uniqueId: "wall-1001",
            category: "OST_Walls",
            name: "Basic Wall: Generic - 200mm",
            parameters: new[]
            {
                new InMemoryRevitParameter("Comments", ParameterStorageType.String, false, "Initial"),
                new InMemoryRevitParameter("Mark", ParameterStorageType.String, false, "W-01")
            });

        var door = new InMemoryRevitElement(
            id: 2001,
            uniqueId: "door-2001",
            category: "OST_Doors",
            name: "Single Flush",
            parameters: new[]
            {
                new InMemoryRevitParameter("Comments", ParameterStorageType.String, false, "Door")
            });

        var document = new InMemoryRevitDocumentContext(
            title: "SampleProject.rvt",
            path: "/tmp/SampleProject.rvt",
            isFamilyDocument: false,
            isModified: true,
            isWorkshared: true,
            activeView: new InMemoryRevitView(101, "Level 1", "FloorPlan"),
            elements: new[] { wall, door });

        document.Select(1001, 2001);
        return document;
    }

    public static InMemoryRevitDocumentContext LoadFromFile(string path)
    {
        var json = File.ReadAllText(path);
        var fixture = JsonSerializer.Deserialize<InMemoryProjectFixture>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidOperationException($"Could not deserialize fixture file '{path}'.");

        var document = new InMemoryRevitDocumentContext(
            title: fixture.Document.Title,
            path: fixture.Document.Path,
            isFamilyDocument: fixture.Document.IsFamilyDocument,
            isModified: fixture.Document.IsModified,
            isWorkshared: fixture.Document.IsWorkshared,
            activeView: new InMemoryRevitView(
                fixture.Document.ActiveView.Id,
                fixture.Document.ActiveView.Name,
                fixture.Document.ActiveView.ViewType),
            elements: fixture.Elements.Select(CreateElement));

        document.Select(fixture.SelectedElementIds.ToArray());
        return document;
    }

    private static InMemoryRevitElement CreateElement(InMemoryElementFixture element)
    {
        return new InMemoryRevitElement(
            id: element.Id,
            uniqueId: element.UniqueId,
            category: element.Category,
            name: element.Name,
            parameters: element.Parameters.Select(CreateParameter));
    }

    private static InMemoryRevitParameter CreateParameter(InMemoryParameterFixture parameter)
    {
        return new InMemoryRevitParameter(
            parameter.Name,
            Enum.Parse<ParameterStorageType>(parameter.StorageType, ignoreCase: true),
            parameter.IsReadOnly,
            ConvertValue(parameter.Value, parameter.StorageType));
    }

    private static object? ConvertValue(JsonElement value, string storageType)
    {
        if (value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return storageType.ToLowerInvariant() switch
        {
            "string" => value.GetString() ?? string.Empty,
            "integer" => value.GetInt32(),
            "double" => value.GetDouble(),
            "boolean" => value.GetBoolean(),
            _ => throw new InvalidOperationException($"Unsupported storage type '{storageType}'.")
        };
    }
}

public sealed class InMemoryProjectFixture
{
    public InMemoryDocumentFixture Document { get; init; } = new();
    public IReadOnlyList<int> SelectedElementIds { get; init; } = Array.Empty<int>();
    public IReadOnlyList<InMemoryElementFixture> Elements { get; init; } = Array.Empty<InMemoryElementFixture>();
}

public sealed class InMemoryDocumentFixture
{
    public string Title { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public bool IsFamilyDocument { get; init; }
    public bool IsModified { get; init; }
    public bool IsWorkshared { get; init; }
    public InMemoryViewFixture ActiveView { get; init; } = new();
}

public sealed class InMemoryViewFixture
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ViewType { get; init; } = string.Empty;
}

public sealed class InMemoryElementFixture
{
    public int Id { get; init; }
    public string UniqueId { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<InMemoryParameterFixture> Parameters { get; init; } = Array.Empty<InMemoryParameterFixture>();
}

public sealed class InMemoryParameterFixture
{
    public string Name { get; init; } = string.Empty;
    public string StorageType { get; init; } = "String";
    public bool IsReadOnly { get; init; }
    public JsonElement Value { get; init; }
}
