using System;
using Game.Interface;
using SML;
using HarmonyLib;
using Server.Shared.Info;
using Server.Shared.Messages;
using Server.Shared.State;
using Service = Services.Service;


namespace tos1UI
{
    
    [HarmonyPatch(typeof(RoleCardPanel), "HandleOnMyIdentityChanged")]
    public class JailorUI
    {
        static void Postfix(PlayerIdentityData playerIdentityData, ref RoleCardPanel __instance)
        {
            if (Service.Game.Sim.simulation.myIdentity.Data.role==Role.JAILOR)
            {
                if (!ModSettings.GetBool("Old Jailor")) return;
                ensureButtonsJailor.setTrue();
                if (ModSettings.GetBool("Safe Mode")) return;
                __instance.specialAbilityPanel.Hide();
            }
        }
    }
    
    [HarmonyPatch(typeof(RoleCardPanel), "ValidateSpecialAbilityPanel")]
    public class JailorUI2
    {
        static void Postfix(ref RoleCardPanel __instance)
        {
            if (Service.Game.Sim.simulation.myIdentity.Data.role==Role.JAILOR&&!ModSettings.GetBool("Safe Mode"))
            {
                if (!ModSettings.GetBool("Old Jailor")) return;
                ensureButtonsJailor.setTrue();
                if (ModSettings.GetBool("Safe Mode")) return;
                __instance.specialAbilityPanel.Hide();
            }
        }
    }
    [HarmonyPatch(typeof(TosAbilityPanelListItem),"Update")]
    public class ensureButtonsJailor
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
            bool canJail = true;
            int lastTarget = AddJailButton.lastTarget;
            int lastTargetFresh = AddJailButton.lastTargetFresh;
            PlayPhase phase = Service.Game.Sim.info.gameInfo.Data.playPhase;
            if (Service.Game.Sim.info.roleCardObservation.Data.specialAbilityRemaining == 0 ||
                !Service.Game.Sim.info.myDiscussionPlayer.Data.alive ||
                Service.Game.Sim.info.menuChoiceObservations[MenuChoiceType.SpecialAbility].Data.choices.Count ==0)
            {
                canJail = false;
            }
            if (__instance.playerRole !=Role.JAILOR && phase !=PlayPhase.NIGHT && phase !=PlayPhase.NIGHT_END_CINEMATIC &&
                phase !=PlayPhase.NIGHT_WRAP_UP && phase!=PlayPhase.WHO_DIED_AND_HOW&& phase!=PlayPhase.POST_TRIAL_WHO_DIED_AND_HOW
                &&__instance.characterPosition!=lastTarget&& phase != PlayPhase.DAY&&phase!=PlayPhase.FIRST_DAY&&phase!=PlayPhase.NONE
               )
            {
                if (!ModStates.IsLoaded("alchlcsystm.recolors"))
                { 
                    __instance.choice2Sprite.sprite = LoadEmbeddedResources.LoadSprite("tos1UI.resources.jail.png");
                }
                __instance.choice2Text.text = "Jail";
                if (!canJail)
                {
                    __instance.choice2ButtonCanvasGroup.DisableRenderingAndInteraction();
                    __instance.choice2Button.gameObject.SetActive(false);
                }
                else
                {
                    __instance.choice2ButtonCanvasGroup.EnableRenderingAndInteraction();
                    if (__instance.characterPosition == lastTargetFresh)
                    {
                        __instance.choice2Button.Select();
                    }

                    if (!__instance.halo.activeSelf)
                    {
                        __instance.choice2Button.gameObject.SetActive(true);
                    } 
                }
                
            }

            reload[__instance.characterPosition] = false;
        }

    }
    
    [HarmonyPatch(typeof(TosAbilityPanelListItem),"HandlePlayPhaseChanged")]
    public class AddJailButton
    {
        public static bool canJail = false;
        public static int lastTarget = -1;
        public static int lastTargetFresh = -1;
        static void Postfix(PlayPhaseState playPhase, ref TosAbilityPanelListItem __instance)
        {
            if (!ModSettings.GetBool("Old Jailor")) return;
            PlayPhase phase = playPhase.playPhase;
            if (phase == PlayPhase.FIRST_DAY || phase == PlayPhase.NONE)
            {
                lastTarget = -1;
                lastTargetFresh = -1;
            }

            if (Service.Game.Sim.simulation.myIdentity.Data.role == Role.JAILOR)
            {
                canJail = true;
            }
            
            if (Service.Game.Sim.info.roleCardObservation.Data.specialAbilityRemaining == 0 ||
                !Service.Game.Sim.info.myDiscussionPlayer.Data.alive)
            {
                canJail = false;
            }
            
            if (Service.Game.Sim.simulation.myIdentity.Data.role==Role.JAILOR && __instance.playerRole !=Role.JAILOR && phase !=PlayPhase.NIGHT && phase !=PlayPhase.NIGHT_END_CINEMATIC &&
                 phase !=PlayPhase.NIGHT_WRAP_UP && phase!=PlayPhase.WHO_DIED_AND_HOW&& phase!=PlayPhase.POST_TRIAL_WHO_DIED_AND_HOW
                 &&__instance.characterPosition!=lastTarget&& phase != PlayPhase.DAY&&phase!=PlayPhase.FIRST_DAY
                 )
            {
                if (!ModStates.IsLoaded("alchlcsystm.recolors"))
                { 
                    __instance.choice2Sprite.sprite = LoadEmbeddedResources.LoadSprite("tos1UI.resources.jail.png");
                }
                __instance.choice2Text.text = "Jail";
                if (!canJail)
                {
                    __instance.choice2ButtonCanvasGroup.DisableRenderingAndInteraction();
                    __instance.choice2Button.gameObject.SetActive(false);
                }
                else
                {
                    __instance.choice2ButtonCanvasGroup.EnableRenderingAndInteraction();
                    if (__instance.characterPosition == lastTargetFresh)
                    {
                        __instance.choice2Button.Select();
                    }

                    if (!__instance.halo.activeSelf)
                    {
                        __instance.choice2Button.gameObject.SetActive(true);
                    } 
                }
            }

            if (phase == PlayPhase.NIGHT)
            {
                lastTarget = lastTargetFresh;
            }
        }
    }

    [HarmonyPatch(typeof(TosAbilityPanelListItem), "OnClickChoice2")]
    public class CleanupJailor
    {
        static bool Prefix(ref TosAbilityPanelListItem __instance)
        {
            if (!ModSettings.GetBool("Old Jailor")) return true;
            if (Service.Game.Sim.simulation.myIdentity.Data.role!= Role.JAILOR) return true;
                __instance.PlaySound("Audio/UI/ClickSound.wav");
                MenuChoiceMessage message = new MenuChoiceMessage();
                message.choiceType = MenuChoiceType.SpecialAbility;
                message.choiceMode = MenuChoiceMode.TargetPosition;
                if (!__instance.choice2Button.selected)
                {
                    message.choiceMode = MenuChoiceMode.Cancel;
                    AddJailButton.lastTargetFresh = -1;
                }
                AddJailButton.lastTargetFresh = __instance.characterPosition;
                message.targetIndex = __instance.characterPosition;
                Service.Game.Network.Send((GameMessage) message);
                return false;
        }
    }
    
    
    

}