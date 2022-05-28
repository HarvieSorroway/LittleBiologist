using OverseerHolograms;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using LittleBiologist.LBio_Navigations;
using static LittleBiologist.LBio_Const;


namespace LittleBiologist
{
    public class LBio_NaviHologram : OverseerHologram
    {
        public AbstractCreature NaviCreature
        {
            get
            {
                if(LBio_NaviHodler.selecetdHolder != null && LBio_NaviHodler.selecetdHolder.AbCreature != null && LBio_NaviHodler.selecetdHolder.AbCreature.Room != null)
                {
                    if(ID != null && LBio_NaviHodler.selecetdHolder.AbCreature.ID.number != ID.Value.number)
                    {
                        stillRelevant = false;
                        return null;
                    }
                    return LBio_NaviHodler.selecetdHolder.AbCreature;
                }
                stillRelevant = false;
                Destroy();
                return null;
            }
        }
        public LBio_NaviHologram(Overseer overseer, Creature communicateWith, float importance) : base(overseer, EnumExt_LBioOverseer.LBio_NaviHologram, communicateWith, importance)
        {
            Log("Init NaviHologram");
            if(NaviCreature != null)
            {
                ID = NaviCreature.ID;
            }
            else
            {
                Log("Init failed");
                Destroy();
                ID = null;
            }

            totalSprites = 0;

            effect = new LBio_LightEffect(this, totalSprites, 1f, 1f, 100f, 20f);
            AddPart(effect);
            arrow = new LBio_NaviArrow(this, totalSprites);
            AddPart(arrow);

            AddPart(new LBio_HoloLight(this,totalSprites));
            

            symbol = new LBio_Symbol(this, totalSprites, NaviCreature.creatureTemplate.type);
            customDirectionFinder = new CustomDirectionFinder(overseer.abstractCreature.world, NaviCreature.Room, this);
            //redCross.partFade = 0f;

            if (symbol != null)
            {
                AddPart(symbol);
                redCross = new LBio_RedCross(this, totalSprites, true);
                AddPart(redCross);
            }
            else
            {
                Destroy();
            }
        }

        public override float DisplayPosScore(IntVector2 testPos)
        {
            float num = base.DisplayPosScore(testPos);

            if (customDirectionFinder == null || !customDirectionFinder.done || NaviCreature == null)
            {
                return num;
            }

            if (room.abstractRoom.index != NaviCreature.Room.index && !NaviCreature.Room.offScreenDen)
            {
                ShortcutData? shortcutData = null;
                for (int i = 0; i < room.shortcuts.Length; i++)
                {
                    AbstractRoom leadTo = room.WhichRoomDoesThisExitLeadTo(room.shortcuts[i].DestTile);
                    if (leadTo != null && leadTo.index == customDirectionFinder.GetForwardRoom(room.abstractRoom.index))
                    {
                        shortcutData = room.shortcuts[i];
                    }
                }
                this.shortcutData = shortcutData;

                if (shortcutData != null && Custom.DistLess(this.room.MiddleOfTile(shortcutData.Value.StartTile), communicateWith.DangerPos, 300f))
                {
                    num -= 10000 - 20 * Vector2.Distance(this.room.MiddleOfTile(shortcutData.Value.StartTile) + Vector2.up * 60f, this.room.MiddleOfTile(testPos));
                }
                else
                {
                    num -= 10000 - 20 * Vector2.Distance(communicateWith.DangerPos + Vector2.up * 60f, this.room.MiddleOfTile(testPos));
                }
            }
            else
            {
                if (NaviCreature.realizedCreature != null && Custom.DistLess(NaviCreature.realizedCreature.DangerPos, communicateWith.DangerPos, 700f))
                {
                    num -= 10000 - 20 * Vector2.Distance(NaviCreature.realizedCreature.DangerPos + Vector2.up * 60f, this.room.MiddleOfTile(testPos));
                }
                else
                {
                    num -= 10000 - 20 * Vector2.Distance(communicateWith.DangerPos + Vector2.up * 60f, this.room.MiddleOfTile(testPos));
                }
            }

            return num;
        }

        public override float InfluenceHoverScoreOfTile(IntVector2 testTile, float f)
        {
            return f -(10000 - Vector2.Distance(overseer.DangerPos,communicateWith.DangerPos) * 20f);
        }

        public override void Destroy()
        {
            base.Destroy();
            stillRelevant = false;
            overseer.hologram = null;
        }

        public override void Update(bool eu)
        {
            if (!stillRelevant)
            {
                if (!slatedForDeletetion)
                {
                    Destroy();
                }
                return;
            }
            if(LBio_NaviHodler.selecetdHolder == null || room != communicateWith.room)
            {
                Destroy();
                return;
            }
            if(LBio_NaviHodler.selecetdHolder != null && ((LBio_NaviHodler.selecetdHolder.AbCreature != null && LBio_NaviHodler.selecetdHolder.AbCreature.ID != ID) || LBio_NaviHodler.selecetdHolder.AbCreature == null))
            {
                Destroy();
                return;
            }
            if (customDirectionFinder == null)
            {
                Destroy();
                return;
            }


            overseer.AI.scaredDistance = 0f;
            base.Update(eu);
            
            customDirectionFinder.Update();
            lookAtCommunicationCreature = true;


            if(Vector2.Distance(overseer.DangerPos,communicateWith.DangerPos) > 350f)
            {
                dontGoToPlayerFrames += 2;
            }
            else
            {
                dontGoToPlayerFrames = 0;
            }


            if (NaviCreature.Room.offScreenDen)
            {
                redCross.partFade = 1f;
                arrow.partFade = 0f;
            }
            else
            {
                redCross.partFade = 0f;
                arrow.partFade = 1f;
            }

            if (overseer.abstractCreature != LBio_NaviOverseer.guideOverseer || communicateWith.dead || NaviCreature == null)
            {
                Destroy();
                customDirectionFinder.destroy = true;
                return;
            }
            else
            {
                if(NaviCreature.Room.index != customDirectionFinder.showToRoom)
                {
                    Destroy();
                    customDirectionFinder.destroy = true;
                    customDirectionFinder = null;
                    return;
                }

                if(dontGoToPlayerFrames < 400)
                {
                    if (room.abstractRoom.index != NaviCreature.Room.index)
                    {
                        if (shortcutData != null)
                        {
                            arrow.offset = Vector2.zero;
                            arrow.dir = (room.MiddleOfTile(shortcutData.Value.StartTile) - pos).normalized;
                        }
                    }
                    else
                    {
                        if (NaviCreature.realizedCreature != null)
                        {
                            arrow.partFade = 1f;
                            if(Vector2.Distance(NaviCreature.realizedCreature.DangerPos, room.MiddleOfTile(displayTile)) < 400f)
                            {
                                arrow.dir = -Vector2.up;
                                arrow.offset = NaviCreature.realizedCreature.DangerPos - (pos - Vector2.up * 60f);
                            }
                            else
                            {
                                arrow.dir = (NaviCreature.realizedCreature.DangerPos - pos + arrow.lastOffset).normalized;
                                arrow.offset = Vector2.zero;
                            }
                            
                        }
                        else
                        {
                            arrow.partFade = 0f;
                        }
                    }
                }
                else
                {
                    this.fade = 1f - UnityEngine.Random.value * 0.1f * Mathf.Sin(Time.time + UnityEngine.Random.value) * Mathf.Sin(Time.time+ UnityEngine.Random.value);
                    arrow.offset = communicateWith.DangerPos - (pos - Vector2.up * 60f);
                    if(shortcutData != null)
                    {
                        arrow.dir = (room.MiddleOfTile(shortcutData.Value.StartTile) - (pos + arrow.offset)).normalized;
                    }
                }
                symbol.offset = arrow.offset;
                effect.offset = arrow.offset;
                redCross.offset = arrow.offset;
            }
        }

        EntityID? ID;

        LBio_NaviArrow arrow;
        LBio_Symbol symbol;
        LBio_LightEffect effect;
        LBio_RedCross redCross;

        int dontGoToPlayerFrames = 0;

        public override Color color => OverseerColorModify.GetColor(806) == null ? new Color(0.44705883f, 0.9019608f, 0.76862746f) : OverseerColorModify.GetColor(806).Value;
        public CustomDirectionFinder customDirectionFinder;
        ShortcutData? shortcutData;
    }

    public class LBio_NaviArrow : OverseerHologram.HologramPart
    {
        public LBio_NaviArrow(OverseerHologram hologram, int firstSprite) : base(hologram, firstSprite)
        {
            base.AddClosed3DPolygon(new List<Vector2>
            {
                new Vector2(0f , 20f) * 1.5f,
                new Vector2(-7f, 15f) * 1.5f,
                new Vector2(0f, 27f) * 1.5f,
                new Vector2(7f, 15f) * 1.5f
            }, 1.5f);
        }

        public override void Update()
        {
            base.Update();
            this.rotation.z = Custom.VecToDeg(new Vector2(-this.dir.x, this.dir.y));
        }
        public Vector2 dir;
    }

    public class LBio_Symbol : OverseerHologram.HologramPart
    {
        public LBio_Symbol(OverseerHologram hologram, int firstSprite, CreatureTemplate.Type type) : base(hologram, firstSprite)
        {
            totalSprites = 1;
            this.type = type;
            allSpritesHologramShader = true;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            IconSymbol.IconSymbolData temp = new IconSymbol.IconSymbolData(type, AbstractPhysicalObject.AbstractObjectType.Creature, 0);
            symbolCol = CreatureSymbol.ColorOfCreature(temp) * 0.8f + hologram.color * 0.3f;
            sLeaser.sprites[firstSprite] = new FSprite(CreatureSymbol.SpriteNameOfCreature(temp), true) { color = CreatureSymbol.ColorOfCreature(temp) };
        }

        public override Color GetToColor => symbolCol;

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, Vector2 partPos, Vector2 headPos, float useFade, float popOut, Color useColor)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos, partPos, headPos, useFade, popOut, useColor);

            FSprite fsprite = sLeaser.sprites[this.firstSprite];
            if (UnityEngine.Random.value > Mathf.InverseLerp(0.5f, 1f, useFade))
            {
                fsprite.isVisible = false;
                return;
            }
            partPos = Vector3.Lerp(headPos, partPos, popOut);
            fsprite.isVisible = true;
            fsprite.SetPosition(partPos - camPos);
            fsprite.color = useColor;
            if (UnityEngine.Random.value > useFade)
            {
                fsprite.scale = UnityEngine.Random.value;
                return;
            }
            fsprite.scale = 1f;
        }

        CreatureTemplate.Type type;
        Color symbolCol;
    }

    public class LBio_RedCross : OverseerHologram.HologramPart
    {
        public LBio_RedCross(OverseerHologram hologram, int firstSprite, bool small) : base(hologram, firstSprite)
        {
            List<Vector2> list = new List<Vector2>();
            for (int i = 0; i < 4; i++)
            {
                Vector2 vector = Custom.DegToVec(45f + 90f * (float)i);
                list.Add(vector * 5f + Custom.PerpendicularVector(vector) * 5f);
                list.Add(vector * 60f + Custom.PerpendicularVector(vector) * 5f);
                list.Add(vector * 60f - Custom.PerpendicularVector(vector) * 5f);
            }
            for (int j = 0; j < list.Count; j++)
            {
                List<Vector2> list3;
                List<Vector2> list2 = list3 = list;
                int num2;
                int num = num2 = j;
                Vector2 a = list3[num2];
                list2[num] = a * ((!small) ? 1f : 0.4f);
            }
            base.AddClosed3DPolygon(list, 5f);
        }

        public override Color GetToColor => new Color(1f, 0f, 0f) * 0.7f + hologram.color * 0.3f;
    }

    public class LBio_LightEffect : OverseerHologram.HologramPart
    {
        public LBio_LightEffect(OverseerHologram hologram, int firstSprite, float lightAlpha, float darkDownAlpha, float lightRad, float darkDownRad) : base(hologram, firstSprite)
        {
            this.darkDownAlpha = darkDownAlpha;
            this.lightAlpha = lightAlpha;
            this.darkDownRad = darkDownRad;
            this.lightRad = lightRad;
            this.allSpritesHologramShader = false;
            this.totalSprites = 0;
            if (darkDownAlpha > 0f)
            {
                this.darkSprite = this.totalSprites;
                this.totalSprites++;
            }
            if (lightAlpha > 0f)
            {
                this.lightSprite = this.totalSprites;
                this.totalSprites++;
            }
            this.totalSprites = ((this.darkSprite <= -1) ? 0 : 1) + ((this.lightSprite <= -1) ? 0 : 1);
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);
            for (int i = 0; i < this.totalSprites; i++)
            {
                sLeaser.sprites[this.firstSprite + i] = new FSprite("Futile_White", true);
                if (i == this.darkSprite)
                {
                    sLeaser.sprites[this.firstSprite + i].shader = rCam.game.rainWorld.Shaders["FlatLight"];
                    sLeaser.sprites[this.firstSprite + i].color = new Color(0f, 0f, 0f);
                }
                else if (i == this.lightSprite)
                {
                    sLeaser.sprites[this.firstSprite + i].shader = rCam.game.rainWorld.Shaders["LightSource"];
                }
            }
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, Vector2 partPos, Vector2 headPos, float useFade, float popOut, Color useColor)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos, partPos, headPos, useFade, popOut, useColor);
            bool notShowThisFrame = UnityEngine.Random.value > Mathf.InverseLerp(0f, 0.5f, useFade);

            for (int i = 0; i < this.totalSprites; i++)
            {
                if (notShowThisFrame)
                {
                    sLeaser.sprites[this.firstSprite + i].isVisible = false;
                    continue;
                }
                sLeaser.sprites[this.firstSprite + i].isVisible = true;
                partPos = Vector3.Lerp(headPos, partPos, popOut);
                sLeaser.sprites[this.firstSprite + i].x = partPos.x - camPos.x;
                sLeaser.sprites[this.firstSprite + i].y = partPos.y - camPos.y;
                if (i == this.lightSprite)
                {
                    sLeaser.sprites[this.firstSprite + i].color = useColor;
                    sLeaser.sprites[this.firstSprite + i].alpha = this.lightAlpha * Mathf.Pow(useFade, 3f);
                    sLeaser.sprites[this.firstSprite + i].scale = this.lightRad / 8f * (0.5f + 0.5f * useFade);
                }
                else if (i == this.darkSprite)
                {
                    sLeaser.sprites[this.firstSprite + i].alpha = this.darkDownAlpha * Mathf.Pow(useFade, 3f);
                    sLeaser.sprites[this.firstSprite + i].scale = this.darkDownRad / 8f * (0.5f + 0.5f * useFade);
                }
            }

        }
        public float darkDownAlpha;
        public float darkDownRad;
        public float lightAlpha;
        public float lightRad;
        public int darkSprite = -1;
        public int lightSprite = -1;
    }

    public class LBio_HoloLight : OverseerHologram.HologramPart
    {
        public LBio_HoloLight(OverseerHologram hologram,int firstSprites) : base(hologram, firstSprites)
        {
            totalSprites = 10;
            allSpritesHologramShader = false;

            overseer = hologram.overseer;
            player = hologram.communicateWith as Player;
        }

        public bool projectorActive
        {
            get
            {
                return hologram.overseer.room == hologram.room && hologram.overseer.mode != Overseer.Mode.Zipping;
            }
        }

        // Token: 0x060022C5 RID: 8901 RVA: 0x00222368 File Offset: 0x00220568
        public Vector2 OverseerEyePos(float timeStacker)
        {
            if (hologram.overseer.graphicsModule == null || hologram.overseer.room == null)
            {
                return Vector2.Lerp(hologram.overseer.mainBodyChunk.lastPos, hologram.overseer.mainBodyChunk.pos, timeStacker);
            }
            return (hologram.overseer.graphicsModule as OverseerGraphics).DrawPosOfSegment(0f, timeStacker);
        }


        public Vector2 GoalPos()
        {
            return Vector2.Lerp(hologram.communicateWith.mainBodyChunk.pos, hologram.communicateWith.bodyChunks[1].pos, 0.3f);
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites[firstSprite] = new FSprite("Futile_White", true);
            sLeaser.sprites[firstSprite].color = hologram.color;
            sLeaser.sprites[firstSprite].shader = rCam.game.rainWorld.Shaders["HoloGridMod"];

            for (int i = 1; i < 10; i++)
            {
                sLeaser.sprites[firstSprite + i] = new FSprite("pixel", true);
                sLeaser.sprites[firstSprite + i].shader = rCam.game.rainWorld.Shaders["Hologram"];
                sLeaser.sprites[firstSprite + i].color = hologram.color;
                sLeaser.sprites[firstSprite + i].anchorY = 0f;
            }
        }

        public override void Update()
        {
            base.Update();
            this.lastPos = this.pos;
            this.lastPower = this.power;
            this.lastDisplace = this.displace;
            this.lastFlicker = this.flicker;
            this.lastProjPos = this.projPos;
            this.lastPushAroundPos = this.pushAroundPos;
            this.projPos = this.OverseerEyePos(1f);
            this.pushAroundPos *= 0.8f;
            if (this.overseer.extended > 0f)
            {
                this.pushAroundPos += (this.overseer.firstChunk.pos - this.overseer.firstChunk.lastPos) * this.overseer.extended;
            }
            if (!Custom.DistLess(this.projPos, this.pos, 1700f))
            {
                this.outOfRangeCounter++;
            }
            else
            {
                this.outOfRangeCounter -= 10;
            }
            this.outOfRangeCounter = Custom.IntClamp(this.outOfRangeCounter, 0, 200);
            this.displace = Vector2.Lerp(Custom.MoveTowards(this.displace, this.displaceGoal, 2f), this.displaceGoal, 0.05f);
            if (UnityEngine.Random.value < 0.06666667f)
            {
                this.displaceGoal += Custom.RNV() * UnityEngine.Random.value * 20f;
                if (this.displaceGoal.magnitude > 60f)
                {
                    this.displaceGoal = Custom.RNV() * UnityEngine.Random.value * UnityEngine.Random.value * 60f;
                }
            }
            if (UnityEngine.Random.value < Custom.LerpMap(Vector2.Distance(this.pos, this.projPos), 500f, 1200f, 0.005f, 0.033333335f))
            {
                this.flickerFac = UnityEngine.Random.value;
            }
            this.flickerFac = Mathf.Max(0f, this.flickerFac - 0.033333335f);
            if (UnityEngine.Random.value < 0.6f * this.flickerFac)
            {
                this.flicker = Mathf.Lerp(this.flicker, 0.5f + 0.5f * this.flickerFac, UnityEngine.Random.value);
            }
            else
            {
                this.flicker = Mathf.Max(0f, this.flicker - 0.1f);
            }
            if (this.projectorActive)
            {
                this.activeLinger = 20;
            }
            else if (this.activeLinger > 0)
            {
                this.activeLinger--;
            }
            if (hologram.communicateWith.room == hologram.room && hologram.communicateWith.enteringShortCut == null && this.activeLinger > 0 && !this.powerDownAndKill)
            {
                if (this.respawn)
                {
                    if (this.power <= 0f && this.lastPower <= 0f)
                    {
                        this.respawn = false;
                        this.pos = this.GoalPos();
                        this.inFront *= 0f;
                    }
                    else
                    {
                        this.power = Mathf.Max(0f, this.power - 0.033333335f);
                    }
                }
                else
                {
                    this.inFront = Vector2.Lerp(this.inFront, this.player.mainBodyChunk.pos - this.player.mainBodyChunk.lastPos, 0.1f);
                    this.pos = Vector2.Lerp(Custom.MoveTowards(this.pos, this.GoalPos(), 7f), this.GoalPos(), 0.1f);
                    this.power = Mathf.Min(((!this.projectorActive) ? 0.9f : 1f) * Mathf.InverseLerp(200f, 150f, (float)this.outOfRangeCounter) * Mathf.InverseLerp(80f, 50f, (float)this.notNeededCounter), this.power + 0.033333335f);
                }
            }
            else
            {
                this.respawn = true;
                this.power = Mathf.Max(0f, this.power - 0.033333335f);
                if (this.powerDownAndKill && this.power <= 0f && this.lastPower <= 0f)
                {
                    partFade = 0f;
                }
                if (!this.projectorActive)
                {
                    this.activeLinger = 0;
                }
            }
            float num = Needed(this.player);
            if (num < 0.1f)
            {
                this.notNeededCounter++;
            }
            else if (num > 0.4f)
            {
                this.notNeededCounter -= 10;
            }
            else if (num > 0.2f)
            {
                this.notNeededCounter--;
            }
            this.notNeededCounter = Custom.IntClamp(this.notNeededCounter, 0, 500);
            if ((hologram.communicateWith.room != null && hologram.communicateWith.room != hologram.room) || this.notNeededCounter >= 500)
            {
                partFade = 0f;
            }
        }
        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, Vector2 partPos, Vector2 headPos, float useFade, float popOut, Color useColor)
        {
            if (hologram.room != null)
            {
                float num = Mathf.Lerp(this.lastFlicker, this.flicker, timeStacker);
                float num2 = Mathf.Lerp(this.lastPower, this.power, timeStacker) * (1f - Mathf.Pow(num, 2f + UnityEngine.Random.value * 2f));
                float num3 = Mathf.Lerp(this.lastPower, this.power, timeStacker) * (1f - 0.05f * num);
                Vector2 vector = (hologram.overseer.room != hologram.room) ? Vector2.Lerp(this.lastProjPos, this.projPos, timeStacker) : this.OverseerEyePos(timeStacker);
                Vector2 vector2 = Vector2.Lerp(this.lastPos, this.pos, timeStacker) + Vector2.Lerp(this.lastDisplace, this.displace, timeStacker);
                vector2 = Vector2.Lerp(vector, vector2, num3) + Vector2.Lerp(this.lastPushAroundPos, this.pushAroundPos, timeStacker);
                num2 *= Custom.LerpMap(Vector2.Distance(vector2, vector), 500f, 1200f, 1f, 0.75f, 0.6f);
                Vector2 vector3 = Vector2.ClampMagnitude(vector - vector2, 240f) / 240f;
                sLeaser.sprites[firstSprite].x = vector2.x - camPos.x;
                sLeaser.sprites[firstSprite].y = vector2.y - camPos.y;
                sLeaser.sprites[firstSprite].scaleX = Mathf.Lerp(15f, 20f, num2 * num3) + Mathf.Sin(num2 * num3 * 3.1415927f) * 30f * 2f;
                sLeaser.sprites[firstSprite].scaleY = Mathf.Lerp(8f, 20f, num2 * num3) * 2f;
                sLeaser.sprites[firstSprite].color = new Color(Mathf.InverseLerp(-1f, 1f, vector3.x), Mathf.InverseLerp(-1f, 1f, vector3.y), num3, num2);
                float num4 = 8f * Mathf.Lerp(6f, 16f, num2 * num3);
                for (int i = 1; i < 10; i++)
                {
                    if (num2 < 0.5f)
                    {
                        sLeaser.sprites[firstSprite + i].isVisible = false;
                    }
                    else
                    {
                        Vector2 vector4 = vector2 + Custom.RNV() * Mathf.Pow(UnityEngine.Random.value, 0.65f) * num4;
                        float num5 = 0.75f;
                        if (hologram.room.GetTile(vector4).Solid)
                        {
                            num5 = 0f;
                        }
                        else if (UnityEngine.Random.value < Custom.LerpMap(num3, 0.5f, 1f, 0.85f, 0.25f))
                        {
                            num5 = 0.75f;
                        }
                        else if (UnityEngine.Random.value < 0.5f && hologram.room.GetTile(vector4).verticalBeam)
                        {
                            vector4.x = hologram.room.MiddleOfTile(vector4).x + ((UnityEngine.Random.value >= 0.5f) ? 2f : -2f);
                            num5 = 1f;
                        }
                        else if (UnityEngine.Random.value < 0.5f && hologram.room.GetTile(vector4).horizontalBeam)
                        {
                            vector4.y = hologram.room.MiddleOfTile(vector4).y + ((UnityEngine.Random.value >= 0.5f) ? 2f : -2f);
                            num5 = 1f;
                        }
                        else
                        {
                            int num6 = UnityEngine.Random.Range(0, 4);
                            for (int j = 0; j < 4; j++)
                            {
                                if (!Custom.DistLess(vector, vector4 + Custom.fourDirections[num6].ToVector2() * 20f, num4))
                                {
                                    num5 = 0f;
                                    break;
                                }
                                if (hologram.room.GetTile(vector4 + Custom.fourDirections[num6].ToVector2() * 20f).Solid)
                                {
                                    vector4 = hologram.room.MiddleOfTile(vector4 + Custom.fourDirections[num6].ToVector2() * 20f) - Custom.fourDirections[num6].ToVector2() * 10f;
                                    num5 = 1f;
                                    break;
                                }
                            }
                        }
                        if (num5 > 0f)
                        {
                            Vector2 vector5 = Vector2.Lerp(vector4, vector, Mathf.Pow(UnityEngine.Random.value, 3f - 1.5f * num2));
                            sLeaser.sprites[firstSprite + i].isVisible = true;
                            sLeaser.sprites[firstSprite + i].x = vector5.x - camPos.x;
                            sLeaser.sprites[firstSprite + i].y = vector5.y - camPos.y;
                            sLeaser.sprites[firstSprite + i].rotation = Custom.AimFromOneVectorToAnother(vector5, vector4);
                            sLeaser.sprites[firstSprite + i].scaleY = Vector2.Distance(vector5, vector4);
                            sLeaser.sprites[firstSprite + i].alpha = num5 * Mathf.Pow(UnityEngine.Random.value, 0.2f) * Mathf.InverseLerp(0.5f, 0.6f, num2);
                        }
                        else
                        {
                            sLeaser.sprites[i].isVisible = false;
                        }
                    }
                }
            }
        }

        public float Needed(Player player)
        {
            if (player.room == null || player.room.Darkness(player.mainBodyChunk.pos) < 0.85f || player.dead || player.room.game.cameras[0].room != player.room)
            {
                return 0f;
            }
            for (int i = 0; i < player.grasps.Length; i++)
            {
                if (player.grasps[i] != null)
                {
                    if (player.grasps[i].grabbed is Lantern)
                    {
                        return 0f;
                    }
                    if (player.grasps[i].grabbed is LanternMouse && (player.grasps[i].grabbed as LanternMouse).State.battery > 200)
                    {
                        return 0f;
                    }
                }
            }
            float num = Mathf.InverseLerp(0.85f, 0.92f, player.room.Darkness(player.mainBodyChunk.pos));
            for (int j = 0; j < player.room.lightSources.Count; j++)
            {
                if (player.room.lightSources[j].Rad > 120f && player.room.lightSources[j].Alpha > 0.2f && Custom.DistLess(player.mainBodyChunk.pos, player.room.lightSources[j].Pos, player.room.lightSources[j].rad + 200f))
                {
                    num -= Mathf.InverseLerp(player.room.lightSources[j].rad + 200f, player.room.lightSources[j].rad * 0.8f, Vector2.Distance(player.mainBodyChunk.pos, player.room.lightSources[j].Pos));
                    if (num <= 0f)
                    {
                        return 0f;
                    }
                }
            }
            return num;
        }

        public Vector2 pos;

        public Vector2 lastPos;

        public Vector2 lastDisplace;

        public Vector2 displace;

        public Vector2 displaceGoal;

        public Vector2 lastPushAroundPos;

        public Vector2 pushAroundPos;

        public Vector2 projPos;

        public Vector2 lastProjPos;

        public float power;

        public float lastPower;

        public int outOfRangeCounter;

        public int notNeededCounter;

        public float flicker;

        public float lastFlicker;

        public float flickerFac;

        public Player player;

        public Overseer overseer;

        public Vector2 inFront;

        public bool powerDownAndKill;

        public bool respawn;

        public int activeLinger;
    }

    public class CustomDirectionFinder
    {
        public CustomDirectionFinder(World world, AbstractRoom destination,LBio_NaviHologram owner)
        {
            this.world = world;
            this.showToRoom = destination.index;
            this.owner = owner;

            for (int m = 0; m < destination.connections.Length; m++)
            {
                this.checkNext.Enqueue(new IntVector2(destination.connections[m],showToRoom));
                forwardRooms.Add(destination.connections[m], showToRoom);
            }

            forwardRooms.Add(showToRoom, showToRoom);
        }

        public void Update()
        {
            if (this.done || this.destroy)
            {
                return;
            }
            if (this.checkNext.Count < 1)
            {
                Log("CheckFinish");
                this.done = true;
            }
            else
            {
                int count = checkNext.Count;
                while(count > 0)
                {
                    count--;
                    IntVector2 connectionToForward = checkNext.Dequeue();

                    AbstractRoom newCheckRoom = world.GetAbstractRoom(connectionToForward.x);
                    if(newCheckRoom != null)
                    {
                        for(int i = 0;i < newCheckRoom.connections.Length; i++)
                        {
                            if (!forwardRooms.ContainsKey(newCheckRoom.connections[i]))
                            {
                                if(newCheckRoom.connections[i] != -1)
                                {
                                    checkNext.Enqueue(new IntVector2(newCheckRoom.connections[i], newCheckRoom.index));
                                    forwardRooms.Add(newCheckRoom.connections[i], newCheckRoom.index);
                                }
                            }
                        }
                    }
                }
            }
        }

        public int GetForwardRoom(int currentRoom)
        {
            if (forwardRooms.ContainsKey(currentRoom))
            {
                return forwardRooms[currentRoom];
            }
            return -10;
        }

        public World world;

        public Dictionary<int,int> forwardRooms = new Dictionary<int, int>();

        //x : newConnection y:forwardRoom
        public Queue<IntVector2> checkNext = new Queue<IntVector2>();

        public bool done;

        public int showToRoom = -1;

        public bool destroy;

        public LBio_NaviHologram owner;
    }
}
