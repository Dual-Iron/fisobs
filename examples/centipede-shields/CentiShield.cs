using RWCustom;
using UnityEngine;

namespace CentiShields
{
    sealed class CentiShield : Weapon
    {
        private static float Rand => Random.value;

        new public float rotation;
        new public float lastRotation;
        public float rotVel;
        public float lastDarkness = -1f;
        public float darkness;

        private Color blackColor;
        private Color earthColor;

        private readonly float rotationOffset;

        public CentiShieldAbstract Abstr { get; }

        public CentiShield(CentiShieldAbstract abstr, Vector2 pos, Vector2 vel) : base(abstr, abstr.world)
        {
            Abstr = abstr;

            bodyChunks = new[] { new BodyChunk(this, 0, pos + vel, 4 * (Abstr.scaleX + Abstr.scaleY), 0.35f) { goThroughFloors = true } };
            bodyChunks[0].lastPos = bodyChunks[0].pos;
            bodyChunks[0].vel = vel;

            bodyChunkConnections = new BodyChunkConnection[0];
            airFriction = 0.999f;
            gravity = 0.9f;
            bounce = 0.6f;
            surfaceFriction = 0.45f;
            collisionLayer = 1;
            waterFriction = 0.92f;
            buoyancy = 0.75f;

            rotation = Rand * 360f;
            lastRotation = rotation;

            rotationOffset = Rand * 30 - 15;

            ResetVel(vel.magnitude);
        }

        public void HitEffect(Vector2 impactVelocity)
        {
            var num = Random.Range(3, 8);
            for (int k = 0; k < num; k++) {
                Vector2 pos = firstChunk.pos + Custom.DegToVec(Rand * 360f) * 5f * Rand;
                Vector2 vel = -impactVelocity * -0.1f + Custom.DegToVec(Rand * 360f) * Mathf.Lerp(0.2f, 0.4f, Rand) * impactVelocity.magnitude;
                room.AddObject(new Spark(pos, vel, new Color(1f, 1f, 1f), null, 10, 170));
            }

            room.AddObject(new StationaryEffect(firstChunk.pos, new Color(1f, 1f, 1f), null, StationaryEffect.EffectType.FlashingOrb));
        }

        public void AddDamage(float damage)
        {
            Abstr.damage += damage * 0.2f;

            if (Abstr.damage > 1)
                Abstr.damage = 1;
        }

        private void Shatter()
        {
            var num = Random.Range(6, 10);
            for (int k = 0; k < num; k++) {
                Vector2 pos = firstChunk.pos + Custom.RNV() * 5f * Rand;
                Vector2 vel = Custom.RNV() * 4f * (1 + Rand);
                room.AddObject(new Spark(pos, vel, new Color(1f, 1f, 1f), null, 10, 170));
            }

            float count = 2 + 4 * (Abstr.scaleX + Abstr.scaleY);

            for (int j = 0; j < count; j++) {
                Vector2 extraVel = Custom.RNV() * Random.value * (j == 0 ? 3f : 6f);

                room.AddObject(new CentipedeShell(firstChunk.pos, Custom.RNV() * Rand * 15 + extraVel, Abstr.hue, Abstr.saturation, 0.25f, 0.25f));
            }

            room.PlaySound(SoundID.Weapon_Skid, firstChunk.pos, 0.75f, 1.25f);

            AllGraspsLetGoOfThisObject(true);
            abstractPhysicalObject.LoseAllStuckObjects();
            Destroy();
        }

        public override void Update(bool eu)
        {
            if (Abstr.damage >= 1 && Random.value < 0.015f) {
                Shatter();
                return;
            }

            ChangeCollisionLayer(grabbedBy.Count == 0 ? 2 : 1);
            firstChunk.collideWithTerrain = grabbedBy.Count == 0;
            firstChunk.collideWithSlopes = grabbedBy.Count == 0;

            base.Update(eu);

            var chunk = firstChunk;

            lastRotation = rotation;
            rotation += rotVel * Vector2.Distance(chunk.lastPos, chunk.pos);

            rotation %= 360;

            if (grabbedBy.Count == 0) {
                if (firstChunk.lastPos == firstChunk.pos) {
                    rotVel *= 0.9f;
                } else if (Mathf.Abs(rotVel) <= 0.01f) {
                    ResetVel((firstChunk.lastPos - firstChunk.pos).magnitude);
                }
            } else {
                var grabberChunk = grabbedBy[0].grabber.mainBodyChunk;
                rotVel *= 0.9f;
                rotation = Mathf.Lerp(rotation, grabberChunk.Rotation.GetAngle() + rotationOffset, 0.25f);
            }

            if (!Custom.DistLess(chunk.lastPos, chunk.pos, 3f) && room.GetTile(chunk.pos).Solid && !room.GetTile(chunk.lastPos).Solid) {
                var firstSolid = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, room.GetTilePosition(chunk.lastPos), room.GetTilePosition(chunk.pos));
                if (firstSolid != null) {
                    FloatRect floatRect = Custom.RectCollision(chunk.pos, chunk.lastPos, room.TileRect(firstSolid.Value).Grow(2f));
                    chunk.pos = floatRect.GetCorner(FloatRect.CornerLabel.D);
                    bool flag = false;
                    if (floatRect.GetCorner(FloatRect.CornerLabel.B).x < 0f) {
                        chunk.vel.x = Mathf.Abs(chunk.vel.x) * 0.15f;
                        flag = true;
                    } else if (floatRect.GetCorner(FloatRect.CornerLabel.B).x > 0f) {
                        chunk.vel.x = -Mathf.Abs(chunk.vel.x) * 0.15f;
                        flag = true;
                    } else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y < 0f) {
                        chunk.vel.y = Mathf.Abs(chunk.vel.y) * 0.15f;
                        flag = true;
                    } else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y > 0f) {
                        chunk.vel.y = -Mathf.Abs(chunk.vel.y) * 0.15f;
                        flag = true;
                    }
                    if (flag) {
                        rotVel *= 0.8f;
                    }
                }
            }
        }

        public override void HitByWeapon(Weapon weapon)
        {
            base.HitByWeapon(weapon);

            if (grabbedBy.Count > 0) {
                Creature grabber = grabbedBy[0].grabber;
                Vector2 push = firstChunk.vel * firstChunk.mass / grabber.firstChunk.mass;
                grabber.firstChunk.vel += push;
            }

            firstChunk.vel = Vector2.zero;

            HitEffect(weapon.firstChunk.vel);
            AddDamage(weapon.HeavyWeapon ? 0.5f : 0.2f);
        }

        public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
        {
            base.TerrainImpact(chunk, direction, speed, firstContact);

            if (speed > 10) {
                room.PlaySound(SoundID.Spear_Fragment_Bounce, firstChunk.pos, 0.35f, 2f);
                ResetVel(speed);
            }
        }

        private void ResetVel(float speed)
        {
            rotVel = Mathf.Lerp(-1f, 1f, Rand) * Custom.LerpMap(speed, 0f, 18f, 5f, 26f);
        }

        public override void ChangeMode(Mode newMode)
        { }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[2];
            sLeaser.sprites[0] = new FSprite("CentipedeBackShell", true);
            sLeaser.sprites[1] = new FSprite("CentipedeBackShell", true);
            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
            float num = Mathf.InverseLerp(305f, 380f, timeStacker);
            pos.y -= 20f * Mathf.Pow(num, 3f);
            float num2 = Mathf.Pow(1f - num, 0.25f);
            lastDarkness = darkness;
            darkness = rCam.room.Darkness(pos);
            darkness *= 1f - 0.5f * rCam.room.LightSourceExposure(pos);

            for (int i = 0; i < 2; i++) {
                sLeaser.sprites[i].x = pos.x - camPos.x;
                sLeaser.sprites[i].y = pos.y - camPos.y;
                sLeaser.sprites[i].rotation = Mathf.Lerp(lastRotation, rotation, timeStacker);
                sLeaser.sprites[i].scaleY = num2 * Abstr.scaleY;
                sLeaser.sprites[i].scaleX = num2 * Abstr.scaleX;
            }

            sLeaser.sprites[0].color = blackColor;
            sLeaser.sprites[0].scaleY *= 1.175f - Abstr.damage * 0.2f;
            sLeaser.sprites[0].scaleX *= 1.175f - Abstr.damage * 0.2f;

            sLeaser.sprites[1].color = Color.Lerp(Custom.HSL2RGB(Abstr.hue, Abstr.saturation, 0.55f), blackColor, darkness);

            if (blink > 0 && Rand < 0.5f) {
                sLeaser.sprites[0].color = blinkColor;
            } else if (num > 0.3f) {
                for (int j = 0; j < 2; j++) {
                    sLeaser.sprites[j].color = Color.Lerp(sLeaser.sprites[j].color, earthColor, Mathf.Pow(Mathf.InverseLerp(0.3f, 1f, num), 1.6f));
                }
            }

            if (slatedForDeletetion || room != rCam.room) {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            blackColor = palette.blackColor;
            earthColor = Color.Lerp(palette.fogColor, palette.blackColor, 0.5f);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
        {
            newContainer ??= rCam.ReturnFContainer("Items");

            foreach (FSprite fsprite in sLeaser.sprites) {
                fsprite.RemoveFromContainer();
                newContainer.AddChild(fsprite);
            }
        }
    }
}