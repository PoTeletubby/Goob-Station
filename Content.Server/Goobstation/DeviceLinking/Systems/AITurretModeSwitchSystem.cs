using Content.Server.Access.Systems;
using Content.Server.DeviceLinking.Components;
using Content.Server.DeviceNetwork;
using Content.Server.Goobstation.DeviceLinking.Components;
using Content.Server.Goobstation.Silicons.AI;
using Content.Server.Popups;
using Content.Shared.Access.Systems;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Interaction;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using SQLitePCL;

namespace Content.Server.Goobstation.DeviceLinking.Systems;

public sealed class AITurretModeSwitchSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly EntityManager _entities = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AITurretModeSwitchComponent, ActivateInWorldEvent>(OnActivated);
    }

    private void OnActivated(EntityUid uid, AITurretModeSwitchComponent comp, ActivateInWorldEvent args)
    {
        if (args.Handled || !args.Complex)
            return;

        if (!_accessReader.IsAllowed(uid, args.Target))
        {
            _popup.PopupEntity("You aren't authorized to use this!", args.Target);
            return;
        }

        if (comp.CurrentMode == "Stun")
        {
            comp.CurrentMode = "Kill";
            TransmitSignal(args.Target, comp.CurrentMode, comp);
            return;
        }

        if (comp.CurrentMode == "Kill")
        {
            comp.CurrentMode = "Stun";
            TransmitSignal(args.Target, comp.CurrentMode, comp);
            return;
        }


        _audio.PlayPvs(comp.ClickSound, uid, AudioParams.Default.WithVariation(0.125f).WithVolume(8f));

        args.Handled = true;
    }

    private void TransmitSignal(EntityUid target, string toTransmit, AITurretModeSwitchComponent comp)
    {
        var allTurrets = _lookup.GetEntitiesInRange<AITurretComponent>(Transform(target).Coordinates, comp.TransmitDistance);
        _popup.PopupEntity("Turrets set to " + comp.CurrentMode, target);
        foreach (var turret in allTurrets)
        {
            var tcomp = _entities.GetComponent<BatteryWeaponFireModesComponent>(turret);
            var ccomp = _entities.GetComponent<ProjectileBatteryAmmoProviderComponent>(turret);
            var gcomp = _entities.GetComponent<GunComponent>(turret);

            if (comp.CurrentMode == "Stun")
            {
               ccomp.Prototype = tcomp.FireModes[0].Prototype;
               ccomp.FireCost = tcomp.FireModes[0].FireCost;
               gcomp.FireRateModified = 0.5f;
               gcomp.SoundGunshotModified = new SoundPathSpecifier("/Audio/Weapons/Guns/Gunshots/taser.ogg");
            }
            if (comp.CurrentMode == "Kill")
            {
                ccomp.Prototype = tcomp.FireModes[1].Prototype;
                ccomp.FireCost = tcomp.FireModes[1].FireCost;
                gcomp.FireRateModified = 6f;
                gcomp.SoundGunshotModified = new SoundPathSpecifier("/Audio/Weapons/Guns/Gunshots/gun_sentry.ogg");
            }
        }
    }
}