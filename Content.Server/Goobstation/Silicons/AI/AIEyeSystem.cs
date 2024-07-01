



using Content.Server.Actions;
using Content.Server.Administration.Logs;
using Content.Server.Administration.Logs.Converters;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Chat.TypingIndicator;
using Content.Server.Chat.V2;
using Content.Server.Chat.V2.Commands;
using Content.Server.Doors.Systems;
using Content.Server.GameTicking;
using Content.Server.Mind;
using Content.Server.Power.EntitySystems;
using Content.Server.Radio;
using Content.Server.Radio.Components;
using Content.Server.Radio.EntitySystems;
using Content.Server.RoundEnd;
using Content.Server.Shuttles.Systems;
using Content.Server.Sprite;
using Content.Server.Station.Systems;
using Content.Shared.Chat;
using Content.Shared.Chat.TypingIndicator;
using Content.Shared.Database;
using Content.Shared.Doors.Components;
using Content.Shared.Emoting;
using Content.Shared.Goobstation.Silicons.AI;
using Content.Shared.Goobstation.Silicons.AI.Components;
using Content.Shared.Mind.Components;
using Content.Shared.NPC.Events;
using Content.Shared.Popups;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Utility;
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
        [Dependency] private readonly IChatManager _chatManager = default!;
        [Dependency] private readonly DoorSystem _door = default!;
        [Dependency] private readonly MindSystem _mind = default!;
        [Dependency] private readonly IAdminLogManager _adminLog = default!;
        [Dependency] private readonly AirlockSystem _airlock = default!;

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
            SubscribeLocalEvent<AIEyeComponent, AILawsUpdatedEvent>(onAILawsUpdated);
            SubscribeLocalEvent<AIShellComponent, GetVerbsEvent<Verb>>(AddShellVerbs);

        }

        private void AddShellVerbs(EntityUid uid, AIShellComponent comp, GetVerbsEvent<Verb> ev)
        {
            //_log.PopupEntity("on get verb", ev.User);
            if (comp.eyePrototype != EntityUid.Invalid || !ev.CanInteract || ev.Hands == null)
                return;

            if (!HasComp<AIEyeComponent>(ev.User) && !HasComp<AIShellComponent>(ev.User))
                return;
            // _log.PopupEntity("has ai eye comp", ev.User);
            if (HasComp<AIEyeComponent>(ev.User))
            {
                ev.Verbs.Add(new Verb
                {
                    Act = () => InhabitShell(ev.User, ev.Target, comp),
                    Text = "Inhabit Shell",
                    Message = "Take control of this chassis",
                    Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/settings.svg.192dpi.png")),
                    ConfirmationPopup = true,
                    Priority = 1,
                });
            }
        }

        private void InhabitShell(EntityUid user, EntityUid entity, AIShellComponent component)
        {
            component.Controlled = true;
            component.eyePrototype = user;
            var mindComp = _entity.GetComponent<MindContainerComponent>(user);
            _mind.TransferTo(mindComp.Mind.GetValueOrDefault(), entity);

            if (!TryComp<ActorComponent>(user, out var actor))
                return;
            var msg = Loc.GetString("ai-notify-shell");
            var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", msg));
            _chatManager.ChatMessageToOne(ChatChannel.Server, msg, wrappedMessage, default, false,
                actor.PlayerSession.Channel, colorOverride: Color.FromHex("#2ed2fd"));
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

        private void onAILawsUpdated(EntityUid uid, AIEyeComponent comp, AILawsUpdatedEvent ev)
        {
            if (!TryComp<ActorComponent>(uid, out var actor))
                return;
            var msg = Loc.GetString("laws-notify");
            var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", msg));
            _chatManager.ChatMessageToOne(ChatChannel.Server, msg, wrappedMessage, default, false,
                actor.PlayerSession.Channel, colorOverride: Color.FromHex("#2ed2fd"));
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

