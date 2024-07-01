
using Content.Shared.DoAfter;
using Content.Shared.Goobstation.Silicons.AI.Components;
using Content.Shared.Interaction;
using Content.Shared.Lock;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Popups;
using Robust.Shared.Serialization;



namespace Content.Shared.Goobstation.Silicons.AI
{
    public abstract class SharedIntellicardSystem : EntitySystem
    {
        [Dependency] SharedDoAfterSystem _doAfterSystem = default!;
        [Dependency] EntityManager _entityManager = default!;
        [Dependency] SharedPopupSystem _popupSystem = default!;
        [Dependency] SharedAppearanceSystem _appearance = default!;
        [Dependency] SharedMindSystem _mind = default!;
        [Dependency] MetaDataSystem _metaData = default!;
        [Dependency] SharedTransformSystem _xform = default!;
 
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<IntellicardComponent, AfterInteractEvent>(onAfterInteract);
            
        }


        private void onAfterInteract(EntityUid uid, IntellicardComponent comp, AfterInteractEvent ev)
        {
         if (ev.Target == null)
            {
                return;
            }
            var target = ev.Target.GetValueOrDefault();
            if (_entityManager.HasComponent<AICoreComponent>(target))
            {

                var core = _entityManager.GetComponent<AICoreComponent>(target);
                var lockcomp = _entityManager.GetComponent<LockComponent>(target);
                if (lockcomp.Locked)
                {
                    _popupSystem.PopupEntity("Cant interact with a locked AI core!", ev.User);
                    return;
                }

                if (core.EyePrototype != EntityUid.Invalid && comp.storedMind == EntityUid.Invalid)
                {
                    var mindComp = _entityManager.GetComponent<MindContainerComponent>(core.EyePrototype);
                    comp.storedMind = core.EyePrototype;
                    core.EyePrototype = EntityUid.Invalid;
                    if (mindComp.HasMind)
                    {
                       _mind.TransferTo(mindComp.Mind.GetValueOrDefault(), ev.Used);
                    }
                    _appearance.SetData(target, AICoreVisuals.Status, AICoreStatus.Inactive);
                    _appearance.SetData(ev.Used, IntellicardVisuals.Status, IntellicardStatus.Filled);
                    comp.storedName = MetaData(target).EntityName;
                    var coreName = _entityManager.GetComponent<MetaDataComponent>(target).EntityName;
                    var cardName = _entityManager.GetComponent<MetaDataComponent>(ev.Used).EntityPrototype!.Name;
                    _metaData.SetEntityName(ev.Used, cardName + " (" + coreName + ")");
                    _metaData.SetEntityName(target, "Empty AI Core");
                    return;
                }

                if (core.EyePrototype != EntityUid.Invalid && comp.storedMind != EntityUid.Invalid)
                {
                    _popupSystem.PopupEntity("Cant transfer stored AI to active AI core!", ev.User);
                    return;
                }

                if (core.EyePrototype == EntityUid.Invalid && comp.storedMind != EntityUid.Invalid)
                {
                    core.EyePrototype = comp.storedMind;
                    comp.storedMind = EntityUid.Invalid;
                    //_entityManager.DeleteEntity(core.EyePrototype);
                    var mindComp = _entityManager.GetComponent<MindContainerComponent>(ev.Used);
                    //var newbody = _entityManager.SpawnAtPosition("AiObserver", Transform(target).Coordinates);
                    _xform.SetCoordinates(core.EyePrototype, Transform(target).Coordinates);
                    _entityManager.GetComponent<AIEyeComponent>(core.EyePrototype).CorePrototype = target;
                    //core.EyePrototype = newbody;
                    _metaData.SetEntityName(target, comp.storedName);
                    comp.storedName = "";
                    if (mindComp.HasMind)
                    {
                        _mind.TransferTo(mindComp.Mind.GetValueOrDefault(), core.EyePrototype);
                    }
                    _appearance.SetData(ev.Used, IntellicardVisuals.Status, IntellicardStatus.Empty);
                    _appearance.SetData(target, AICoreVisuals.Status, AICoreStatus.Active);
                    var cardName = _entityManager.GetComponent<MetaDataComponent>(ev.Used).EntityPrototype!.Name;
                    _metaData.SetEntityName(ev.Used, cardName);
                    
                    return;
                }

                if (core.EyePrototype == EntityUid.Invalid && comp.storedMind == EntityUid.Invalid)
                {
                    _popupSystem.PopupEntity("No stored AI to transfer into AI core!", ev.User);
                    return;
                }
            }
        }



    }
}
