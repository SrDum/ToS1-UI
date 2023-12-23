using Mentions.UI;
using Home.Common;
using Server.Shared.Messages;
using Server.Shared.State.Chat;
using Services;
using UnityEngine;

namespace tos1UI.borrowedCode
{
    //Borrowed and (barely) modified from https://github.com/JustAnotherNoob3/MovableWills/blob/main/Utils/ChatUtils.cs
    static public class ChatUtils{
        static public void AddMessage(string message, string style = "good_alert", bool playSound = false, bool stayInChatlogs = true, bool showInChat = true){
            //Always makes the same pre-message. Styles maintain all message.
            MentionPanel mp = (MentionPanel)Object.FindObjectOfType(typeof(MentionPanel));
            ChatLogCustomTextEntry chatLogCustomTextEntry = new(mp.mentionsProvider.DecodeText(message), style)
            {
                showInChatLog = stayInChatlogs,
                showInChat = showInChat
            };
            ChatLogMessage chatLogMessage = new(chatLogCustomTextEntry);
            Service.Game.Sim.simulation.HandleChatLog(chatLogMessage);
            if(playSound)Object.FindObjectOfType<UIController>().PlaySound("Audio/UI/Error", false);
            
        }
    }
}