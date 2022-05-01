using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Reflection;
using static LittleBiologist.LittleBiologist_Const;

namespace LittleBiologist
{
    public class LittleBiologist_Label
    {
        //静态字段
        static Region _lastRegion;
        public static Region lastRegion
        {
            get
            {
                return _lastRegion;
            }
            set
            {
                if(_lastRegion != value && _lastRegion != null)
                {
                    _lastRegion = value;

                    if(labels.Count > 0)
                    {
                        
                        for(int i = labels.Count - 1;i >= 0;i--)
                        {
                            if(labels[i].region != value)
                            {
                                if (labels[i].abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
                                {
                                    labels[i].region = value;
                                }
                                else
                                {
                                    labels[i].Destory();
                                }
                            }
                        }
                    }
                }
            }
        }
        public static List<LittleBiologist_Label> labels = new List<LittleBiologist_Label>();

        //基础信息
        public Region region;
        public AbstractCreature abstractCreature;

        //标签信息
        public bool neverRevealed = false;
        bool _revealed = false;
        public bool revealed
        {
            get 
            {
                return _revealed;
            }
            set
            {
                _revealed = value;
                label.revealed = value;
            }
        }
        public Label_HUDPart label;

        public LittleBiologist_Label(AbstractCreature abstractCreature)
        {
            this.abstractCreature = abstractCreature;
            region = lastRegion;
            lastRegion = abstractCreature.Room.world.region;
            
            Patch();

            Log("Init Label for", abstractCreature);

            label = new Label_HUDPart(this);
        }

        public void RegetLabel()
        {
            if(abstractCreature.realizedCreature != null && abstractCreature.realizedCreature.graphicsModule != null && abstractCreature.realizedCreature.room == LittleBiologist.currentCamera.room)
            {
                RevealLabel();
            }
            else
            {
                HideLabel();
            }
        }

        public void RevealLabel()
        {
            if (!revealed)
            {
                revealed = true;
                Log("Reaval Label for", abstractCreature);
            }
        }

        public void HideLabel()
        {
            if (revealed)
            {
                revealed = false;
                Log("Hide Label for", abstractCreature);
            }
        }

        public void Destory()
        {
            label.Destroy();
            Log("Destroy Label for", abstractCreature);
            DisPatch();
            labels.Remove(this);
        }

        void DisPatch()
        {
            On.Room.AddObject -= Room_AddObject;
            On.UpdatableAndDeletable.Destroy -= UpdatableAndDeletable_Destroy;

            On.AbstractWorldEntity.Destroy -= AbstractWorldEntity_Destroy;
        }

        void Patch()
        {
            //标签
            //  基础功能
            On.Room.AddObject += Room_AddObject;
            On.UpdatableAndDeletable.Destroy += UpdatableAndDeletable_Destroy;

            //本体
            On.AbstractWorldEntity.Destroy += AbstractWorldEntity_Destroy;
        }

        private void AbstractWorldEntity_Destroy(On.AbstractWorldEntity.orig_Destroy orig, AbstractWorldEntity self)
        {
            if(self is AbstractCreature)
            {
                if((self as AbstractCreature) == abstractCreature)
                {
                    Destory();
                }
            }
            orig.Invoke(self);
        }

        private void UpdatableAndDeletable_Destroy(On.UpdatableAndDeletable.orig_Destroy orig, UpdatableAndDeletable self)
        {
            if(self is Creature)
            {
                if ((self as Creature).abstractCreature == abstractCreature)
                {
                    if ((self as Creature).room == LittleBiologist.currentCamera.room && (self as Creature).graphicsModule != null)
                    {
                        LittleBiologist.instance.StartCoroutine(Late_Creature_Die((self as Creature).graphicsModule));
                    }
                }
            }
            orig.Invoke(self);
        }

        private void Room_AddObject(On.Room.orig_AddObject orig, Room self, UpdatableAndDeletable obj)
        {
            orig.Invoke(self, obj);

            LittleBiologist.instance.StartCoroutine(Late_Room_AddObject(self,obj));
        }

        public IEnumerator Late_Room_AddObject(Room self, UpdatableAndDeletable obj)
        {
            yield return new WaitForEndOfFrame();

            if (obj is Creature)
            {
                if ((obj as Creature).abstractCreature == abstractCreature)
                {
                    if (self != LittleBiologist.currentCamera.room)
                    {
                        HideLabel();
                    }
                    else
                    {
                        if((obj as Creature).graphicsModule != null && self.drawableObjects.Contains((obj as Creature).graphicsModule))
                        {
                            RevealLabel();
                        }          
                    }
                }
            }
        }

        public IEnumerator Late_Creature_Die(GraphicsModule graphicsModule)
        {
            for(int i = 0;i < 1; i++)
            {
                yield return new WaitForFixedUpdate();
            }
            
            if (!LittleBiologist.currentCamera.room.drawableObjects.Contains(graphicsModule))
            {
                neverRevealed = true;
                HideLabel();
            }
        }

    }

    public class Label_HUDPart
    {
        //基础信息
        public LittleBiologist_Label owner;

        bool _revealed = false;
        public bool revealed
        {
            get { return _revealed; }
            set
            {
                _revealed = value;
                if (value)
                {
                    Vector2 pos = owner.abstractCreature.realizedCreature.mainBodyChunk.pos - LittleBiologist.currentCamera.pos;
                    pos.y += 30;

                    aimPos = pos;
                    currentPos = pos;
                    lastPos = pos;
                }
            }
        }

        //平滑信息
        public float lastAlpha = 0f;
        public float currentAlpha = 0f;
        public float aimAlpha = 0f;

        public Vector2 lastPos;
        public Vector2 currentPos;
        public Vector2 aimPos;
        //透明度
        public float BaseAlpha
        {
            get
            {
                return revealed ? 1f : 0f;
            }
        }

        //标签
        public FLabel label;
        public FSprite background;

        public Label_HUDPart(LittleBiologist_Label owner)
        {
            this.owner = owner;

            LittleBiologist_HUD.AddPartToInstance(this);    
        }

        public virtual void InitSprite()
        {
            Log("Init sprites for", owner.abstractCreature);

            if(label == null || background == null)
            {
                label = new FLabel("DisplayFont", owner.abstractCreature.ID.number.ToString()) { alpha = BaseAlpha, scale = 0.6f };
                background = new FSprite("pixel", true) { scaleX = label.text.Length * 8f, scaleY = 15f, color = Color.black };
            }

            LittleBiologist_HUD.instance.AddNode(background);
            LittleBiologist_HUD.instance.AddNode(label);

            if (owner.abstractCreature.realizedCreature != null)
            {
                if (owner.abstractCreature.realizedCreature.room != null && owner.abstractCreature.realizedCreature.room == LittleBiologist.currentCamera.room)
                {
                    owner.RevealLabel();
                }
            }
        }

        public virtual void Draw(float timeStacker)
        {
            aimAlpha = BaseAlpha;

            currentAlpha = Mathf.Lerp(lastAlpha, aimAlpha, 0.1f);
            if (currentAlpha < 0.001f)
            {
                currentAlpha = 0f;
            }
            lastAlpha = currentAlpha;

            label.alpha = currentAlpha;
            background.alpha = currentAlpha;

            if (owner.abstractCreature.realizedCreature != null && (BaseAlpha > 0f || currentAlpha > 0.001f || owner.revealed))
            {
                //pre-process
                //  提升显示效果:进入管道时即不显示
                if (owner.revealed)
                {
                    if (owner.abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Overseer)
                    {
                        Overseer overseer = owner.abstractCreature.realizedCreature as Overseer;
                        
                        if (overseer.mode == Overseer.Mode.SittingInWall || overseer.mode == Overseer.Mode.Zipping || overseer.mode == Overseer.Mode.Withdrawing)
                        {
                            if (revealed)
                            {
                                revealed = false;
                            }
                        }
                        else
                        {
                            if (!revealed)
                            {
                                revealed = true;
                            }
                        }
                    }
                    else
                    {
                        if (owner.abstractCreature.realizedCreature.inShortcut)
                        {
                            if (revealed)
                            {
                                revealed = false;
                            }
                        }
                        else
                        {
                            if (!revealed)
                            {
                                revealed = true;
                            }
                        }
                    }
                }

                //set aim
                aimPos = owner.abstractCreature.realizedCreature.mainBodyChunk.pos - LittleBiologist.currentCamera.pos;
                aimPos.y += 30f;

                //lerp
                if (revealed)
                {
                    currentPos = Vector2.Lerp(lastPos, aimPos, 0.1f);
                    lastPos = currentPos;
                }

                //set value
                background.SetPosition(currentPos);
                label.SetPosition(currentPos);
            }
            else
            {
                if (owner.neverRevealed)
                {
                    owner.Destory();
                }
            }
        }

        public virtual void Destroy()
        {
            label.isVisible = false;
            background.isVisible = false;
            LittleBiologist_HUD.instance.RemoveNode(label);
            LittleBiologist_HUD.instance.RemoveNode(background);
            LittleBiologist_HUD.instance.RemovePart(this);
        }
    }

    
}
