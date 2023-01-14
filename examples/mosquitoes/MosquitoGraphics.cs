// This code was made by ratrat (https://github.com/ratrat44) and is included in Fisobs with his permission.

using RWCustom;
using UnityEngine;

namespace Mosquitoes
{
    sealed class MosquitoGraphics : GraphicsModule
    {
        const int meshSegs = 9;

        const float squeeze = -0.1f;
        const float squirmAdd = 0;
        const float squirmWidth = 0;
        const float squirmAmp = 0;

        readonly Mosquito mos;
        readonly Vector2[,] body = new Vector2[2, 3];
        readonly float[,] squirm = new float[meshSegs, 3];
        readonly float sizeFac;

        float squirmOffset;
        float darkness;
        float lastDarkness;
        Color yellow;
        float wingFlap;
        float lastWingFlap;
        RoomPalette roomPalette;
        ChunkSoundEmitter? soundLoop;

        readonly TriangleMesh[] m = new TriangleMesh[2]; // mesh sprites 0 and 1
        readonly CustomFSprite[] w = new CustomFSprite[2]; // wing sprites 0 and 1

        public MosquitoGraphics(Mosquito mosquito) : base(mosquito, false)
        {
            mos = mosquito;

            int seed = Random.seed;
            Random.seed = mosquito.abstractCreature.ID.RandomSeed;
            sizeFac = Custom.ClampedRandomVariation(0.8f, 0.2f, 0.5f);
            body = new Vector2[2, 3];
            Random.seed = seed;
        }

        public override void Reset()
        {
            base.Reset();

            Vector2 dir = Custom.RNV();

            for (int i = 0; i < body.GetLength(0); i++) {
                body[i, 0] = mos.firstChunk.pos - dir * i;
                body[i, 1] = body[i, 0];
                body[i, 2] *= 0f;
            }
        }

        // TODO make dead mosquito body flop
        public override void Update()
        {
            base.Update();
            if (culled) {
                return;
            }

            UpdateSounds();

            lastWingFlap = wingFlap;

            if (mos.Consious && mos.grasps[0] == null) {
                wingFlap += 0.4f + Random.value * 0.05f;
            }

            for (int i = 0; i < body.GetLength(0); i++) {
                body[i, 1] = body[i, 0];
                body[i, 0] += body[i, 2];
                body[i, 2] *= mos.airFriction;
                body[i, 2].y -= mos.gravity;
                body[i, 2] += mos.bloat * -mos.needleDir * 3f;
            }

            for (int j = 0; j < body.GetLength(0); j++) {
                SharedPhysics.TerrainCollisionData terrainCollisionData = new(body[j, 0], body[j, 1], body[j, 2], (2.5f - j * 0.5f) * sizeFac, default, mos.firstChunk.goThroughFloors);
                terrainCollisionData = SharedPhysics.VerticalCollision(mos.room, terrainCollisionData);
                terrainCollisionData = SharedPhysics.HorizontalCollision(mos.room, terrainCollisionData);
                terrainCollisionData = SharedPhysics.SlopesVertically(mos.room, terrainCollisionData);
                body[j, 0] = terrainCollisionData.pos;
                body[j, 2] = terrainCollisionData.vel;

                if (terrainCollisionData.contactPoint.y < 0) {
                    body[j, 2].x *= 0.4f;
                }

                if (j == 0) {
                    Vector2 a = Custom.DirVec(body[j, 0], mos.firstChunk.pos) * (Vector2.Distance(body[j, 0], mos.firstChunk.pos) - 5f * sizeFac);
                    body[j, 0] += a;
                    body[j, 2] += a;
                } else {
                    Vector2 a = Custom.DirVec(body[j, 0], body[j - 1, 0]) * (Vector2.Distance(body[j, 0], body[j - 1, 0]) - 5f * sizeFac);
                    body[j, 0] += a * 0.5f;
                    body[j, 2] += a * 0.5f;
                    body[j - 1, 0] -= a * 0.5f;
                    body[j - 1, 2] -= a * 0.5f;
                }
            }

            float d = Mathf.Pow(Mathf.InverseLerp(0.25f, -0.75f, Vector2.Dot((mos.firstChunk.pos - body[0, 0]).normalized, (body[0, 0] - body[1, 0]).normalized)), 2f);
            body[1, 2] -= Custom.DirVec(body[1, 0], mos.firstChunk.pos) * d * 3f * sizeFac;
            body[1, 0] -= Custom.DirVec(body[1, 0], mos.firstChunk.pos) * d * 3f * sizeFac;

            mos.needleDir = (mos.needleDir + Custom.DirVec(body[0, 0], mos.firstChunk.pos) * 0.2f).normalized;

            squirmOffset += squirmAdd * 0.2f;

            for (int k = 0; k < squirm.GetLength(0); k++) {
                squirm[k, 1] = squirm[k, 0];
                squirm[k, 0] = Mathf.Sin(squirmOffset + k * Mathf.Lerp(0.5f, 2f, squirmWidth)) * squirmAmp * (1f - mos.bloat);
            }
        }

        private void UpdateSounds()
        {
            if (soundLoop == null && mos.Consious) {
                soundLoop = mos.room.PlaySound(SoundID.Cicada_Wings_LOOP, mos.firstChunk, true, 1f, 1f);
                soundLoop.requireActiveUpkeep = true;
            } else if (soundLoop != null && (!mos.Consious || mos.room != soundLoop.room || soundLoop.slatedForDeletetion)) {
                soundLoop.alive = false;
                soundLoop.Destroy();
                soundLoop = null;
            }

            if (soundLoop != null) {
                soundLoop.alive = true;
                soundLoop.volume = Custom.LerpMap(Mathf.Sin(wingFlap * 0.5f), -1, 1, 0.75f, 0.8f);
                soundLoop.pitch = Custom.LerpMap(Mathf.Cos(wingFlap * 1.5f), -1, 1, 1.65f, 1.85f);
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[4];
            sLeaser.sprites[0] = m[0] = TriangleMesh.MakeLongMesh(meshSegs, false, true);
            sLeaser.sprites[1] = m[1] = TriangleMesh.MakeLongMesh(meshSegs - 3, false, true);
            for (int i = 0; i < 2; i++) {
                sLeaser.sprites[2 + i] = w[i] = new CustomFSprite("CentipedeWing") {
                    shader = rCam.room.game.rainWorld.Shaders["CicadaWing"]
                };
            }

            AddToContainer(sLeaser, rCam, null);

            base.InitiateSprites(sLeaser, rCam);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
        {
            newContainer ??= rCam.ReturnFContainer("Midground");

            for (int i = 0; i < sLeaser.sprites.Length; i++) {
                newContainer.AddChild(sLeaser.sprites[i]);
            }
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            if (culled) {
                return;
            }

            Vector2 chk0Pos = Vector2.Lerp(mos.firstChunk.lastPos, mos.firstChunk.pos, timeStacker);
            Vector2 bodyPos = Vector2.Lerp(body[0, 1], body[0, 0], timeStacker);
            Vector2 headPos = Vector2.Lerp(body[1, 1], body[1, 0], timeStacker);
            Vector2 segmentDir = -Vector3.Slerp(mos.lastNeedleDir, mos.needleDir, timeStacker);
            Vector2 chkDir = Custom.DirVec(chk0Pos, bodyPos);
            Vector2 bodyDir = Custom.DirVec(bodyPos, headPos);

            if (mos.room != null) {
                lastDarkness = darkness;
                darkness = mos.room.DarknessOfPoint(rCam, bodyPos);
                if (darkness != lastDarkness) {
                    ApplyPalette(sLeaser, rCam, rCam.currentPalette);
                }
            }

            chk0Pos -= segmentDir * 7f * sizeFac;
            headPos += chkDir * (7f * (1f - mos.bloat)) * sizeFac;
            Vector2 vector4 = chk0Pos - segmentDir * 18f;
            Vector2 v = vector4;
            float num = 0f;
            float num2 = Custom.LerpMap(Vector2.Distance(chk0Pos, bodyPos) + Vector2.Distance(bodyPos, headPos), 20f, 140f, 1f, 0.3f, 2f);
            Vector2 a4 = Custom.DegToVec(-45f);

            for (int i = 0; i < meshSegs; i++) {
                float iN = Mathf.InverseLerp(1f, meshSegs - 1, i); // i, normalized
                float num5 = i < 2 ? (0.5f + i) : (Custom.LerpMap(iN, 0.5f, 1f, Mathf.Lerp(3f, 2.5f, iN), 1f, 3f) * num2);
                if (mos.bloat > 0f && i > 1) {
                    num5 = Mathf.Lerp(num5 * (1.2f + 0.65f * Mathf.Sin(Mathf.PI * iN) * mos.bloat * 2f), 1f, (0.5f + 0.5f * squeeze) * Mathf.InverseLerp(1f - squeeze - 0.1f, 1f - squeeze + 0.1f, iN));
                }
                num5 *= sizeFac;

                Vector2 vector5;
                if (i == 0) {
                    vector5 = chk0Pos - segmentDir * 4f;
                } else if (iN < 0.5f) {
                    vector5 = Custom.Bezier(chk0Pos, chk0Pos + segmentDir * 2f, bodyPos, bodyPos - chkDir * 4f, Mathf.InverseLerp(0f, 0.5f, iN));
                } else {
                    vector5 = Custom.Bezier(bodyPos, bodyPos + chkDir * 4f, headPos, headPos - bodyDir * 2f, Mathf.InverseLerp(0.5f, 1f, iN));
                }

                Vector2 vector6 = vector5;
                Vector2 a5 = Custom.PerpendicularVector(vector6, v);
                vector5 += a5 * Mathf.Lerp(squirm[i, 1], squirm[i, 0], timeStacker) * num5 * (iN * 0.3f + Mathf.Sin(iN * 3.1415927f));
                Vector2 a6 = Custom.PerpendicularVector(vector5, vector4);
                m[0].MoveVertice(i * 4, (vector4 + vector5) / 2f - a6 * (num5 + num) * 0.5f - camPos);
                m[0].MoveVertice(i * 4 + 1, (vector4 + vector5) / 2f + a6 * (num5 + num) * 0.5f - camPos);
                m[0].MoveVertice(i * 4 + 2, vector5 - a6 * num5 - camPos);
                m[0].MoveVertice(i * 4 + 3, vector5 + a6 * num5 - camPos);

                if (i > 1 && i < meshSegs - 1) {
                    float d = Mathf.Lerp(0.2f, 0.5f, Mathf.Sin(3.1415927f * Mathf.Pow(Mathf.InverseLerp(2f, meshSegs - 2, i), 0.5f)));
                    m[1].MoveVertice((i - 2) * 4, (vector4 + a4 * num * d + vector5 + a4 * num5 * d) / 2f - a6 * (num5 + num) * 0.5f * d - camPos);
                    m[1].MoveVertice((i - 2) * 4 + 1, (vector4 + a4 * num * d + vector5 + a4 * num5 * d) / 2f + a6 * (num5 + num) * 0.5f * d - camPos);
                    m[1].MoveVertice((i - 2) * 4 + 2, vector5 + a4 * num5 * d - a6 * num5 * d - camPos);
                    m[1].MoveVertice((i - 2) * 4 + 3, vector5 + a4 * num5 * d + a6 * num5 * d - camPos);
                }

                vector4 = vector5;
                v = vector6;
                num = num5;
            }

            const float wingsSize = .7f;

            for (int m = 0; m < 2; m++) {
                Vector2 firstChunkPos = Vector2.Lerp(mos.firstChunk.lastPos, mos.firstChunk.pos, timeStacker);

                Vector2 wingVert = firstChunkPos;

                if (mos.Consious && mos.grasps[0] == null) {
                    Vector2 wingVertOff = new(m == 0 ? 1f : -1f, Mathf.Sin((Mathf.Lerp(lastWingFlap, wingFlap, timeStacker) + (m == 0 ? 0.33f : 0f)) * Mathf.PI * 2f) * .8f);

                    wingVert += (wingVertOff + segmentDir * .1f).normalized * wingsSize * 33f;
                } else {
                    wingVert += (segmentDir + Custom.PerpendicularVector(segmentDir) * (m == 0 ? 1f : -1f) * .2f).normalized * wingsSize * 33f;
                }

                Vector2 offset2 = Vector3.Slerp(Custom.PerpendicularVector(segmentDir) * (m == 0 ? -1f : 1f), new Vector2(m == 0 ? -1f : 1f, 0f), num);

                w[m].MoveVertice(1, wingVert + offset2 * 2f * wingsSize - camPos);
                w[m].MoveVertice(0, wingVert - offset2 * 2f * wingsSize - camPos);
                w[m].MoveVertice(2, firstChunkPos + offset2 * 2f * wingsSize - camPos);
                w[m].MoveVertice(3, firstChunkPos - offset2 * 2f * wingsSize - camPos);
            }

            ApplyPalette(sLeaser, rCam, roomPalette);
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            roomPalette = palette;

            yellow = Color.Lerp(new Color(0.95f, 0.8f, 0.55f), palette.fogColor, 0.2f);
            yellow = Color.Lerp(yellow, palette.blackColor, .8f);

            Color to = new(1f, 0f, 0f);
            Color wColors = new(1f, 1f, 1f);

            if (darkness > 0f) {
                yellow = Color.Lerp(yellow, palette.blackColor, darkness);
                to = Color.Lerp(to, palette.blackColor, darkness);
                wColors = Color.Lerp(wColors, palette.blackColor, Mathf.Pow(darkness, 1.5f));
            }

            for (int i = 0; i < m[0].verticeColors.Length; i++) {
                float value = Mathf.InverseLerp(0f, m[0].verticeColors.Length - 1, i);
                m[0].verticeColors[i] = Color.Lerp(yellow, to, 0.25f + Mathf.InverseLerp(0f, 0.2f + 0.8f, value) * 0.75f * mos.bloat);
            }

            m[0].verticeColors[0] = wColors;
            m[0].verticeColors[1] = wColors;

            for (int j = 0; j < m[1].verticeColors.Length; j++) {
                float value2 = Mathf.InverseLerp(0f, m[1].verticeColors.Length - 1, j);
                m[1].verticeColors[j] = Custom.RGB2RGBA(wColors, Mathf.InverseLerp(0f, 0.2f, value2) * (1f - 0.75f - 0.25f));
            }

            for (int n = 0; n < 2; n++) {
                w[n].verticeColors[2] = Color.Lerp(palette.fogColor, yellow, 0.5f);
                w[n].verticeColors[3] = Color.Lerp(palette.fogColor, yellow, 0.5f);
                w[n].verticeColors[0] = Color.Lerp(palette.fogColor, new Color(1f, 1f, 1f), 0.5f);
                w[n].verticeColors[1] = Color.Lerp(palette.fogColor, new Color(1f, 1f, 1f), 0.5f);
            }
        }
    }
}
