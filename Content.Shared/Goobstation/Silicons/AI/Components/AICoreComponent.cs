
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Toolshed.TypeParsers;

namespace Content.Shared.Goobstation.Silicons.AI.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AICoreComponent : Component
{
    [DataField("active"), ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public bool Active;

    [DataField("eyeprototype")]
    public EntityUid EyePrototype = EntityUid.Invalid;

    public static string LawModuleSlotId = "AICore-lawslot";

    [DataField]
    public ItemSlot lawModuleSlot = new();


}

[ByRefEvent]
public record struct AILawsUpdatedEvent;