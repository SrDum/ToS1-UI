using System;
using System.Reflection;
using Game.Interface;
using SML;
using HarmonyLib;
using Server.Shared.Info;
using Server.Shared.Messages;
using Server.Shared.State;
using tos1UI.MonoBehaviors;
using UnityEngine;
using Service = Services.Service;

namespace tos1UI
{
    [Mod.SalemMod]
    public class Main
    {

        public static AssetBundle Bundle;
        public static GameObject CoinCanvas;
        public static GameObject JailorCoinCanvas;
        
        public void Start()
        {
            Bundle = FromAssetBundle.GetAssetBundleFromResources("tos1UI.resources.tos1ui",
                Assembly.GetExecutingAssembly());
            CoinCanvas = Bundle.LoadAsset<GameObject>("Canvas");
            JailorCoinCanvas = Bundle.LoadAsset<GameObject>("Canvas");
            Console.Out.Write(("[ToS1 UI] has loaded!"));
        }
    }
}