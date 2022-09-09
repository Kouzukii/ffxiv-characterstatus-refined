using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Dalamud;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Memory;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.STD;
using Lumina.Data;

namespace CharacterPanelRefined;

public class CharacterPanelRefinedPlugin : IDalamudPlugin {
    public string Name => "Character Panel Refined";
    
    public Configuration Configuration { get; }
    public ConfigWindow ConfigWindow { get; }

    private readonly Hook<AddonOnSetup> characterStatusOnSetup;
    private readonly Hook<RequestUpdate> characterStatusRequestUpdate;
    private readonly Tooltips tooltips = new();

    private unsafe delegate void* AddonOnSetup(AtkUnitBase* atkUnitBase, int a2, void* a3);

    private unsafe delegate void* RequestUpdate(AtkUnitBase* atkUnitBase, void* a2);


    private IntPtr characterStatusPtr;
    private unsafe AtkTextNode* dhChancePtr;
    private unsafe AtkTextNode* detDmgIncreasePtr;
    private unsafe AtkTextNode* critDmgPtr;
    private unsafe AtkTextNode* critChancePtr;
    private unsafe AtkTextNode* physMitPtr;
    private unsafe AtkTextNode* magicMitPtr;
    private unsafe AtkTextNode* sksSpeedIncreasePtr;
    private unsafe AtkTextNode* sksGcdPtr;
    private unsafe AtkTextNode* spsSpeedIncreasePtr;
    private unsafe AtkTextNode* spsGcdPtr;
    private unsafe AtkTextNode* sps28GcdPtr;
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

    public CharacterPanelRefinedPlugin(DalamudPluginInterface pluginInterface) {
        
        Service.Initialize(pluginInterface);

        Configuration = Configuration.Get(pluginInterface);
        ConfigWindow = new ConfigWindow(this);
        
        

        var characterStatusOnSetupPtr =
            Service.SigScanner.ScanText("4C 8B DC 55 53 41 56 49 8D 6B A1 48 81 EC F0 00 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 45 07");
        var characterStatusRequestUpdatePtr = Service.SigScanner.ScanText(
            "48 89 5c 24 08 48 89 6c 24 10 48 89 74 24 18 57 48 83 ec 50 48 8b 7a 18 48 8b f1 48 8b aa ?? ?? ?? ?? 48 8b 47 20");

        unsafe {
            characterStatusOnSetup = Hook<AddonOnSetup>.FromAddress(characterStatusOnSetupPtr, CharacterStatusOnSetup);
            characterStatusRequestUpdate = Hook<RequestUpdate>.FromAddress(characterStatusRequestUpdatePtr, CharacterStatusRequestUpdate);
        }

        characterStatusOnSetup.Enable();
        characterStatusRequestUpdate.Enable();

        pluginInterface.UiBuilder.Draw += ConfigWindow.Draw;
        pluginInterface.UiBuilder.OpenConfigUi += () => ConfigWindow.ShowConfig = true;
    }
    
    private void LoadLocalization() {
        var lang = "";
        
        if (Configuration.UseGameLanguage)
        {
            lang = Service.ClientState.ClientLanguage switch {
                ClientLanguage.English => "",
                ClientLanguage.French => "fr",
                ClientLanguage.German => "de",
                ClientLanguage.Japanese => "ja",
                _ => ""
            };
        }
        
        PluginLog.LogInformation("lang: " + lang);
        
        Languages.Culture = new CultureInfo(lang);
    }

    private unsafe void* CharacterStatusRequestUpdate(AtkUnitBase* atkUnitBase, void* a2) {
        try {
            if ((IntPtr)atkUnitBase == characterStatusPtr) {
                var uiState = UIState.Instance();
                var lvl = uiState->PlayerState.CurrentLevel;
                var levelModifier = LevelModifiers.LevelTable[lvl];
                var statInfo = new StatInfo();

                var dh = Equations.CalcDh(uiState->PlayerState.Attributes[(int)Attributes.DirectHit], ref statInfo, ref levelModifier);
                dhChancePtr->SetText($"{statInfo.DisplayValue:P1}");
                tooltips.Update(Tooltips.Entry.DirectHit, ref statInfo);

                var det = Equations.CalcDet(uiState->PlayerState.Attributes[(int)Attributes.Determination], ref statInfo, ref levelModifier);
                tooltips.Update(Tooltips.Entry.Determination, ref statInfo);
                detDmgIncreasePtr->SetText($"{statInfo.DisplayValue:P1}");

                var critRate = Equations.CalcCritRate(uiState->PlayerState.Attributes[(int)Attributes.CriticalHit], ref statInfo, ref levelModifier);
                tooltips.Update(Tooltips.Entry.Crit, ref statInfo);
                critChancePtr->SetText($"{statInfo.DisplayValue:P1}");

                var critDmg = Equations.CalcCritDmg(uiState->PlayerState.Attributes[(int)Attributes.CriticalHit], ref statInfo, ref levelModifier);
                critDmgPtr->SetText($"{statInfo.DisplayValue:P1}");

                Equations.CalcMagicDef(uiState->PlayerState.Attributes[(int)Attributes.MagicDefense], ref statInfo, ref levelModifier);
                magicMitPtr->SetText($"{statInfo.DisplayValue:P0}");
                tooltips.Update(Tooltips.Entry.MagicDefense, ref statInfo);

                Equations.CalcDef(uiState->PlayerState.Attributes[(int)Attributes.Defense], ref statInfo, ref levelModifier);
                physMitPtr->SetText($"{statInfo.DisplayValue:P0}");
                tooltips.Update(Tooltips.Entry.Defense, ref statInfo);

                var jobId = (JobId)(*((byte*)&uiState->PlayerState + 0x6E));

                StatInfo gcd25 = new(), gcd28 = new();
                if (jobId.IsCaster()) {
                    var isBlm = jobId == JobId.BLM;
                    Equations.CalcSpeed(uiState->PlayerState.Attributes[(int)Attributes.SpellSpeed], ref statInfo, ref gcd25, ref gcd28, ref levelModifier, isBlm);
                    spsSpeedIncreasePtr->SetText($"{statInfo.DisplayValue:P1}");
                    spsGcdPtr->SetText($"{gcd25.DisplayValue:N2}s");
                    sps28GcdPtr->SetText($"{gcd28.DisplayValue:N2}s");
                    tooltips.UpdateSpeed(ref statInfo, ref gcd25, ref gcd28, isBlm);
                    SetTooltip(spsSpeedIncreasePtr, isBlm ? Tooltips.Entry.Speed28 : Tooltips.Entry.Speed);
                } else {
                    Equations.CalcSpeed(uiState->PlayerState.Attributes[(int)Attributes.SkillSpeed], ref statInfo, ref gcd25, ref gcd28, ref levelModifier, false);
                    sksSpeedIncreasePtr->SetText($"{statInfo.DisplayValue:P1}");
                    sksGcdPtr->SetText($"{gcd25.DisplayValue:N2}s");
                    tooltips.UpdateSpeed(ref statInfo, ref gcd25, ref gcd28, false);
                }

                Equations.CalcPiety(uiState->PlayerState.Attributes[(int)Attributes.Piety], ref statInfo, ref levelModifier);
                pieManaPtr->SetText($"{statInfo.DisplayValue:N0}");
                tooltips.Update(Tooltips.Entry.Piety, ref statInfo);

                var ten = Equations.CalcTenacity(uiState->PlayerState.Attributes[(int)Attributes.Tenacity], ref statInfo, ref levelModifier);
                tenMitPtr->SetText($"{statInfo.DisplayValue:P1}");
                tooltips.Update(Tooltips.Entry.Tenacity, ref statInfo);
                
                Equations.CalcHp(uiState, jobId, out var hpPerVitality, out var hpModifier);
                tooltips.UpdateVitality(jobId.ToString(), hpPerVitality, hpModifier);

                expectedDamagePtr->SetText($"{Equations.CalcRawDamage(uiState, jobId, det, critDmg, critRate, dh, ten, ref levelModifier):N0}");
                if (jobId != lastJob) {
                    UpdateCharacterPanelForJob(jobId);
                }
            } else {
                ClearPointers();
            }
        } catch (Exception e) {
            PluginLog.LogError(e, Languages.ERROR_Failed_to_update_CharacterStatus);
        }

        return characterStatusRequestUpdate.Original(atkUnitBase, a2);
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
        sksGcdPtr = null;
        spsSpeedIncreasePtr = null;
        spsGcdPtr = null;
        sps28GcdPtr = null;
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
            expectedDamagePtr->AtkResNode.ParentNode->ToggleVisibility(false);
        } else {
            offensivePtr->ToggleVisibility(true);
            defensivePtr->ToggleVisibility(true);
            physPropertiesPtr->ToggleVisibility(true);
            expectedDamagePtr->AtkResNode.ParentNode->ToggleVisibility(true);
            if (job.IsCaster()) {
                skillSpeedPtr->ToggleVisibility(false);
                spellSpeedPtr->ToggleVisibility(true);
                skillSpeedPtr->ToggleVisibility(false);
                spellSpeedPtr->ToggleVisibility(true);
                sps28GcdPtr->AtkResNode.ToggleVisibility(job == JobId.BLM);
                sps28GcdPtr->AtkResNode.PrevSiblingNode->ToggleVisibility(job == JobId.BLM);
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
    
    

    private unsafe void* CharacterStatusOnSetup(AtkUnitBase* atkUnitBase, int a2, void* a3) {
        
        LoadLocalization();
        
        var val = characterStatusOnSetup.Original(atkUnitBase, a2, a3);

        try {
            var job = (JobId)UIState.Instance()->PlayerState.CurrentClassJobId;

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

            var gearProp = atkUnitBase->UldManager.SearchNodeById(80);
            gearProp->Y = 80;
            var avgItemLvlNode = gearProp->ChildNode;
            avgItemLvlNode->PrevSiblingNode->ToggleVisibility(false); // header
            expectedDamagePtr = AddStatRow((AtkComponentNode*)avgItemLvlNode, Languages.Damage_per_100_Potency);
            expectedDamagePtr->AtkResNode.NextSiblingNode->ToggleVisibility(false);
            expectedDamagePtr->AtkResNode.NextSiblingNode->NextSiblingNode->SetScale(0, 0); // toggle visibility doesn't work?
            SetTooltip(expectedDamagePtr, Tooltips.Entry.ExpectedDamage);

            var dexNode = vitalityNode->PrevSiblingNode;
            dexNode->Y = 40;
            SetTooltip((AtkComponentNode*)dexNode, Tooltips.Entry.MainStat);
            var strNode = dexNode->PrevSiblingNode;
            strNode->Y = 40;
            SetTooltip((AtkComponentNode*)strNode, Tooltips.Entry.MainStat);

            offensivePtr = atkUnitBase->UldManager.SearchNodeById(36);
            offensivePtr->Y = 150;
            var dh = offensivePtr->ChildNode;
            dh->Y = 120;
            dhChancePtr = AddStatRow((AtkComponentNode*)dh, Languages.Direct_Hit_Chance);
            SetTooltip(dhChancePtr, Tooltips.Entry.DirectHit);
            var det = dh->PrevSiblingNode;
            det->Y = 80;
            detDmgIncreasePtr = AddStatRow((AtkComponentNode*)det, Languages.Damage_Increase);
            SetTooltip(detDmgIncreasePtr, Tooltips.Entry.Determination);
            var crit = det->PrevSiblingNode;
            critChancePtr = AddStatRow((AtkComponentNode*)crit, Languages.Crit_Chance);
            critDmgPtr = AddStatRow((AtkComponentNode*)crit, Languages.Crit_Damage);
            SetTooltip(critChancePtr, Tooltips.Entry.Crit);

            defensivePtr = atkUnitBase->UldManager.SearchNodeById(44);
            defensivePtr->Y = 150;
            var magicDef = defensivePtr->ChildNode;
            magicDef->Y = 60;
            magicMitPtr = AddStatRow((AtkComponentNode*)magicDef, Languages.Magic_Mitigation);
            SetTooltip(magicMitPtr, Tooltips.Entry.MagicDefense);
            var def = magicDef->PrevSiblingNode;
            physMitPtr = AddStatRow((AtkComponentNode*)def, Languages.Physical_Mitigation);
            SetTooltip(physMitPtr, Tooltips.Entry.Defense);

            var mentProperties = atkUnitBase->UldManager.SearchNodeById(58);
            mentProperties->X = 0;
            mentProperties->Y = 280;
            spellSpeedPtr = mentProperties->ChildNode;
            spellSpeedPtr->PrevSiblingNode->ToggleVisibility(false); // Magic Attack Potency
            spellSpeedPtr->PrevSiblingNode->PrevSiblingNode->ToggleVisibility(false); // Magic Heal Potency
            spellSpeedPtr->PrevSiblingNode->PrevSiblingNode->PrevSiblingNode->ToggleVisibility(false); // Header
            spsSpeedIncreasePtr = AddStatRow((AtkComponentNode*)spellSpeedPtr, Languages.Skill_Speed_Increase);
            spsGcdPtr = AddStatRow((AtkComponentNode*)spellSpeedPtr, Languages.GCD);
            sps28GcdPtr = AddStatRow((AtkComponentNode*)spellSpeedPtr, Languages.Fire_IV_GCD);
            SetTooltip(spsSpeedIncreasePtr, Tooltips.Entry.Speed);

            physPropertiesPtr = atkUnitBase->UldManager.SearchNodeById(51);
            physPropertiesPtr->Y = 320;
            skillSpeedPtr = physPropertiesPtr->ChildNode;
            skillSpeedPtr->Y = 20;
            skillSpeedPtr->PrevSiblingNode->ToggleVisibility(false); // Attack Power
            ((AtkTextNode*)skillSpeedPtr->PrevSiblingNode->PrevSiblingNode->ChildNode->PrevSiblingNode)->SetText(Languages.Speed_Properties);
            sksSpeedIncreasePtr = AddStatRow((AtkComponentNode*)skillSpeedPtr, Languages.Skill_Speed_Increase);
            sksGcdPtr = AddStatRow((AtkComponentNode*)skillSpeedPtr, Languages.GCD);
            SetTooltip(sksSpeedIncreasePtr, Tooltips.Entry.Speed);

            var roleProp = atkUnitBase->UldManager.SearchNodeById(86);
            roleProp->Y = 60;
            pietyPtr = roleProp->ChildNode;
            pietyPtr->Y = 20;
            pieManaPtr = AddStatRow((AtkComponentNode*)pietyPtr, Languages.Mana_per_Tick);
            SetTooltip(pieManaPtr, Tooltips.Entry.Piety);
            tenacityPtr = pietyPtr->PrevSiblingNode;
            tenMitPtr = AddStatRow((AtkComponentNode*)tenacityPtr, Languages.Damage___Mitigation);
            tenacityPtr->PrevSiblingNode->ToggleVisibility(false); // header
            SetTooltip(tenMitPtr, Tooltips.Entry.Tenacity);

            var craftingPtr = atkUnitBase->UldManager.SearchNodeById(73);
            craftingPtr->X = 0;
            craftingPtr->Y = 80;
            craftingPtr->ChildNode->PrevSiblingNode->PrevSiblingNode->ToggleVisibility(false); // header

            var gatheringPtr = atkUnitBase->UldManager.SearchNodeById(66);
            gatheringPtr->X = 0;
            gatheringPtr->Y = 80;
            gatheringPtr->ChildNode->PrevSiblingNode->PrevSiblingNode->ToggleVisibility(false); // header

            characterStatusPtr = (IntPtr)atkUnitBase;

            UpdateCharacterPanelForJob(job);
        } catch (Exception e) {
            PluginLog.LogError(e, Languages.ERROR_Failed_to_modify_character);
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
        characterStatusOnSetup.Dispose();
        characterStatusRequestUpdate.Disable();
        tooltips.Dispose();
    }
}
