namespace J4JSoftware.FileHistory
{
    public class FileHistoryRule
    {
        public string Rule { get; internal set; }
        public RuleType Type { get; internal set; }
        public ProtectedItemCategory Category { get; internal set; }
    }
}