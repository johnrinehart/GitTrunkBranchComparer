namespace GitTrunkBranchComparer;
using LibGit2Sharp;

public class CommitDisplay : IComparable
{
    private string? normalizedMessage;

    public string CommitMessage { get; set; } = string.Empty;

    // Body of the commit message (everything after the first line);
    // this is where git appends "# Conflicts:" on conflicted cherry-picks
    public string CommitDescription
    {
        get
        {
            var index = Commit.Message.IndexOf('\n');
            return index < 0 ? string.Empty : Commit.Message[(index + 1)..].Trim();
        }
    }

    public Commit Commit { get; set; } = null!;

    // Message without the "# Conflicts:" section git appends on conflicted cherry-picks,
    // so a conflicted cherry-pick still matches its original commit
    private string NormalizedMessage => normalizedMessage ??= StripConflictsSection(Commit.Message);

    public int CompareTo(object? obj) => NormalizedMessage.CompareTo((obj as CommitDisplay)!.NormalizedMessage);

    public override bool Equals(object? obj) => obj is CommitDisplay other && NormalizedMessage.Equals(other.NormalizedMessage);

    public override int GetHashCode() => NormalizedMessage.GetHashCode();

    private static string StripConflictsSection(string message)
    {
        var index = message.StartsWith("# Conflicts:", StringComparison.Ordinal)
            ? 0
            : message.IndexOf("\n# Conflicts:", StringComparison.Ordinal);

        return (index < 0 ? message : message[..index]).TrimEnd();
    }
}
