namespace RevitMcp.Core.Revit;

public interface IRevitView
{
    int Id { get; }
    string Name { get; }
    string ViewType { get; }
}
