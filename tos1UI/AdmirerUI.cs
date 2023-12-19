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
    public class AdmirerUI
    {
        static void Postfix(PlayerIdentityData playerIdentityData, ref RoleCardPanel __instance)
        {
            if (Service.Game.Sim.simulation.myIdentity.Data.role==Role.ADMIRER)
            {
                if (!ModSettings.GetBool("Old Admirer")) return;
                ensureButtonsAdmirer.setTrue();
                if (ModSettings.GetBool("Safe Mode")) return;
                __instance.specialAbilityPanel.Hide();
            }
        }
    }
    
    [HarmonyPatch(typeof(RoleCardPanel), "ValidateSpecialAbilityPanel")]
    public class AdmirerUI2
    {
        static void Postfix(ref RoleCardPanel __instance)
        {
            if (Service.Game.Sim.simulation.myIdentity.Data.role==Role.ADMIRER&&!ModSettings.GetBool("Safe Mode"))
            {
                if (!ModSettings.GetBool("Old Admirer")) return;
                ensureButtonsAdmirer.setTrue();
                if (ModSettings.GetBool("Safe Mode")) return;
                __instance.specialAbilityPanel.Hide();
            }
        }
    }
    [HarmonyPatch(typeof(TosAbilityPanelListItem),"Update")]
    public class ensureButtonsAdmirer
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
            bool canPropose = true;
            int lastTarget = AddProposeButton.lastTarget;
            PlayPhase phase = Service.Game.Sim.info.gameInfo.Data.playPhase;
            if (phase == PlayPhase.FIRST_DAY || phase==PlayPhase.FIRST_DISCUSSION)
            {
                lastTarget = -1;
            }
            if (Service.Game.Sim.info.roleCardObservation.Data.specialAbilityRemaining == 0 ||
                !Service.Game.Sim.info.myDiscussionPlayer.Data.alive ||
                Service.Game.Sim.info.menuChoiceObservations[MenuChoiceType.SpecialAbility].Data.choices.Count ==0)
            {
                canPropose = false;
            }
            if (__instance.playerRole !=Role.ADMIRER && phase !=PlayPhase.NIGHT && phase !=PlayPhase.NIGHT_END_CINEMATIC &&
                phase !=PlayPhase.NIGHT_WRAP_UP && phase!=PlayPhase.WHO_DIED_AND_HOW&& phase!=PlayPhase.POST_TRIAL_WHO_DIED_AND_HOW 
                && phase != PlayPhase.DAY&&phase!=PlayPhase.FIRST_DAY&&phase!=PlayPhase.NONE && phase != PlayPhase.FIRST_DISCUSSION
               )
            {
                if (!ModStates.IsLoaded("alchlcsystm.recolors"))
                { 
                    __instance.choice2Sprite.sprite = LoadEmbeddedResources.LoadSprite("tos1UI.resources.propose.png");
                }
                if (!ModSettings.GetBool("Icon Recolors Compatibility"))
                {
                    __instance.choice2Sprite.sprite = LoadEmbeddedResources.LoadSprite("tos1UI.resources.propose.png");
                }
                __instance.choice2Text.text = "Propose";
                if (!canPropose)
                {
                    __instance.choice2ButtonCanvasGroup.DisableRenderingAndInteraction();
                    __instance.choice2Button.gameObject.SetActive(false);
                }
                else
                {
                    __instance.choice2ButtonCanvasGroup.EnableRenderingAndInteraction();
                    if (__instance.characterPosition == lastTarget)
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
    public class AddProposeButton
    {
        public static bool canPropose = false;
        public static int lastTarget = -1;
        static void Postfix(PlayPhaseState playPhase, ref TosAbilityPanelListItem __instance)
        {
            if (!ModSettings.GetBool("Old Admirer")) return;
            PlayPhase phase = playPhase.playPhase;

            if (Service.Game.Sim.simulation.myIdentity.Data.role == Role.ADMIRER)
            {
                canPropose = true;
            }
            
            if (Service.Game.Sim.info.roleCardObservation.Data.specialAbilityRemaining == 0 ||
                !Service.Game.Sim.info.myDiscussionPlayer.Data.alive||
                Service.Game.Sim.info.menuChoiceObservations[MenuChoiceType.SpecialAbility].Data.choices.Count ==0)
            {
                canPropose = false;
            }
            
            if (Service.Game.Sim.simulation.myIdentity.Data.role==Role.ADMIRER && __instance.playerRole !=Role.ADMIRER && phase !=PlayPhase.NIGHT && phase !=PlayPhase.NIGHT_END_CINEMATIC &&
                 phase !=PlayPhase.NIGHT_WRAP_UP && phase!=PlayPhase.WHO_DIED_AND_HOW&& phase!=PlayPhase.POST_TRIAL_WHO_DIED_AND_HOW
                 && phase != PlayPhase.DAY&&phase!=PlayPhase.FIRST_DAY && phase != PlayPhase.FIRST_DISCUSSION
                 )
            {
                if (!ModStates.IsLoaded("alchlcsystm.recolors"))
                { 
                    __instance.choice2Sprite.sprite = LoadEmbeddedResources.LoadSprite("tos1UI.resources.propose.png");
                }
                
                if (!ModSettings.GetBool("Icon Recolors Compatibility"))
                {
                    __instance.choice2Sprite.sprite = LoadEmbeddedResources.LoadSprite("tos1UI.resources.propose.png");
                }
                
                __instance.choice2Text.text = "Propose";
                if (!canPropose)
                {
                    __instance.choice2ButtonCanvasGroup.DisableRenderingAndInteraction();
                    __instance.choice2Button.gameObject.SetActive(false);
                }
                else
                {
                    __instance.choice2ButtonCanvasGroup.EnableRenderingAndInteraction();
                    if (__instance.characterPosition == lastTarget)
                    {
                        __instance.choice2Button.Select();
                    }

                    if (!__instance.halo.activeSelf)
                    {
                        __instance.choice2Button.gameObject.SetActive(true);
                    } 
                }
            }
        }
    }

    [HarmonyPatch(typeof(TosAbilityPanelListItem), "OnClickChoice2")]
    public class CleanupAdmirer
    {
        static bool Prefix(ref TosAbilityPanelListItem __instance)
        {
            if (!ModSettings.GetBool("Old Admirer")) return true;
            if (Service.Game.Sim.simulation.myIdentity.Data.role!= Role.ADMIRER) return true;
                __instance.PlaySound("Audio/UI/ClickSound.wav");
                MenuChoiceMessage message = new MenuChoiceMessage();
                message.choiceType = MenuChoiceType.SpecialAbility;
                message.choiceMode = MenuChoiceMode.TargetPosition;
                if (!__instance.choice2Button.selected)
                {
                    message.choiceMode = MenuChoiceMode.Cancel;
                    AddProposeButton.lastTarget = -1;
                }
                AddProposeButton.lastTarget = __instance.characterPosition;
                message.targetIndex = __instance.characterPosition;
                Service.Game.Network.Send((GameMessage) message);
                return false;
        }
    }
    
    
    

}