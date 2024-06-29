

using Content.Shared.Actions;
using Content.Shared.Goobstation.Silicons.AI;

namespace Content.Client.Goobstation.Silicons.AI
{
    public sealed class AIEyeSystem : SharedAIEyeSystem
    {
        [Dependency] private readonly SharedActionsSystem _actions = default!;
    }
}