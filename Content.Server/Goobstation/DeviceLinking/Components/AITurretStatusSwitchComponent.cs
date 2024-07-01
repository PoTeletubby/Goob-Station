using Content.Server.DeviceLinking.Systems;
using Content.Server.Goobstation.DeviceLinking.Systems;
using Content.Shared.DeviceLinking;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Goobstation.DeviceLinking.Components;

/// <summary>
///     Simple switch that will fire ports when toggled on or off. A button is jsut a switch that signals on the
///     same port regardless of its state.
/// </summary>
[RegisterComponent, Access(typeof(AITurretModeSwitchSystem))]
public sealed partial class AITurretStatusSwitchComponent : Component
{
    /// <summary>
    ///     The port that gets signaled when the switch turns on.
    /// </summary>
    [DataField("currentMode")]
    public string CurrentMode = "On"; // On or Off

    [DataField("clickSound")]
    public SoundSpecifier ClickSound = new SoundPathSpecifier("/Audio/Machines/lightswitch.ogg");

    [DataField("transmitDistance")]
    public float TransmitDistance = 25f;
}