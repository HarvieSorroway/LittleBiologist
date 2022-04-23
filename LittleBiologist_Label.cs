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
                            if(labels[i].region != value)
                            {
                                labels[i].Destory();
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
        public bool revealed = false;   

        public LittleBiologist_Label(AbstractCreature abstractCreature)
        {
            this.abstractCreature = abstractCreature;
            lastRegion = abstractCreature.Room.world.region;
            region = lastRegion;
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
                        RevealLabel();
                    }
                }
            }
        }

        public IEnumerator Late_Creature_Die(GraphicsModule graphicsModule)
        {
            for(int i = 0;i < 3; i++)
            {
                yield return new WaitForFixedUpdate();
            }
            
            if (!LittleBiologist.currentCamera.room.drawableObjects.Contains(graphicsModule))
            {
                HideLabel();
            }
        }
    }
}
