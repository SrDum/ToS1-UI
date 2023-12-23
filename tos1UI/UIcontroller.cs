using System;
using System.Collections.Generic;
using Game.Interface;
using Game.Services;
using SML;
using HarmonyLib;
using Server.Shared.Info;
using Server.Shared.Messages;
using Server.Shared.State;
using Server.Shared.State.Chat;
using tos1UI.borrowedCode;
using UnityEngine;
using UnityEngine.UI;
using Service = Services.Service;

namespace tos1UI
{
    [HarmonyPatch]
    public static class UIcontroller
    {

        public static Role role = Role.NONE;
        public static Sprite abilityIcon;
        public static int lastClicked = -1;
        public static bool flag = false;
        public static bool specialUnlocked = false;
        public static string abilityName = "";
        public static int specialCharges = -69;

        public static bool[] render =
        {
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false,
            false
        };

        public static void set(bool b)
        {
            for (int x = 0; x < 16; x++) render[x] = b;
        }
        
        
        [HarmonyPatch(typeof(RoleCardPanel),nameof(RoleCardPanel.ValidateSpecialAbilityPanel))]
        [HarmonyPostfix]
        public static void onRoleChange(ref RoleCardPanel __instance)
        {
            flag = false;
            specialUnlocked = false;
            set(false);
            role = Service.Game.Sim.simulation.myIdentity.Data.role;
            RoleInfo info = RoleInfoProvider.getInfo(role);
            if (info.isModified && !ModSettings.GetBool("Safe Mode")) __instance.specialAbilityPanel.Hide(); 
            if (info.isModified) abilityIcon = __instance.specialAbilityPanel.useButton.abilityIcon.sprite;
            if (info.isModified) abilityName = __instance.specialAbilityPanel.abilityText.text;
        }

        [HarmonyPatch(typeof(RoleCardPanel),nameof(RoleCardPanel.Update))]
        [HarmonyPostfix]
        public static void Update(ref RoleCardPanel __instance)
        {
            if (!flag) return;
            RoleInfo info = RoleInfoProvider.getInfo(role);
            if(info.isModified) specialUnlocked = __instance.specialAbilityPanel.IsSpecialUsable();
            if (info.isModified) abilityIcon = __instance.specialAbilityPanel.useButton.abilityIcon.sprite;
            if (info.isModified) abilityName = __instance.specialAbilityPanel.abilityText.text;
            flag = false;
            set(true);
        }

        [HarmonyPatch(typeof(RoleCardPanel), nameof(RoleCardPanel.HandleOnRoleCardDataChanged))]
        [HarmonyPostfix]
        public static void getRemaining(RoleCardData data)
        {
            
            if (data.specialAbilityTotal > 0)
            {
                specialCharges = data.specialAbilityRemaining;
            }
            else
            {
                specialCharges = -69;
            }
        }

        [HarmonyPatch(typeof(TosAbilityPanel), nameof(TosAbilityPanel.HandlePlayPhaseChanged))]
        [HarmonyPostfix]
        public static void onPlayPhaseChanged()
        {
            flag = true;
        }

        [HarmonyPatch(typeof(TosAbilityPanelListItem), nameof(TosAbilityPanelListItem.Update))]
        [HarmonyPostfix]
        public static void renderButtons(ref TosAbilityPanelListItem __instance)
        {
                int pos = __instance.characterPosition;
                if (!specialUnlocked || !render[pos]) return;
                RoleInfo info = RoleInfoProvider.getInfo(role);
                if (!info.isModified) return;
                if (!ModSettings.GetBool("Old " + info.configName)) return;
                
                if (info.track && specialCharges >= 0 && !ModSettings.GetBool("Safe Mode") && specialUnlocked)
                {
                    ChatUtils.AddMessage(message:"You have "+specialCharges+" "+abilityName+"s remaining.");
                }
                
                if (info.AbilityTargetType == SpecialAbilityTargetType.Menu)
                {
                    List<int> choices = Service.Game.Sim.info.menuChoiceObservations[MenuChoiceType.SpecialAbility].Data
                        .choices;
                    if (choices.Contains(pos))
                    {
                        __instance.choice2Sprite.sprite = abilityIcon;
                        __instance.choice2Text.text = abilityName;
                        __instance.choice2ButtonCanvasGroup.EnableRenderingAndInteraction();
                        if (lastClicked == pos) __instance.choice2Button.Select();
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
                        __instance.choice1Sprite.sprite = abilityIcon;
                        __instance.choice1Text.text = abilityName;
                        __instance.choice1ButtonCanvasGroup.EnableRenderingAndInteraction();
                        __instance.choice1Button.gameObject.SetActive(true);
                    }
                }

                render[pos] = false;
        }


        [HarmonyPatch(typeof(TosAbilityPanelListItem),nameof(TosAbilityPanelListItem.OnClickChoice2))]
        [HarmonyPrefix]
        public static bool onClickChoice2(ref TosAbilityPanelListItem __instance)
        {
            RoleInfo info = RoleInfoProvider.getInfo(role);
            if (info.AbilityTargetType == SpecialAbilityTargetType.Necromancer) return true;
            Console.Out.Write("[ToS 1 UI] Role is not Necromancer");
            if (!info.isModified) return true;
            Console.Out.Write("[ToS 1 UI] role is modded");
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

           
            return true;
        }

        [HarmonyPatch(typeof(TosAbilityPanelListItem), nameof(TosAbilityPanelListItem.OnClickChoice1))]
        [HarmonyPrefix]
        public static bool onClickChoice1(ref TosAbilityPanelListItem __instance)
        {
            
            RoleInfo info = RoleInfoProvider.getInfo(role);
            if (info.AbilityTargetType == SpecialAbilityTargetType.Necromancer) return true;
            if (!info.isModified) return true;
            if (!ModSettings.GetBool("Old " + info.configName)) return true;
            int myPos = Service.Game.Sim.simulation.myIdentity.Data.position;
            MenuChoiceMessage message = new MenuChoiceMessage();
            message.choiceType = MenuChoiceType.SpecialAbility;
            message.choiceMode = MenuChoiceMode.TargetPosition;
            if (info.AbilityTargetType == SpecialAbilityTargetType.Self)
            {
                if (__instance.characterPosition == myPos)
                {
                    __instance.PlaySound("Audio/UI/ClickSound.wav");
                    if (!__instance.choice1Button.selected)
                    {
                        message.choiceMode = MenuChoiceMode.Cancel;
                    }
                    Service.Game.Network.Send((GameMessage) message);
                    return false;

                }
               
            }

            return true;
        }
    }
}