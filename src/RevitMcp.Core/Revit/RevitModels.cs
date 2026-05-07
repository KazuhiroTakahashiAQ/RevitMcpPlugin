namespace RevitMcp.Core.Revit;

public sealed record RevitPointMm(double X, double Y, double Z);

public sealed record RevitParameterMatch(string Name, string Value);

public sealed record RevitElementQuery(
    string? Category,
    string? NameContains,
    RevitParameterMatch? ParameterEquals,
    int Limit);

public sealed record RevitLineWallDefinition(
    RevitPointMm Start,
    RevitPointMm End,
    string LevelName,
    string? WallTypeName,
    double HeightMm);
