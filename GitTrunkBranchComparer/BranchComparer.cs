namespace GitTrunkBranchComparer;
using LibGit2Sharp;
using Spectre.Console;

internal static class BranchComparer
{
    internal static void CompareBranches(string directory, string after, string branch1, string branch2, bool contains,
        string filter)
    {
        var afterOffset = DateTimeOffset.Parse(after); //"2023-09-05"; // date to start looking for commits

        using var repo = new Repository(directory);
        var commits1 = repo.Commits.QueryBy(new CommitFilter
        {
            // Sort by time in the reverse order
            SortBy = CommitSortStrategies.Time | CommitSortStrategies.Reverse,
            IncludeReachableFrom = branch1
        }).Where(commit => commit.Author.When > afterOffset);

        var commits2 = repo.Commits.QueryBy(new CommitFilter
        {
            SortBy = CommitSortStrategies.Time | CommitSortStrategies.Reverse,
            IncludeReachableFrom = branch2
        }).Where(commit => commit.Author.When > afterOffset);

        var firstBranch = commits1.Select(commit =>
            new CommitDisplay
            {
                CommitMessage = $"{commit.Author.When:yyyy/MM/dd HH:mm:ss} {commit.MessageShort}",
                Commit = commit
            })
            .ToList();

        var secondBranch = commits2.Select(commit =>
            new CommitDisplay
            {
                CommitMessage = $"{commit.Author.When:yyyy/MM/dd HH:mm:ss} {commit.MessageShort}",
                Commit = commit
            })
            .ToList();

        var commitsNotOnSecondBranch = firstBranch.Except(secondBranch).ToList();

        // Log program version
        var version = typeof(Program).Assembly.GetName().Version;
        AnsiConsole.WriteLine($"Program Version: {version}\n");

        var commitsMessage = $"Commits since \t[gold3_1]{after}[/] \n" +
                             $"on \t\t[gold3_1]{branch1}[/] \n" +
                             $"but not on \t[gold3_1]{branch2}[/]";

        if (!string.IsNullOrEmpty(filter))
        {
            commitsMessage += contains ? " \nincluding " : " \nexcluding ";
            commitsMessage += $"\t[gold3_1]{filter}[/]";
            commitsNotOnSecondBranch = contains
                ? [.. commitsNotOnSecondBranch.Where(x => x.CommitMessage.Contains(filter, StringComparison.OrdinalIgnoreCase))]
                : [.. commitsNotOnSecondBranch.Where(x => !x.CommitMessage.Contains(filter, StringComparison.OrdinalIgnoreCase))];
        }

        AnsiConsole.MarkupLine(commitsMessage + ":\n");

        foreach (var item in commitsNotOnSecondBranch)
        {
            var datePart = item.CommitMessage[..19];
            var messagePart = item.CommitMessage[20..];
            AnsiConsole.MarkupLine($"[springgreen1]{datePart} [/][steelblue3]{messagePart}[/]");
        }

        var cherrypicksToApply = AnsiConsole.Prompt(
            new MultiSelectionPrompt<CommitDisplay>()
            .Title("Select commits to cherry-pick")
            .InstructionsText(
            "[grey](Press [blue]<space>[/] to toggle a commit, " +
            "[green]<enter>[/] to accept)[/]")
            .AddChoices(commitsNotOnSecondBranch)
            .UseConverter(x => x.CommitMessage));

        // Get git email
        var email = repo.Config.Get<string>("user.email").Value;

        // Get git name
        var name = repo.Config.Get<string>("user.name").Value;

        var when = DateTimeOffset.Now;

        var commiter = new Signature(name, email, when);

        // abort on conflict
        var cherrypickOptions = new CherryPickOptions
        {
            FileConflictStrategy = CheckoutFileConflictStrategy.Normal,
            FailOnConflict = true,
        };

        // change repo branch to cherry-pick to if not already on it
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
