using CommandLine;

namespace Kemocade.Actions.VRChatDataTracker;

public class ActionInputs
{
    [Option('w', "workspace", Required = true)]
    public string Workspace { get; set; } = null!;

    [Option('d', "directory", Required = true)]
    public string Directory { get; set; } = null!;

    [Option('u', "username", Required = true)]
    public string Username { get; set; } = null!;

    [Option('p', "password", Required = true)]
    public string Password { get; set; } = null!;

    [Option('g', "group", Required = true)]
    public string Group { get; set; } = null!;
}
