using System;
using System.Linq;
using System.Runtime.InteropServices;
using CharacterPanelRefined.Jobs;
using Dalamud.Game;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Memory;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;

namespace CharacterPanelRefined; 

public class CharacterPanelRefinedPlugin : IDalamudPlugin {
    public string Name => "Character Panel Refined";

    private readonly Hook<AddonOnSetup> characterStatusOnSetup;

    private unsafe delegate void* AddonOnSetup(AtkUnitBase* atkUnitBase, int a2, void* a3);

    private IntPtr characterStatusPtr;
    private unsafe AtkTextNode* dhChancePtr;
    private unsafe AtkTextNode* detDmgIncreasePtr;
    private unsafe AtkTextNode* critDmgPtr;
    private unsafe AtkTextNode* critChancePtr;
    private unsafe AtkTextNode* physMitPtr;
    private unsafe AtkTextNode* magicMitPtr;
    private unsafe AtkTextNode* sksSpeedIncreasePtr;
    private unsafe AtkTextNode* spsSpeedIncreasePtr;
    private unsafe AtkTextNode* tenMitPtr;
    private unsafe AtkTextNode* pieManaPtr;
    private unsafe AtkTextNode* expectedDamagePtr;
    private unsafe AtkResNode* attributesPtr;
    private unsafe AtkResNode* offensivePtr;
    private unsafe AtkResNode* defensivePtr;
    private unsafe AtkResNode* physPropertiesPtr;
    private unsafe AtkResNode* pietyPtr;
    private unsafe AtkResNode* tenacityPtr;
    private unsafe AtkResNode* spellSpeedPtr;
    private unsafe AtkResNode* skillSpeedPtr;
    private JobId lastJob;
    private ulong lastHash;

    public CharacterPanelRefinedPlugin(DalamudPluginInterface pluginInterface) {
        Service.Initialize(pluginInterface);

        var characterStatusOnSetupPtr =
            Service.SigScanner.ScanText("4C 8B DC 55 53 41 56 49 8D 6B A1 48 81 EC F0 00 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 45 07");

        unsafe {
            characterStatusOnSetup = Hook<AddonOnSetup>.FromAddress(characterStatusOnSetupPtr, CharacterStatusOnSetup);
        }

        characterStatusOnSetup.Enable();

        Service.Framework.Update += FrameworkOnUpdate;
    }

    private unsafe void FrameworkOnUpdate(Framework framework) {
        if (characterStatusPtr == IntPtr.Zero) return;
        var charStatus = Service.GameGui.GetAddonByName("CharacterStatus", 1);
        if (charStatus == characterStatusPtr) {
            var uiState = UIState.Instance();
            var computedHash = Fnv1AHash((byte*)uiState->PlayerState.Attributes, 296);
            if (computedHash != lastHash) {
                lastHash = computedHash;
                var dh = CalcDh(uiState);
                dhChancePtr->SetText($"{dh:P1}");
                var det = CalcDet(uiState);
                detDmgIncreasePtr->SetText($"{det:P1}");
                var critRate = CalcCritRate(uiState);
                critChancePtr->SetText($"{critRate:P1}");
                var critDmg = CalcCritDmg(uiState);
                critDmgPtr->SetText($"{critDmg:P1}");
                magicMitPtr->SetText($"{CalcMagicDef(uiState):P0}");
                physMitPtr->SetText($"{CalcDef(uiState):P0}");
                spsSpeedIncreasePtr->SetText($"{CalcSpellSpeed(uiState):P1}");
                sksSpeedIncreasePtr->SetText($"{CalcSkillSpeed(uiState):P1}");
                pieManaPtr->SetText($"{CalcPiety(uiState):N0}");
                var ten = CalcTenacity(uiState);
                tenMitPtr->SetText($"{ten:P1}");
                var jobId = (JobId)(*((byte*)&uiState->PlayerState + 0x6E));
                expectedDamagePtr->SetText($"{CalcRawDamage(uiState, jobId, det, critDmg, critRate, dh):N0}");
                if (jobId != lastJob) {
                    UpdateCharacterPanelForJob(jobId);
                }

            }
        } else {
            ClearPointers();
        }
    }

    private unsafe void ClearPointers() {
        characterStatusPtr = IntPtr.Zero;
        dhChancePtr = null;
        detDmgIncreasePtr = null;
        critDmgPtr = null;
        critChancePtr = null;
        physMitPtr = null;
        magicMitPtr = null;
        sksSpeedIncreasePtr = null;
        spsSpeedIncreasePtr = null;
        tenMitPtr = null;
        pieManaPtr = null;
        expectedDamagePtr = null;
        attributesPtr = null;
        offensivePtr = null;
        defensivePtr = null;
        physPropertiesPtr = null;
        spellSpeedPtr = null;
        skillSpeedPtr = null;
    }

    private unsafe void UpdateCharacterPanelForJob(JobId job) {
        if (job.IsCrafter() || job.IsGatherer()) {
            attributesPtr->ChildNode->ToggleVisibility(false);
            attributesPtr->ChildNode->PrevSiblingNode->ToggleVisibility(false);
            attributesPtr->ChildNode->PrevSiblingNode->PrevSiblingNode->PrevSiblingNode->ToggleVisibility(false);
            attributesPtr->ChildNode->PrevSiblingNode->PrevSiblingNode->PrevSiblingNode->PrevSiblingNode->ToggleVisibility(false);
            offensivePtr->ToggleVisibility(false);
            defensivePtr->ToggleVisibility(false);
            physPropertiesPtr->ToggleVisibility(false);
            expectedDamagePtr->AtkResNode.ToggleVisibility(false);
            expectedDamagePtr->AtkResNode.PrevSiblingNode->ToggleVisibility(false);
        } else {
            offensivePtr->ToggleVisibility(true);
            defensivePtr->ToggleVisibility(true);
            physPropertiesPtr->ToggleVisibility(true);
            expectedDamagePtr->AtkResNode.ToggleVisibility(true);
            expectedDamagePtr->AtkResNode.PrevSiblingNode->ToggleVisibility(true);
            if (job.IsCaster()) {
                skillSpeedPtr->ToggleVisibility(false);
                spellSpeedPtr->ToggleVisibility(true);
                skillSpeedPtr->ToggleVisibility(false);
                spellSpeedPtr->ToggleVisibility(true);
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

    private unsafe void* CharacterStatusOnSetup(AtkUnitBase* atkUnitBase, int a2, void* a3) {
        var val = characterStatusOnSetup.Original(atkUnitBase, a2, a3);

        try {
            var job = (JobId)UIState.Instance()->PlayerState.CurrentClassJobId;

            attributesPtr = atkUnitBase->UldManager.SearchNodeById(26);
            attributesPtr->ChildNode->X = 10; // Mind
            attributesPtr->ChildNode->Y = 40;
            attributesPtr->ChildNode->PrevSiblingNode->X = 10; // Intelligence
            attributesPtr->ChildNode->PrevSiblingNode->Y = 40;
                
            var vitalityNode = attributesPtr->ChildNode->PrevSiblingNode->PrevSiblingNode;
            vitalityNode->Y = 20;
            expectedDamagePtr = AddStatRow((AtkComponentNode*)vitalityNode, "Damage per 100 Pot");
            // this is not great TODO: refactor this
            ((AtkResNode*)expectedDamagePtr)->Y = 40;
            ((AtkResNode*)expectedDamagePtr)->PrevSiblingNode->Y = 40;
            vitalityNode->Height -= 20;
            ((AtkComponentNode*)vitalityNode)->Component->UldManager.RootNode->Height -= 20;
                
            vitalityNode->PrevSiblingNode->Y = 40; // Dexterity
            vitalityNode->PrevSiblingNode->PrevSiblingNode->Y = 40; // Strength

            offensivePtr = atkUnitBase->UldManager.SearchNodeById(36);
            offensivePtr->Y = 150;
            var dh = offensivePtr->ChildNode;
            dh->Y = 120;
            dhChancePtr = AddStatRow((AtkComponentNode*)dh, "Direct Hit Chance");
            var det = dh->PrevSiblingNode;
            det->Y = 80;
            detDmgIncreasePtr = AddStatRow((AtkComponentNode*)det, "Damage Increase");
            var crit = det->PrevSiblingNode;
            critChancePtr = AddStatRow((AtkComponentNode*)crit, "Crit Chance");
            critDmgPtr = AddStatRow((AtkComponentNode*)crit, "Crit Damage");

            defensivePtr = atkUnitBase->UldManager.SearchNodeById(44);
            defensivePtr->Y = 150;
            var magicDef = defensivePtr->ChildNode;
            magicDef->Y = 60;
            magicMitPtr = AddStatRow((AtkComponentNode*)magicDef, "Magic Mitigation");
            var def = magicDef->PrevSiblingNode;
            physMitPtr = AddStatRow((AtkComponentNode*)def, "Physical Mitigation");

            var mentProperties = atkUnitBase->UldManager.SearchNodeById(58);
            mentProperties->X = 0;
            mentProperties->Y = 280;
            spellSpeedPtr = mentProperties->ChildNode;
            spellSpeedPtr->PrevSiblingNode->ToggleVisibility(false); // Magic Attack Potency
            spellSpeedPtr->PrevSiblingNode->PrevSiblingNode->ToggleVisibility(false); // Magic Heal Potency
            spellSpeedPtr->PrevSiblingNode->PrevSiblingNode->PrevSiblingNode->ToggleVisibility(false); // Header
            spsSpeedIncreasePtr = AddStatRow((AtkComponentNode*)spellSpeedPtr, "Speed Increase");

            physPropertiesPtr = atkUnitBase->UldManager.SearchNodeById(51);
            physPropertiesPtr->Y = 320;
            skillSpeedPtr = physPropertiesPtr->ChildNode;
            skillSpeedPtr->Y = 20;
            skillSpeedPtr->PrevSiblingNode->ToggleVisibility(false); // Attack Power
            ((AtkTextNode*)skillSpeedPtr->PrevSiblingNode->PrevSiblingNode->ChildNode->PrevSiblingNode)->SetText("Speed Properties");
            sksSpeedIncreasePtr = AddStatRow((AtkComponentNode*)skillSpeedPtr, "Speed Increase");

            var gearProp = atkUnitBase->UldManager.SearchNodeById(80);
            gearProp->ToggleVisibility(false); // hide gear tab

            var roleProp = atkUnitBase->UldManager.SearchNodeById(86);
            roleProp->Y = 60;
            pietyPtr = roleProp->ChildNode;
            pietyPtr->Y = 20;
            pieManaPtr = AddStatRow((AtkComponentNode*)pietyPtr, "Mana per Tick");
            tenacityPtr = pietyPtr->PrevSiblingNode;
            tenMitPtr = AddStatRow((AtkComponentNode*)tenacityPtr, "Damage & Mitigation");
            tenacityPtr->PrevSiblingNode->ToggleVisibility(false); // header

            var craftingPtr = atkUnitBase->UldManager.SearchNodeById(73);
            craftingPtr->X = 0;
            craftingPtr->Y = 80;
            craftingPtr->ChildNode->PrevSiblingNode->PrevSiblingNode->ToggleVisibility(false); // header

            var gatheringPtr = atkUnitBase->UldManager.SearchNodeById(66);
            gatheringPtr->X = 0;
            gatheringPtr->Y = 80;
            gatheringPtr->ChildNode->PrevSiblingNode->PrevSiblingNode->ToggleVisibility(false); // header

            characterStatusPtr = (IntPtr)atkUnitBase;
            lastHash = 0;

            UpdateCharacterPanelForJob(job);
        } catch (Exception e) {
            PluginLog.LogError(e, "Failed to modify character");
        }

        return val;
    }

    private unsafe AtkTextNode* AddStatRow(AtkComponentNode* parentNode, string label) {
        ExpandNodeList(parentNode, 2);
        parentNode->AtkResNode.Height += 20;
        var collisionNode = parentNode->Component->UldManager.RootNode;
        collisionNode->Height += 20;
        var numberNode = (AtkTextNode*)collisionNode->PrevSiblingNode;
        var labelNode = (AtkTextNode*)numberNode->AtkResNode.PrevSiblingNode;
        var newNumberNode = CloneNode(numberNode);
        var prevSiblingNode = labelNode->AtkResNode.PrevSiblingNode;
        labelNode->AtkResNode.PrevSiblingNode = (AtkResNode*)newNumberNode;
        newNumberNode->AtkResNode.ParentNode = (AtkResNode*)parentNode;
        newNumberNode->AtkResNode.NextSiblingNode = (AtkResNode*)labelNode;
        newNumberNode->AtkResNode.Y = parentNode->AtkResNode.Height - 24;
        newNumberNode->TextColor = new ByteColor { A = 0xFF, R = 0xA0, G = 0xA0, B = 0xA0 };
        newNumberNode->NodeText.StringPtr = (byte*)MemoryHelper.GameAllocateUi((ulong)newNumberNode->NodeText.BufSize);
        parentNode->Component->UldManager.NodeList[parentNode->Component->UldManager.NodeListCount++] = (AtkResNode*)newNumberNode;
        var newLabelNode = CloneNode(labelNode);
        newNumberNode->AtkResNode.PrevSiblingNode = (AtkResNode*)newLabelNode;
        newLabelNode->AtkResNode.ParentNode = (AtkResNode*)parentNode;
        newLabelNode->AtkResNode.PrevSiblingNode = prevSiblingNode;
        newLabelNode->AtkResNode.NextSiblingNode = (AtkResNode*)newNumberNode;
        newLabelNode->AtkResNode.Y = parentNode->AtkResNode.Height - 24;
        newLabelNode->TextColor = new ByteColor { A = 0xFF, R = 0xA0, G = 0xA0, B = 0xA0 };
        newLabelNode->NodeText.StringPtr = (byte*)MemoryHelper.GameAllocateUi((ulong)newLabelNode->NodeText.BufSize);
        newLabelNode->SetText(label);
        parentNode->Component->UldManager.NodeList[parentNode->Component->UldManager.NodeListCount++] = (AtkResNode*)newLabelNode;
        return newNumberNode;
    }

    private unsafe double CalcDh(UIState* uiState) {
        var dh = uiState->PlayerState.Attributes[(int)Attributes.DirectHit];
        var lvl = uiState->PlayerState.CurrentLevel;
        return Math.Floor(550d * (dh - LevelModifiers.LevelTable[lvl].Sub) / LevelModifiers.LevelTable[lvl].Div) / 1000d;
    }

    private unsafe double CalcDet(UIState* uiState) {
        var det = uiState->PlayerState.Attributes[(int)Attributes.Determination];
        var lvl = uiState->PlayerState.CurrentLevel;
        return Math.Floor(140d * (det - LevelModifiers.LevelTable[lvl].Main) / LevelModifiers.LevelTable[lvl].Div) / 1000d;
    }

    private unsafe double CalcCritRate(UIState* uiState) {
        var crit = uiState->PlayerState.Attributes[(int)Attributes.CriticalHit];
        var lvl = uiState->PlayerState.CurrentLevel;
        return Math.Floor(200d * (crit - LevelModifiers.LevelTable[lvl].Sub) / LevelModifiers.LevelTable[lvl].Div + 50) / 1000d;
    }

    private unsafe double CalcCritDmg(UIState* uiState) {
        var crit = uiState->PlayerState.Attributes[(int)Attributes.CriticalHit];
        var lvl = uiState->PlayerState.CurrentLevel;
        return Math.Floor(200d * (crit - LevelModifiers.LevelTable[lvl].Sub) / LevelModifiers.LevelTable[lvl].Div + 1400) / 1000d;
    }

    private unsafe double CalcDef(UIState* uiState) {
        var def = uiState->PlayerState.Attributes[(int)Attributes.Defense];
        var lvl = uiState->PlayerState.CurrentLevel;
        return Math.Floor(15d * def / LevelModifiers.LevelTable[lvl].Div) / 100d;
    }

    private unsafe double CalcMagicDef(UIState* uiState) {
        var def = uiState->PlayerState.Attributes[(int)Attributes.MagicDefense];
        var lvl = uiState->PlayerState.CurrentLevel;
        return Math.Floor(15d * def / LevelModifiers.LevelTable[lvl].Div) / 100d;
    }

    private unsafe double CalcTenacity(UIState* uiState) {
        var ten = uiState->PlayerState.Attributes[(int)Attributes.Tenacity];
        var lvl = uiState->PlayerState.CurrentLevel;
        return Math.Floor(100d * (ten - LevelModifiers.LevelTable[lvl].Sub) / LevelModifiers.LevelTable[lvl].Div) / 1000d;
    }

    private unsafe double CalcPiety(UIState* uiState) {
        var pie = uiState->PlayerState.Attributes[(int)Attributes.Piety];
        var lvl = uiState->PlayerState.CurrentLevel;
        return 200 + Math.Floor(150d * (pie - LevelModifiers.LevelTable[lvl].Main) / LevelModifiers.LevelTable[lvl].Div);
    }

    private unsafe double CalcSkillSpeed(UIState* uiState) {
        var speed = uiState->PlayerState.Attributes[(int)Attributes.SkillSpeed];
        var lvl = uiState->PlayerState.CurrentLevel;
        return Math.Floor(130d * (speed - LevelModifiers.LevelTable[lvl].Sub) / LevelModifiers.LevelTable[lvl].Div) / 1000d;
    }

    private unsafe double CalcSpellSpeed(UIState* uiState) {
        var speed = uiState->PlayerState.Attributes[(int)Attributes.SpellSpeed];
        var lvl = uiState->PlayerState.CurrentLevel;
        return Math.Floor(130d * (speed - LevelModifiers.LevelTable[lvl].Sub) / LevelModifiers.LevelTable[lvl].Div) / 1000d;
    }

    private static unsafe double CalcRawDamage(UIState* uiState, JobId jobId, double det, double critDmg, double critRate, double dh) {
        var lvl = uiState->PlayerState.CurrentLevel;
        var ap = uiState->PlayerState.Attributes[(int)(jobId.IsCaster() ? Attributes.AttackMagicPotency : Attributes.AttackPower)];
        var main = LevelModifiers.LevelTable[lvl].Main;
        var equippedWeapon = InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems)->Items[0];
        var weaponItem = Service.DataManager.GetExcelSheet<Item>()?.GetRow(equippedWeapon.ItemID);
        var weaponBaseDamage = (jobId.IsCaster() ? weaponItem?.DamageMag : weaponItem?.DamagePhys) ?? 0;
        if (equippedWeapon.Flags.HasFlag(InventoryItem.ItemFlags.HQ)) {
            weaponBaseDamage += (ushort) (weaponItem?.UnkData73.FirstOrDefault(d => d.BaseParamSpecial == 12)?.BaseParamValueSpecial ?? 0);
        }
        var weaponDamage = Math.Floor(main * jobId.AttackModifier() / 1000.0 + weaponBaseDamage) / 100.0;
        var atk = Math.Floor(LevelModifiers.AttackModifier(lvl) * (ap - main) / main + 100) / 100.0;
        var rawDamage = Math.Floor(100 * atk * weaponDamage * (1 + det) * jobId.TraitModifiers(lvl) * (1 + (critDmg - 1) * critRate) * (1 + dh * 0.25));
        return rawDamage;
    }

    private static unsafe AtkTextNode* CloneNode(AtkTextNode* original) {
        var size = sizeof(AtkTextNode);
        var allocation = MemoryHelper.GameAllocateUi((ulong)size);
        var bytes = new byte[size];
        Marshal.Copy(new IntPtr(original), bytes, 0, bytes.Length);
        Marshal.Copy(bytes, 0, allocation, bytes.Length);

        var newNode = (AtkResNode*)allocation;
        newNode->ParentNode = null;
        newNode->ChildNode = null;
        newNode->ChildCount = 0;
        newNode->PrevSiblingNode = null;
        newNode->NextSiblingNode = null;
        return (AtkTextNode*)newNode;
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

    private static unsafe ulong Fnv1AHash(byte* bytesToHash, int length) {
        var hash = 14695981039346656037L;

        for (var i = 0; i < length; i++) {
            hash ^= bytesToHash[i];
            hash *= 1099511628211L;
        }

        return hash;
    }

    public void Dispose() {
        ClearPointers();
        Service.Framework.Update -= FrameworkOnUpdate;
        characterStatusOnSetup.Dispose();
    }
}