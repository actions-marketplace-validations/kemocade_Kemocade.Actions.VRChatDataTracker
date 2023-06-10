using CommandLine;
using Kemocade.Actions.VRChatDataTracker;
using Kemocade.Actions.VRChatDataTracker.Models;
using System.Text.Json;
using VRChat.API.Api;
using VRChat.API.Client;
using VRChat.API.Model;
using static System.Console;
using static System.IO.File;
using static System.Text.Json.JsonSerializer;

// Configure Cancellation
using CancellationTokenSource tokenSource = new();
CancelKeyPress += delegate { tokenSource.Cancel(); };

// Configure Inputs
ParserResult<ActionInputs> parser = Parser.Default.ParseArguments<ActionInputs>(args);
if (parser.Errors.ToArray() is { Length: > 0 } errors)
{
    foreach (CommandLine.Error error in errors)
    { WriteLine($"{nameof(error)}: {error.Tag}"); }
    Environment.Exit(2);
    return;
}
ActionInputs inputs = parser.Value;

// Find Local Files
DirectoryInfo workspace = new(inputs.Workspace);
DirectoryInfo output = workspace.CreateSubdirectory(inputs.Output);

// Authentication credentials
Configuration Config = new()
{
    Username = inputs.Username,
    Password = inputs.Password
};

// Create instances of API's we'll need
AuthenticationApi AuthApi = new(Config);
GroupsApi groupsApi = new(Config);

TrackedGroup trackedGroup;

try
{
    // Log in
    CurrentUser CurrentUser = AuthApi.GetCurrentUser();
    WriteLine($"Logged in as {CurrentUser.DisplayName}");

    // Get group
    Group group = groupsApi.GetGroup(inputs.Group);
    int memberCount = group.MemberCount - 1;
    WriteLine($"Got Group {group.Name}, Members: {memberCount}");

    // Get group roles
    List<GroupRole> groupRoles = groupsApi.GetGroupRoles(inputs.Group);

    // Get group members
    // TODO: member count off by 1
    WriteLine("Getting members...");
    List<GroupMember> groupMembers = new();
    while (groupMembers.Count < memberCount)
    {
        groupMembers.AddRange(groupsApi.GetGroupMembers(inputs.Group, 100, groupMembers.Count));
        WriteLine(groupMembers.Count);
        await Task.Delay(1000);
    }
    WriteLine($"Got members: {groupMembers.Count}");

    trackedGroup = new()
    { Group = group, GroupMembers = groupMembers, GroupRoles = groupRoles };
}
catch (ApiException e)
{
    WriteLine("Exception when calling API: {0}", e.Message);
    WriteLine("Status Code: {0}", e.ErrorCode);
    WriteLine(e.ToString());
    Environment.Exit(2);
    return;
}

JsonSerializerOptions jsonSerializerOptions = new()
{ PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

string trackedGroupJson = Serialize(trackedGroup, jsonSerializerOptions);
WriteLine(trackedGroupJson);

FileInfo outputJson = new(Path.Join(output.FullName, "output.json"));
WriteAllText(outputJson.FullName, trackedGroupJson);

WriteLine("Done!");
Environment.Exit(0);