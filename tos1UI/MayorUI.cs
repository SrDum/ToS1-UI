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
    public class MayorUI
    {
        static void Postfix(PlayerIdentityData playerIdentityData, ref RoleCardPanel __instance)
        {
            if (Service.Game.Sim.simulation.myIdentity.Data.role==Role.MAYOR)
            {
                if (!ModSettings.GetBool("Old Mayor")) return;
                ensureButtonsMayor.setTrue();
                if (ModSettings.GetBool("Safe Mode")) return;
                __instance.specialAbilityPanel.Hide();
            }
        }
    }
    
    [HarmonyPatch(typeof(RoleCardPanel), "ValidateSpecialAbilityPanel")]
    public class MayorUI2
    {
        static void Postfix(ref RoleCardPanel __instance)
        {
            if (Service.Game.Sim.simulation.myIdentity.Data.role==Role.MAYOR&&!ModSettings.GetBool("Safe Mode"))
            {
                if (!ModSettings.GetBool("Old Mayor")) return;
                ensureButtonsMayor.setTrue();
                if (ModSettings.GetBool("Safe Mode")) return;
                __instance.specialAbilityPanel.Hide();
            }
        }
    }

    [HarmonyPatch(typeof(TosAbilityPanelListItem), "Update")]
    public class ensureButtonsMayor
    {
        public static bool[] reload =
        {
            false, false, false, false, false, false, false, false, false, false, false, false, false, false, false,
            false
        };

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
            bool canReveal = true;
            PlayPhase phase = Service.Game.Sim.info.gameInfo.Data.playPhase;
            if (Service.Game.Sim.info.roleCardObservation.Data.specialAbilityRemaining == 0 ||
                !Service.Game.Sim.info.myDiscussionPlayer.Data.alive||
                !Service.Game.Sim.info.roleCardObservation.Data.specialAbilityAvailable)
            {
                canReveal = false;
            }

            if (__instance.playerRole == Role.MAYOR  && (phase==PlayPhase.VOTING || phase == PlayPhase.DISCUSSION))
            {
                if (!ModStates.IsLoaded("alchlcsystm.recolors"))
                {
                    __instance.choice2Sprite.sprite = LoadEmbeddedResources.LoadSprite("tos1UI.resources.reveal.png");
                }

                __instance.choice2Text.text = "Reveal";
                if (!canReveal)
                {
                    __instance.choice2ButtonCanvasGroup.DisableRenderingAndInteraction();
                    __instance.choice2Button.gameObject.SetActive(false);
                }
                else
                {
                    __instance.choice2ButtonCanvasGroup.EnableRenderingAndInteraction();

                    if (!__instance.halo.activeSelf)
                    {
                        __instance.choice2Button.gameObject.SetActive(true);
                    }
                }

            }

            reload[__instance.characterPosition] = false;
        }
    }

    [HarmonyPatch(typeof(TosAbilityPanelListItem), "HandlePlayPhaseChanged")]
    public class playPhaseChange
    {
        static void Postfix(PlayPhaseState playPhase, ref TosAbilityPanelListItem __instance)
        {
            bool canReveal = true;
            PlayPhase phase = playPhase.playPhase;
            if (Service.Game.Sim.info.roleCardObservation.Data.specialAbilityRemaining == 0 ||
                !Service.Game.Sim.info.myDiscussionPlayer.Data.alive ||
                !Service.Game.Sim.info.roleCardObservation.Data.specialAbilityAvailable)
            {
                canReveal = false;
            }

            if (__instance.playerRole == Role.MAYOR && (phase == PlayPhase.VOTING || phase == PlayPhase.DISCUSSION))
            {
                if (!ModStates.IsLoaded("alchlcsystm.recolors"))
                {
                    __instance.choice2Sprite.sprite = LoadEmbeddedResources.LoadSprite("tos1UI.resources.reveal.png");
                }

                __instance.choice2Text.text = "Reveal";
                if (!canReveal)
                {
                    __instance.choice2ButtonCanvasGroup.DisableRenderingAndInteraction();
                    __instance.choice2Button.gameObject.SetActive(false);
                }
                else
                {
                    __instance.choice2ButtonCanvasGroup.EnableRenderingAndInteraction();

                    if (!__instance.halo.activeSelf)
                    {
                        __instance.choice2Button.gameObject.SetActive(true);
                    }
                }

            }
        }
    }
    
    [HarmonyPatch(typeof(TosAbilityPanelListItem), "OnClickChoice2")]
    public class CleanupMayor
    {
        static bool Prefix(ref TosAbilityPanelListItem __instance)
        {
            if (!ModSettings.GetBool("Old Mayor")) return true;
            if (Service.Game.Sim.simulation.myIdentity.Data.role!= Role.MAYOR || __instance.playerRole!=Role.MAYOR) return true;
            __instance.PlaySound("Audio/UI/ClickSound.wav");
            MenuChoiceMessage message = new MenuChoiceMessage();
            message.choiceType = MenuChoiceType.SpecialAbility;
            message.choiceMode = MenuChoiceMode.None;
            Service.Game.Network.Send((GameMessage) message);
            ensureButtonsMayor.setTrue();
            return false;
        }
    }
}