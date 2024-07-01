using Content.Client.Goobstation.Silicons.AI.UI;
using Content.Shared.Goobstation.Silicons.AI.Components;
using Content.Shared.Labels;
using Content.Shared.Labels.Components;
using Robust.Client.GameObjects;

namespace Content.Client.Labels.UI
{
    /// <summary>
    /// Initializes a <see cref="HandLabelerWindow"/> and updates it when new server messages are received.
    /// </summary>
    public sealed class AIModuleBoundUserInterface : BoundUserInterface
    {
        [Dependency] private readonly IEntityManager _entManager = default!;

        [ViewVariables]
        private AIModuleWindow? _window;

        public AIModuleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
            IoCManager.InjectDependencies(this);
        }

        protected override void Open()
        {
            base.Open();

            _window = new AIModuleWindow();
            if (State != null)
                UpdateState(State);

            _window.OpenCentered();

            _window.OnClose += Close;
            _window.OnFreeformChanged += OnFreeformChanged;
            Reload();
        }



        private void OnFreeformChanged(string newLaw)
        {
            // Focus moment
            if (_entManager.TryGetComponent(Owner, out AIModuleComponent? mod) &&
                 mod.Law!.Equals(newLaw))
                return;
        }

        public void Reload()
        {
            if (_window == null || !_entManager.TryGetComponent(Owner, out AIModuleComponent? component))
                return;

            _window.SetCurrentLaw(component.Law!);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;
            _window?.Dispose();
        }
    }

}