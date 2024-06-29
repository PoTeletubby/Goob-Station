



using Content.Server.Actions;
using Content.Server.Administration.Logs.Converters;
using Content.Server.Chat.Systems;
using Content.Server.Chat.TypingIndicator;
using Content.Server.Chat.V2;
using Content.Server.Chat.V2.Commands;
using Content.Server.GameTicking;
using Content.Server.Radio;
using Content.Server.Radio.Components;
using Content.Server.Radio.EntitySystems;
using Content.Server.RoundEnd;
using Content.Server.Shuttles.Systems;
using Content.Server.Sprite;
using Content.Server.Station.Systems;
using Content.Shared.Chat;
using Content.Shared.Chat.TypingIndicator;
using Content.Shared.Emoting;
using Content.Shared.Goobstation.Silicons.AI;
using Content.Shared.Goobstation.Silicons.AI.Components;
using Content.Shared.NPC.Events;
using Content.Shared.Popups;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Network;
using Robust.Shared.Player;
using System.Diagnostics;
using System.Linq;
using System.Threading.Channels;

namespace Content.Server.Goobstation.Silicons.AI
{

    public sealed class AIEyeSystem : SharedAIEyeSystem
    {
        [Dependency] private readonly EntityLookupSystem _lookup = default!;
        [Dependency] private readonly EntityManager _entity = default!;
        [Dependency] private readonly StationSystem _station = default!;
        [Dependency] private readonly ChatSystem _chat = default!;
        [Dependency] private readonly RadioSystem _radio = default!;
        [Dependency] private readonly MetaDataSystem _metaData = default!;
        [Dependency] private readonly AppearanceSystem _appearance = default!;
        [Dependency] private readonly TransformSystem _transform = default!;
        [Dependency] private readonly ActionsSystem _actions = default!;
        [Dependency] private readonly SharedUserInterfaceSystem _userInterface = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;

        List<string> cores = new List<string> {"AICore", "AlienAICore", "AngelAICore" , "ClownAICore", "DatabaseAICore", "GentooAICore", "GlitchmanAICore" ,
        "GoonAICore", "HadesAICore", "HalAICore", "HouseAICore", "InvertedAICore", "MonochromeAICore", "MuricaAICore", "RedAICore", "ThinkingAICore", "WeirdAICore" };
        static Random rand = new Random();

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<AIEyeComponent, PlayerSpawnCompleteEvent>(onPlayerSpawn);
            SubscribeLocalEvent<AIEyeComponent, EntitySpokeEvent>(onAISpeak);
            SubscribeLocalEvent<AIEyeComponent, ReturnToCoreEvent>(onReturnToCore);
            SubscribeLocalEvent<AIEyeComponent, MapInitEvent>(onMapInit);
        }

        private void onAISpeak(EntityUid uid, AIEyeComponent comp, EntitySpokeEvent ev)
        {
          if (comp.CorePrototype != EntityUid.Invalid)
          {
             if (ev.Channel != null && TryComp(uid, out ActiveRadioComponent? activeRadio) && activeRadio.Channels.Contains(ev.Channel.ID))
                {
                    _chat.TrySendInGameICMessage(comp.CorePrototype, ev.Message, InGameICChatType.Whisper, false);
                    _radio.SendRadioMessage(comp.CorePrototype, ev.Message, ev.Channel, comp.CorePrototype);
                    ev.Channel = null;
                    return;
                }
             if (ev.ObfuscatedMessage != null)
                {
                    _chat.TrySendInGameICMessage(comp.CorePrototype, ev.Message, InGameICChatType.Whisper, false);
                    ev.Channel = null;
                    return;
                }
            _chat.TrySendInGameICMessage(comp.CorePrototype, ev.Message, InGameICChatType.Speak, false);
                ev.Channel = null;
            }
        }

        private void onMapInit(EntityUid uid, AIEyeComponent component, MapInitEvent ev)
        {
            _actions.AddAction(uid, ref component.RTCActionEntity, component.RTCAction);
        }

        private void onPlayerSpawn(EntityUid uid, AIEyeComponent comp, PlayerSpawnCompleteEvent ev)
        {
            var spawned = ev.Mob;
            _actions.AddAction(uid, ref comp.RTCActionEntity, comp.RTCAction);
            var query = EntityQueryEnumerator<AICoreComponent>();
            while (query.MoveNext(out var ent, out var core))
            {
                if (core.EyePrototype == EntityUid.Invalid)
                {
                    core.EyePrototype = spawned;
                    _entity.GetComponent<AIEyeComponent>(spawned).CorePrototype = ent;
                    _metaData.SetEntityDescription(ent, "An AI core currently housing a Nanotrasen brand AI.");
                    _appearance.SetData(ent, AICoreVisuals.Status, AICoreStatus.Active);
                    _transform.SetCoordinates(spawned, Transform(ent).Coordinates);
                    return;
                }
            }
             int rcore = rand.Next(cores.Count);
             var newcore = _entity.SpawnAtPosition(cores[rcore], Transform(spawned).Coordinates);
            _entity.GetComponent<AICoreComponent>(newcore).EyePrototype = spawned;
            _entity.GetComponent<AIEyeComponent>(spawned).CorePrototype = newcore;
            _metaData.SetEntityDescription(newcore, "An AI core currently housing a Nanotrasen brand AI.");
            _appearance.SetData(newcore, AICoreVisuals.Status, AICoreStatus.Active);
            _transform.SetCoordinates(spawned, Transform(newcore).Coordinates);
            _entity.GetComponent<SiliconLawProviderComponent>(spawned).Lawset = _entity.GetComponent<SiliconLawProviderComponent>(newcore).Lawset;
            _entity.GetComponent<SiliconLawProviderComponent>(spawned).Laws = _entity.GetComponent<SiliconLawProviderComponent>(newcore).Laws;

        }

        private void onReturnToCore(EntityUid uid, AIEyeComponent comp, ReturnToCoreEvent ev)
        {
            ev.Handled = true;
            if (comp.CorePrototype != EntityUid.Invalid)
            {
                _transform.SetCoordinates(uid, Transform(comp.CorePrototype).Coordinates);
            }

        }
    }

}

