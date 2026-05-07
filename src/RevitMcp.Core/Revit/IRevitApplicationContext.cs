namespace RevitMcp.Core.Revit;

public interface IRevitApplicationContext
{
    IRevitDocumentContext? ActiveDocument { get; }

    IRevitTransaction BeginTransaction(string name);
}
