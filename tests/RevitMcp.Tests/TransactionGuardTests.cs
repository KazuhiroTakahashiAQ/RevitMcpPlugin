using RevitMcp.RevitAdapter.InMemory;

namespace RevitMcp.Tests;

public sealed class TransactionGuardTests
{
    [Fact]
    public void RollBack_AfterCommit_DoesNotOverwriteCommittedState()
    {
        var transaction = new InMemoryRevitTransaction("test");

        transaction.Commit();
        transaction.RollBack();

        Assert.True(transaction.IsCommitted);
        Assert.False(transaction.IsRolledBack);
    }

    [Fact]
    public void RollBack_CalledTwice_IsIdempotent()
    {
        var transaction = new InMemoryRevitTransaction("test");

        transaction.RollBack();
        transaction.RollBack();

        Assert.True(transaction.IsRolledBack);
        Assert.False(transaction.IsCommitted);
    }

    [Fact]
    public void Commit_CalledTwice_IsIdempotent()
    {
        var transaction = new InMemoryRevitTransaction("test");

        transaction.Commit();
        transaction.Commit();

        Assert.True(transaction.IsCommitted);
        Assert.False(transaction.IsRolledBack);
    }
}
