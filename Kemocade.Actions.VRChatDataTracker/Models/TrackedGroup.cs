using VRChat.API.Model;

namespace Kemocade.Actions.VRChatDataTracker.Models;

internal record TrackedGroup
{
    public required Group Group { get; init; }
    public required List<GroupRole> GroupRoles { get; init; }
    public required IEnumerable<GroupMember> GroupMembers { get; init; }
}
