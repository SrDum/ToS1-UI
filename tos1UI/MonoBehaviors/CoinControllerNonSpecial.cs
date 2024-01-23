using System;
using BMG.UI;
using Game.Interface;
using JetBrains.Annotations;
using Server.Shared.Messages;
using Server.Shared.State;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace tos1UI.MonoBehaviors
{
    public class CoinControllerNonSpecial: MonoBehaviour
    {

        private Animation _animation;
        private AnimationClip _enable;
        private AnimationClip _disable;
        private Text counter;
        private GameObject counterFrame;
        private Image ability;
        [CanBeNull] public TosAbilityPanelListItem ListItem;
        public void Awake()
        {
            _animation = gameObject.GetComponent<Animation>();
            _enable = _animation.GetClip("FlipLeft");
            _disable = _animation.GetClip("FlipRight");
            _animation.clip = _disable;
            counter = gameObject.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<Text>();
            counterFrame = gameObject.transform.GetChild(1).GetChild(1).gameObject;
            ability = gameObject.transform.GetChild(1).GetChild(0).GetComponent<Image>();
            ability.sprite = UIcontroller.normalIcon;
            gameObject.transform.GetChild(1).GetChild(2).GetComponent<Button>().onClick.AddListener(OnClick);
        }

        public void Update()
        {
            if (!UIcontroller.normalUnlocked && _animation.clip == _enable) 
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
            if (UIcontroller.normalIcon != ability.sprite) ability.sprite = UIcontroller.normalIcon;
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
            if(ListItem==null) return;
            var button = ListItem.choice1Button;
            if (button.selected)
            {
                button.Deselect();
            }
            else
            {
                button.Select();
            }
            button.onClick.Invoke();
            
        }

        public void OnDestroy()
        {
            UIcontroller.hasSpawned = false;
        }

        public bool isEnabled => _animation.clip == _enable;
    }
}