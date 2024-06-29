


using Content.Shared.Containers.ItemSlots;
using Content.Shared.Goobstation.Silicons.AI.Components;
using Content.Shared.Interaction;
using Content.Shared.Lock;
using Content.Shared.Popups;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Goobstation.Silicons.AI
{

    public abstract class SharedAICoreSystem : EntitySystem
    {
        [Dependency] EntityManager _entityManager = default!;
        [Dependency] SharedPopupSystem _popupSystem = default!;
        [Dependency] SharedSiliconLawSystem _lawSystem = default!;
        [Dependency] IPrototypeManager _prototype = default!;
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<AICoreComponent, LockToggledEvent>(onLockToggled);
            SubscribeLocalEvent<AICoreComponent, InteractHandEvent>(onHandInteract);
        }

        private void onLockToggled(EntityUid uid, AICoreComponent comp, LockToggledEvent ev)
        {
            _entityManager.GetComponent<ItemSlotsComponent>(uid).Slots["AICore-lawslot"].Locked = ev.Locked;
        }

        private void onHandInteract(EntityUid uid, AICoreComponent comp, InteractHandEvent ev)
        {
            if (ev.Handled)
                return;
            var lawslot = _entityManager.GetComponent<ItemSlotsComponent>(uid).Slots["AICore-lawslot"];
            if (lawslot.HasItem)
            {
                var loadedItem = lawslot.Item.GetValueOrDefault();
                if (comp.EyePrototype == EntityUid.Invalid)
                {
                    _popupSystem.PopupEntity("No AI loaded into core to upload to!", uid);
                    return;
                }

                if (!HasComp<AIModuleComponent>(lawslot.Item))
                {
                    _popupSystem.PopupClient("Inserted item has no AI Module component, how did you do this?", ev.User);
                    return;
                }
                var lawcomp = _entityManager.GetComponent<SiliconLawProviderComponent>(uid);
                if (CheckIfValid(loadedItem, ev.User, uid))
                {
                    var modcomp = _entityManager.GetComponent<AIModuleComponent>(loadedItem);
                    if (modcomp.PurgeLaws)
                    {
                        if (modcomp.LawSectorOverride)
                        {
                            _popupSystem.PopupEntity("All laws purged!", uid);
                            lawcomp.Lawset!.Laws.Clear();
                            return;
                        }
                        List<SiliconLaw> lawTable = new List<SiliconLaw>();
                        foreach (var law in lawcomp.Lawset!.Laws)
                        {
                            if (law.Order > 0)
                            {
                                lawTable.Add(law);
                            }
                        }
                        foreach(var law in lawTable)
                        {
                            lawcomp.Lawset.Laws.Remove(law);
                        }

                        _popupSystem.PopupEntity("All detected laws purged!", uid);
                        return;
                    }

                    if (modcomp.ResetLaws)
                    {
                        var lawset = lawcomp.Laws.Id!;
                        var proto = _prototype.Index<SiliconLawsetPrototype>(lawset!);
                        List<SiliconLaw> lawTable = new List<SiliconLaw>();
                        if (modcomp.LawSectorOverride)
                        {
                            _popupSystem.PopupEntity("All laws reset!", uid);
                            lawcomp.Lawset!.Laws.Clear();
                            foreach (var law in proto.Laws)
                            {
                                lawcomp.Lawset.Laws.Add(_prototype.Index<SiliconLawPrototype>(law));
                            }
                            return;
                        }
                        foreach (var law in lawcomp.Lawset!.Laws)
                        {
                            if (law.Order > 0)
                            {
                                lawTable.Add(law);
                            }
                        }
                        foreach (var law in lawTable)
                        {
                            lawcomp.Lawset.Laws.Remove(law);
                        }
                        foreach (var law in proto.Laws)
                        {
                            lawcomp.Lawset.Laws.Add(_prototype.Index<SiliconLawPrototype>(law));
                        }
                        _popupSystem.PopupEntity("All detected laws reset!", uid);
                        return;
                    }

                    if (modcomp.RemoveLaw)
                    {
                        var law = lawcomp.Lawset!.Laws[modcomp.lawSector];
                        if (law == null)
                        {
                            _popupSystem.PopupEntity("Law number " + modcomp.lawSector+ " could not be found!", uid);
                            return;
                        }
                        if (modcomp.LawSectorOverride)
                        {
                            lawcomp.Lawset.Laws.Remove(law);
                            _popupSystem.PopupEntity("Removed law number "+modcomp.lawSector, uid);
                            return;
                        }
                        if (modcomp.lawSector <= 1)
                        {
                            _popupSystem.PopupEntity("Failed to remove law number " + modcomp.lawSector, uid);
                            return;
                        }
                        lawcomp.Lawset.Laws.Remove(law);
                        _popupSystem.PopupEntity("Removed law number " + modcomp.lawSector, uid);
                        return;
                    }

                    if (modcomp.IsFreeform)
                    {
                        if (modcomp.LawSectorOverride)
                        {
                            var overridelaw = new SiliconLaw();
                            overridelaw.Order = modcomp.lawSector;
                            overridelaw.LawString = modcomp.Law!;
                            lawcomp.Lawset!.Laws.Add(overridelaw);
                            _popupSystem.PopupEntity("Added new freeform law!", uid);
                            return;
                        }
                        if (modcomp.lawSector < 15)
                        {
                            _popupSystem.PopupEntity("Freeform laws must have a sector of 15 or higher!", uid);
                            return;
                        }
                        var newlaw = new SiliconLaw();
                        newlaw.Order = modcomp.lawSector;
                        newlaw.LawString = modcomp.Law!;
                        lawcomp.Lawset!.Laws.Add(newlaw);
                        _popupSystem.PopupEntity("Added new freeform law!", uid);
                        return;
                    }

                    var proto2 = _prototype.Index<SiliconLawsetPrototype>(modcomp.Lawset.Id);
                    List<SiliconLaw> lawsetTable = new List<SiliconLaw>();
                    if (modcomp.LawSectorOverride)
                    {
                        _popupSystem.PopupEntity("AI lawset successfully changed!", uid);
                        lawcomp.Lawset!.Laws.Clear();
                        foreach (var law in proto2.Laws)
                        {
                            lawcomp.Lawset.Laws.Add(_prototype.Index<SiliconLawPrototype>(law));
                        }
                        return;
                    }
                    foreach (var law in lawcomp.Lawset!.Laws)
                    {
                        if (law.Order > 0)
                        {
                            lawsetTable.Add(law);
                        }
                    }
                    foreach (var law in lawsetTable)
                    {
                        lawcomp.Lawset.Laws.Remove(law);
                    }
                    foreach (var law in proto2.Laws)
                    {
                        lawcomp.Lawset.Laws.Add(_prototype.Index<SiliconLawPrototype>(law));
                    }
                    _popupSystem.PopupEntity("AI lawset successfully changed!", uid);
                    return;
                }
                if (comp.EyePrototype != EntityUid.Invalid)
                {
                    _entityManager.GetComponent<SiliconLawProviderComponent>(comp.EyePrototype).Lawset = lawcomp.Lawset;
                    _entityManager.GetComponent<SiliconLawProviderComponent>(comp.EyePrototype).Laws = lawcomp.Laws;
                }
                ev.Handled = true;
            }
        }

        private bool CheckIfValid(EntityUid uid, EntityUid User, EntityUid Core)
        {
            if (!HasComp<AIModuleComponent>(uid))
            {
                _popupSystem.PopupClient("Inserted item has no AI Module component, how did you do this?", User); // redundancy
                return false;
            }
                         // MODULE PRIORITY LEVELS \\
            // Purge > Reset > Lawset > Remove > Freeform \\

            //var comp = _entityManager.GetComponent<AIModuleComponent>(uid);
            return true;
        }
    }
}
