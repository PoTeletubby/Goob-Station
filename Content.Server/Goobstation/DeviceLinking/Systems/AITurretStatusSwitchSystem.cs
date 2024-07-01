using Content.Server.DeviceLinking.Components;
using Content.Server.DeviceNetwork;
using Content.Server.Goobstation.DeviceLinking.Components;
using Content.Server.Goobstation.Silicons.AI;
using Content.Server.Popups;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Interaction;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Systems;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using SQLitePCL;

namespace Content.Server.Goobstation.DeviceLinking.Systems;

public sealed class AITurretModeStatusSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly EntityManager _entities = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly NpcFactionSystem _factions = default!;
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AITurretStatusSwitchComponent, ActivateInWorldEvent>(OnActivated);
    }

    private void OnActivated(EntityUid uid, AITurretStatusSwitchComponent comp, ActivateInWorldEvent args)
    {

        if (args.Handled || !args.Complex)
            return;

        if (!_accessReader.IsAllowed(uid, args.Target))
        {
            _popup.PopupEntity("You aren't authorized to use this!", args.Target);
            return;
        }

        if (comp.CurrentMode == "On")
        {
            comp.CurrentMode = "Off";
            TransmitSignal(args.Target, comp.CurrentMode, comp);
            return;
        }

        if (comp.CurrentMode == "Off")
        {
            comp.CurrentMode = "On";
            TransmitSignal(args.Target, comp.CurrentMode, comp);
            return;
        }


        _audio.PlayPvs(comp.ClickSound, uid, AudioParams.Default.WithVariation(0.125f).WithVolume(8f));

        args.Handled = true;
    }

    private void TransmitSignal(EntityUid target, string toTransmit, AITurretStatusSwitchComponent comp)
    {
        var allTurrets = _lookup.GetEntitiesInRange<AITurretComponent>(Transform(target).Coordinates, comp.TransmitDistance);
        _popup.PopupEntity("Turrets are now " + comp.CurrentMode, target);
        foreach (var turret in allTurrets)
        {
            var tcomp = _entities.GetComponent<NpcFactionMemberComponent>(turret);

            if (comp.CurrentMode == "On")
            {
                _factions.AddFaction((turret, tcomp), "AITurret");
                _factions.RemoveFaction((turret, tcomp), "Passive");
            }
            if (comp.CurrentMode == "Off")
            {
                _factions.AddFaction((turret, tcomp), "Passive");
                _factions.RemoveFaction((turret, tcomp), "AITurret");
            }
        }
    }
}