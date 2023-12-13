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
    public class PirateUI
    {
        static void Postfix(PlayerIdentityData playerIdentityData, ref RoleCardPanel __instance)
        {
            if (Service.Game.Sim.simulation.myIdentity.Data.role == Role.PIRATE)
            {
                if (!ModSettings.GetBool("Old Pirate")) return;
                ensureButtonsPirate.setTrue();
                if (ModSettings.GetBool("Safe Mode")) return;
                __instance.specialAbilityPanel.Hide();
            }
        }
    }

    [HarmonyPatch(typeof(RoleCardPanel), "ValidateSpecialAbilityPanel")]
    public class PirateUI2
    {
        static void Postfix(ref RoleCardPanel __instance)
        {
            if (Service.Game.Sim.simulation.myIdentity.Data.role == Role.PIRATE && !ModSettings.GetBool("Safe Mode"))
            {
                if (!ModSettings.GetBool("Old Pirate")) return;
                ensureButtonsPirate.setTrue();
                if (ModSettings.GetBool("Safe Mode")) return;
                __instance.specialAbilityPanel.Hide();
            }
        }
    }

    [HarmonyPatch(typeof(TosAbilityPanelListItem), "Update")]
    public class ensureButtonsPirate
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
            bool canDuel = true;
            int lastTarget = AddDuelButton.lastTarget;
            int lastTargetFresh = AddDuelButton.lastTargetFresh;
            PlayPhase phase = Service.Game.Sim.info.gameInfo.Data.playPhase;
            if (Service.Game.Sim.info.roleCardObservation.Data.specialAbilityRemaining == 0 ||
                !Service.Game.Sim.info.myDiscussionPlayer.Data.alive)
            {
                canDuel = false;
            }

            if (canDuel && __instance.playerRole != Role.PIRATE&& phase != PlayPhase.NIGHT &&
                phase != PlayPhase.NIGHT_END_CINEMATIC &&
                phase != PlayPhase.NIGHT_WRAP_UP && phase != PlayPhase.WHO_DIED_AND_HOW &&
                phase != PlayPhase.POST_TRIAL_WHO_DIED_AND_HOW
                && __instance.characterPosition != lastTarget && phase != PlayPhase.DAY &&
                phase != PlayPhase.FIRST_DAY && phase != PlayPhase.NONE
               )
            {
                if (!ModStates.IsLoaded("alchlcsystm.recolors"))
                {
                    __instance.choice2Sprite.sprite = LoadEmbeddedResources.LoadSprite("tos1UI.resources.duel.png");
                }

                __instance.choice2Text.text = "Duel";
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

            reload[__instance.characterPosition] = false;
        }

    }

    [HarmonyPatch(typeof(TosAbilityPanelListItem), "HandlePlayPhaseChanged")]
    public class AddDuelButton
    {
        public static bool canDuel = false;
        public static int lastTarget = -1;
        public static int lastTargetFresh = -1;

        static void Postfix(PlayPhaseState playPhase, ref TosAbilityPanelListItem __instance)
        {
            if (!ModSettings.GetBool("Old Pirate")) return;
            PlayPhase phase = playPhase.playPhase;
            if (phase == PlayPhase.FIRST_DAY || phase == PlayPhase.NONE)
            {
                lastTarget = -1;
                lastTargetFresh = -1;
            }

            if (Service.Game.Sim.simulation.myIdentity.Data.role == Role.PIRATE)
            {
                canDuel = true;
            }

            if (Service.Game.Sim.info.roleCardObservation.Data.specialAbilityRemaining == 0 ||
                !Service.Game.Sim.info.myDiscussionPlayer.Data.alive)
            {
                canDuel = false;
            }

            if (canDuel && Service.Game.Sim.simulation.myIdentity.Data.role == Role.PIRATE &&
                __instance.playerRole != Role.PIRATE && phase != PlayPhase.NIGHT &&
                phase != PlayPhase.NIGHT_END_CINEMATIC &&
                phase != PlayPhase.NIGHT_WRAP_UP && phase != PlayPhase.WHO_DIED_AND_HOW &&
                phase != PlayPhase.POST_TRIAL_WHO_DIED_AND_HOW
                && __instance.characterPosition != lastTarget && phase != PlayPhase.DAY && phase != PlayPhase.FIRST_DAY
               )
            {
                if (!ModStates.IsLoaded("alchlcsystm.recolors"))
                {
                    __instance.choice2Sprite.sprite = LoadEmbeddedResources.LoadSprite("tos1UI.resources.duel.png");
                }

                __instance.choice2Text.text = "Duel";
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

            if (phase == PlayPhase.NIGHT)
            {
                lastTarget = lastTargetFresh;
            }
        }
    }

    [HarmonyPatch(typeof(TosAbilityPanelListItem), "OnClickChoice2")]
    public class CleanupPirate
    {
        static bool Prefix(ref TosAbilityPanelListItem __instance)
        {
            if (!ModSettings.GetBool("Old Pirate")) return true;
            if (Service.Game.Sim.simulation.myIdentity.Data.role != Role.PIRATE) return true;
            __instance.PlaySound("Audio/UI/ClickSound.wav");
            MenuChoiceMessage message = new MenuChoiceMessage();
            message.choiceType = MenuChoiceType.SpecialAbility;
            message.choiceMode = MenuChoiceMode.TargetPosition;
            if (!__instance.choice2Button.selected)
            {
                message.choiceMode = MenuChoiceMode.Cancel;
                AddDuelButton.lastTargetFresh = -1;
            }

            AddDuelButton.lastTargetFresh = __instance.characterPosition;
            message.targetIndex = __instance.characterPosition;
            Service.Game.Network.Send((GameMessage)message);
            return false;
        }
    }



}