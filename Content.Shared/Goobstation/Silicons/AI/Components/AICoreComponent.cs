
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Toolshed.TypeParsers;

namespace Content.Shared.Goobstation.Silicons.AI.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AICoreComponent : Component
{
    [DataField("activated"), ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public bool Activated;

    [DataField("coretype"), ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public string CoreType = "blue";

    [DataField("eyeprototype")]
    public EntityUid EyePrototype = EntityUid.Invalid;
}
