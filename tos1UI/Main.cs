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
    [Mod.SalemMod]
    public class Main
    {
        public void Start()
        {
            Console.Out.Write(("[ToS1 UI] has loaded!"));
        }
    }
}