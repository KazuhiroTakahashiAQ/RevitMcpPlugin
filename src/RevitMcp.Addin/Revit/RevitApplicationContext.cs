using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitMcp.Core.Revit;

namespace RevitMcp.Addin.Revit;

public sealed class RevitApplicationContext : IRevitApplicationContext
{
    private readonly UIApplication _application;

    public RevitApplicationContext(UIApplication application)
    {
        _application = application;
    }

    public IRevitDocumentContext? ActiveDocument
    {
        get
        {
            var uiDocument = _application.ActiveUIDocument;
            return uiDocument is null ? null : new RevitDocumentContext(uiDocument);
        }
    }

    public IRevitTransaction BeginTransaction(string name)
    {
        var document = _application.ActiveUIDocument?.Document
            ?? throw new InvalidOperationException("Active document is not available.");

        return new RevitTransaction(document, name);
    }
}
