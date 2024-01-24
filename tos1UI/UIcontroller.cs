using System;
using System.Collections.Generic;
using BMG.UI;
using Game.Interface;
using Game.Services;
using SML;
using HarmonyLib;
using Server.Shared.Extensions;
using Server.Shared.Info;
using Server.Shared.Messages;
using Server.Shared.State;
using tos1UI.MonoBehaviors;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Service = Services.Service;

namespace tos1UI
{
    [HarmonyPatch]
    public static class UIcontroller
    {

        public static Role role = Role.NONE;
        public static Sprite abilityIcon;
        public static Sprite normalIcon;
        public static int lastClicked = -1;
        public static bool flag = false;
        public static bool specialUnlocked = false;
        public static bool normalUnlocked = false;
        public static string abilityName = "";
        public static int specialCharges = -69;
        public static int normalCharges;
        public static bool rememberPressed = false;
        public static bool isDay = false;
        private static PipController pips;
        private static PipController fpips;
        private static RoleCardPanel panel;
        private static RoleCardPopupPanel foo;
        private static string abilityDec;
        public static BMG_Button selfButton;
        public static TosAbilityPanelListItem ownListItem;
        public static GameObject coinCanvas;
        public static GameObject coin;
        public static bool hasSpawned = false;


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
        public static void onSpecialAbilityInfoChange(ref RoleCardPanel __instance)
        {
            flag = false;
            lastClicked = -1;
            specialUnlocked = __instance.cachedRoleData.specialAbilityAvailable; 
            rememberPressed = false;
            set(false);
            role = Service.Game.Sim.simulation.myIdentity.Data.role;
            RoleInfo info = RoleInfoProvider.getInfo(role);
            if (info.isModified  && !ModSettings.GetBool("Also Keep Vanilla Buttons")) __instance.specialAbilityPanel.Hide(); 
            if (info.isModified) abilityIcon = __instance.specialAbilityPanel.useButton.abilityIcon.sprite;
            if (info.isModified) abilityName = __instance.specialAbilityPanel.abilityText.text;
            if (info.isModified) abilityDec = __instance.specialAbilityPanel.abilityDesc;
            if (info.isModified && (info.AbilityTargetType == SpecialAbilityTargetType.Self ||
                                    info.AbilityTargetType == SpecialAbilityTargetType.SelfAndOthers 
                                    )
                 && !hasSpawned)
            {
                coinCanvas = GameObject.Instantiate(Main.CoinCanvas);
                coin = coinCanvas.transform.GetChild(0).gameObject;
                coin.AddComponent<CoinController>();
                hasSpawned = true;
            }

            if (info.isJailor && !hasSpawned)
            {
                coinCanvas = GameObject.Instantiate(Main.JailorCoinCanvas);
                coin = coinCanvas.transform.GetChild(0).gameObject;
                coin.AddComponent<CoinControllerNonSpecial>();
                hasSpawned = true;
            }
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
            RoleInfo info = RoleInfoProvider.getInfo(Service.Game.Sim.info.simulation.myIdentity.Data.role);
            if (data.specialAbilityTotal > 0 && info.isModified)
            {
                specialCharges = data.specialAbilityRemaining;
                pips.SetCurrentPips(specialCharges);
            }
            else
            {
                specialCharges = -69;
            }

            if (info.isJailor)
            {
                normalCharges = data.normalAbilityRemaining;
            }
        }

        [HarmonyPatch(typeof(TosAbilityPanel), nameof(TosAbilityPanel.HandlePlayPhaseChanged))]
        [HarmonyPostfix]
        public static void onPlayPhaseChanged(ref TosAbilityPanel __instance)
        {
            flag = true;
            RoleInfo info = RoleInfoProvider.getInfo(Service.Game.Sim.simulation.myIdentity.Data.role);
            if (info.AbilityTargetType == SpecialAbilityTargetType.DeadMenu && Service.Game.Sim.info.daytime.Data.IsDaytime() && !isDay)
            {
                isDay = true;
                __instance.OnClickFilterAll();
            }
            else
            {
                isDay = false;
            }

            if (info.isJailor)
            {
                MenuChoiceObservation observation;
                normalUnlocked = Service.Game.Sim.info.menuChoiceObservations.TryGetValue(MenuChoiceType.NightAbility,
                    out observation);
                if (observation == null)
                {
                    normalUnlocked = false;
                    return;
                }
                normalUnlocked = observation.Data.choices.Count > 0;
                if (normalUnlocked)
                {
                    var controller = coin.GetComponent<CoinControllerNonSpecial>();
                    controller.ListItem =
                        __instance.playerListPlayers.Find(item =>
                            item.characterPosition == observation.Data.choices[0]);
                    controller.Enable(normalCharges);
                }
            }
        }
        

        [HarmonyPatch(typeof(TosAbilityPanelListItem), nameof(TosAbilityPanelListItem.Update))]
        [HarmonyPostfix]
        public static void renderButtons(ref TosAbilityPanelListItem __instance)
        {
                int pos = __instance.characterPosition;
                if (!specialUnlocked || !render[pos]) return;
                RoleInfo info = RoleInfoProvider.getInfo(role);
                if (!info.isModified) return;
                if (!ModSettings.GetBool(info.configName)) return;
                if (info.AbilityTargetType == SpecialAbilityTargetType.Menu || info.AbilityTargetType == SpecialAbilityTargetType.DeadMenu)
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
                        selfButton = __instance.choice1Button;
                        ownListItem = __instance;
                        coin.GetComponent<CoinController>().Enable(specialCharges);
                    }
                }

                if (info.AbilityTargetType == SpecialAbilityTargetType.SelfAndOthers)
                {
                    if (pos == Service.Game.Sim.simulation.myIdentity.Data.position)
                    {
                        __instance.choice2Sprite.sprite = abilityIcon;
                        __instance.choice2Text.text = abilityName;
                        if(rememberPressed) __instance.choice2Button.Select();
                        __instance.choice2ButtonCanvasGroup.EnableRenderingAndInteraction();
                        __instance.choice2Button.gameObject.SetActive(true);
                        selfButton = __instance.choice2Button;
                        ownListItem = __instance;
                        coin.GetComponent<CoinController>().Enable(specialCharges);
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
            if (!specialUnlocked || !ModSettings.GetBool(info.configName)) return true;
            __instance.PlaySound("Audio/UI/ClickSound.wav");
            MenuChoiceMessage message = new MenuChoiceMessage();
            message.choiceType = MenuChoiceType.SpecialAbility;
            message.choiceMode = MenuChoiceMode.TargetPosition;
            int pos = __instance.characterPosition;
            int myPos = Service.Game.Sim.simulation.myIdentity.Data.position;
            
            if (info.AbilityTargetType == SpecialAbilityTargetType.Menu || info.AbilityTargetType == SpecialAbilityTargetType.DeadMenu)
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

            if (info.AbilityTargetType == SpecialAbilityTargetType.SelfAndOthers)
            {
                if (__instance.characterPosition == myPos)
                {
                    __instance.PlaySound("Audio/UI/ClickSound.wav");
                    if (!__instance.choice2Button.selected)
                    {
                        message.choiceMode = MenuChoiceMode.Cancel;
                        if (info.remember) rememberPressed = false;
                    }

                    if (info.remember) rememberPressed = true;
                    Service.Game.Network.Send((GameMessage) message);
                    return false;

                }
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
            if (!ModSettings.GetBool(info.configName)) return true;
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
        [HarmonyPatch(typeof(RoleCardPanel),nameof(RoleCardPanel.DetermineFrameAndSlots_AbilityIcon2))]
        [HarmonyPostfix]
        public static void LoadSpecial(ref RoleCardPanel __instance)
        {
            role = Service.Game.Sim.simulation.myIdentity.Data.role;
            RoleInfo info = RoleInfoProvider.getInfo(role);
            if (!info.isModified) return;
            if (info.isJailor)
            {
                normalIcon = __instance.roleInfoButtons[0].abilityIcon.sprite;
            }
            int uses = Service.Game.Sim.info.roleCardObservation.Data.specialAbilityTotal;
            panel = __instance;
            if (uses == -1)
            {
                uses = 4;
            }
            BaseAbilityButton button = __instance.roleInfoButtonTemplates.GetElement<BaseAbilityButton>(uses);
            if ((UnityEngine.Object)button == (UnityEngine.Object)null)
            {
                Console.WriteLine($"[ToS 1 UI]: Failed to find template with {0} total uses", uses);
            }
            else
            {
                BaseAbilityButton baseAbilityButton = UnityEngine.Object.Instantiate<BaseAbilityButton>(button, button.transform.parent);
                __instance.roleInfoButtons.Add(baseAbilityButton);
                pips = baseAbilityButton.GetComponentInChildren<PipController>();
                BaseAbilityButton element2 = __instance.roleInfoButtons.GetElement<BaseAbilityButton>(__instance.infoButtonsShowing);
                if ((UnityEngine.Object) element2 == (UnityEngine.Object) null)
                {
                    Debug.LogError(string.Format("[ToS 1 UI]: Failed to find activateButton at infoButtonsShowing {0}", (object) __instance.infoButtonsShowing));
                }
                else
                {
                    element2.gameObject.SetActive(true);
                    element2.abilityIcon.sprite = abilityIcon;
                    EventTrigger.Entry entry = new EventTrigger.Entry();
                    entry.eventID = EventTriggerType.PointerEnter;
                    entry.callback.AddListener((UnityAction<BaseEventData>) (eventData => panel.OnRollOverSpecialAbility(abilityDec)));
                    EventTrigger component = element2.GetComponent<EventTrigger>();
                    if ((UnityEngine.Object) component != (UnityEngine.Object) null)
                        component.triggers.Add(entry);
                    else
                        Console.Out.WriteLine("[ToS 1 UI]: Failed to find event triggers");
                    __instance.infoButtonsShowing=__instance.infoButtonsShowing+1;
                }   
            }
        }

        [HarmonyPatch(typeof(RoleCardPopupPanel),nameof(RoleCardPopupPanel.DetermineFrameAndSlots))]
        [HarmonyPrefix]
        public static bool popupUI(ref RoleCardPopupPanel __instance, Role role, RoleCardData roleCardData)
        { 
            if (roleCardData == null) return true;
            RoleInfo info = RoleInfoProvider.getInfo(role);
            if (!info.isModified) return true;
            __instance.ShowAttackAndDefense(roleCardData);
            __instance.infoButtonsShowing = 0;
            foo = __instance;
            if (__instance.roleInfoButtons.Count > 0){
                foreach (Component roleInfoButton in __instance.roleInfoButtons) UnityEngine.Object.Destroy((UnityEngine.Object) roleInfoButton.gameObject);
            }
            __instance.roleInfoButtons.Clear();
            if ((UnityEngine.Object) __instance.myData.abilityIcon != (UnityEngine.Object) null)
            {
                int index = roleCardData.normalAbilityTotal;
                if (index == -1) index = 4;
                BaseAbilityButton baseAbilityButton = UnityEngine.Object.Instantiate<BaseAbilityButton>(__instance.roleInfoButtonTemplates[index], __instance.roleInfoButtonTemplates[index].transform.parent);
                __instance.roleInfoButtons.Add(baseAbilityButton);
                __instance.pips = baseAbilityButton.GetComponentInChildren<PipController>();
                __instance.roleInfoButtons[__instance.infoButtonsShowing].gameObject.SetActive(true);
                __instance.roleInfoButtons[__instance.infoButtonsShowing].abilityIcon.sprite = __instance.myData.abilityIcon;
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerEnter;
                entry.callback.AddListener((UnityAction<BaseEventData>) (eventData => foo.OnClickRoleAbility()));
                __instance.roleInfoButtons[__instance.infoButtonsShowing].GetComponent<EventTrigger>().triggers.Add(entry);
                ++__instance.infoButtonsShowing;
            }
            if ((UnityEngine.Object) __instance.myData.abilityIcon2 != (UnityEngine.Object) null)
            {
                __instance.roleInfoButtons.Add(UnityEngine.Object.Instantiate<BaseAbilityButton>(__instance.roleInfoButtonTemplates[0], __instance.roleInfoButtonTemplates[0].transform.parent));
                __instance.roleInfoButtons[__instance.infoButtonsShowing].gameObject.SetActive(true);
                __instance.roleInfoButtons[__instance.infoButtonsShowing].abilityIcon.sprite = __instance.myData.abilityIcon2;
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerEnter;
                entry.callback.AddListener((UnityAction<BaseEventData>) (eventData => foo.OnClickRoleAbility2()));
                __instance.roleInfoButtons[__instance.infoButtonsShowing].GetComponent<EventTrigger>().triggers.Add(entry);
                ++__instance.infoButtonsShowing;
            } 
            if (info.isModified && roleCardData.specialAbilityAvailable)
            {
                string desc = __instance.l10n(__instance.myData.specialAbilityDesc);
                int index = roleCardData.specialAbilityTotal;
                if (index == -1) index = 4;
                 BaseAbilityButton baseAbilityButton = UnityEngine.Object.Instantiate<BaseAbilityButton>(__instance.roleInfoButtonTemplates[index], __instance.roleInfoButtonTemplates[index].transform.parent);
                __instance.roleInfoButtons.Add(baseAbilityButton);
                fpips = baseAbilityButton.GetComponentInChildren<PipController>();
                __instance.roleInfoButtons[__instance.infoButtonsShowing].gameObject.SetActive(true);
                __instance.roleInfoButtons[__instance.infoButtonsShowing].abilityIcon.sprite = __instance.myData.specialAbilityIcon;
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerEnter;
                entry.callback.AddListener((UnityAction<BaseEventData>) (eventData => foo.OnRollOverSpecialAbility(desc)));
                __instance.roleInfoButtons[__instance.infoButtonsShowing].GetComponent<EventTrigger>().triggers.Add(entry);
                ++__instance.infoButtonsShowing;
            }
            if ((UnityEngine.Object) __instance.myData.attributeIcon != (UnityEngine.Object) null)
            {
                __instance.roleInfoButtons.Add(UnityEngine.Object.Instantiate<BaseAbilityButton>(__instance.roleInfoButtonTemplates[0], __instance.roleInfoButtonTemplates[0].transform.parent));
                __instance.roleInfoButtons[__instance.infoButtonsShowing].gameObject.SetActive(true);
                __instance.roleInfoButtons[__instance.infoButtonsShowing].abilityIcon.sprite = __instance.myData.attributeIcon;
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerEnter;
                entry.callback.AddListener((UnityAction<BaseEventData>) (eventData => foo.OnClickRoleAttributes()));
                __instance.roleInfoButtons[__instance.infoButtonsShowing].GetComponent<EventTrigger>().triggers.Add(entry);
                ++__instance.infoButtonsShowing;
            }
            if (role.IsCovenAligned())
            { 
                __instance.roleInfoButtons.Add(UnityEngine.Object.Instantiate<BaseAbilityButton>(__instance.roleInfoButtonTemplates[0], __instance.roleInfoButtonTemplates[0].transform.parent));
                __instance.roleInfoButtons[__instance.infoButtonsShowing].gameObject.SetActive(true);
                __instance.roleInfoButtons[__instance.infoButtonsShowing].abilityIcon.sprite = __instance.roleData.generalData.abilityIcon2;
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerEnter;
                entry.callback.AddListener((UnityAction<BaseEventData>) (eventData => foo.OnClickNecronomicon()));
                __instance.roleInfoButtons[__instance.infoButtonsShowing].GetComponent<EventTrigger>().triggers.Add(entry);
                ++__instance.infoButtonsShowing;
            }
            if (!((UnityEngine.Object) __instance.infoSlots[__instance.infoButtonsShowing] != (UnityEngine.Object) null))
                return true; 
            __instance.infoSlot.sprite = __instance.infoSlots[__instance.infoButtonsShowing].sprite;
            return false; 
        }

        [HarmonyPatch(typeof(RoleCardPopupPanel), nameof(RoleCardPopupPanel.ValidateSpecialAbilityPanel))]
        [HarmonyPostfix]
        public static void hide(ref RoleCardPopupPanel __instance)
        {
            RoleInfo info = RoleInfoProvider.getInfo(__instance.myData.role);
            if (info.isModified && !ModSettings.GetBool("Also Keep Vanilla Buttons"))
            {
                __instance.specialAbilityPanel.Hide();
            }

           
        }

        [HarmonyPatch(typeof(CinematicService), nameof(CinematicService.StartCinematic))]
        [HarmonyPostfix]
        public static void OnStartCinematic()
        {
            if (coin != null)
            {
                coinCanvas.GetComponent<Canvas>().enabled = false;
            }
        }

        [HarmonyPatch(typeof(CinematicService), nameof(CinematicService.EndCinematic))]
        [HarmonyPostfix]
        public static void OnEndCinematic()
        {
            if (coin != null)
            {
                coinCanvas.GetComponent<Canvas>().enabled = true;
            }
        }
    }
}