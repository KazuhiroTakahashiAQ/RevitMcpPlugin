using Autodesk.Revit.DB;
using RevitMcp.Core.Revit;

namespace RevitMcp.Addin.Revit;

public sealed class RevitTransaction : IRevitTransaction
{
    private readonly Transaction _transaction;

    public RevitTransaction(Document document, string name)
    {
        _transaction = new Transaction(document, name);
        _transaction.Start();
    }

    public void Commit()
    {
        _transaction.Commit();
    }

    public void Dispose()
    {
        _transaction.Dispose();
    }

    public void RollBack()
    {
        _transaction.RollBack();
    }
}
