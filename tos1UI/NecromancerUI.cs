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
                && !AddGhoulButton.isUsingGhoul && __instance.playerRole != Role.NECROMANCER)
            {
                if (!__instance.halo.activeSelf)
                {
                    if(__instance.choice2Button.selected) __instance.choice2Button.Deselect();
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
            if (!ModSettings.GetBool("Old Necromancer")) return;
            PlayPhase phase = playPhase.playPhase;
            if (phase != PlayPhase.NIGHT)
            {
                isUsingGhoul = false;
                changeFilterAuto.flag = false;
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
                __result = true;
                return false;
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
        
        static bool Prefix(ref TosAbilityPanelListItem __instance)
        {
            if (!ModSettings.GetBool("Old Necromancer")) return true;
            if (Service.Game.Sim.simulation.myIdentity.Data.role != Role.NECROMANCER) return true;
            if (__instance.playerRole == Role.NECROMANCER)
            {
                __instance.PlaySound("Audio/UI/ClickSound.wav");
                if (!__instance.choice1Button.selected)
                {
                    AddGhoulButton.isUsingGhoul = false;
                    MenuChoiceMessage message = new MenuChoiceMessage();
                    message.choiceType = MenuChoiceType.SpecialAbility; ;
                    message.choiceMode = MenuChoiceMode.Cancel;
                    message.targetIndex = __instance.characterPosition;
                    Service.Game.Network.Send((GameMessage) message);
                    ensureButtonsNecromancer.setTrue();
                    return false;
                }
                else
                {
                    AddGhoulButton.isUsingGhoul = true;
                    changeFilterAuto.flag = true;
                    
                    return false;
                }
            }
            AddGhoulButton.isUsingGhoul = false;
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
            if (Service.Game.Sim.simulation.myIdentity.Data.role != Role.NECROMANCER) return true;
            if (AddGhoulButton.isUsingGhoul)
            {
                __instance.PlaySound("Audio/UI/ClickSound.wav");
                MenuChoiceMessage message = new MenuChoiceMessage();
                message.choiceType = MenuChoiceType.SpecialAbility;
                message.choiceMode = MenuChoiceMode.TargetPosition;
                if (!__instance.choice2Button.selected)
                    message.choiceMode = MenuChoiceMode.Cancel;
                message.targetIndex = __instance.characterPosition;
                Service.Game.Network.Send((GameMessage) message);
                return false;
            }

            return true;
        }
    }
}