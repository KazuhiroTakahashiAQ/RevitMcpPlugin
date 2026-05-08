using Autodesk.Revit.DB;
using RevitMcp.Core.Revit;

namespace RevitMcp.Addin.Revit;

public sealed class RevitElement : IRevitElement
{
    private readonly Element _element;

    public RevitElement(Element element)
    {
        _element = element;
    }

    public int Id => (int)_element.Id.Value;

    public string UniqueId => _element.UniqueId;

    public string Category => _element.Category?.Name ?? string.Empty;

    public string Name => _element.Name ?? string.Empty;

    public IReadOnlyCollection<IRevitParameter> GetParameters()
    {
        return _element.Parameters
            .Cast<Parameter>()
            .Select(parameter => (IRevitParameter)new RevitParameter(parameter))
            .ToArray();
    }

    public IRevitParameter? GetParameter(string name)
    {
        var parameter = _element.LookupParameter(name);
        return parameter is null ? null : new RevitParameter(parameter);
    }
}
