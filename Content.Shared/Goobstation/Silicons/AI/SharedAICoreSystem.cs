


using Content.Shared.Chat;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Goobstation.Silicons.AI.Components;
using Content.Shared.Interaction;
using Content.Shared.Lock;
using Content.Shared.Popups;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Goobstation.Silicons.AI
{

    public abstract class SharedAICoreSystem : EntitySystem
    {
        [Dependency] EntityManager _entityManager = default!;
        [Dependency] SharedPopupSystem _popupSystem = default!;
        [Dependency] SharedSiliconLawSystem _lawSystem = default!;
        [Dependency] IPrototypeManager _prototype = default!;
        [Dependency] MetaDataSystem _metaData = default!;
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

        private void onHandInteract(EntityUid uid, AICoreComponent comp, InteractHandEvent ev) // my horrible shitcode, will remake later lmao - Po
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

                    if (modcomp.Useable == false)
                    {
                        _popupSystem.PopupEntity("AI Module is unuseable!", uid);
                        return;
                    }
                       if (modcomp.PurgeLaws)
                        {
                        if (modcomp.LawSectorOverride)
                        {
                            _popupSystem.PopupEntity("All laws purged!", uid);
                            lawcomp.Lawset!.Laws.Clear();
                            _metaData.SetEntityName(loadedItem, "Used " + MetaData(loadedItem).EntityPrototype!.Name);
                            modcomp.Useable = false;
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
                        _metaData.SetEntityName(loadedItem, "Used " + MetaData(loadedItem).EntityPrototype!.Name);
                        modcomp.Useable = false;
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
                            _metaData.SetEntityName(loadedItem, "Used " + MetaData(loadedItem).EntityPrototype!.Name);
                            modcomp.Useable = false;
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
                        _metaData.SetEntityName(loadedItem, "Used " + MetaData(loadedItem).EntityPrototype!.Name);
                        modcomp.Useable = false;
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
                        _metaData.SetEntityName(loadedItem, "Used " + MetaData(loadedItem).EntityPrototype!.Name);
                        modcomp.Useable = false;
                        return;
                    }

                    if (modcomp.IsFreeform)
                    {
                        if (modcomp.LawSectorOverride)
                        {
                            var overridelaw = new SiliconLaw();
                            if (modcomp.IsZeroth)
                            {
                                overridelaw.Order = 0;
                            }
                            else
                            {
                                overridelaw.Order = modcomp.lawSector;
                            }
                            overridelaw.LawIdentifierOverride = modcomp.IdentifierOverride;
                            overridelaw.LawString = modcomp.Law!;
                            lawcomp.Lawset!.Laws.Add(overridelaw);
                            _popupSystem.PopupEntity("Added new freeform law!", uid);
                            _metaData.SetEntityName(loadedItem, "Used " + MetaData(loadedItem).EntityPrototype!.Name);
                            modcomp.Useable = false;
                            return;
                        }
                        if (modcomp.lawSector < 15)
                        {
                            _popupSystem.PopupEntity("Freeform laws must have a sector of 15 or higher!", uid);
                            return;
                        }
                        var newlaw = new SiliconLaw();
                        if (modcomp.IsZeroth)
                        {
                            newlaw.Order = 0;
                        }
                        else
                        {
                            newlaw.Order = modcomp.lawSector;
                        }
                        newlaw.LawIdentifierOverride = modcomp.IdentifierOverride;
                        newlaw.LawString = modcomp.Law!;
                        lawcomp.Lawset!.Laws.Add(newlaw);
                        _popupSystem.PopupEntity("Added new freeform law!", uid);
                        _metaData.SetEntityName(loadedItem, "Used " + MetaData(loadedItem).EntityPrototype!.Name);
                        modcomp.Useable = false;
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
                        _metaData.SetEntityName(loadedItem, "Used " + MetaData(loadedItem).EntityPrototype!.Name);
                        modcomp.Useable = false;
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
                    _metaData.SetEntityName(loadedItem, "Used " + MetaData(loadedItem).EntityPrototype!.Name);
                    modcomp.Useable = false;
                    return;
                }
                if (comp.EyePrototype != EntityUid.Invalid)
                {
                    _entityManager.GetComponent<SiliconLawProviderComponent>(comp.EyePrototype).Lawset = lawcomp.Lawset;
                    _entityManager.GetComponent<SiliconLawProviderComponent>(comp.EyePrototype).Laws = lawcomp.Laws;
                    var aiEv = new AILawsUpdatedEvent();
                    RaiseLocalEvent(comp.EyePrototype, ref aiEv);
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
