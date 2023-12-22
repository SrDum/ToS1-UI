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
    [HarmonyPatch]
    public class OldPlayerPosition
    {

        [HarmonyPatch(typeof(TosAbilityPanel), nameof(TosAbilityPanel.FilterList))]
        [HarmonyPrefix]
        public static bool phaseChange(ref TosAbilityPanel __instance)
        {
            if (!ModSettings.GetBool("Old Player List")) return true;
            if (__instance.selectedFilter == TosAbilityPanel.FilterType.SHOW_LIVING)
            {
                foreach (TosAbilityPanelListItem playerListPlayer in __instance.playerListPlayers)
                {
                    TosAbilityPanelListItem player = playerListPlayer;
                    if (Service.Game.Sim.info.discussionPlayers.Find((Predicate<DiscussionPlayerObservation>) (listItem => listItem.Data.position == player.characterPosition)).Data.alive)
                        player.gameObject.transform.GetChild(1).gameObject.SetActive(true);
                    else player.gameObject.transform.GetChild(1).gameObject.SetActive(false);
                }

                return false;
            }
            else
            {
                foreach (TosAbilityPanelListItem playerListPlayer in __instance.playerListPlayers)
                {
                    TosAbilityPanelListItem player = playerListPlayer;
                    player.gameObject.transform.GetChild(1).gameObject.SetActive(true);
                }

                return true;
            }
        }
    }
}