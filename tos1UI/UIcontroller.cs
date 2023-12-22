using System;
using System.Collections.Generic;
using Game.Interface;
using Game.Services;
using SML;
using HarmonyLib;
using Server.Shared.Info;
using Server.Shared.Messages;
using Server.Shared.State;
using UnityEngine.UI;
using Service = Services.Service;

namespace tos1UI
{
    [HarmonyPatch]
    public static class UIcontroller
    {

        public static Role role = Role.NONE;
        public static Image abilityIcon;
        public static int lastClicked = -1;
        public static bool flag = false;
        public static bool specialUnlocked = false;
        [HarmonyPatch(typeof(RoleCardPanel),nameof(RoleCardPanel.HandleOnMyIdentityChanged))]
        [HarmonyPostfix]
        public static void onRoleChange(ref RoleCardPanel __instance)
        {
            flag = false;
            specialUnlocked = false;
            role = Service.Game.Sim.simulation.myIdentity.Data.role;
            RoleInfo info = RoleInfoProvider.getInfo(role);
            if (info.isModified && !ModSettings.GetBool("Safe Mode")) __instance.specialAbilityPanel.Hide();
            if(info.isModified) abilityIcon = __instance.specialAbilityPanel.useButton.abilityIcon;
        }

        [HarmonyPatch(typeof(RoleCardPanel),nameof(RoleCardPanel.Update))]
        [HarmonyPostfix]
        public static void Update(ref RoleCardPanel __instance)
        {
            if (!flag) return;
            flag = false;
            specialUnlocked = __instance.specialAbilityPanel.IsSpecialUsable();
        }

        [HarmonyPatch(typeof(TosAbilityPanelListItem), nameof(TosAbilityPanelListItem.HandlePlayPhaseChanged))]
        [HarmonyPrefix]
        public static void checkSpecial()
        {
            flag = true;
        }
        [HarmonyPatch(typeof(TosAbilityPanelListItem), nameof(TosAbilityPanelListItem.HandlePlayPhaseChanged))]
        [HarmonyPostfix]
        public static void onPlayPhaseChange(ref TosAbilityPanelListItem __instance)
        {
            RoleInfo info = RoleInfoProvider.getInfo(role);
            if (!info.isModified) return;
            if (!specialUnlocked || !ModSettings.GetBool("Old " + info.configName)) return;
            int pos = __instance.characterPosition;
            if (info.AbilityTargetType == SpecialAbilityTargetType.Menu)
            {
                List<int> choices = Service.Game.Sim.info.menuChoiceObservations[MenuChoiceType.SpecialAbility].Data
                    .choices;
                if (choices.Contains(pos))
                {
                    __instance.choice2Sprite = abilityIcon;
                    __instance.choice2ButtonCanvasGroup.EnableRenderingAndInteraction();
                    if(lastClicked==pos) __instance.choice2Button.Select();
                    __instance.choice2Button.gameObject.SetActive(true);
                }
                else
                {
                    __instance.choice2ButtonCanvasGroup.DisableRenderingAndInteraction();
                    __instance.choice2Button.gameObject.SetActive(false);
                }
            }

            if (info.AbilityTargetType == SpecialAbilityTargetType.Self)
            {
                if (pos == Service.Game.Sim.simulation.myIdentity.Data.position)
                {
                    __instance.choice2Sprite = abilityIcon;
                    __instance.choice2ButtonCanvasGroup.EnableRenderingAndInteraction();
                    __instance.choice2Button.gameObject.SetActive(true);
                }
            }
        }
        
        
        [HarmonyPatch(typeof(TosAbilityPanelListItem),nameof(TosAbilityPanelListItem.OnClickChoice2))]
        [HarmonyPrefix]
        public static bool onClickChoice2(ref TosAbilityPanelListItem __instance)
        {
            RoleInfo info = RoleInfoProvider.getInfo(role);
            if (info.AbilityTargetType == SpecialAbilityTargetType.Necromancer) return true;
            if (!info.isModified) return true;
            if (!specialUnlocked || !ModSettings.GetBool("Old " + info.configName)) return true;
            __instance.PlaySound("Audio/UI/ClickSound.wav");
            MenuChoiceMessage message = new MenuChoiceMessage();
            message.choiceType = MenuChoiceType.SpecialAbility;
            message.choiceMode = MenuChoiceMode.TargetPosition;
            int pos = __instance.characterPosition;
            
            if (info.AbilityTargetType == SpecialAbilityTargetType.Menu)
            {
                lastClicked = pos;
                message.targetIndex = pos;
                if (!__instance.choice2Button.selected)
                {
                    message.choiceMode = MenuChoiceMode.Cancel;
                    lastClicked = -1;
                }
                Service.Game.Network.Send((GameMessage) message);
                return false;
            }

            if (info.AbilityTargetType == SpecialAbilityTargetType.Self)
            {
                message.targetIndex = 0;
                if (!__instance.choice2Button.selected)
                {
                    message.choiceMode = MenuChoiceMode.Cancel;
                    lastClicked = -1;
                }
                Service.Game.Network.Send((GameMessage) message);
                return false;
            }
            return true;
        }
    }
}