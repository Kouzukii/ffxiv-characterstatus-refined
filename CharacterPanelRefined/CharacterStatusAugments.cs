using System;
using System.Collections.Generic;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace CharacterPanelRefined; 

public sealed unsafe class CharacterStatusAugments : IDisposable {
    private readonly CharacterPanelRefinedPlugin plugin;
    private readonly Tooltips tooltips = new();
    
    private JobId lastJob;
    
    private AtkUnitBase* characterStatusPtr;
    private AtkTextNode* dhChancePtr;
    private AtkTextNode* dhDamagePtr;
    private AtkTextNode* detDmgIncreasePtr;
    private AtkTextNode* critDmgPtr;
    private AtkTextNode* critChancePtr;
    private AtkTextNode* critDmgIncreasePtr;
    private AtkTextNode* physMitPtr;
    private AtkTextNode* magicMitPtr;
    private AtkTextNode* sksSpeedIncreasePtr;
    private AtkTextNode* sksGcdPtr;
    private AtkTextNode* spsSpeedIncreasePtr;
    private AtkTextNode* spsGcdPtr;
    private AtkTextNode* spsAltGcdPtr;
    private AtkTextNode* tenMitPtr;
    private AtkTextNode* pieManaPtr;
    private AtkTextNode* expectedDamagePtr;
    private AtkTextNode* expectedHealPtr;
    private AtkTextNode* ilvlSyncPtr;
    private AtkResNode* attributesPtr;
    private AtkResNode* offensivePtr;
    private AtkResNode* defensivePtr;
    private AtkResNode* physPropertiesPtr;
    private AtkResNode* gearPtr;
    private AtkResNode* pietyPtr;
    private AtkResNode* tenacityPtr;
    private AtkResNode* spellSpeedPtr;
    private AtkResNode* skillSpeedPtr;
    private AtkTextNode* craftsmanshipBasePtr;
    private AtkTextNode* controlBasePtr;
    private AtkTextNode* cpPtr;
    private AtkTextNode* cpBasePtr;
    private AtkTextNode* gatheringBasePtr;
    private AtkTextNode* perceptionBasePtr;
    private AtkTextNode* gpPtr;
    private AtkTextNode* gpBasePtr;

    public CharacterStatusAugments(CharacterPanelRefinedPlugin plugin) => this.plugin = plugin;

    internal void OnSetup(AddonEvent type, AddonArgs args) {
        var atkUnitBase = (AtkUnitBase*)args.Addon;
        var uiState = UIState.Instance();
        var job = (JobId)uiState->PlayerState.CurrentClassJobId;
        var lvl = uiState->PlayerState.CurrentLevel;

        attributesPtr = atkUnitBase->UldManager.SearchNodeById(26);
        var mndNode = attributesPtr->ChildNode;
        mndNode->X = 10;
        SetTooltip((AtkComponentNode*)mndNode, Tooltips.Entry.MainStat);
        mndNode->Y = 40;
        var intNode = mndNode->PrevSiblingNode;
        intNode->X = 10;
        intNode->Y = 40;
        SetTooltip((AtkComponentNode*)intNode, Tooltips.Entry.MainStat);

        var vitalityNode = intNode->PrevSiblingNode;
        vitalityNode->Y = 20;
        SetTooltip((AtkComponentNode*)vitalityNode, Tooltips.Entry.Vitality);

        var attributesHeight = 130;

        var mentProperties = atkUnitBase->UldManager.SearchNodeById(58);
        var magAtkPotency = mentProperties->ChildNode->PrevSiblingNode;
        var healMagPotency = magAtkPotency->PrevSiblingNode;
        if (plugin.Configuration.ShowAvgHealing) {
            attributesHeight += 20;
            healMagPotency->Y = -attributesHeight - 10;
            expectedHealPtr = AddStatRow((AtkComponentNode*)healMagPotency, Localization.Panel_Heal_per_100_Potency, true);
            SetTooltip(expectedHealPtr, Tooltips.Entry.ExpectedHeal);
        } else {
            healMagPotency->ToggleVisibility(false);
        }
        if (plugin.Configuration.ShowAvgDamage) {
            attributesHeight += 20;
            magAtkPotency->Y = -attributesHeight - 10;
            magAtkPotency->PrevSiblingNode->ToggleVisibility(false); // header
            expectedDamagePtr = AddStatRow((AtkComponentNode*)magAtkPotency, Localization.Panel_Damage_per_100_Potency, true);
            SetTooltip(expectedDamagePtr, Tooltips.Entry.ExpectedDamage);
        } else {
            magAtkPotency->ToggleVisibility(false);
        }

        var dexNode = vitalityNode->PrevSiblingNode;
        dexNode->Y = 40;
        SetTooltip((AtkComponentNode*)dexNode, Tooltips.Entry.MainStat);
        var strNode = dexNode->PrevSiblingNode;
        strNode->Y = 40;
        SetTooltip((AtkComponentNode*)strNode, Tooltips.Entry.MainStat);

        var offensiveHeight = 130;

        offensivePtr = atkUnitBase->UldManager.SearchNodeById(36);
        offensivePtr->Y = attributesHeight;
        var dh = offensivePtr->ChildNode;
        dh->Y = 120;
        dhChancePtr = AddStatRow((AtkComponentNode*)dh, Localization.Panel_Direct_Hit_Chance);
        if (plugin.Configuration.ShowDhDamageIncrease) {
            offensiveHeight += 20;
            magAtkPotency->Y -= 20;
            healMagPotency->Y -= 20;
            dhDamagePtr = AddStatRow((AtkComponentNode*)dh, Localization.Panel_Damage_Increase);
        }

        SetTooltip(dhChancePtr, Tooltips.Entry.DirectHit);
        var det = dh->PrevSiblingNode;
        det->Y = 80;
        detDmgIncreasePtr = AddStatRow((AtkComponentNode*)det, Localization.Panel_Damage_Increase);
        SetTooltip(detDmgIncreasePtr, Tooltips.Entry.Determination);
        var crit = det->PrevSiblingNode;
        critChancePtr = AddStatRow((AtkComponentNode*)crit, Localization.Panel_Crit_Chance);
        critDmgPtr = AddStatRow((AtkComponentNode*)crit, Localization.Panel_Crit_Damage);
        if (plugin.Configuration.ShowCritDamageIncrease) {
            offensiveHeight += 20;
            dh->Y += 20;
            det->Y += 20;
            magAtkPotency->Y -= 20;
            healMagPotency->Y -= 20;
            critDmgIncreasePtr = AddStatRow((AtkComponentNode*)crit, Localization.Panel_Damage_Increase);
        }

        SetTooltip(critChancePtr, Tooltips.Entry.Crit);

        defensivePtr = atkUnitBase->UldManager.SearchNodeById(44);
        defensivePtr->Y = attributesHeight;
        var magicDef = defensivePtr->ChildNode;
        magicDef->Y = 60;
        magicMitPtr = AddStatRow((AtkComponentNode*)magicDef, Localization.Panel_Magic_Mitigation);
        SetTooltip(magicMitPtr, Tooltips.Entry.MagicDefense);
        var def = magicDef->PrevSiblingNode;
        physMitPtr = AddStatRow((AtkComponentNode*)def, Localization.Panel_Physical_Mitigation);
        SetTooltip(physMitPtr, Tooltips.Entry.Defense);

        mentProperties->X = 0;
        mentProperties->Y = attributesHeight + offensiveHeight;
        spellSpeedPtr = mentProperties->ChildNode;
        magAtkPotency->PrevSiblingNode->PrevSiblingNode->ToggleVisibility(false); // Header
        spsSpeedIncreasePtr = AddStatRow((AtkComponentNode*)spellSpeedPtr, Localization.Panel_Skill_Speed_Increase);
        spsGcdPtr = AddStatRow((AtkComponentNode*)spellSpeedPtr, Localization.Panel_GCD);
        spsAltGcdPtr = AddStatRow((AtkComponentNode*)spellSpeedPtr, "");
        SetTooltip(spsSpeedIncreasePtr, Tooltips.Entry.Speed);

        physPropertiesPtr = atkUnitBase->UldManager.SearchNodeById(51);
        physPropertiesPtr->Y = attributesHeight + offensiveHeight + 40;
        skillSpeedPtr = physPropertiesPtr->ChildNode;
        skillSpeedPtr->Y = 20;
        skillSpeedPtr->PrevSiblingNode->ToggleVisibility(false); // Attack Power
        ((AtkTextNode*)skillSpeedPtr->PrevSiblingNode->PrevSiblingNode->ChildNode->PrevSiblingNode)->SetText(Localization.Panel_Speed_Properties);
        sksSpeedIncreasePtr = AddStatRow((AtkComponentNode*)skillSpeedPtr, Localization.Panel_Skill_Speed_Increase);
        sksGcdPtr = AddStatRow((AtkComponentNode*)skillSpeedPtr, Localization.Panel_GCD);
        SetTooltip(sksSpeedIncreasePtr, Tooltips.Entry.Speed);
        
        gearPtr = atkUnitBase->UldManager.SearchNodeById(80);
        if (plugin.Configuration.ShowGearProperties) {
            gearPtr->Y = attributesHeight + offensiveHeight + 40;
            gearPtr->X = 183;
            var avgItemLevelPtr = (AtkComponentNode*)gearPtr->ChildNode;
            ilvlSyncPtr = AddStatRow(avgItemLevelPtr, Localization.Panel_Item_level_Sync, copyColor: true, expandCollisionNode: false);
            CreateNewTooltip(atkUnitBase, ilvlSyncPtr, Tooltips.Entry.ItemLevelSync);
        } else {
            gearPtr->ToggleVisibility(false);
        }

        var roleProp = atkUnitBase->UldManager.SearchNodeById(86);
        roleProp->Y = 60;
        pietyPtr = roleProp->ChildNode;
        pietyPtr->Y = 20;
        pieManaPtr = AddStatRow((AtkComponentNode*)pietyPtr, Localization.Panel_Mana_per_Tick);
        SetTooltip(pieManaPtr, Tooltips.Entry.Piety);
        tenacityPtr = pietyPtr->PrevSiblingNode;
        tenMitPtr = AddStatRow((AtkComponentNode*)tenacityPtr, Localization.Panel_Damage_and_Mitigation);
        tenacityPtr->PrevSiblingNode->ToggleVisibility(false); // header
        SetTooltip(tenMitPtr, Tooltips.Entry.Tenacity);

        var craftingPtr = atkUnitBase->UldManager.SearchNodeById(73);
        craftingPtr->X = 0;
        craftingPtr->Y = 80;
        var control = craftingPtr->ChildNode;
        var craftsmanship = control->PrevSiblingNode;
        craftsmanship->PrevSiblingNode->ToggleVisibility(false); // header
        if (plugin.Configuration.ShowDoHDoLStatsWithoutFood) {
            control->Y += 20;
            controlBasePtr = AddStatRow((AtkComponentNode*)control, Localization.Panel_excluding_Consumables);
            cpPtr = AddStatRow((AtkComponentNode*)control, Localization.Panel_CP, copyColor: true, expandCollisionNode: false);
            cpBasePtr = AddStatRow((AtkComponentNode*)control, Localization.Panel_excluding_Consumables, expandCollisionNode: false);
            craftsmanshipBasePtr = AddStatRow((AtkComponentNode*)craftsmanship, Localization.Panel_excluding_Consumables);
        }

        var gatheringPtr = atkUnitBase->UldManager.SearchNodeById(66);
        gatheringPtr->X = 0;
        gatheringPtr->Y = 80;
        var perception = gatheringPtr->ChildNode;
        var gathering = perception->PrevSiblingNode;
        gathering->PrevSiblingNode->ToggleVisibility(false); // header
        if (plugin.Configuration.ShowDoHDoLStatsWithoutFood) {
            perception->Y += 20;
            perceptionBasePtr = AddStatRow((AtkComponentNode*)perception, Localization.Panel_excluding_Consumables);
            gpPtr = AddStatRow((AtkComponentNode*)perception, Localization.Panel_GP, copyColor: true, expandCollisionNode: false);
            gpBasePtr = AddStatRow((AtkComponentNode*)perception, Localization.Panel_excluding_Consumables, expandCollisionNode: false);
            gatheringBasePtr = AddStatRow((AtkComponentNode*)gathering, Localization.Panel_excluding_Consumables);
        }

        characterStatusPtr = atkUnitBase;

        UpdateCharacterPanelForJob(job, lvl);
    }

    private void ToggleCustomTooltipNode(AtkTextNode* node, bool enable) {
        node->AtkResNode.ToggleVisibility(enable);
        node->AtkResNode.PrevSiblingNode->ToggleVisibility(enable);
        if (enable)
            node->AtkResNode.PrevSiblingNode->PrevSiblingNode->NodeFlags |= NodeFlags.EmitsEvents;
        else
            node->AtkResNode.PrevSiblingNode->PrevSiblingNode->NodeFlags &= ~NodeFlags.EmitsEvents;
    }

    private void CreateNewTooltip(AtkUnitBase* parent, AtkTextNode* forTextNode, Tooltips.Entry tooltip) {
        var component = (AtkComponentNode*)forTextNode->AtkResNode.ParentNode;
        var newCollNode = Util.CloneNode((AtkCollisionNode*)component->Component->UldManager.RootNode);
        forTextNode->AtkResNode.PrevSiblingNode->PrevSiblingNode = (AtkResNode*)newCollNode;
        newCollNode->AtkResNode.NextSiblingNode = forTextNode->AtkResNode.PrevSiblingNode->PrevSiblingNode;
        newCollNode->AtkResNode.Y = forTextNode->AtkResNode.Y;
        newCollNode->AtkResNode.AtkEventManager.Event = null;
        component->Component->UldManager.UpdateDrawNodeList();
        var tooltipArgs = new AtkTooltipManager.AtkTooltipArgs { Text = (byte*)tooltips[tooltip], Flags = 0xFFFFFFFF };
        AtkStage.GetSingleton()->TooltipManager.AddTooltip(AtkTooltipManager.AtkTooltipType.Text, parent->ID, (AtkResNode*)newCollNode, &tooltipArgs);
    }

    private AtkTextNode* AddStatRow(AtkComponentNode* parentNode, string label, bool hideOriginal = false, bool copyColor = false, bool expandCollisionNode = true) {
        var collisionNode = parentNode->Component->UldManager.RootNode;
        if (!hideOriginal) {
            parentNode->AtkResNode.Height += 20;
            if (expandCollisionNode)
                collisionNode->Height += 20;
        }

        var numberNode = (AtkTextNode*)collisionNode->PrevSiblingNode;
        var labelNode = (AtkTextNode*)numberNode->AtkResNode.PrevSiblingNode;
        var newNumberNode = Util.CloneNode(numberNode);
        var prevSiblingNode = labelNode->AtkResNode.PrevSiblingNode;
        labelNode->AtkResNode.PrevSiblingNode = (AtkResNode*)newNumberNode;
        newNumberNode->AtkResNode.NextSiblingNode = (AtkResNode*)labelNode;
        newNumberNode->AtkResNode.Y = parentNode->AtkResNode.Height - 24;
        if (!copyColor)
            newNumberNode->TextColor = new ByteColor { A = 0xFF, R = 0xA0, G = 0xA0, B = 0xA0 };
        newNumberNode->NodeText.StringPtr = (byte*)MemoryHelper.GameAllocateUi((ulong)newNumberNode->NodeText.BufSize);
        var newLabelNode = Util.CloneNode(labelNode);
        newNumberNode->AtkResNode.PrevSiblingNode = (AtkResNode*)newLabelNode;
        newLabelNode->AtkResNode.PrevSiblingNode = prevSiblingNode;
        newLabelNode->AtkResNode.NextSiblingNode = (AtkResNode*)newNumberNode;
        newLabelNode->AtkResNode.Y = parentNode->AtkResNode.Height - 24;
        if (!copyColor)
            newLabelNode->TextColor = new ByteColor { A = 0xFF, R = 0xA0, G = 0xA0, B = 0xA0 };
        newLabelNode->NodeText.StringPtr = (byte*)MemoryHelper.GameAllocateUi((ulong)newLabelNode->NodeText.BufSize);
        newLabelNode->SetText(label);
        if (hideOriginal) {
            labelNode->AtkResNode.ToggleVisibility(false);
            numberNode->TextColor.A = 0; // toggle visibility doesn't work since it's constantly updated by the game
        }
        
        parentNode->Component->UldManager.UpdateDrawNodeList();

        return newNumberNode;
    }
    
    private void SetTooltip(AtkComponentNode* parentNode, Tooltips.Entry entry) {
        if (!plugin.Configuration.ShowTooltips)
            return;
        if (parentNode == null)
            return;
        var collisionNode = parentNode->Component->UldManager.RootNode;
        if (collisionNode == null)
            return;

        var ttMgr = AtkStage.GetSingleton()->TooltipManager;
        var ttMsg = Util.Find(ttMgr.TooltipMap.Head->Parent, collisionNode).Value;
        ttMsg->AtkTooltipArgs.Text = (byte*)tooltips[entry];
    }

    private void SetTooltip(AtkTextNode* node, Tooltips.Entry entry) {
        SetTooltip((AtkComponentNode*)node->AtkResNode.ParentNode, entry);
    }
    
    internal void RequestedUpdate(AddonEvent type, AddonArgs args) {
        if (args.Addon != (IntPtr)characterStatusPtr) {
            ClearPointers();
            return;
        }

        var uiState = UIState.Instance();
        var lvl = uiState->PlayerState.CurrentLevel;
        var levelModifier = LevelModifiers.LevelTable[lvl];
        var statInfo = new StatInfo();

        var dh = Equations.CalcDh(uiState->PlayerState.Attributes[(int)Attributes.DirectHit], ref statInfo, levelModifier);
        dhChancePtr->SetText($"{statInfo.DisplayValue:P1}");
        if (dhDamagePtr != null)
            dhDamagePtr->SetText($"{statInfo.DisplayValue * 0.25:P1}");
        tooltips.Update(Tooltips.Entry.DirectHit, statInfo);

        var det = Equations.CalcDet(uiState->PlayerState.Attributes[(int)Attributes.Determination], ref statInfo, levelModifier);
        tooltips.Update(Tooltips.Entry.Determination, statInfo);
        detDmgIncreasePtr->SetText($"{statInfo.DisplayValue:P1}");

        var critRate = Equations.CalcCritRate(uiState->PlayerState.Attributes[(int)Attributes.CriticalHit], ref statInfo, levelModifier);
        tooltips.Update(Tooltips.Entry.Crit, statInfo);
        critChancePtr->SetText($"{statInfo.DisplayValue:P1}");

        var critDmg = Equations.CalcCritDmg(uiState->PlayerState.Attributes[(int)Attributes.CriticalHit], ref statInfo, levelModifier);
        critDmgPtr->SetText($"{statInfo.DisplayValue:P1}");
        if (critDmgIncreasePtr != null)
            critDmgIncreasePtr->SetText($"{critRate * (critDmg - 1):P1}");

        Equations.CalcMagicDef(uiState->PlayerState.Attributes[(int)Attributes.MagicDefense], ref statInfo, levelModifier);
        magicMitPtr->SetText($"{statInfo.DisplayValue:P0}");
        tooltips.Update(Tooltips.Entry.MagicDefense, statInfo);

        Equations.CalcDef(uiState->PlayerState.Attributes[(int)Attributes.Defense], ref statInfo, levelModifier);
        physMitPtr->SetText($"{statInfo.DisplayValue:P0}");
        tooltips.Update(Tooltips.Entry.Defense, statInfo);

        var (ilvlSync, ilvlSyncType) = IlvlSync.GetCurrentIlvlSync();
        if (ilvlSyncPtr != null) {
            ToggleCustomTooltipNode(ilvlSyncPtr, ilvlSync != null);
            if (ilvlSync != null) {
                ilvlSyncPtr->SetText($"{ilvlSync}");
            }
        }

        var jobId = (JobId)uiState->PlayerState.CurrentClassJobId;

        StatInfo gcdMain = new(), gcdAlt = new();
        var altGcd = jobId.AltGcd(lvl);
        var gcdMod = jobId.GcdMod(lvl);
        var withMod = gcdMod != null && plugin.CtrlHeld != gcdMod.Passive;
        Equations.CalcSpeed(uiState->PlayerState.Attributes[jobId.IsCaster() ? (int)Attributes.SpellSpeed : (int)Attributes.SkillSpeed], ref statInfo,
            ref gcdMain, ref gcdAlt, levelModifier, altGcd, withMod ? gcdMod : null, out var baseGcd, out var altBaseGcd);
        tooltips.UpdateSpeed(statInfo, gcdMain, gcdAlt, baseGcd, altBaseGcd, !plugin.CtrlHeld ? gcdMod : null);
        if (jobId.IsCaster()) {
            spsSpeedIncreasePtr->SetText($"{statInfo.DisplayValue:P1}");
            ((AtkTextNode*)spsGcdPtr->AtkResNode.PrevSiblingNode)->SetText($"{Localization.Panel_GCD}{(withMod ? $" ({gcdMod!.Abbrev})" : "")}");
            spsGcdPtr->SetText($"{gcdMain.DisplayValue:N2}s");
            if (altGcd != null) {
                ((AtkTextNode*)spsAltGcdPtr->AtkResNode.PrevSiblingNode)->SetText($"{altGcd.Name}{(withMod ? $" ({gcdMod!.Abbrev})" : "")}");
                spsAltGcdPtr->SetText($"{gcdAlt.DisplayValue:N2}s");
            }
        } else {
            sksSpeedIncreasePtr->SetText($"{statInfo.DisplayValue:P1}");
            ((AtkTextNode*)sksGcdPtr->AtkResNode.PrevSiblingNode)->SetText($"{Localization.Panel_GCD}{(withMod ? $" ({gcdMod!.Abbrev})" : "")}");
            sksGcdPtr->SetText($"{gcdMain.DisplayValue:N2}s");
        }

        Equations.CalcPiety(uiState->PlayerState.Attributes[(int)Attributes.Piety], ref statInfo, levelModifier);
        pieManaPtr->SetText($"{statInfo.DisplayValue:N0}");
        tooltips.Update(Tooltips.Entry.Piety, statInfo);

        var ten = Equations.CalcTenacity(uiState->PlayerState.Attributes[(int)Attributes.Tenacity], ref statInfo, levelModifier);
        tenMitPtr->SetText($"{statInfo.DisplayValue:P1}");
        tooltips.Update(Tooltips.Entry.Tenacity, statInfo);

        Equations.CalcHp(uiState, jobId, out var hpPerVitality, out var hpModifier);
        tooltips.UpdateVitality(jobId.ToString(), hpPerVitality, hpModifier);

        if (expectedDamagePtr != null || expectedHealPtr != null) {
            var (avgDamage, normalDamage, critDamage, avgHeal, normalHeal, critHeal) =
                Equations.CalcExpectedOutput(uiState, jobId, det, critDmg, critRate, dh, ten, levelModifier, ilvlSync, ilvlSyncType);
            if (expectedDamagePtr != null) {
                expectedDamagePtr->SetText($"{avgDamage:N0}");
                tooltips.UpdateExpectedOutput(Tooltips.Entry.ExpectedDamage, normalDamage, critDamage);
            }

            if (expectedHealPtr != null) {
                expectedHealPtr->SetText($"{avgHeal:N0}");
                tooltips.UpdateExpectedOutput(Tooltips.Entry.ExpectedHeal, normalHeal, critHeal);
            }
        }

        if (plugin.Configuration.ShowDoHDoLStatsWithoutFood) {
            if (jobId.IsCrafter()) {
                var fd = Equations.EstimateBaseStats(uiState);
                craftsmanshipBasePtr->SetText(fd.GetValueOrDefault(Attributes.Craftsmanship, uiState->PlayerState.Attributes[(int)Attributes.Craftsmanship])
                    .ToString());
                controlBasePtr->SetText(fd.GetValueOrDefault(Attributes.Control, uiState->PlayerState.Attributes[(int)Attributes.Control]).ToString());
                cpPtr->SetText(uiState->PlayerState.Attributes[(int)Attributes.MaxCp].ToString());
                cpBasePtr->SetText(fd.GetValueOrDefault(Attributes.MaxCp, uiState->PlayerState.Attributes[(int)Attributes.MaxCp]).ToString());
            } else if (jobId.IsGatherer()) {
                var fd = Equations.EstimateBaseStats(uiState);
                gatheringBasePtr->SetText(fd.GetValueOrDefault(Attributes.Gathering, uiState->PlayerState.Attributes[(int)Attributes.Gathering])
                    .ToString());
                perceptionBasePtr->SetText(fd.GetValueOrDefault(Attributes.Perception, uiState->PlayerState.Attributes[(int)Attributes.Perception])
                    .ToString());
                gpPtr->SetText(uiState->PlayerState.Attributes[(int)Attributes.MaxGp].ToString());
                gpBasePtr->SetText(fd.GetValueOrDefault(Attributes.MaxGp, uiState->PlayerState.Attributes[(int)Attributes.MaxGp]).ToString());
            }
        }

        if (jobId != lastJob) {
            UpdateCharacterPanelForJob(jobId, lvl);
        }
    }

    private void UpdateCharacterPanelForJob(JobId job, int lvl) {
        if (job.IsCrafter() || job.IsGatherer()) {
            attributesPtr->ChildNode->ToggleVisibility(false);
            attributesPtr->ChildNode->PrevSiblingNode->ToggleVisibility(false);
            attributesPtr->ChildNode->PrevSiblingNode->PrevSiblingNode->PrevSiblingNode->ToggleVisibility(false);
            attributesPtr->ChildNode->PrevSiblingNode->PrevSiblingNode->PrevSiblingNode->PrevSiblingNode->ToggleVisibility(false);
            offensivePtr->ToggleVisibility(false);
            defensivePtr->ToggleVisibility(false);
            physPropertiesPtr->ToggleVisibility(false);
            gearPtr->ToggleVisibility(false);
            if (expectedDamagePtr != null)
                expectedDamagePtr->AtkResNode.ParentNode->ToggleVisibility(false);
            if (expectedHealPtr != null)
                expectedHealPtr->AtkResNode.ParentNode->ToggleVisibility(false);
        } else {
            offensivePtr->ToggleVisibility(true);
            defensivePtr->ToggleVisibility(true);
            physPropertiesPtr->ToggleVisibility(true);
            if (plugin.Configuration.ShowGearProperties)
                gearPtr->ToggleVisibility(true);
            if (expectedDamagePtr != null)
                expectedDamagePtr->AtkResNode.ParentNode->ToggleVisibility(true);
            if (expectedHealPtr != null)
                expectedHealPtr->AtkResNode.ParentNode->ToggleVisibility(true);
            if (job.IsCaster()) {
                skillSpeedPtr->ToggleVisibility(false);
                spellSpeedPtr->ToggleVisibility(true);
                skillSpeedPtr->ToggleVisibility(false);
                spellSpeedPtr->ToggleVisibility(true);
                spsAltGcdPtr->AtkResNode.ToggleVisibility(job.AltGcd(lvl) != null);
                spsAltGcdPtr->AtkResNode.PrevSiblingNode->ToggleVisibility(job.AltGcd(lvl) != null);
                if (job.UsesMind()) {
                    attributesPtr->ChildNode->ToggleVisibility(true);
                    attributesPtr->ChildNode->PrevSiblingNode->ToggleVisibility(false);
                    attributesPtr->ChildNode->PrevSiblingNode->PrevSiblingNode->PrevSiblingNode->ToggleVisibility(false);
                    attributesPtr->ChildNode->PrevSiblingNode->PrevSiblingNode->PrevSiblingNode->PrevSiblingNode->ToggleVisibility(false);
                    pietyPtr->ToggleVisibility(true);
                    tenacityPtr->ToggleVisibility(false);
                } else {
                    attributesPtr->ChildNode->ToggleVisibility(false);
                    attributesPtr->ChildNode->PrevSiblingNode->ToggleVisibility(true);
                    attributesPtr->ChildNode->PrevSiblingNode->PrevSiblingNode->PrevSiblingNode->ToggleVisibility(false);
                    attributesPtr->ChildNode->PrevSiblingNode->PrevSiblingNode->PrevSiblingNode->PrevSiblingNode->ToggleVisibility(false);
                    pietyPtr->ToggleVisibility(false);
                    tenacityPtr->ToggleVisibility(false);
                }
            } else {
                skillSpeedPtr->ToggleVisibility(true);
                spellSpeedPtr->ToggleVisibility(false);
                if (job.UsesDexterity()) {
                    attributesPtr->ChildNode->ToggleVisibility(false);
                    attributesPtr->ChildNode->PrevSiblingNode->ToggleVisibility(false);
                    attributesPtr->ChildNode->PrevSiblingNode->PrevSiblingNode->PrevSiblingNode->ToggleVisibility(true);
                    attributesPtr->ChildNode->PrevSiblingNode->PrevSiblingNode->PrevSiblingNode->PrevSiblingNode->ToggleVisibility(false);
                    pietyPtr->ToggleVisibility(false);
                    tenacityPtr->ToggleVisibility(false);
                } else {
                    attributesPtr->ChildNode->ToggleVisibility(false);
                    attributesPtr->ChildNode->PrevSiblingNode->ToggleVisibility(false);
                    attributesPtr->ChildNode->PrevSiblingNode->PrevSiblingNode->PrevSiblingNode->ToggleVisibility(false);
                    attributesPtr->ChildNode->PrevSiblingNode->PrevSiblingNode->PrevSiblingNode->PrevSiblingNode->ToggleVisibility(true);
                    pietyPtr->ToggleVisibility(false);
                    tenacityPtr->ToggleVisibility(job.UsesTenacity());
                }
            }
        }

        lastJob = job;
    }

    internal void Update() {
        var charStatus = AtkStage.GetSingleton()->RaptureAtkUnitManager->GetAddonByName("CharacterStatus");
        if (charStatus != null && charStatus->IsVisible) {
            RequestedUpdate(AddonEvent.PostRequestedUpdate, new AddonRequestedUpdateArgs {Addon = (IntPtr)charStatus});
            // Refresh the currently active tooltip
            var tooltipManager = &AtkStage.GetSingleton()->TooltipManager;
            var currentTooltipNode = ((AtkResNode**)tooltipManager)[4];
            if (currentTooltipNode == null)
                return;
            plugin.GameFunctions.AtkTooltipManagerShowNodeTooltip(tooltipManager, currentTooltipNode);
        }
    }

    private void ClearPointers() {
        characterStatusPtr = null;
        dhChancePtr = null;
        dhDamagePtr = null;
        detDmgIncreasePtr = null;
        critDmgPtr = null;
        critChancePtr = null;
        critDmgIncreasePtr = null;
        physMitPtr = null;
        magicMitPtr = null;
        sksSpeedIncreasePtr = null;
        sksGcdPtr = null;
        spsSpeedIncreasePtr = null;
        spsGcdPtr = null;
        spsAltGcdPtr = null;
        tenMitPtr = null;
        pieManaPtr = null;
        expectedDamagePtr = null;
        expectedHealPtr = null;
        ilvlSyncPtr = null;
        attributesPtr = null;
        offensivePtr = null;
        defensivePtr = null;
        physPropertiesPtr = null;
        gearPtr = null;
        pietyPtr = null;
        tenacityPtr = null;
        spellSpeedPtr = null;
        skillSpeedPtr = null;
        craftsmanshipBasePtr = null;
        controlBasePtr = null;
        cpPtr = null;
        cpBasePtr = null;
        gatheringBasePtr = null;
        perceptionBasePtr = null;
        gpPtr = null;
        gpBasePtr = null;
    }

    public void ReloadLocs() {
        tooltips.Reload();
    }

    public void Dispose() {
        ClearPointers();
        tooltips.Dispose();
    }
}
