
using Content.Shared.Actions;
using Content.Shared.Administration.Logs;
using Content.Shared.Goobstation.Silicons.AI.Components;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Goobstation.Silicons.AI
{

    public abstract class AIShellSystem : EntitySystem
    {
        [Dependency] private readonly SharedMindSystem _mind = default!;
        [Dependency] private readonly EntityManager _entityManager = default!;
        [Dependency] private readonly SharedPopupSystem _log = default!;
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<AIShellComponent, ComponentRemove>(onComponentRemove);
            SubscribeLocalEvent<AIShellComponent, MapInitEvent>(onMapInit);
            //SubscribeLocalEvent<AIShellComponent, ComponentInit>(onCompInit);
        }



        private void onComponentRemove(EntityUid uid, AIShellComponent comp, ComponentRemove ev)
        {
            //comp.
            if (comp.eyePrototype != EntityUid.Invalid)
            {
                var mindComp = _entityManager.GetComponent<MindContainerComponent>(uid);
                _mind.TransferTo(mindComp.Mind.GetValueOrDefault(), comp.eyePrototype);
            }
        }

        private void onMapInit(EntityUid uid, AIShellComponent comp, MapInitEvent ev)
        {
            _actions.AddAction(uid, ref comp.ActionEntity, comp.Action);
        }



    }
}