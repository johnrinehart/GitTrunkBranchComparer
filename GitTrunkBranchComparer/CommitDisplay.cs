namespace GitTrunkBranchComparer;
using LibGit2Sharp;

public class CommitDisplay : IComparable
{
    public string CommitMessage { get; set; } = string.Empty;

    public Commit Commit { get; set; } = null!;

    //Compare SHA
    public int CompareTo(object? obj) => Commit.Message.CompareTo((obj as CommitDisplay)!.Commit.Message);

    public override bool Equals(object? obj) => obj is CommitDisplay other && Commit.Message.Equals(other.Commit.Message);

    public override int GetHashCode() => Commit.Message.GetHashCode();
}