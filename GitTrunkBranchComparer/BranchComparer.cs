namespace GitTrunkBranchComparer;
using LibGit2Sharp;
using Spectre.Console;

internal static class BranchComparer
{
    internal static void CompareBranches(string directory, string after, string branch1, string branch2, bool contains, string filter)
    {
        var afterOffset = DateTimeOffset.Parse(after);

        using var repo = new Repository(directory);

        var firstBranchCommits = GetCommits(repo, branch1, afterOffset);
        var secondBranchCommits = GetCommits(repo, branch2, afterOffset);

        var commitsNotOnSecondBranch = FilterCommits(firstBranchCommits, secondBranchCommits, contains, filter);

        DisplayCommits(after, branch1, branch2, filter, contains, commitsNotOnSecondBranch);

        var cherrypicksToApply = PromptForCherryPicks(commitsNotOnSecondBranch);

        ApplyCherryPicks(repo, branch2, cherrypicksToApply);
    }

    private static List<CommitDisplay> GetCommits(Repository repo, string branch, DateTimeOffset afterOffset)
    {
        return [.. repo.Commits.QueryBy(new CommitFilter
            {
                SortBy = CommitSortStrategies.Time | CommitSortStrategies.Reverse,
                IncludeReachableFrom = branch
            })
            .Where(commit => commit.Author.When > afterOffset)
            .Select(commit => new CommitDisplay
            {
                CommitMessage = $"{commit.Author.When:yyyy/MM/dd HH:mm:ss} {commit.MessageShort}",
                Commit = commit
            })];
    }

    private static List<CommitDisplay> FilterCommits(
        List<CommitDisplay> firstBranchCommits,
        List<CommitDisplay> secondBranchCommits,
        bool contains,
        string filter)
    {
        var commitsNotOnSecondBranch = firstBranchCommits.Except(secondBranchCommits).ToList();

        if (!string.IsNullOrEmpty(filter))
        {
            commitsNotOnSecondBranch = contains
                ? [.. commitsNotOnSecondBranch.Where(x => x.CommitMessage.Contains(filter, StringComparison.OrdinalIgnoreCase))]
                : [.. commitsNotOnSecondBranch.Where(x => !x.CommitMessage.Contains(filter, StringComparison.OrdinalIgnoreCase))];
        }

        return commitsNotOnSecondBranch;
    }

    private static void DisplayCommits(
        string after,
        string branch1,
        string branch2,
        string filter,
        bool contains,
        List<CommitDisplay> commitsNotOnSecondBranch)
    {
        var version = typeof(Program).Assembly.GetName().Version;
        AnsiConsole.WriteLine($"Program Version: {version}\n");

        var commitsMessage = $"Commits since \t[gold3_1]{after}[/] \n" +
                             $"on \t\t[gold3_1]{branch1}[/] \n" +
                             $"but not on \t[gold3_1]{branch2}[/]";

        if (!string.IsNullOrEmpty(filter))
        {
            commitsMessage += contains ? " \nincluding " : " \nexcluding ";
            commitsMessage += $"\t[gold3_1]{filter}[/]";
        }

        AnsiConsole.MarkupLine(commitsMessage + ":\n");

        foreach (var item in commitsNotOnSecondBranch)
        {
            var datePart = item.CommitMessage[..19];
            var messagePart = item.CommitMessage[20..];
            AnsiConsole.MarkupLine($"[springgreen1]{datePart} [/][steelblue3]{messagePart}[/]");
        }
    }

    private static List<CommitDisplay> PromptForCherryPicks(List<CommitDisplay> commitsNotOnSecondBranch)
    {
        return AnsiConsole.Prompt(
            new MultiSelectionPrompt<CommitDisplay>()
                .Title("Select commits to cherry-pick")
                .InstructionsText(
                    "[grey](Press [blue]<space>[/] to toggle a commit, " +
                    "[green]<enter>[/] to accept)[/]")
                .AddChoices(commitsNotOnSecondBranch)
                .UseConverter(x => x.CommitMessage));
    }

    private static void ApplyCherryPicks(Repository repo, string branch2, List<CommitDisplay> cherrypicksToApply)
    {
        var email = repo.Config.Get<string>("user.email").Value;
        var name = repo.Config.Get<string>("user.name").Value;
        var commiter = new Signature(name, email, DateTimeOffset.Now);

        var cherrypickOptions = new CherryPickOptions
        {
            FileConflictStrategy = CheckoutFileConflictStrategy.Normal,
            FailOnConflict = true,
        };

        if (repo.Head.FriendlyName != branch2)
        {
            Commands.Checkout(repo, branch2);
        }

        foreach (var cherrypick in cherrypicksToApply)
        {
            repo.CherryPick(cherrypick.Commit, commiter, cherrypickOptions);
        }
    }
}
