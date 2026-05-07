namespace RevitMcp.Core.Revit;

public interface IRevitElement
{
    int Id { get; }
    string UniqueId { get; }
    string Category { get; }
    string Name { get; }

    IReadOnlyCollection<IRevitParameter> GetParameters();

    IRevitParameter? GetParameter(string name);
}
