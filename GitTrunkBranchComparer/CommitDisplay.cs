namespace GitTrunkBranchComparer;
using LibGit2Sharp;

public class CommitDisplay : IComparable
{
    public string CommitMessage { get; set; } = string.Empty;

    public Commit Commit { get; set; } = null!;

    //Compare SHA
    public int CompareTo(object? obj) => Commit.Sha.CompareTo((obj as CommitDisplay)!.Commit.Sha);

    public override bool Equals(object? obj) => obj is CommitDisplay other && Commit.Sha.Equals(other.Commit.Sha);

    public override int GetHashCode() => Commit.Sha.GetHashCode();
}