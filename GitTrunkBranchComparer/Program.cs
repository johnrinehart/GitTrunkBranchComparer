using Spectre.Console;
using System.Management.Automation;

var directory = args[0];// directory of the git repository
var after = args[1];//"2023-09-05"; // date to start looking for commits
var branch1 = args[2];//"dev";
var branch2 = args[3];//"master";

using (var powershell = PowerShell.Create())
{
    powershell.AddScript($"cd {directory}");

    powershell.AddScript(@$"git log --pretty=format:""%ai %s"" --after=""{after}"" {branch1}");
    var results1 = powershell.Invoke();
    var list1 = results1.Select(x => x.ToString()).Order().ToList();

    powershell.AddScript(@$"git log --pretty=format:""%ai %s"" --after=""{after}"" {branch2}");
    var results2 = powershell.Invoke();
    var list2 = results2.Select(x => x.ToString()).Order().ToList();

    var list3 = list1.Except(list2).ToList();

    AnsiConsole.MarkupLineInterpolated($"Commits since [gold3_1]{after}[/] that are on [gold3_1]{branch1}[/] but not on [gold3_1]{branch2}[/]:\n");
    foreach (var item in list3)
    {
        var datepart = item[..20];
        var messagepart = item[26..];
        AnsiConsole.MarkupLineInterpolated($"[springgreen1]{datepart}[/][red]{messagepart}[/]");
    }
}