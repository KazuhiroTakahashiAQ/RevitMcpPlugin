using Autodesk.Revit.DB;
using RevitMcp.Core.Revit;

namespace RevitMcp.Addin.Revit;

public sealed class RevitView : IRevitView
{
    private readonly View _view;

    public RevitView(View view)
    {
        _view = view;
    }

    public int Id => (int)_view.Id.Value;

    public string Name => _view.Name;

    public string ViewType => _view.ViewType.ToString();
}
