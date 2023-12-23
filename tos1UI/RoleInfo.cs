

using System.ComponentModel;

//I have to do this because otherwise I get a very annoying error while using record.
//See this link for exactly what Is going on: https://www.mking.net/blog/error-cs0518-isexternalinit-not-defined
namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit { }
}

namespace tos1UI
{
    
    
    public record RoleInfo(SpecialAbilityTargetType AbilityTargetType, bool isModified, string configName, bool track);
}