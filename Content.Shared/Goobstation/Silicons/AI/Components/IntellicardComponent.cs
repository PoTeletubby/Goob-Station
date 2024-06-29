

using Robust.Shared.GameStates;

namespace Content.Shared.Goobstation.Silicons.AI.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class IntellicardComponent : Component
{
    [DataField("storedMind"), AutoNetworkedField]
    public EntityUid storedMind = EntityUid.Invalid;

    [DataField("storedName"), ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public string storedName = "Robor";
}
