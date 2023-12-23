using System.Collections.Generic;
using System.Linq;
using Server.Shared.State;

namespace tos1UI
{
    public static class RoleInfoProvider
    {
        private static readonly List<Role> MenuRoles = new List<Role> { Role.JAILOR, Role.ADMIRER, Role.PIRATE };
        private static readonly List<Role> SelfTargetRoles = new List<Role> { Role.MAYOR};

        private static readonly Dictionary<Role, string> configNames = new Dictionary<Role, string>()
        {
            { Role.JAILOR, "Jailor" },{Role.ADMIRER, "Admirer"},{Role.MAYOR, "Mayor"},{Role.PIRATE, "Pirate"},
            {Role.NECROMANCER, "Necromancer"}
        };
        
        private static List<Role> modifiedRoles = new List<Role>();
        

        static RoleInfoProvider()
        {
            modifiedRoles.AddRange(MenuRoles);
            modifiedRoles.AddRange(SelfTargetRoles);
        }
        
        public static RoleInfo getInfo(Role role)
        {
            if (role == Role.NECROMANCER) return new RoleInfo(SpecialAbilityTargetType.Necromancer, false,"");
            SpecialAbilityTargetType targetType = SpecialAbilityTargetType.None;
            bool modified = modifiedRoles.Contains(role);
            string configName ="";
            if (modified) configName = configNames[role];
            if (MenuRoles.Contains(role)) targetType = SpecialAbilityTargetType.Menu;
            if (SelfTargetRoles.Contains(role)) targetType = SpecialAbilityTargetType.Self;
            return new RoleInfo(targetType, modified,configName);
        }
    }
}