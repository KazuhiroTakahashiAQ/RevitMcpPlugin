using RevitMcp.Core.Revit;

namespace RevitMcp.RevitAdapter.InMemory;

public sealed class InMemoryRevitApplicationContext : IRevitApplicationContext
{
    public InMemoryRevitApplicationContext(InMemoryRevitDocumentContext? activeDocument = null)
    {
        ActiveDocument = activeDocument;
    }

    public IRevitDocumentContext? ActiveDocument { get; set; }

    public IRevitTransaction BeginTransaction(string name)
    {
        return new InMemoryRevitTransaction(name);
    }
}
