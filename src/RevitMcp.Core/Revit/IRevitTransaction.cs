namespace RevitMcp.Core.Revit;

public interface IRevitTransaction : IDisposable
{
    void Commit();

    void RollBack();
}
