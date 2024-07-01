

using Content.Server.Actions;
using Content.Shared.Goobstation.Silicons.AI;
using Content.Shared.Goobstation.Silicons.AI.Components;

namespace Content.Server.Goobstation.Silicons.AI
{
    public sealed class AICoreSystem : SharedAICoreSystem
    {
        [Dependency] private readonly ActionsSystem _actions = default!;
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<AIShellComponent, MapInitEvent>(onMapInit);
            

        }

        private void onMapInit(EntityUid uid, AIShellComponent comp, MapInitEvent ev)
        {
            _actions.AddAction(uid, ref comp.ActionEntity, comp.Action);
        }
    }
}
