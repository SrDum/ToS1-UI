using System;
using BMG.UI;
using Game.Interface;
using Server.Shared.Messages;
using Server.Shared.State;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace tos1UI.MonoBehaviors
{
    public class CoinController: MonoBehaviour
    {

        private Animation _animation;
        private AnimationClip _enable;
        private AnimationClip _disable;
        private Text counter;
        private GameObject counterFrame;
        public void Awake()
        {
            _animation = gameObject.GetComponent<Animation>();
            _enable = _animation.GetClip("FlipLeft");
            _disable = _animation.GetClip("FlipRight");
            _animation.clip = _disable;
            counter = gameObject.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<Text>();
            counterFrame = gameObject.transform.GetChild(1).GetChild(1).gameObject;
            gameObject.transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = UIcontroller.abilityIcon;
            gameObject.transform.GetChild(1).GetChild(2).GetComponent<Button>().onClick.AddListener(OnClick);
        }

        public void Update()
        {
            if (!UIcontroller.specialUnlocked && _animation.clip == _enable) 
            {
                Disable();
            }

            if (Service.Game.Sim.info.gameInfo.Data.gamePhase != GamePhase.PLAY)
            {
                Destroy(gameObject);
                UIcontroller.hasSpawned = false;
            }
            
        }

        public void Enable(int charges)
        {
            if (_animation.clip == _enable) return;
            if(charges<0) counterFrame.SetActive(false);
            else counterFrame.SetActive(true);
            counter.text = $"<b>{charges}</b>";
            _animation.clip = _enable;
            _animation.Play();
        }

        public void Disable()
        {
            _animation.clip = _disable;
            _animation.Play();
        }

        public void OnClick()
        {
            var info = RoleInfoProvider.getInfo(UIcontroller.role);
            MenuChoiceMessage message = new MenuChoiceMessage();
            message.choiceType = MenuChoiceType.SpecialAbility;
            message.choiceMode = MenuChoiceMode.TargetPosition;
            UIcontroller.ownListItem.PlaySound("Audio/UI/ClickSound.wav");
            if (UIcontroller.selfButton.selected)
            {
                UIcontroller.selfButton.Deselect();
            }
            else
            {
                UIcontroller.selfButton.Select();
            }
            if (!UIcontroller.selfButton.selected)
            {
                message.choiceMode = MenuChoiceMode.Cancel;
                if (info.remember) UIcontroller.rememberPressed = false;
            }

            if (info.remember) UIcontroller.rememberPressed = true;
            Service.Game.Network.Send((GameMessage) message);
        }

        public void OnDestroy()
        {
            UIcontroller.hasSpawned = false;
        }

        public bool isEnabled => _animation.clip == _enable;
    }
}