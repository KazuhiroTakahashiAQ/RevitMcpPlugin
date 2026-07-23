using Autodesk.Revit.DB;
using RevitMcp.Core.Revit;

namespace RevitMcp.Addin.Revit;

public sealed class RevitTransaction : IRevitTransaction
{
    private readonly Transaction _transaction;
    private bool _finished;

    public RevitTransaction(Document document, string name)
    {
        _transaction = new Transaction(document, name);
        _transaction.Start();
    }

    public void Commit()
    {
        if (_finished)
        {
            return;
        }

        _transaction.Commit();
        _finished = true;
    }

    public void Dispose()
    {
        _transaction.Dispose();
    }

    public void RollBack()
    {
        if (_finished)
        {
            return;
        }

        _transaction.RollBack();
        _finished = true;
    }
}
