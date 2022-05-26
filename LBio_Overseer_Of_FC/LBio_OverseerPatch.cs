using CoralBrain;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static LittleBiologist.LBio_Const;


namespace LittleBiologist
{
    public static class LBio_OverseerPatch
    {
        public static void Patch()
        {
            OverseerColorModify.Patch();
            OverseerColorModify.AddColor(806, new Color(82f / 256f, 98f / 256f, 169f / 256f));
        }
    }

    public static class OverseerColorModify
    {
        public static void Patch()
        {
            On.OverseerCarcass.AbstractOverseerCarcass.ctor += AbstractOverseerCarcass_ctor;
            On.OverseerGraphics.ColorOfSegment += OverseerGraphics_ColorOfSegment;
            On.OverseerGraphics.DrawSprites += OverseerGraphics_DrawSprites;
            On.OverseerGraphics.HologramMatrix.DrawSprites += HologramMatrix_DrawSprites;
            On.OverseerGraphics.Update += OverseerGraphics_Update;
        }

        private static void OverseerGraphics_Update(On.OverseerGraphics.orig_Update orig, OverseerGraphics self)
        {
            int iterator = (self.overseer.abstractCreature.abstractAI as OverseerAbstractAI).ownerIterator;
            if (IteratorAndColor.ContainsKey(iterator))
            {
                Color col = IteratorAndColor[iterator];

                self.UpdateDrawPositions(1f);
                self.lastComseticLookAt = self.cosmeticLookAt;
                self.cosmeticLookAt = self.overseer.AI.CosmeticLookAt;
                self.lastConvoMode = self.convoMode;
                if (self.overseer.mode == Overseer.Mode.Conversing)
                {
                    self.convoMode = Mathf.Min(1f, self.convoMode + 0.033333335f);
                }
                else
                {
                    self.convoMode = Mathf.Max(0f, self.convoMode - 0.033333335f);
                }
                self.lastHoloLensUp = self.holoLensUp;
                if (self.overseer.AI.bringUpLens > 0.7f)
                {
                    self.holoLensUp = Mathf.Min(1f, self.holoLensUp + 0.033333335f);
                }
                else if (self.overseer.AI.bringUpLens < 0.3f)
                {
                    self.holoLensUp = Mathf.Max(0f, self.holoLensUp - 0.033333335f);
                }
                else if (self.holoLensUp > 0.5f)
                {
                    self.holoLensUp = Mathf.Min(1f, self.holoLensUp + 0.033333335f);
                }
                else if (self.holoLensUp <= 0.5f)
                {
                    self.holoLensUp = Mathf.Max(0f, self.holoLensUp - 0.033333335f);
                }
                self.holoLensUp = Mathf.Min(new float[]
                {
                    self.holoLensUp,
                    1f - self.convoMode,
                    1f - self.overseer.dying
                });
                if (self.overseer.mode == Overseer.Mode.Zipping)
                {
                    self.holoLensUp = ((self.overseer.AI.bringUpLens >= 0.5f) ? 1f : 0f);
                }
                if (self.overseer.dying > 0f)
                {
                    if (self.overseer.room.ViewedByAnyCamera(self.overseer.mainBodyChunk.pos, 400f))
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            self.overseer.room.AddObject(new OverseerEffect(self.DrawPosOfSegment(UnityEngine.Random.value, 1f), Custom.RNV() * UnityEngine.Random.value * 0.1f, col, Mathf.Lerp(80f, 10f, self.overseer.dying), Mathf.Lerp(1f, 0.1f, self.overseer.dying)));
                        }
                        for (int j = 0; j < 3; j++)
                        {
                            self.overseer.room.AddObject(new Spark(self.DrawPosOfSegment(UnityEngine.Random.value, 1f), self.overseer.mainBodyChunk.vel * 0.5f + Custom.RNV() * 14f * UnityEngine.Random.value, col, null, 14, 21));
                        }
                    }
                    self.cosmeticLookAt = self.overseer.mainBodyChunk.pos + Custom.DirVec(self.overseer.rootPos, self.overseer.mainBodyChunk.pos) * 1000f;
                }
                self.lastCulled = self.culled;
                self.culled = self.ShouldBeCulled;
                if (!self.culled && self.lastCulled)
                {
                    self.Reset();
                }
                if (!self.culled)
                {
                    for (int i = 0; i < self.subModules.Count; i++)
                    {
                        self.subModules[i].Update();
                    }
                }
                self.zipLoop.Update();
                self.zipLoop.pos = self.DrawPosOfSegment(0f, 1f);
                self.zipLoop.volume = 1f - self.overseer.extended;
                for (int k = 0; k < self.myceliaMovements.GetLength(0); k++)
                {
                    self.myceliaMovements[k, 0] = Mathf.Lerp(self.myceliaMovements[k, 1], self.myceliaMovements[k, 2], Custom.SCurve(self.myceliaMovements[k, 3], 0.5f));
                    self.myceliaMovements[k, 3] = Mathf.Min(1f, self.myceliaMovements[k, 3] + self.myceliaMovements[k, 4]);
                    if (UnityEngine.Random.value < 1f / ((k != 0) ? 20f : 80f) && self.myceliaMovements[k, 3] == 1f)
                    {
                        if (k == 0)
                        {
                            self.myceliaMovements[k, 1] = self.myceliaMovements[k, 0];
                            self.myceliaMovements[k, 3] = 0f;
                            self.myceliaMovements[k, 2] = self.myceliaMovements[k, 0] + Mathf.Pow(UnityEngine.Random.value, 3f) * 0.8f * ((UnityEngine.Random.value >= 0.5f) ? 1f : -1f);
                            self.myceliaMovements[k, 4] = 0.01f / Mathf.Abs(self.myceliaMovements[k, 1] - self.myceliaMovements[k, 2]);
                        }
                        else
                        {
                            self.myceliaMovements[k, 1] = self.myceliaMovements[k, 0];
                            self.myceliaMovements[k, 3] = 0f;
                            self.myceliaMovements[k, 2] = UnityEngine.Random.value;
                            self.myceliaMovements[k, 4] = 0.02f / Mathf.Abs(self.myceliaMovements[k, 1] - self.myceliaMovements[k, 2]);
                        }
                    }
                }
                float urX = -self.useDir.y / 6.2831855f;
                float urY = self.useDir.x / 6.2831855f;
                for (int l = 0; l < self.mycelia.Length; l++)
                {
                    self.mycelia[l].Update();
                    self.mycelia[l].conRad = self.myceliaConRad * self.overseer.extended * Mathf.Lerp(1f, 2f - self.overseer.size, self.convoMode);
                    Vector2 a = Custom.DegToVec((float)l / (float)self.mycelia.Length * 360f);
                    a *= Mathf.Lerp(25f, 15f, self.myceliaMovements[1, 0]) * self.overseer.size;
                    Vector2 vector = self.overseer.mainBodyChunk.pos + self.MyceliaPosTo2D(new Vector3(a.x, a.y, self.myceliaMovements[1, 0] * -25f * self.overseer.size - Custom.LerpMap(self.overseer.size, 0.5f, 1f, 5f, 0f)), urX, urY, self.myceliaMovements[0, 0]);
                    Vector2 v = Custom.DirVec(self.overseer.mainBodyChunk.pos, vector);
                    for (int m = 0; m < self.mycelia[l].points.GetLength(0); m++)
                    {
                        float num = (float)m / (float)(self.mycelia[l].points.GetLength(0) - 1);
                        self.mycelia[l].points[m, 2] -= self.overseer.mainBodyChunk.vel;
                        self.mycelia[l].points[m, 2] *= 0.9f * self.overseer.extended;
                        self.mycelia[l].points[m, 2] += self.overseer.mainBodyChunk.vel;
                        self.mycelia[l].points[m, 2] += new Vector2(Vector3.Slerp(-self.useDir, v, 0.5f).x, Vector3.Slerp(-self.useDir, v, 0.5f).y) * (1f - num);
                        self.mycelia[l].points[m, 2] += (vector - self.mycelia[l].points[m, 0]) * 0.05f * Mathf.Sin(num * 3.1415927f);
                        self.mycelia[l].points[m, 2] += new Vector2(self.useDir.x, self.useDir.y) * Mathf.Pow(num, 2f) * 0.5f;
                        self.mycelia[l].points[m, 0] = Vector2.Lerp(self.mycelia[l].points[m, 0], self.ConnectionPos(m, 1f), 1f - self.overseer.extended);
                    }
                    if (self.mycelia[l].connection == null)
                    {
                        if (self.overseer.mode == Overseer.Mode.Conversing && self.overseer.conversationPartner != null && self.overseer.conversationPartner.graphicsModule != null)
                        {
                            Mycelium mycelium = (self.overseer.conversationPartner.graphicsModule as OverseerGraphics).mycelia[UnityEngine.Random.Range(0, (self.overseer.conversationPartner.graphicsModule as OverseerGraphics).mycelia.Length)];
                            if (mycelium != self.mycelia[l] && mycelium.owner != self.owner && mycelium.connection == null && Custom.DistLess(self.mycelia[l].Base, mycelium.Base, (self.mycelia[l].length + mycelium.length) * 0.75f))
                            {
                                self.mycelia[l].connection = new Mycelium.MyceliaConnection(self.mycelia[l], mycelium);
                                mycelium.connection = self.mycelia[l].connection;
                            }
                        }
                    }
                    else if (self.overseer.mode != Overseer.Mode.Conversing || self.mycelia[l].connection.Other(self.mycelia[l]).connection != self.mycelia[l].connection)
                    {
                        self.mycelia[l].connection.Other(self.mycelia[l]).connection = null;
                        self.mycelia[l].connection = null;
                    }
                }
            }
            else
            {
                orig.Invoke(self);
            }
        }

        private static void HologramMatrix_DrawSprites(On.OverseerGraphics.HologramMatrix.orig_DrawSprites orig, OverseerGraphics.HologramMatrix self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig.Invoke(self, sLeaser, rCam, timeStacker, camPos);
            if (self.overseerGraphics.holoLensUp == 0f && self.overseerGraphics.lastHoloLensUp == 0f)
            {
                return;
            }
            int iterator = (self.overseerGraphics.overseer.abstractCreature.abstractAI as OverseerAbstractAI).ownerIterator;

            if (IteratorAndColor.ContainsKey(iterator))
            {
                Color color = Color.Lerp(IteratorAndColor[iterator], self.overseerGraphics.ColorOfSegment(0f, timeStacker), UnityEngine.Random.value);
                for (int j = 0; j < self.totalSprites; j++)
                {
                    sLeaser.sprites[self.firstSprite + j].color = color;
                }
            }
        }

        private static void OverseerGraphics_DrawSprites(On.OverseerGraphics.orig_DrawSprites orig, OverseerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig.Invoke(self, sLeaser, rCam, timeStacker, camPos);
            if (self.overseer.room == null)
            {
                return;
            }
            int iterator = (self.overseer.abstractCreature.abstractAI as OverseerAbstractAI).ownerIterator;

            if (IteratorAndColor.ContainsKey(iterator))
            {
                sLeaser.sprites[self.GlowSprite].color = IteratorAndColor[iterator];
            }
        }

        public static void AddColor(int ownIterator, Color color)
        {
            try
            {
                IteratorAndColor.Add(ownIterator, color);
            }
            catch
            {
                Log("Color of iterator already exists", ownIterator.ToString());
            }
        }

        private static Color OverseerGraphics_ColorOfSegment(On.OverseerGraphics.orig_ColorOfSegment orig, OverseerGraphics self, float f, float timeStacker)
        {
            int iterator = (self.overseer.abstractCreature.abstractAI as OverseerAbstractAI).ownerIterator;

            if (IteratorAndColor.ContainsKey(iterator))
            {
                Color col = IteratorAndColor[iterator];

                Color a = Custom.RGB2RGBA((col + new Color(0f, 0f, 1f) + self.earthColor * 8f) / 10f, 0.5f);
                a = Color.Lerp(a, Color.Lerp(col, Color.Lerp(self.NeutralColor, self.earthColor, Mathf.Pow(f, 2f)), (!self.overseer.SandboxOverseer) ? 0.5f : 0.15f), self.ExtensionOfSegment(f, timeStacker));
                return Color.Lerp(a, Custom.RGB2RGBA(col, 0f), Mathf.Lerp(self.overseer.lastDying, self.overseer.dying, timeStacker));
            }
            return orig.Invoke(self, f, timeStacker);
        }

        private static void AbstractOverseerCarcass_ctor(On.OverseerCarcass.AbstractOverseerCarcass.orig_ctor orig, OverseerCarcass.AbstractOverseerCarcass self, World world, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, Color color, int ownerIterator)
        {
            Color newCol = color;
            if (IteratorAndColor.ContainsKey(ownerIterator))
            {
                newCol = IteratorAndColor[ownerIterator];
            }
            orig.Invoke(self, world, realizedObject, pos, ID, color, ownerIterator);
        }

        public static Color? GetColor(int iterator)
        {
            if (IteratorAndColor.ContainsKey(iterator))
            {
                return IteratorAndColor[iterator];
            }
            return null;
        }
        
        static Dictionary<int, Color> IteratorAndColor = new Dictionary<int, Color>();
    }
}


