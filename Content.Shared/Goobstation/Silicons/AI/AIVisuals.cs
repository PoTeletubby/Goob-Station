

using Robust.Shared.Serialization;

namespace Content.Shared.Goobstation.Silicons.AI;

[Serializable, NetSerializable]
public enum AICoreVisuals : byte
{
    Status
}

[Serializable, NetSerializable]
public enum AICoreStatus : byte
{
    Inactive,
    Active,
    Dead
}
