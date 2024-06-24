



using Robust.Shared.GameStates;

namespace Content.Shared.Goobstation.Silicons.AI.Components;

[RegisterComponent, NetworkedComponent]

public sealed partial class AIEyeComponent : Component
{
    [DataField("aicore")]
    public EntityUid CorePrototype;
}
