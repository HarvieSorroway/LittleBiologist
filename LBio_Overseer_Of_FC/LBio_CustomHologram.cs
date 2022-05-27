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
            message = EnumExt_LBioOverseer.LBio_NaviHologram;

            effect = new HologramLightEffect(this, totalSprites, 1f, 1f, 100f, 20f);
            AddPart(effect);
            arrow = new LBio_NaviArrow(this, totalSprites);
            AddPart(arrow);

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
                return;
            }
            if(LBio_NaviHodler.selecetdHolder == null)
            {
                Destroy();
                return;
            }
            if(LBio_NaviHodler.selecetdHolder != null && LBio_NaviHodler.selecetdHolder.AbCreature != null && LBio_NaviHodler.selecetdHolder.AbCreature.ID != ID)
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
            }
        }

        EntityID? ID;

        LBio_NaviArrow arrow;
        LBio_Symbol symbol;
        HologramLightEffect effect;
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
            symbolCol = CreatureSymbol.ColorOfCreature(temp);
            sLeaser.sprites[firstSprite] = new FSprite(CreatureSymbol.SpriteNameOfCreature(temp), true) { color = CreatureSymbol.ColorOfCreature(temp) };
        }

        public override Color GetToColor => symbolCol;

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, Vector2 partPos, Vector2 headPos, float useFade, float popOut, Color useColor)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos, partPos, headPos, useFade, popOut, useColor);
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

        public override Color GetToColor
        {
            get
            {
                return new Color(1f, 0f, 0f);
            }
        }
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
