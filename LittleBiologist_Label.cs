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
                if(_lastRegion != value)
                {
                    _lastRegion = value;

                    if(labels.Count > 0)
                    {
                        for(int i = labels.Count - 1;i >= 0;i--)
                        {
                            labels[i].Destory();
                        }
                    }
                }
            }
        }
        public static List<LittleBiologist_Label> labels = new List<LittleBiologist_Label>();

        //基础信息
        public Creature creature;
        public AbstractCreature abstractCreature;

        //标签信息
        public InfoLabel infoLabel;
        public bool revealed = false;   

        public LittleBiologist_Label(AbstractCreature abstractCreature)
        {
            this.abstractCreature = abstractCreature;

            Patch();

            if (abstractCreature.realizedCreature != null)
            {
                if(abstractCreature.realizedCreature.room != null && abstractCreature.realizedCreature.room == LittleBiologist.currentCamera.room)
                {
                    RevealLabel();
                }
            }
        }

        public void RegetLabel()
        {
            if(abstractCreature.realizedCreature != null && abstractCreature.realizedCreature.graphicsModule != null && !abstractCreature.realizedCreature.inShortcut)
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

        public void RemoveLabel()
        {
            Log("Remove Label for", abstractCreature);
        }
        public void Destory()
        {
            DisPatch();
            labels.Remove(this);
        }

        void DisPatch()
        {
        }

        void Patch()
        {
            On.Room.AddObject += Room_AddObject;
        }

        private void Room_AddObject(On.Room.orig_AddObject orig, Room self, UpdatableAndDeletable obj)
        {
            orig.Invoke(self, obj);

            if(obj is Creature)
            {
                if((obj as Creature).abstractCreature == abstractCreature)
                {
                    if (self != LittleBiologist.currentCamera.room)
                    {
                        HideLabel();
                    }
                    else
                    {
                        RevealLabel();
                    }
                }
            }
        }
    }

    public class InfoLabel : CosmeticSprite
    {
        //基础信息
        public LittleBiologist_Label owner;

        public FLabel label;
        public RoomCamera.SpriteLeaser leaser;
        public RoomCamera camera;

        public List<FSprite> sprites = new List<FSprite>();
        public bool changeSprites = false;

        //基础字段
        Vector2 _pos;
        public Vector2 pos
        {
            get { return _pos; }
            set
            {
                _pos = value;
            }
        }

        string _text;
        public string text
        {
            get
            {
                return _text;
            }
            set
            {
                if(value != _text)
                {
                    _text = value;
                    if(label != null)
                    {
                        label.text = value;
                    }
                }
            }
        }

        public InfoLabel(LittleBiologist_Label owner, string text)
        {
            this.owner = owner;
            this.text = text;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            leaser = sLeaser;
            camera = rCam;
            
            label = new FLabel("DisplayFont", text) { scale = 0.6f};

            leaser.sprites = sprites.ToArray();
            AddToContainer(leaser, rCam,null);
        }
        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            
            base.AddToContainer(leaser, rCam, null);
            label.RemoveFromContainer();
            rCam.ReturnFContainer("HUD").AddChild(label);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(leaser, rCam, timeStacker, camPos);
        }

        public override void Update(bool eu)
        {
            if (changeSprites)
            {
                changeSprites = false;
                leaser.sprites = sprites.ToArray();
            }

            label.SetPosition(owner.creature.mainBodyChunk.pos - camera.pos);

        }
        public override void Destroy()
        {
            owner.infoLabel = null;
            label.RemoveFromContainer();
            RemoveFromRoom();

            base.Destroy();
        }
    }
}
