using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Properties;
using Fisobs.Sandbox;

namespace CentiShields;

sealed class CentiShieldFisob : Fisob
{
    public static readonly AbstractPhysicalObject.AbstractObjectType CentiShield = new("CentiShield", true);
    public static readonly MultiplayerUnlocks.SandboxUnlockID RedCentiShield = new("RedCentiShield", true);
    public static readonly MultiplayerUnlocks.SandboxUnlockID OrangeCentiShield = new("OrangeCentiShield", true);

    public CentiShieldFisob() : base(CentiShield)
    {
        // Fisobs auto-loads the `icon_CentiShield` embedded resource as a texture.
        // See `CentiShields.csproj` for how you can add embedded resources to your project.

        // If you want a simple grayscale icon, you can omit the following line.
        Icon = new CentiShieldIcon();

        SandboxPerformanceCost = new(linear: 0.35f, exponential: 0f);

        RegisterUnlock(OrangeCentiShield, parent: MultiplayerUnlocks.SandboxUnlockID.BigCentipede, data: 70);
        RegisterUnlock(RedCentiShield, parent: MultiplayerUnlocks.SandboxUnlockID.RedCentipede, data: 0);
    }

    public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData, SandboxUnlock? unlock)
    {
        // Centi shield data is just floats separated by ; characters.
        string[] p = saveData.CustomData.Split(';');

        if (p.Length < 5) {
            p = new string[5];
        }

        var result = new CentiShieldAbstract(world, saveData.Pos, saveData.ID) {
            hue = float.TryParse(p[0], out var h) ? h : 0,
            saturation = float.TryParse(p[1], out var s) ? s : 1,
            scaleX = float.TryParse(p[2], out var x) ? x : 1,
            scaleY = float.TryParse(p[3], out var y) ? y : 1,
            damage = float.TryParse(p[4], out var r) ? r : 0
        };

        // If this is coming from a sandbox unlock, the hue and size should depend on the data value (see CentiShieldIcon below).
        if (unlock is SandboxUnlock u) {
            result.hue = u.Data / 1000f;

            if (u.Data == 0) {
                result.scaleX += 0.2f;
                result.scaleY += 0.2f;
            }
        }

        return result;
    }

    private static readonly CentiShieldProperties properties = new();

    public override ItemProperties Properties(PhysicalObject forObject)
    {
        // If you need to use the forObject parameter, pass it to your ItemProperties class's constructor.
        // The Mosquitoes example demonstrates this.
        return properties;
    }
}
