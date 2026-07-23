using RevitMcp.Core.Revit;

namespace RevitMcp.RevitAdapter.InMemory;

public sealed class InMemoryRevitTransaction : IRevitTransaction
{
    public InMemoryRevitTransaction(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public bool IsCommitted { get; private set; }

    public bool IsRolledBack { get; private set; }

    public void Commit()
    {
        if (IsCommitted || IsRolledBack)
        {
            return;
        }

        IsCommitted = true;
    }

    public void Dispose()
    {
    }

    public void RollBack()
    {
        if (IsCommitted || IsRolledBack)
        {
            return;
        }

        IsRolledBack = true;
    }
}
