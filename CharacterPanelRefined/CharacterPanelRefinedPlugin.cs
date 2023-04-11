using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Dalamud;
using Dalamud.Game;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Command;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Memory;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.Interop;
using FFXIVClientStructs.STD;
using Lumina.Excel.GeneratedSheets;

namespace CharacterPanelRefined;

public class CharacterPanelRefinedPlugin : IDalamudPlugin {
    public string Name => "Character Panel Refined";

    public Configuration Configuration { get; }
    public ConfigWindow ConfigWindow { get; }

    private readonly Tooltips tooltips;
    private readonly Hooks hooks;

    private bool ctrlHeld;
    private IntPtr characterStatusPtr;
    private unsafe AtkTextNode* dhChancePtr;
    private unsafe AtkTextNode* dhDamagePtr;
    private unsafe AtkTextNode* detDmgIncreasePtr;
    private unsafe AtkTextNode* critDmgPtr;
    private unsafe AtkTextNode* critChancePtr;
    private unsafe AtkTextNode* critDmgIncreasePtr;
    private unsafe AtkTextNode* physMitPtr;
    private unsafe AtkTextNode* magicMitPtr;
    private unsafe AtkTextNode* sksSpeedIncreasePtr;
    private unsafe AtkTextNode* sksGcdPtr;
    private unsafe AtkTextNode* spsSpeedIncreasePtr;
    private unsafe AtkTextNode* spsGcdPtr;
    private unsafe AtkTextNode* spsAltGcdPtr;
    private unsafe AtkTextNode* tenMitPtr;
    private unsafe AtkTextNode* pieManaPtr;
    private unsafe AtkTextNode* expectedDamagePtr;
    private unsafe AtkTextNode* expectedHealPtr;
    private unsafe AtkTextNode* ilvlSyncPtr;
    private unsafe AtkResNode* attributesPtr;
    private unsafe AtkResNode* offensivePtr;
    private unsafe AtkResNode* defensivePtr;
    private unsafe AtkResNode* physPropertiesPtr;
    private unsafe AtkResNode* gearPtr;
    private unsafe AtkResNode* pietyPtr;
    private unsafe AtkResNode* tenacityPtr;
    private unsafe AtkResNode* spellSpeedPtr;
    private unsafe AtkResNode* skillSpeedPtr;
    private unsafe AtkTextNode* craftsmanshipBasePtr;
    private unsafe AtkTextNode* controlBasePtr;
    private unsafe AtkTextNode* cpPtr;
    private unsafe AtkTextNode* cpBasePtr;
    private unsafe AtkTextNode* gatheringBasePtr;
    private unsafe AtkTextNode* perceptionBasePtr;
    private unsafe AtkTextNode* gpPtr;
    private unsafe AtkTextNode* gpBasePtr;
    private JobId lastJob;

    public CharacterPanelRefinedPlugin(DalamudPluginInterface pluginInterface) {
        Service.Initialize(pluginInterface);

        Configuration = Configuration.Get(pluginInterface);
        ConfigWindow = new ConfigWindow(this);
        tooltips = new Tooltips();
        hooks = new Hooks(this);

        UpdateLanguage();
        
        Service.Framework.Update += FrameworkOnUpdate;

        pluginInterface.UiBuilder.Draw += ConfigWindow.Draw;
        pluginInterface.UiBuilder.OpenConfigUi += () => ConfigWindow.ShowConfig = true;
        Service.CommandManager.AddHandler("/cprconfig",
            new CommandInfo((_, _) => ConfigWindow.ShowConfig ^= true) { HelpMessage = "Open the Character Panel Refined configuration." });
    }

    private unsafe void FrameworkOnUpdate(Framework framework) {
        var ctrlState = Service.KeyState[VirtualKey.CONTROL];
        if (ctrlState == ctrlHeld)
            return;
        ctrlHeld = ctrlState;
        var charStatus = AtkStage.GetSingleton()->RaptureAtkUnitManager->GetAddonByName("CharacterStatus");
        if (charStatus == null || !charStatus->IsVisible) return;
        CharacterStatusRequestUpdate(charStatus);
        // Update the currently active tooltip
        var tooltipManager = &AtkStage.GetSingleton()->TooltipManager;
        var currentTooltipNode = ((AtkResNode**)tooltipManager)[4];
        if (currentTooltipNode == null) return;
        hooks.AtkTooltipManagerShowNodeTooltip(tooltipManager, currentTooltipNode);
    }

    public void UpdateLanguage() {
        var lang = "";

        if (Configuration.UseGameLanguage) {
            lang = Service.ClientState.ClientLanguage switch {
                ClientLanguage.English => "",
                ClientLanguage.French => "fr",
                ClientLanguage.German => "de",
                ClientLanguage.Japanese => "ja",
                _ => ""
            };
        }

        Localization.Culture = new CultureInfo(lang);

        tooltips.Reload();
    }

    internal unsafe void CharacterStatusRequestUpdate(AtkUnitBase* atkUnitBase) {
        if ((IntPtr)atkUnitBase == characterStatusPtr) {
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
            ToggleCustomTooltipNode(ilvlSyncPtr, ilvlSync != null);
            if (ilvlSync != null) {
                ilvlSyncPtr->SetText($"{ilvlSync}");
            }

            var jobId = (JobId)uiState->PlayerState.CurrentClassJobId;

            StatInfo gcdMain = new(), gcdAlt = new();
            var altGcd = jobId.AltGcd(lvl);
            var gcdMod = jobId.GcdMod(lvl);
            var withMod = gcdMod != null && ctrlHeld != gcdMod.Passive;
            Equations.CalcSpeed(uiState->PlayerState.Attributes[jobId.IsCaster() ? (int)Attributes.SpellSpeed : (int)Attributes.SkillSpeed], ref statInfo,
                ref gcdMain, ref gcdAlt, levelModifier, altGcd, withMod ? gcdMod : null, out var baseGcd, out var altBaseGcd);
            tooltips.UpdateSpeed(statInfo, gcdMain, gcdAlt, baseGcd, altBaseGcd, !ctrlHeld ? gcdMod : null);
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

            if (Configuration.ShowDoHDoLStatsWithoutFood) {
                if (jobId.IsCrafter()) {
                    var fd = Equations.EstimateBaseStats(uiState);
                    craftsmanshipBasePtr->SetText(fd.GetValueOrDefault(Attributes.Craftsmanship, uiState->PlayerState.Attributes[(int)Attributes.Craftsmanship]).ToString());
                    controlBasePtr->SetText(fd.GetValueOrDefault(Attributes.Control, uiState->PlayerState.Attributes[(int)Attributes.Control]).ToString());
                    cpPtr->SetText(uiState->PlayerState.Attributes[(int)Attributes.MaxCp].ToString());
                    cpBasePtr->SetText(fd.GetValueOrDefault(Attributes.MaxCp, uiState->PlayerState.Attributes[(int)Attributes.MaxCp]).ToString());
                } else if (jobId.IsGatherer()) {
                    var fd = Equations.EstimateBaseStats(uiState);
                    gatheringBasePtr->SetText(fd.GetValueOrDefault(Attributes.Gathering, uiState->PlayerState.Attributes[(int)Attributes.Gathering]).ToString());
                    perceptionBasePtr->SetText(fd.GetValueOrDefault(Attributes.Perception, uiState->PlayerState.Attributes[(int)Attributes.Perception]).ToString());
                    gpPtr->SetText(uiState->PlayerState.Attributes[(int)Attributes.MaxGp].ToString());
                    gpBasePtr->SetText(fd.GetValueOrDefault(Attributes.MaxGp, uiState->PlayerState.Attributes[(int)Attributes.MaxGp]).ToString());
                }
            }

            if (jobId != lastJob) {
                UpdateCharacterPanelForJob(jobId, lvl);
            }
        } else {
            ClearPointers();
        }
    }

    private unsafe void UpdateCharacterPanelForJob(JobId job, int lvl) {
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
            if (Configuration.ShowGearProperties)
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

    private unsafe void SetTooltip(AtkComponentNode* parentNode, Tooltips.Entry entry) {
        if (!Configuration.ShowTooltips)
            return;
        if (parentNode == null)
            return;
        var collisionNode = parentNode->Component->UldManager.RootNode;
        if (collisionNode == null)
            return;

        var ttMgr = AtkStage.GetSingleton()->TooltipManager;
        var ttMsg = Find(ttMgr.TooltipMap.Head->Parent, collisionNode).Value;
        ttMsg->AtkTooltipArgs.Text = (byte*)tooltips[entry];
    }

    private unsafe void SetTooltip(AtkTextNode* node, Tooltips.Entry entry) {
        SetTooltip((AtkComponentNode*)node->AtkResNode.ParentNode, entry);
    }

    // Traverse a std::map

    private unsafe TVal Find<TKey, TVal>(StdMap<Pointer<TKey>, TVal>.Node* node, TKey* item) where TKey : unmanaged where TVal : unmanaged {
        while (!node->IsNil) {
            if (node->KeyValuePair.Item1.Value < item) {
                node = node->Right;
                continue;
            }

            if (node->KeyValuePair.Item1.Value > item) {
                node = node->Left;
                continue;
            }

            return node->KeyValuePair.Item2;
        }

        return default;
    }

    internal unsafe void CharacterStatusOnSetup(AtkUnitBase* atkUnitBase) {
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
        if (Configuration.ShowAvgHealing) {
            attributesHeight += 20;
            healMagPotency->Y = -attributesHeight - 10;
            expectedHealPtr = AddStatRow((AtkComponentNode*)healMagPotency, Localization.Panel_Heal_per_100_Potency, true);
            SetTooltip(expectedHealPtr, Tooltips.Entry.ExpectedHeal);
        } else {
            healMagPotency->ToggleVisibility(false);
        }
        if (Configuration.ShowAvgDamage) {
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
        if (Configuration.ShowDhDamageIncrease) {
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
        if (Configuration.ShowCritDamageIncrease) {
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
        if (Configuration.ShowGearProperties) {
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
        if (Configuration.ShowDoHDoLStatsWithoutFood) {
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
        if (Configuration.ShowDoHDoLStatsWithoutFood) {
            perception->Y += 20;
            perceptionBasePtr = AddStatRow((AtkComponentNode*)perception, Localization.Panel_excluding_Consumables);
            gpPtr = AddStatRow((AtkComponentNode*)perception, Localization.Panel_GP, copyColor: true, expandCollisionNode: false);
            gpBasePtr = AddStatRow((AtkComponentNode*)perception, Localization.Panel_excluding_Consumables, expandCollisionNode: false);
            gatheringBasePtr = AddStatRow((AtkComponentNode*)gathering, Localization.Panel_excluding_Consumables);
        }

        characterStatusPtr = (IntPtr)atkUnitBase;

        UpdateCharacterPanelForJob(job, lvl);
    }

    private unsafe void ToggleCustomTooltipNode(AtkTextNode* node, bool enable) {
        node->AtkResNode.ToggleVisibility(enable);
        node->AtkResNode.PrevSiblingNode->ToggleVisibility(enable);
        if (enable)
            node->AtkResNode.PrevSiblingNode->PrevSiblingNode->Flags |= (short)NodeFlags.EmitsEvents;
        else
            node->AtkResNode.PrevSiblingNode->PrevSiblingNode->Flags &= ~(short)NodeFlags.EmitsEvents;
    }

    private unsafe void CreateNewTooltip(AtkUnitBase* parent, AtkTextNode* forTextNode, Tooltips.Entry tooltip) {
        var component = (AtkComponentNode*)forTextNode->AtkResNode.ParentNode;
        var newCollNode = CloneNode((AtkCollisionNode*)component->Component->UldManager.RootNode);
        component->Component->UldManager.NodeList[component->Component->UldManager.NodeListCount++] = (AtkResNode*)newCollNode;
        forTextNode->AtkResNode.PrevSiblingNode->PrevSiblingNode = (AtkResNode*)newCollNode;
        newCollNode->AtkResNode.NextSiblingNode = forTextNode->AtkResNode.PrevSiblingNode->PrevSiblingNode;
        newCollNode->AtkResNode.Y = forTextNode->AtkResNode.Y;
        newCollNode->AtkResNode.AtkEventManager.Event = null;
        ExpandNodeList(component, 1);
        var tooltipArgs = new AtkTooltipManager.AtkTooltipArgs { Text = (byte*)tooltips[tooltip], Flags = 0xFFFFFFFF };
        AtkStage.GetSingleton()->TooltipManager.AddTooltip(AtkTooltipManager.AtkTooltipType.Text, parent->ID, (AtkResNode*)newCollNode, &tooltipArgs);
    }

    private unsafe AtkTextNode* AddStatRow(AtkComponentNode* parentNode, string label, bool hideOriginal = false, bool copyColor = false, bool expandCollisionNode = true) {
        ExpandNodeList(parentNode, 2);
        var collisionNode = parentNode->Component->UldManager.RootNode;
        if (!hideOriginal) {
            parentNode->AtkResNode.Height += 20;
            if (expandCollisionNode)
                collisionNode->Height += 20;
        }

        var numberNode = (AtkTextNode*)collisionNode->PrevSiblingNode;
        var labelNode = (AtkTextNode*)numberNode->AtkResNode.PrevSiblingNode;
        var newNumberNode = CloneNode(numberNode);
        var prevSiblingNode = labelNode->AtkResNode.PrevSiblingNode;
        labelNode->AtkResNode.PrevSiblingNode = (AtkResNode*)newNumberNode;
        newNumberNode->AtkResNode.NextSiblingNode = (AtkResNode*)labelNode;
        newNumberNode->AtkResNode.Y = parentNode->AtkResNode.Height - 24;
        if (!copyColor)
            newNumberNode->TextColor = new ByteColor { A = 0xFF, R = 0xA0, G = 0xA0, B = 0xA0 };
        newNumberNode->NodeText.StringPtr = (byte*)MemoryHelper.GameAllocateUi((ulong)newNumberNode->NodeText.BufSize);
        parentNode->Component->UldManager.NodeList[parentNode->Component->UldManager.NodeListCount++] = (AtkResNode*)newNumberNode;
        var newLabelNode = CloneNode(labelNode);
        newNumberNode->AtkResNode.PrevSiblingNode = (AtkResNode*)newLabelNode;
        newLabelNode->AtkResNode.PrevSiblingNode = prevSiblingNode;
        newLabelNode->AtkResNode.NextSiblingNode = (AtkResNode*)newNumberNode;
        newLabelNode->AtkResNode.Y = parentNode->AtkResNode.Height - 24;
        if (!copyColor)
            newLabelNode->TextColor = new ByteColor { A = 0xFF, R = 0xA0, G = 0xA0, B = 0xA0 };
        newLabelNode->NodeText.StringPtr = (byte*)MemoryHelper.GameAllocateUi((ulong)newLabelNode->NodeText.BufSize);
        newLabelNode->SetText(label);
        parentNode->Component->UldManager.NodeList[parentNode->Component->UldManager.NodeListCount++] = (AtkResNode*)newLabelNode;
        if (hideOriginal) {
            labelNode->AtkResNode.ToggleVisibility(false);
            numberNode->TextColor.A = 0; // toggle visibility doesn't work since it's constantly updated by the game
        }

        return newNumberNode;
    }

    private static unsafe T* CloneNode<T>(T* original) where T : unmanaged {
        var size = sizeof(T);
        var allocation = MemoryHelper.GameAllocateUi((ulong)size);
        var bytes = new byte[size];
        Marshal.Copy(new IntPtr(original), bytes, 0, bytes.Length);
        Marshal.Copy(bytes, 0, allocation, bytes.Length);

        var newNode = (AtkResNode*)allocation;
        newNode->ChildNode = null;
        newNode->ChildCount = 0;
        newNode->PrevSiblingNode = null;
        newNode->NextSiblingNode = null;
        return (T*)newNode;
    }

    private unsafe void ExpandNodeList(AtkComponentNode* componentNode, ushort addSize) {
        var originalList = componentNode->Component->UldManager.NodeList;
        var originalSize = componentNode->Component->UldManager.NodeListCount;
        var newSize = (ushort)(componentNode->Component->UldManager.NodeListCount + addSize);
        var oldListPtr = new IntPtr(originalList);
        var newListPtr = MemoryHelper.GameAllocateUi((ulong)((newSize + 1) * 8));
        var clone = new IntPtr[originalSize];
        Marshal.Copy(oldListPtr, clone, 0, originalSize);
        Marshal.Copy(clone, 0, newListPtr, originalSize);
        componentNode->Component->UldManager.NodeList = (AtkResNode**)newListPtr;
    }

    private unsafe void ClearPointers() {
        characterStatusPtr = IntPtr.Zero;
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

    public void Dispose() {
        Service.Framework.Update -= FrameworkOnUpdate;
        ClearPointers();
        Service.CommandManager.RemoveHandler("/cprconfig");
        hooks.Dispose();
        tooltips.Dispose();
    }

    public unsafe void UpdateSyncedStats(NumberArrayData* numberArrayData, StringArrayData* stringArrayData) {
        if (ctrlHeld) return;
        if (!Configuration.ShowSyncedStatsOnTooltip) return;
        if ((numberArrayData->IntArray[4] & (1 << 30)) != 0) return; // already processed
        if (IlvlSync.GetCurrentIlvlSync() is not ({ } ilvlSync, var ilvlSyncType)) return;
        
        var itemId = Service.GameGui.HoveredItem;
        if (itemId is >=500000 and <1000000 or >=2000000)
            return; // collectibles & key items
        
        var item = Service.DataManager.GetExcelSheet<Item>()!.GetRow((uint)(itemId % 500000))!;
        
        if (item.EquipSlotCategory.Row == 0)
            return;

        if (ilvlSyncType == IlvlSyncType.LevelBased && item.LevelEquip <= UIState.Instance()->PlayerState.CurrentLevel)
            return;

        var ilvl = item.LevelItem.Value!;
        var sync = Service.DataManager.GetExcelSheet<ItemLevel>()!.GetRow(ilvlSync)!;
        var cult = CultureInfo.InvariantCulture;
        // weapon
        if ((numberArrayData->IntArray[4] & 0x2000) != 0) {
            if (ParseUnknownLength(stringArrayData->StringArray[9]) is not null) {
                var encoded = EncodeHighlighted(sync.PhysicalDamage.ToString(cult));
                stringArrayData->SetValue(9, encoded, false, true, true);
            }

            if (ParseUnknownLength(stringArrayData->StringArray[8]) is { } autoAtk) {
                var encoded = EncodeHighlighted((double.Parse(autoAtk.Text!, cult) * sync.PhysicalDamage / ilvl.PhysicalDamage - 0.001).ToString("F2", cult));
                stringArrayData->SetValue(8, encoded, false, true, true);
            }
        } else {
            if (ParseUnknownLength(stringArrayData->StringArray[8]) is { } def) {
                var defVal = double.Parse(def.Text!, cult);
                if (defVal > 1) {
                    var encoded = EncodeHighlighted((defVal * sync.Defense / ilvl.Defense - 0.1).ToString("F0", cult));
                    stringArrayData->SetValue(8, encoded, false, true, true);
                } else {
                    stringArrayData->SetValue(8, def.Encode(), false, true, true);
                }
            }

            if (ParseUnknownLength(stringArrayData->StringArray[7]) is { } magicDef) {
                var mDefVal = double.Parse(magicDef.Text!, cult);
                if (mDefVal > 1) {
                    var encoded = EncodeHighlighted((mDefVal * sync.MagicDefense / ilvl.MagicDefense - 0.1).ToString("F0", cult));
                    stringArrayData->SetValue(7, encoded, false, true, true);
                } else {
                    stringArrayData->SetValue(7, magicDef.Encode(), false, true, true);
                }
            }
        }

        if (ParseUnknownLength(stringArrayData->StringArray[37]) is { } vit) {
            var s = vit.Text!.Split('+');
            var encoded = EncodeHighlighted($"{s[0]}+{(double.Parse(s[1], cult) * sync.Vitality / ilvl.Vitality - 0.1).ToString("F0", cult)}");
            stringArrayData->SetValue(37, encoded, false, true, true);
        }

        if (ParseUnknownLength(stringArrayData->StringArray[38]) is { } main) {
            var s = main.Text!.Split('+');
            var encoded = EncodeHighlighted($"{s[0]}+{(double.Parse(s[1], cult) * sync.Strength / ilvl.Strength - 0.1).ToString("F0", cult)}");
            stringArrayData->SetValue(38, encoded, false, true, true);
        }

        var subStrs = new TextPayload?[4];
        var split = new string[4][];
        var parse = new double[4];
        for (var i = 0; i < 4; i++) {
            if ((subStrs[i] = ParseUnknownLength(stringArrayData->StringArray[39 + i])) is not { } sub) continue;
            split[i] = sub.Text!.Split('+');
            parse[i] = double.Parse(split[i][1], cult);
        }

        var subMax = parse.Max() * sync.CriticalHit / ilvl.CriticalHit - 0.1;
        for (var i = 0; i < 4; i++) {
            if (subStrs[i] is not { } sub) continue;
            if (parse[i] > subMax) {
                var encoded = EncodeHighlighted($"{split[i][0]}+{Math.Min(parse[i], subMax).ToString("F0", cult)}");
                stringArrayData->SetValue(39 + i, encoded, false, true, true);
            } else {
                stringArrayData->SetValue(39 + i, sub.Encode(), false, true, true);
            }
        }

        for (var i = 0; i < 5; i++) {
            if (ParseUnknownLength(stringArrayData->StringArray[58 + i]) is { } mat) {
                var encoded = EncodeHighlighted($"{mat.Text!.Split('+')[0]}+0");
                stringArrayData->SetValue(58 + i, encoded, false, true, true);
            }
        }

        numberArrayData->IntArray[4] |= 1 << 30;
    }

    private unsafe TextPayload? ParseUnknownLength(byte* ptr) {
        if (ptr == null) return null;
        var end = 0;
        for (; ptr[end] != 0; end++)
            if (end >= 4096) return null;
        return SeString.Parse(ptr, end).Payloads switch {
            [_, _, _, TextPayload p, _, _, _] => p,
            [TextPayload p] => p,
            _ => null
        };
    }

    private static readonly byte[] FgPayload = new UIForegroundPayload(573).Encode();
    private static readonly byte[] GlPayload = new UIGlowPayload(574).Encode();
    private static readonly byte[] EmPayload = new EmphasisItalicPayload(true).Encode();
    private static readonly byte[] FgClPayload = new UIForegroundPayload(0).Encode();
    private static readonly byte[] GlClPayload = new UIGlowPayload(0).Encode();
    private static readonly byte[] EmClPayload = new EmphasisItalicPayload(false).Encode();
    
    private byte[] EncodeHighlighted(string newText) {
        return new[] {
            FgPayload, GlPayload, EmPayload,
            new TextPayload(newText).Encode(),
            EmClPayload, FgClPayload, GlClPayload
        }.SelectMany(i => i).ToArray();
    }
}
