namespace RevitMcp.Core.Revit;

public interface IRevitDocumentContext
{
    string Title { get; }
    string Path { get; }
    bool IsFamilyDocument { get; }
    bool IsModified { get; }
    bool IsWorkshared { get; }
    IRevitView ActiveView { get; }

    IReadOnlyCollection<IRevitElement> GetSelectedElements();

    IReadOnlyCollection<IRevitElement> FindElements(RevitElementQuery query);

    IRevitElement? GetElement(int elementId);

    IRevitElement CreateWallLine(RevitLineWallDefinition definition);
}
