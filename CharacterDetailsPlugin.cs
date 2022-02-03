using System;
using System.Runtime.InteropServices;
using Dalamud.Game;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Memory;
using Dalamud.Plugin;
using DeathRecap;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace CharacterDetails {
    public class CharacterDetailsPlugin : IDalamudPlugin {
        public string Name => "Character Panel Refined";

        private readonly Hook<AddonOnSetup> characterStatusOnSetup;

        private unsafe delegate void* AddonOnSetup(AtkUnitBase* atkUnitBase, int a2, void* a3);

        private IntPtr characterStatusPtr;
        private IntPtr dhChancePtr;
        private IntPtr detDmgIncreasePtr;
        private IntPtr critDmgPtr;
        private IntPtr critChancePtr;
        private IntPtr physMitPtr;
        private IntPtr magicMitPtr;
        private IntPtr sksSpeedIncreasePtr;
        private IntPtr spsSpeedIncreasePtr;
        private IntPtr tenMitPtr;
        private IntPtr pieManaPtr;

        public CharacterDetailsPlugin(DalamudPluginInterface pluginInterface) {
            Service.Initialize(pluginInterface);

            var characterStatusOnSetupPtr =
                Service.SigScanner.ScanText("4C 8B DC 55 53 41 56 49 8D 6B A1 48 81 EC F0 00 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 45 07");

            unsafe {
                characterStatusOnSetup = new Hook<AddonOnSetup>(characterStatusOnSetupPtr, CharacterStatusOnSetup);
            }

            characterStatusOnSetup.Enable();

            Service.Framework.Update += FrameworkOnUpdate;
        }

        private unsafe void FrameworkOnUpdate(Framework framework) {
            if (characterStatusPtr == IntPtr.Zero) return;
            var charStatus = Service.GameGui.GetAddonByName("CharacterStatus", 1);
            if (charStatus == characterStatusPtr) {
                var uiState = UIState.Instance();
                ((AtkTextNode*)dhChancePtr)->SetText($"{CalcDh(uiState):P1}");
                ((AtkTextNode*)detDmgIncreasePtr)->SetText($"{CalcDet(uiState):P1}");
                ((AtkTextNode*)critChancePtr)->SetText($"{CalcCritRate(uiState):P1}");
                ((AtkTextNode*)critDmgPtr)->SetText($"{CalcCritDmg(uiState):P1}");
                ((AtkTextNode*)magicMitPtr)->SetText($"{CalcMagicDef(uiState):P0}");
                ((AtkTextNode*)physMitPtr)->SetText($"{CalcDef(uiState):P0}");
                ((AtkTextNode*)spsSpeedIncreasePtr)->SetText($"{CalcSpellSpeed(uiState):P1}");
                ((AtkTextNode*)sksSpeedIncreasePtr)->SetText($"{CalcSkillSpeed(uiState):P1}");
                ((AtkTextNode*)pieManaPtr)->SetText($"{CalcPiety(uiState):N0}");
                ((AtkTextNode*)tenMitPtr)->SetText($"{CalcTenacity(uiState):P1}");
            } else {
                ClearPointers();
            }
        }

        private void ClearPointers() {
            characterStatusPtr = IntPtr.Zero;
            dhChancePtr = IntPtr.Zero;
            detDmgIncreasePtr = IntPtr.Zero;
            critDmgPtr = IntPtr.Zero;
            critChancePtr = IntPtr.Zero;
            physMitPtr = IntPtr.Zero;
            magicMitPtr = IntPtr.Zero;
            sksSpeedIncreasePtr = IntPtr.Zero;
            spsSpeedIncreasePtr = IntPtr.Zero;
            tenMitPtr = IntPtr.Zero;
            pieManaPtr = IntPtr.Zero;
        }

        private unsafe void* CharacterStatusOnSetup(AtkUnitBase* atkUnitBase, int a2, void* a3) {
            var val = characterStatusOnSetup.Original(atkUnitBase, a2, a3);
            
            try {
                var offensive = atkUnitBase->UldManager.SearchNodeById(36);
                offensive->Height += 20 * 4;
                var dh = offensive->ChildNode;
                dh->Y += 20 * 3;
                dhChancePtr = AddStatRow((AtkComponentNode*)dh, "Direct Hit Chance");
                var det = dh->PrevSiblingNode;
                det->Y += 20 * 2;
                detDmgIncreasePtr = AddStatRow((AtkComponentNode*)det, "Damage Increase");
                var crit = det->PrevSiblingNode;
                critChancePtr = AddStatRow((AtkComponentNode*)crit, "Crit Chance");
                critDmgPtr = AddStatRow((AtkComponentNode*)crit, "Crit Damage");

                var defensive = atkUnitBase->UldManager.SearchNodeById(44);
                defensive->Height += 20 * 2;
                var magicDef = defensive->ChildNode;
                magicDef->Y += 20;
                magicMitPtr = AddStatRow((AtkComponentNode*)magicDef, "Magic Mitigation");
                var def = magicDef->PrevSiblingNode;
                physMitPtr = AddStatRow((AtkComponentNode*)def, "Physical Mitigation");

                var mentProperties = atkUnitBase->UldManager.SearchNodeById(58);
                mentProperties->Y += 20 * 4;
                mentProperties->X = 0;
                var spellSpeedNode = mentProperties->ChildNode;
                spellSpeedNode->PrevSiblingNode->ToggleVisibility(false); // Magic Attack Potency
                spellSpeedNode->PrevSiblingNode->PrevSiblingNode->ToggleVisibility(false); // Magic Heal Potency
                spellSpeedNode->PrevSiblingNode->PrevSiblingNode->PrevSiblingNode->ToggleVisibility(false); // Header
                spsSpeedIncreasePtr = AddStatRow((AtkComponentNode*)spellSpeedNode, "Speed Increase");

                var physProperties = atkUnitBase->UldManager.SearchNodeById(51);
                physProperties->Y += 20 * 4;
                var sks = physProperties->ChildNode;
                sks->Y -= 20;
                sks->PrevSiblingNode->ToggleVisibility(false);
                ((AtkTextNode*)sks->PrevSiblingNode->PrevSiblingNode->ChildNode->PrevSiblingNode)->SetText("Speed Properties");
                sksSpeedIncreasePtr = AddStatRow((AtkComponentNode*)sks, "Speed Increase");

                var gearProp = atkUnitBase->UldManager.SearchNodeById(80);
                gearProp->ToggleVisibility(false);

                var roleProp = atkUnitBase->UldManager.SearchNodeById(86);
                roleProp->Y = 240 + 20 * 4;
                roleProp->Height += 20 * 1;
                var piety = roleProp->ChildNode;
                piety->Y += 20;
                pieManaPtr = AddStatRow((AtkComponentNode*)piety, "Mana per Tick");
                var tenacity = piety->PrevSiblingNode;
                tenMitPtr = AddStatRow((AtkComponentNode*)tenacity, "Damage & Mitigation");

                characterStatusPtr = (IntPtr)atkUnitBase;
            } catch (Exception e) {
                PluginLog.LogError(e, "Failed to modify character");
            }

            return val;
        }

        private unsafe IntPtr AddStatRow(AtkComponentNode* parentNode, string label) {
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
            return (IntPtr)newNumberNode;
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
            return Math.Floor(150d * (pie - LevelModifiers.LevelTable[lvl].Main) / LevelModifiers.LevelTable[lvl].Div);
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

        public void Dispose() {
            ClearPointers();
            Service.Framework.Update -= FrameworkOnUpdate;
            characterStatusOnSetup.Dispose();
        }
    }
}