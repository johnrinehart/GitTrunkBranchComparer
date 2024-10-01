using LibGit2Sharp;
using Spectre.Console;

static internal class Program2
{
    static internal void CompareBranches2(string directory, string after, string branch1, string branch2, bool contains, string filter)
    {

        var afterOffset = DateTimeOffset.Parse(after);//"2023-09-05"; // date to start looking for commits

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

        var list1 = commits1.Select(commit => $"{commit.Author.When:yyyy/MM/dd HH:mm:ss} {commit.MessageShort}").ToList();
        var list2 = commits2.Select(commit => $"{commit.Author.When:yyyy/MM/dd HH:mm:ss} {commit.MessageShort}").ToList();
        var list3 = list1.Except(list2).Order().ToList();

        if (!string.IsNullOrEmpty(filter))
        {
            list3 = contains ? list3.Where(x => x.Contains(filter)).ToList() : list3.Where(x => !x.Contains(filter)).ToList();
        }

        // Log program version
        var version = typeof(Program).Assembly.GetName().Version;
        AnsiConsole.WriteLine($"Program Version: {version}\n");

        AnsiConsole.MarkupLine($"Commits since [gold3_1]{after}[/] that are on [gold3_1]{branch1}[/] but not on [gold3_1]{branch2}[/]:\n");
        foreach (var item in list3)
        {
            var datepart = item[..19];
            var messagepart = item[20..];
            AnsiConsole.MarkupLine($"[springgreen1]{datepart} [/][steelblue3]{messagepart}[/]");
        }
    }

    //static internal void CompareBranches(string directory, string after, string branch1, string branch2)
    //{
    //    using var powershell = PowerShell.Create();
    //    powershell.AddScript($"cd {directory}");

    //    powershell.AddScript(@$"git log --pretty=format:""%ai %s"" --after=""{after}"" {branch1}");
    //    var results1 = powershell.Invoke();
    //    var list1 = results1.Select(x => x.ToString()).Order().ToList();

    //    powershell.AddScript(@$"git log --pretty=format:""%ai %s"" --after=""{after}"" {branch2}");
    //    var results2 = powershell.Invoke();
    //    var list2 = results2.Select(x => x.ToString()).Order().ToList();

    //    var list3 = list1.Except(list2).ToList();

    //    AnsiConsole.MarkupLineInterpolated($"Commits since [gold3_1]{after}[/] that are on [gold3_1]{branch1}[/] but not on [gold3_1]{branch2}[/]:\n");
    //    foreach (var item in list3)
    //    {
    //        var datepart = item[..20];
    //        var messagepart = item[26..];
    //        AnsiConsole.MarkupLineInterpolated($"[springgreen1]{datepart}[/][steelblue3]{messagepart}[/]");
    //    }
    //}
}
