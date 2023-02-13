using BepInEx;
using Fisobs.Core;
using System.Security.Permissions;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace Mosquitoes;

[BepInPlugin("org.dual.mosquitoes", nameof(Mosquitoes), "1.0.1")]
sealed class Plugin : BaseUnityPlugin
{
    public void OnEnable()
    {
        Content.Register(new MosquitoCritob());
    }
}
