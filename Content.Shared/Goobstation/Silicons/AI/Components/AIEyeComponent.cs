



using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Goobstation.Silicons.AI.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AIEyeComponent : Component
{
    [DataField("aicore"), AutoNetworkedField]
    public EntityUid CorePrototype;

    [DataField("selectedhologram"), AutoNetworkedField]
    public EntProtoId SelectedHologram;

    [DataField, AutoNetworkedField]
    public string RTCAction = "ActionAIReturnToCore";

    [DataField, AutoNetworkedField]
    public EntityUid? RTCActionEntity;
}

public sealed partial class ReturnToCoreEvent : InstantActionEvent { }