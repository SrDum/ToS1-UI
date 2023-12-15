using System;
using Game.Interface;
using SML;
using HarmonyLib;
using Server.Shared.Extensions;
using Server.Shared.Info;
using Server.Shared.Messages;
using Server.Shared.State;
using Service = Services.Service;


namespace tos1UI
{

    [HarmonyPatch(typeof(RoleCardPanel), "HandleOnMyIdentityChanged")]
    public class NecromancerUI
    {
        static void Postfix(PlayerIdentityData playerIdentityData, ref RoleCardPanel __instance)
        {
            if (Service.Game.Sim.simulation.myIdentity.Data.role == Role.NECROMANCER)
            {
                if (!ModSettings.GetBool("Old Necromancer")) return;
                ensureButtonsNecromancer.setTrue();
                if (ModSettings.GetBool("Safe Mode")) return;
                __instance.specialAbilityPanel.Hide();
            }
        }
    }

    [HarmonyPatch(typeof(RoleCardPanel), "ValidateSpecialAbilityPanel")]
    public class NecromancerUI2
    {
        static void Postfix(ref RoleCardPanel __instance)
        {
            if (Service.Game.Sim.simulation.myIdentity.Data.role == Role.NECROMANCER && !ModSettings.GetBool("Safe Mode"))
            {
                if (!ModSettings.GetBool("Old Necromancer")) return;
                ensureButtonsNecromancer.setTrue();
                if (ModSettings.GetBool("Safe Mode")) return;
                __instance.specialAbilityPanel.Hide();
            }
        }
    }

    [HarmonyPatch(typeof(TosAbilityPanelListItem), "Update")]
    public class ensureButtonsNecromancer
    {
        public static bool[] reload =
            { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false ,false};

        public static void setFalse()
        {
            for (int x = 0; x < 16; x++)
            {
                reload[x] = false;
            }
        }
        public static void setTrue()
        {
            for (int x = 0; x < 16; x++)
            {
                reload[x] = true;
            }
        }

        public static bool isPressed = false;
        
        public static void Postfix(ref TosAbilityPanelListItem __instance)
        {
            if (!reload[__instance.characterPosition]) return;
            PlayPhase phase = Service.Game.Sim.info.gameInfo.Data.playPhase;
            if (!ModSettings.GetBool("Old Necromancer")) return;
            if (phase != PlayPhase.NIGHT)
            {
                AddGhoulButton.isUsingGhoul = false;
                changeFilterAuto.flag = false;
            }

            if (Service.Game.Sim.simulation.myIdentity.Data.role == Role.NECROMANCER && phase == PlayPhase.NIGHT && __instance.playerRole != Role.NECROMANCER)
            {
                __instance.OverrideIconAndText(TosAbilityPanelListItem.OverrideAbilityType.DEFAULT);
            }
            if (Service.Game.Sim.simulation.myIdentity.Data.role == Role.NECROMANCER && phase == PlayPhase.NIGHT 
                && AddGhoulButton.isUsingGhoul && __instance.playerRole!=Role.NECROMANCER && !__instance.playerRole.IsCovenAligned()
                &&Service.Game.Sim.info.roleCardObservation.Data.powerUp==POWER_UP_TYPE.NECRONOMICON)
            {
                if (!__instance.halo.activeSelf)
                {
                    __instance.choice2ButtonCanvasGroup.EnableRenderingAndInteraction();
                    __instance.choice2Button.gameObject.SetActive(true);
                }
            }

            if (Service.Game.Sim.simulation.myIdentity.Data.role == Role.NECROMANCER && phase == PlayPhase.NIGHT
                && !isPressed &&Service.Game.Sim.info.roleCardObservation.Data.powerUp==POWER_UP_TYPE.NECRONOMICON)
            {
                if (!__instance.halo.activeSelf)
                {
                    if(__instance.choice2Button.selected) __instance.choice2Button.Deselect();
                    __instance.choice2ButtonCanvasGroup.DisableRenderingAndInteraction();
                    __instance.choice2Button.gameObject.SetActive(false);
                }
            }
            
            if (Service.Game.Sim.simulation.myIdentity.Data.role == Role.NECROMANCER &&
                __instance.playerRole == Role.NECROMANCER && phase == PlayPhase.NIGHT
                &&Service.Game.Sim.info.roleCardObservation.Data.powerUp==POWER_UP_TYPE.NECRONOMICON
               )
            {
                if (!ModStates.IsLoaded("alchlcsystm.recolors"))
                {
                    __instance.choice1Sprite.sprite = LoadEmbeddedResources.LoadSprite("tos1UI.resources.ghoul.png");
                }

                __instance.choice1Text.text = "Raise Ghoul";
                __instance.choice1ButtonCanvasGroup.EnableRenderingAndInteraction();
                if (!__instance.halo.activeSelf)
                {
                    __instance.choice1Button.gameObject.SetActive(true);
                }

                if (AddGhoulButton.isUsingGhoul)
                {
                    __instance.choice2ButtonCanvasGroup.DisableRenderingAndInteraction();
                    __instance.choice2Button.gameObject.SetActive(false);
                }
                
            }

            reload[__instance.characterPosition] = false;
        }

    }

    [HarmonyPatch(typeof(TosAbilityPanelListItem), "HandlePlayPhaseChanged")]
    public class AddGhoulButton
    {

        public static bool isUsingGhoul = false;
        
        static void Postfix(PlayPhaseState playPhase, ref TosAbilityPanelListItem __instance)
        {
            CleanupNecromancer.lastTarget = -1;
            CleanupNecromancer.isCov = false;
            if (!ModSettings.GetBool("Old Necromancer")) return;
            PlayPhase phase = playPhase.playPhase;
            if (phase != PlayPhase.NIGHT)
            {
                isUsingGhoul = false;
                changeFilterAuto.flag = false;
            }
            if (Service.Game.Sim.simulation.myIdentity.Data.role == Role.NECROMANCER && phase == PlayPhase.NIGHT && __instance.playerRole != Role.NECROMANCER
                && Service.Game.Sim.info.roleCardObservation.Data.powerUp == POWER_UP_TYPE.NECRONOMICON)
            {
                __instance.OverrideIconAndText(TosAbilityPanelListItem.OverrideAbilityType.DEFAULT);
            }
            if (Service.Game.Sim.simulation.myIdentity.Data.role == Role.NECROMANCER &&
                __instance.playerRole == Role.NECROMANCER && phase == PlayPhase.NIGHT
                &&Service.Game.Sim.info.roleCardObservation.Data.powerUp==POWER_UP_TYPE.NECRONOMICON
               )
            {
                if (!ModStates.IsLoaded("alchlcsystm.recolors"))
                {
                    __instance.choice1Sprite.sprite = LoadEmbeddedResources.LoadSprite("tos1UI.resources.ghoul.png");
                }

                __instance.choice1Text.text = "Raise Ghoul";
                __instance.choice1ButtonCanvasGroup.EnableRenderingAndInteraction();
                if (!__instance.halo.activeSelf)
                {
                    __instance.choice1Button.gameObject.SetActive(true);
                }
            }
        }
    }

    [HarmonyPatch(typeof(TosAbilityPanel), "IsPlayerTargetable")]
    public class makeSelfTargetable
    {
        public static bool Prefix(int postion, ref bool __result,ref TosAbilityPanel __instance)
        {
            if (Service.Game.Sim.simulation.myIdentity.Data.role == Role.NECROMANCER &&
                Service.Game.Sim.info.gameInfo.Data.playPhase == PlayPhase.NIGHT
                && postion==Service.Game.Sim.simulation.myIdentity.Data.position 
                &&Service.Game.Sim.info.roleCardObservation.Data.powerUp==POWER_UP_TYPE.NECRONOMICON
               )
            {
                __result = true;
                return false;
            }

            if (Service.Game.Sim.simulation.myIdentity.Data.role == Role.NECROMANCER &&
                Service.Game.Sim.info.gameInfo.Data.playPhase == PlayPhase.NIGHT
                && postion != Service.Game.Sim.simulation.myIdentity.Data.position
                && AddGhoulButton.isUsingGhoul  &&Service.Game.Sim.info.roleCardObservation.Data.powerUp==POWER_UP_TYPE.NECRONOMICON)
            {
                foreach(TosAbilityPanelListItem listItem in __instance.playerListPlayers)
                {
                    if (listItem.characterPosition == postion && !listItem.halo.activeSelf)
                    {
                        __result = true;
                        return false;
                    }
                    if(listItem.characterPosition==postion && listItem.halo.activeSelf)
                    {
                        return true;
                    }
                }
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(TosAbilityPanel), "Update")]
    public class changeFilterAuto
    {
        public static bool flag = false;
        public static void Postfix(ref TosAbilityPanel __instance)
        {
            if (!flag) return;
            if (Service.Game.Sim.simulation.myIdentity.Data.role == Role.NECROMANCER &&
                Service.Game.Sim.info.gameInfo.Data.playPhase == PlayPhase.NIGHT)
            {
                __instance.OnClickFilterTargets();
                ensureButtonsNecromancer.setTrue();
                flag = false;
            }
        }
    }

    [HarmonyPatch(typeof(TosAbilityPanelListItem), "OnClickChoice1")]
    public class CleanupNecromancer
    {
        public static int lastTarget = -1;
        public static bool isCov = false;
        static bool Prefix(ref TosAbilityPanelListItem __instance)
        {
            if (!ModSettings.GetBool("Old Necromancer")) return true;
            if (Service.Game.Sim.simulation.myIdentity.Data.role != Role.NECROMANCER) return true;
            if (__instance.playerRole == Role.NECROMANCER &&Service.Game.Sim.info.roleCardObservation.Data.powerUp==POWER_UP_TYPE.NECRONOMICON)
            {
                __instance.PlaySound("Audio/UI/ClickSound.wav");
                if (!__instance.choice1Button.selected)
                {
                    AddGhoulButton.isUsingGhoul = false;
                    ensureButtonsNecromancer.isPressed = true;
                    MenuChoiceMessage message = new MenuChoiceMessage();
                    message.choiceType = MenuChoiceType.SpecialAbility; ;
                    message.choiceMode = MenuChoiceMode.Cancel;
                    message.targetIndex = __instance.characterPosition;
                    Service.Game.Network.Send((GameMessage) message);
                    ensureButtonsNecromancer.setTrue();
                    return false;
                }else if (lastTarget!=-1)
                {
                    AddGhoulButton.isUsingGhoul = true;
                    ensureButtonsNecromancer.isPressed = true;
                    changeFilterAuto.flag = true;
                    MenuChoiceMessage message = new MenuChoiceMessage();
                    message.choiceType = MenuChoiceType.SpecialAbility;
                    message.choiceMode = MenuChoiceMode.TargetPosition;
                    message.targetIndex = lastTarget;
                    if (isCov)
                    {
                        lastTarget = -1;
                        isCov = false;
                        changeFilterAuto.flag = true;
                        return false;
                    }
                    message.targetIndex = __instance.characterPosition;
                    Service.Game.Network.Send((GameMessage) message);
                }
                else
                {
                    AddGhoulButton.isUsingGhoul = true;
                    changeFilterAuto.flag = true;
                    ensureButtonsNecromancer.isPressed = true;
                    return false;
                }
            }
            AddGhoulButton.isUsingGhoul = false;
            ensureButtonsNecromancer.isPressed = __instance.choice1Button.selected;
            changeFilterAuto.flag = true;
            return true;
            }
        }
    
    [HarmonyPatch(typeof(TosAbilityPanelListItem), "OnClickChoice2")]
    public class CleanupNecromancer2
    {
        
        static bool Prefix(ref TosAbilityPanelListItem __instance)
        {
            if (!ModSettings.GetBool("Old Necromancer")) return true;
            if (Service.Game.Sim.simulation.myIdentity.Data.role != Role.NECROMANCER || Service.Game.Sim.info.roleCardObservation.Data.powerUp!=POWER_UP_TYPE.NECRONOMICON) return true;
            if (__instance.playerRole.IsCovenAligned())
            {
                CleanupNecromancer.isCov = true;
            }
            else
            {
                CleanupNecromancer.isCov = false;
            }
            if (AddGhoulButton.isUsingGhoul)
            {
                __instance.PlaySound("Audio/UI/ClickSound.wav");
                MenuChoiceMessage message = new MenuChoiceMessage();
                message.choiceType = MenuChoiceType.SpecialAbility;
                message.choiceMode = MenuChoiceMode.TargetPosition;
                if (!__instance.choice2Button.selected)
                {
                    CleanupNecromancer.isCov = false;
                    message.choiceMode = MenuChoiceMode.Cancel;
                }
                message.targetIndex = __instance.characterPosition;
                Service.Game.Network.Send((GameMessage) message);
                return false;
            }

            return true;
        }
    }
}