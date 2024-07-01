

using Content.Shared.Silicons.Laws;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Goobstation.Silicons.AI.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class AIModuleComponent : Component
{
    [DataField("lawset"), ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<SiliconLawsetPrototype> Lawset;

    [DataField("freeformLaw"), ViewVariables(VVAccess.ReadWrite)]
    public string? Law;

    [DataField("lawSector"), ViewVariables(VVAccess.ReadWrite)]
    public int lawSector = 1; // Law number selected (for removal/addition)

    [DataField("isFreeform"), ViewVariables(VVAccess.ReadWrite)]
    public bool IsFreeform = false; // Freeform laws have to be numbered 15 or higher

    [DataField("isZeroth"), ViewVariables(VVAccess.ReadWrite)]
    public bool IsZeroth = false; // Will add a law with the order number of 0

    [DataField("removeLaw"), ViewVariables(VVAccess.ReadWrite)]
    public bool RemoveLaw = false; // Removes a law, must be higher than 1

    [DataField("purgeLaws"), ViewVariables(VVAccess.ReadWrite)]
    public bool PurgeLaws = false; // Removes all laws

    [DataField("resetLaws"), ViewVariables(VVAccess.ReadWrite)]
    public bool ResetLaws = false; // Removes all non-core laws

    [DataField("limitOverride"), ViewVariables(VVAccess.ReadWrite)]
    public bool LawSectorOverride = false; // Bypasses all limits on law sectors

    [DataField("identifierOverride"), ViewVariables(VVAccess.ReadWrite)]
    public string? IdentifierOverride; // Displays custom string instead of order number

    [DataField("lockedModule"), ViewVariables(VVAccess.ReadWrite)]
    public bool LockedModule = false; // If set to true, the module cannot be edited

    [DataField("useable"), ViewVariables(VVAccess.ReadWrite)]
    public bool Useable = true; // If set to false, the module cannot be uploaded to an AI
}