


using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Goobstation.Silicons.AI.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AIShellComponent : Component
{
    [DataField("controlled"), AutoNetworkedField]
    public bool Controlled = false;

    [DataField("eyePrototype"), AutoNetworkedField]
    public EntityUid eyePrototype = EntityUid.Invalid;

    [DataField]
    public EntProtoId Action = "ActionAILeaveShell";

    [DataField]
    public EntityUid? ActionEntity;
}
