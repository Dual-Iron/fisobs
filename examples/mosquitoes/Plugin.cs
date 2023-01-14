using BepInEx;
using Fisobs.Core;
using System.Security.Permissions;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace Mosquitoes
{
    [BepInPlugin("org.dual.mosquitoes", nameof(Mosquitoes), "1.0.0")]
    sealed class Plugin : BaseUnityPlugin
    {
        public void OnEnable()
        {
            // TODO remove this when int(ExtEnum<T>) is fixed
            // Initialize MSC crit types
            MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.RegisterValues();

            // Once that type is initialized, this enum can be registered.
            MosquitoCritob.Mosquito = new("Mosquito", true);

            Content.Register(new MosquitoCritob());
        }
    }
}
