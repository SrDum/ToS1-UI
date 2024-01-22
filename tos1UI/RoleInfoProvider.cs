using System;
using Server.Shared.State;

namespace tos1UI
{
    public static class RoleInfoProvider
    {

        public static RoleInfo getInfo(Role role)
        {
          
                if (SML.ModStates.IsLoaded("curtis.tuba.better.tos2"))
                {
                    return RoleInfoProviderBtos.getInfo(role);
                }
            return RoleInfoProviderVanilla.getInfo(role);
        }           
    
    }
}