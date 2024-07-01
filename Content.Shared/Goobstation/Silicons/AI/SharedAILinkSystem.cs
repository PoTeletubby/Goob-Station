


namespace Content.Shared.Goobstation.Silicons.AI
{

    public abstract class SharedAILinkSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();
            //SubscribeLocalEvent<AICoreComponent, LockToggledEvent>(onLockToggled);
        }
    }
}