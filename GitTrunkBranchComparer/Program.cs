using GitTrunkBranchComparer;

var directory = args[0]; // directory of the git repository
var after = args[1]; //"2023-09-05"; // date to start looking for commits
var branch1 = args[2]; //"dev";
var branch2 = args[3]; //"master";
var contains = false;
if (args.Length > 4 && bool.TryParse(args[4], out contains))
{
}

var filter = args.Length > 5 ? args[5] : string.Empty;

BranchComparer.CompareBranches(directory, after, branch1, branch2, contains, filter);