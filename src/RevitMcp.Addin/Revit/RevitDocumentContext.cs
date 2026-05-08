using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitMcp.Core.Revit;

namespace RevitMcp.Addin.Revit;

public sealed class RevitDocumentContext : IRevitDocumentContext
{
    private readonly UIDocument _uiDocument;
    private readonly Document _document;

    public RevitDocumentContext(UIDocument uiDocument)
    {
        _uiDocument = uiDocument;
        _document = uiDocument.Document;
    }

    public string Title => _document.Title;

    public string Path => _document.PathName;

    public bool IsFamilyDocument => _document.IsFamilyDocument;

    public bool IsModified => _document.IsModified;

    public bool IsWorkshared => _document.IsWorkshared;

    public IRevitView ActiveView => new RevitView(_uiDocument.ActiveView);

    public IReadOnlyCollection<IRevitElement> GetSelectedElements()
    {
        return _uiDocument.Selection.GetElementIds()
            .Select(id => _document.GetElement(id))
            .Where(element => element is not null)
            .Select(element => (IRevitElement)new RevitElement(element!))
            .ToArray();
    }

    public IReadOnlyCollection<IRevitElement> FindElements(RevitElementQuery query)
    {
        IEnumerable<Element> elements = new FilteredElementCollector(_document)
            .WhereElementIsNotElementType()
            .ToElements();

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            if (!Enum.TryParse<BuiltInCategory>(query.Category, ignoreCase: true, out var category))
            {
                throw new InvalidOperationException($"Unknown BuiltInCategory '{query.Category}'.");
            }

            elements = elements.Where(element => element.Category?.Id.Value == (int)category);
        }

        if (!string.IsNullOrWhiteSpace(query.NameContains))
        {
            elements = elements.Where(element =>
                (element.Name ?? string.Empty).Contains(query.NameContains, StringComparison.OrdinalIgnoreCase));
        }

        if (query.ParameterEquals is not null)
        {
            elements = elements.Where(element =>
            {
                var parameter = element.LookupParameter(query.ParameterEquals.Name);
                return parameter is not null &&
                       string.Equals(ParameterValueReader.ReadAsString(parameter), query.ParameterEquals.Value, StringComparison.Ordinal);
            });
        }

        return elements
            .Take(query.Limit)
            .Select(element => (IRevitElement)new RevitElement(element))
            .ToArray();
    }

    public IRevitElement? GetElement(int elementId)
    {
        var element = _document.GetElement(new ElementId(elementId));
        return element is null ? null : new RevitElement(element);
    }

    public IRevitElement CreateWallLine(RevitLineWallDefinition definition)
    {
        if (!TryFindLevel(definition.LevelName, out var level))
        {
            throw new InvalidOperationException($"Level '{definition.LevelName}' was not found.");
        }

        var wallType = TryFindWallType(definition.WallTypeName);
        var line = Line.CreateBound(ToXyz(definition.Start), ToXyz(definition.End));

        Wall wall;
        if (wallType is null)
        {
            wall = Wall.Create(_document, line, level.Id, false);
        }
        else
        {
            wall = Wall.Create(_document, line, wallType.Id, level.Id, ConvertMillimetersToFeet(definition.HeightMm), 0, false, false);
        }

        return new RevitElement(wall);
    }

    private bool TryFindLevel(string levelName, out Level level)
    {
        level = new FilteredElementCollector(_document)
            .OfClass(typeof(Level))
            .Cast<Level>()
            .FirstOrDefault(candidate => string.Equals(candidate.Name, levelName, StringComparison.Ordinal))
            ?? null!;

        return level is not null;
    }

    private WallType? TryFindWallType(string? wallTypeName)
    {
        if (string.IsNullOrWhiteSpace(wallTypeName))
        {
            return null;
        }

        return new FilteredElementCollector(_document)
            .OfClass(typeof(WallType))
            .Cast<WallType>()
            .FirstOrDefault(candidate => string.Equals(candidate.Name, wallTypeName, StringComparison.Ordinal));
    }

    private static XYZ ToXyz(RevitPointMm point)
    {
        return new XYZ(
            ConvertMillimetersToFeet(point.X),
            ConvertMillimetersToFeet(point.Y),
            ConvertMillimetersToFeet(point.Z));
    }

    private static double ConvertMillimetersToFeet(double millimeters)
    {
        return UnitUtils.ConvertToInternalUnits(millimeters, UnitTypeId.Millimeters);
    }
}
