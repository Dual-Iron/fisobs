using BepInEx;
using Fisobs.Core;

namespace Mosquitoes
{
    // ⚠ It's important that you add this BepInDependency attribute:

    [BepInDependency("github.notfood.BepInExPartialityWrapper", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin("org.dual.mosquitoes", nameof(Mosquitoes), "0.1.0")]
    sealed class Plugin : BaseUnityPlugin
    {
        public void OnEnable()
        {
            Content.Register(new MosquitoCritob());
        }
    }
}
