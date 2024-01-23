using System.Collections.Generic;
using System.Linq;
using BetterTOS2;
using Server.Shared.State;

namespace tos1UI
{
    public static class RoleInfoProviderBtos
    {
        private static readonly List<Role> MenuRolesBtos = new List<Role>
        {
            RolePlus.JAILOR, RolePlus.PIRATE,RolePlus.EXECUTIONER
        };
        private static readonly List<Role> SelfTargetRolesBtos = new List<Role>
        {
            RolePlus.MAYOR, RolePlus.VETERAN, RolePlus.CLERIC, RolePlus.BODYGUARD, RolePlus.JESTER, RolePlus.TRAPPER,RolePlus.ARSONIST,
            RolePlus.MARSHAL, RolePlus.JUDGE, RolePlus.STARSPAWN
        };

        private static readonly List<Role> SafeModeTrackingBtos = new List<Role>
        {
            RolePlus.CLERIC, RolePlus.VETERAN, RolePlus.BODYGUARD, RolePlus.JESTER
        };

        private static readonly List<Role> SelfAndOthersRolesBtos = new List<Role>
        {
            RolePlus.SHROUD, RolePlus.SERIAL_KILLER, RolePlus.POISONER,RolePlus.AUDITOR, RolePlus.INQUISITOR
        };

        private static readonly List<Role> DeadMenuRolesBtos = new List<Role>
        {
            RolePlus.CORONER
        };


        private static readonly Dictionary<Role, string> configNamesBtos = new Dictionary<Role, string>()
        {
            { RolePlus.JAILOR, "Jailor" },{RolePlus.ADMIRER, "Admirer"},{RolePlus.MAYOR, "Mayor"},{RolePlus.PIRATE, "Pirate"},
            {RolePlus.EXECUTIONER, "Executioner"},{RolePlus.CLERIC,"Cleric"},{RolePlus.BODYGUARD,"Bodyguard"},{RolePlus.VETERAN, "Veteran"},
            {RolePlus.JESTER, "Jester"}, {RolePlus.TRAPPER,"Trapper"},{RolePlus.ARSONIST,"Arsonist"}, {RolePlus.CORONER, "Coroner"},{RolePlus.SHROUD, "Shroud"},
            {RolePlus.SERIAL_KILLER, "Serial Killer"}, {RolePlus.POISONER, "Poisoner"}, {RolePlus.MARSHAL, "Marshal"}, {RolePlus.JUDGE, "Judge"}, {RolePlus.AUDITOR, "Auditor"},
            { RolePlus.STARSPAWN, "Starspawn"}, {RolePlus.INQUISITOR, "Inquisitor"}
        };

        private static readonly List<Role> rememberBtos = new List<Role>
        {
            RolePlus.SHROUD, RolePlus.SERIAL_KILLER
        };
        
     
        
        private static List<Role> modifiedRolesBtos = new List<Role>();
        
        static RoleInfoProviderBtos()
        {
            modifiedRolesBtos.AddRange(MenuRolesBtos); 
            modifiedRolesBtos.AddRange(SelfTargetRolesBtos); 
            modifiedRolesBtos.AddRange(DeadMenuRolesBtos);
            modifiedRolesBtos.AddRange(SelfAndOthersRolesBtos);
        }
        
        public static RoleInfo getInfo(Role role)
        {
            if (!BTOSInfo.IS_MODDED) return RoleInfoProviderVanilla.getInfo(role);
            if (role == RolePlus.NECROMANCER) return new RoleInfo(SpecialAbilityTargetType.Necromancer, false,"", false, false);
            SpecialAbilityTargetType targetType = SpecialAbilityTargetType.None;
            bool modified = modifiedRolesBtos.Contains(role);
            bool track = SafeModeTrackingBtos.Contains(role);
            bool remem = rememberBtos.Contains(role);
            string configName ="";
            if (modified) configName = configNamesBtos[role];
            if (MenuRolesBtos.Contains(role)) targetType = SpecialAbilityTargetType.Menu;
            if (SelfTargetRolesBtos.Contains(role)) targetType = SpecialAbilityTargetType.Self;
            if (DeadMenuRolesBtos.Contains(role)) targetType = SpecialAbilityTargetType.DeadMenu;
            if (SelfAndOthersRolesBtos.Contains(role)) targetType = SpecialAbilityTargetType.SelfAndOthers;
            return new RoleInfo(targetType, modified,configName, track, remem);
        }
    }
}