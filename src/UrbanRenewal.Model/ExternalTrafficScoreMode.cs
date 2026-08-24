namespace UrbanRenewal.Model
{
  public static class ExternalTrafficScoreMode
  {
    public const string Raw = "Raw";

    public const string Normalized = "Normalized";

    public static bool IsNormalized(string mode)
    {
      return string.Equals(mode, Normalized, System.StringComparison.OrdinalIgnoreCase);
    }
  }
}
