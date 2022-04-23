using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using System.Reflection;
using UnityEngine;
using System.Collections;
using static LittleBiologist.LittleBiologist_Const;


namespace LittleBiologist
{
    [BepInPlugin("LittleBiologist3", "LittleBiologist3","1.0.0")]
    public class LittleBiologist : BaseUnityPlugin
    {
        public static LittleBiologist instance;
        public static RoomCamera currentCamera;

        public List<Creature> creatures;

        public Creature this[EntityID entityID]
        {
            get
            {
                foreach(Creature creature in creatures)
                {
                    if(creature.abstractCreature.ID == entityID)
                    {
                        return creature;
                    }
                }
                return null;
            }
        }
        public Creature this[int index]
        {
            get
            {
                if(index > creatures.Count-1)
                {
                    return null;
                }
                else
                {
                    return creatures[index];
                }
            }
            set
            {
                if(index > creatures.Count-1)
                {
                    creatures.Add(value);
                }
                else
                {
                    creatures[index] = value;
                }
            }
        }

        public OnUpdate update;

        public void Start()
        {
            instance = this;
            //On.Creature.ctor += Creature_ctor;
            On.AbstractCreature.ctor += AbstractCreature_ctor;
            //camera hooks
            On.RoomCamera.ctor += RoomCamera_ctor;
            On.RoomCamera.MoveCamera_Room_int += RoomCamera_MoveCamera_Room_int;
            On.RoomCamera.ChangeRoom += RoomCamera_ChangeRoom;
        }

        private void AbstractCreature_ctor(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
        {
            orig.Invoke(self, world, creatureTemplate, realizedCreature, pos, ID);
            StartCoroutine(AddLabel(self));
        }

        #region camrea_patch
        private void RoomCamera_ChangeRoom(On.RoomCamera.orig_ChangeRoom orig, RoomCamera self, Room newRoom, int cameraPosition)
        {
            //LittleBiologist_Label.ClearAllLabel();
            orig.Invoke(self, newRoom, cameraPosition);
            foreach(var label in LittleBiologist_Label.labels)
            {
                label.RegetLabel();
            }
        }
        private void RoomCamera_MoveCamera_Room_int(On.RoomCamera.orig_MoveCamera_Room_int orig, RoomCamera self, Room newRoom, int camPos)
        {
            //LittleBiologist_Label.ClearAllLabel();
            orig.Invoke(self, newRoom, camPos);
        }

        private void RoomCamera_ctor(On.RoomCamera.orig_ctor orig, RoomCamera self, RainWorldGame game, int cameraNumber)
        {
            orig.Invoke(self,game,cameraNumber);
            currentCamera = self;
        }
        #endregion

        public void Update()
        {
            update?.Invoke();
            if (Input.GetMouseButtonDown(2))
            {
                for(int i = 0;i < LittleBiologist_Label.labels.Count; i++)
                {
                    Log(LittleBiologist_Label.labels[i].abstractCreature);
                }
            }
        }
        
        public IEnumerator AddLabel(AbstractCreature abstractCreature)
        {
            yield return new WaitForFixedUpdate();
            Log("Try to add label for", abstractCreature);

            bool contains = false;
            foreach(var label in LittleBiologist_Label.labels)
            {
                if(abstractCreature == label.abstractCreature)
                {
                    contains = true;
                }
            }
            if (!contains)
            {
                LittleBiologist_Label.labels.Add(new LittleBiologist_Label(abstractCreature));
                LittleBiologist_Const.Log("All labels:", LittleBiologist_Label.labels.Count);
            }
            else
            {
                Log(abstractCreature, "already exist");
            }

        }

        public IEnumerator LastRegetAll()
        {
            yield return new WaitForFixedUpdate();
        }

        public delegate void OnUpdate();
    }
}
