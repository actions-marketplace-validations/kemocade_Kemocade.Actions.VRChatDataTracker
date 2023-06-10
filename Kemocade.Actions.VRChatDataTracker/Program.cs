using CommandLine;
using Kemocade.Actions.VRChatDataTracker;
using Kemocade.Actions.VRChatDataTracker.Models;
using Newtonsoft.Json;
using VRChat.API.Api;
using VRChat.API.Client;
using VRChat.API.Model;

static void Log(string message) => Console.WriteLine(message);

static void LogLabel(string label, string message) =>
    Log($"{label}: {message}");

// Configure Cancellation
using CancellationTokenSource tokenSource = new();
Console.CancelKeyPress += delegate { tokenSource.Cancel(); };

// Configure Inputs
ParserResult<ActionInputs> parser = Parser.Default.ParseArguments<ActionInputs>(args);
if (parser.Errors.ToArray() is { Length: > 0 } errors)
{
    foreach (CommandLine.Error error in errors)
    { LogLabel(nameof(error), error.Tag.ToString()); }
    Environment.Exit(2);
    return;
}
ActionInputs inputs = parser.Value;

// Find Local Files
DirectoryInfo workspace = new(inputs.Workspace);
DirectoryInfo directory = workspace.CreateSubdirectory(inputs.Directory);

// Authentication credentials
Configuration Config = new()
{
    Username = inputs.Username,
    Password = inputs.Password
};

// Create instances of API's we'll need
AuthenticationApi AuthApi = new(Config);
GroupsApi groupsApi = new(Config);

try
{
    // Calling "GetCurrentUser(Async)" logs you in if you are not already logged in.
    CurrentUser CurrentUser = AuthApi.GetCurrentUser();
    Console.WriteLine($"Logged in as {CurrentUser.DisplayName}.");

    Group group = groupsApi.GetGroup(inputs.Group);
    // TODO: Is GetGroupMembers returning 1 too few members??
    int memberCount = group.MemberCount - 1;
    Console.WriteLine(memberCount);

    List<GroupRole> groupRoles = groupsApi.GetGroupRoles(inputs.Group);

    Console.WriteLine("Getting members");
    List<GroupMember> groupMembers = new();
    while (groupMembers.Count < memberCount)
    {
        groupMembers.AddRange(groupsApi.GetGroupMembers(inputs.Group, 100, groupMembers.Count));
        Console.WriteLine(groupMembers.Count);
        await Task.Delay(1000);
    }

    TrackedGroup trackedGroup = new()
    { Group = group, GroupMembers = groupMembers, GroupRoles = groupRoles };

    Console.WriteLine(JsonConvert.SerializeObject(trackedGroup));

    Console.WriteLine("Done!");
    Console.WriteLine(groupMembers.Count);
}
catch (ApiException e)
{
    Console.WriteLine("Exception when calling API: {0}", e.Message);
    Console.WriteLine("Status Code: {0}", e.ErrorCode);
    Console.WriteLine(e.ToString());
    Environment.Exit(2);
    return;
}

Log("Done!");
Environment.Exit(0);