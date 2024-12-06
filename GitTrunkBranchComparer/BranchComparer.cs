using LibGit2Sharp;
using Spectre.Console;

namespace GitTrunkBranchComparer;

internal static class BranchComparer
{
    internal static void CompareBranches(string directory, string after, string branch1, string branch2, bool contains,
        string filter)
    {
        var afterOffset = DateTimeOffset.Parse(after); //"2023-09-05"; // date to start looking for commits

        using var repo = new Repository(directory);
        var commits1 = repo.Commits.QueryBy(new CommitFilter
        {
            SortBy = CommitSortStrategies.Time,
            IncludeReachableFrom = branch1
        }).Where(commit => commit.Author.When > afterOffset);

        var commits2 = repo.Commits.QueryBy(new CommitFilter
        {
            SortBy = CommitSortStrategies.Time,
            IncludeReachableFrom = branch2
        }).Where(commit => commit.Author.When > afterOffset);

        var list1 = commits1.Select(commit => $"{commit.Author.When:yyyy/MM/dd HH:mm:ss} {commit.MessageShort}")
            .ToList();
        var list2 = commits2.Select(commit => $"{commit.Author.When:yyyy/MM/dd HH:mm:ss} {commit.MessageShort}")
            .ToList();
        var list3 = list1.Except(list2).Order().ToList();

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
            list3 = contains
                ? list3.Where(x => x.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList()
                : list3.Where(x => !x.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        AnsiConsole.MarkupLine(commitsMessage + ":\n");

        foreach (var item in list3)
        {
            var datePart = item[..19];
            var messagePart = item[20..];
            AnsiConsole.MarkupLine($"[springgreen1]{datePart} [/][steelblue3]{messagePart}[/]");
        }
    }
}