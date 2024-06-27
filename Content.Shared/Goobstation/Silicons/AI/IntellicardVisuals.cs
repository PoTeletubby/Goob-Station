

using Robust.Shared.Serialization;

namespace Content.Shared.Goobstation.Silicons.AI;

[Serializable, NetSerializable]
public enum IntellicardVisuals : byte
{
    Status
}

[Serializable, NetSerializable]
public enum IntellicardStatus : byte
{
    Empty,
    Filled
}
