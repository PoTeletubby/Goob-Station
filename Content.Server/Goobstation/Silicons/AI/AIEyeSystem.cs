



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
using Content.Server.Station.Systems;
using Content.Shared.Chat;
using Content.Shared.Chat.TypingIndicator;
using Content.Shared.Emoting;
using Content.Shared.Goobstation.Silicons.AI;
using Content.Shared.Goobstation.Silicons.AI.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Network;
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
        

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<AIEyeComponent, PlayerSpawnCompleteEvent>(onPlayerSpawn);
            SubscribeLocalEvent<AIEyeComponent, AISpokeEvent>(onAISpeak);

        }

        
        private void onAISpeak(EntityUid uid, AIEyeComponent comp, AISpokeEvent ev)
        {
          if (comp.CorePrototype != EntityUid.Invalid)
          {
             if (ev.Channel != null && TryComp(uid, out ActiveRadioComponent? activeRadio) && activeRadio.Channels.Contains(ev.Channel.ID))
                {
                    _radio.SendRadioMessage(comp.CorePrototype, ev.Message, ev.Channel, comp.CorePrototype);
                    ev.Channel = null;
                    return;
                }
             if (ev.ObfuscatedMessage != null)
                {
                 _chat.TrySendInGameICMessage(comp.CorePrototype, ev.Message, InGameICChatType.Whisper, false);
                    //ev.
                    ev.Channel = null;
                    return;
                }
            _chat.TrySendInGameICMessage(comp.CorePrototype, ev.Message, InGameICChatType.Speak, false);
                ev.Channel = null;
            }
        }

        // End of Goobstation (AISpeak)

        private void onPlayerSpawn(EntityUid uid, AIEyeComponent comp, PlayerSpawnCompleteEvent ev)
        {
            var spawned = ev.Mob;
            var mainstation = _station.GetOwningStation(spawned).GetValueOrDefault();

            foreach (var ent in _lookup.GetEntitiesInRange(mainstation, 10000f))
            {
              if (_entity.HasComponent<AICoreComponent>(ent))
              {
                var corecomp = _entity.GetComponent<AICoreComponent>(ent);
                if (corecomp.EyePrototype == EntityUid.Invalid)
                {
                   corecomp.EyePrototype = spawned;
                   _entity.GetComponent<AIEyeComponent>(spawned).CorePrototype = ent;
                   return;
                }
              }
            }
          var newcore = _entity.SpawnAtPosition("AICore", Transform(spawned).Coordinates);
         _entity.GetComponent<AICoreComponent>(newcore).EyePrototype = spawned;
         _entity.GetComponent<AIEyeComponent>(spawned).CorePrototype = newcore;
        }
    }
}

