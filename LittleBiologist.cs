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
    [BepInPlugin("DoveVisibleID", "DoveVisibleID", "1.0.0")]
    public class LittleBiologist : BaseUnityPlugin
    {
        public static LittleBiologist instance;
        public static RoomCamera currentCamera;
        public static HUD.HUD currentHUD;


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

            On.AbstractCreature.ctor += AbstractCreature_ctor;

            //camera hooks
            On.RoomCamera.ctor += RoomCamera_ctor;
            On.RoomCamera.ChangeRoom += RoomCamera_ChangeRoom;

            On.HUD.HUD.ctor += HUD_ctor;
            //On.Player.ctor += Player_ctor;
        }

        private void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig.Invoke(self, abstractCreature, world);

            if(currentHUD != null)
            {
                new LittleBiologist_HUD(currentHUD);
            }
        }

        //用于添加HUD管理类
        private void HUD_ctor(On.HUD.HUD.orig_ctor orig, HUD.HUD self, FContainer[] fContainers, RainWorld rainWorld, HUD.IOwnAHUD owner)
        {
            orig.Invoke(self,fContainers,rainWorld,owner);
            Log("Init HUD", owner,rainWorld);

            if(owner != null)
            {
                if(owner.GetOwnerType() == HUD.HUD.OwnerType.Player || owner.GetOwnerType() == HUD.HUD.OwnerType.ArenaSession)
                {
                    if(LittleBiologist_HUD.instance != null)
                    {
                        if(owner.GetOwnerType() == HUD.HUD.OwnerType.Player)
                        {
                            LittleBiologist_HUD.instance.Destroy(true);
                        }
                        else
                        {
                            LittleBiologist_HUD.instance.Destroy();
                        }
                    }

                    if(owner.GetOwnerType() == HUD.HUD.OwnerType.ArenaSession)
                    {
                        foreach(var creature in currentCamera.room.abstractRoom.creatures)
                        {
                            StartCoroutine(AddLabel(creature));
                        }
                    }
                    
                    new LittleBiologist_HUD(self);

                    return;
                }
            }
            if (LittleBiologist_HUD.instance != null)
            {
                LittleBiologist_HUD.instance.Destroy();
                LittleBiologist_HUD.instance = null;
            }
        }

        //创建label
        private void AbstractCreature_ctor(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
        {
            orig.Invoke(self, world, creatureTemplate, realizedCreature, pos, ID);
            StartCoroutine(AddLabel(self));
        }

        #region camrea_patch
        //重新展现标签
        private void RoomCamera_ChangeRoom(On.RoomCamera.orig_ChangeRoom orig, RoomCamera self, Room newRoom, int cameraPosition)
        {
            //LittleBiologist_Label.ClearAllLabel();
            orig.Invoke(self, newRoom, cameraPosition);
            if(LittleBiologist_Label.labels.Count > 0)
            {
                foreach (var label in LittleBiologist_Label.labels)
                {
                    label.RegetLabel();
                }
            }
        }

        //绑定相机
        private void RoomCamera_ctor(On.RoomCamera.orig_ctor orig, RoomCamera self, RainWorldGame game, int cameraNumber)
        {
            orig.Invoke(self,game,cameraNumber);
            currentCamera = self;
            Log("Init Camera");
        }
        #endregion

        public void Update()
        {
            update?.Invoke();
        }
        

        //延迟添加标签
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
                Log("All labels:", LittleBiologist_Label.labels.Count);
            }
            else
            {
                Log(abstractCreature, "already exist");
            }

        }

        public IEnumerator Late_AddHUD()
        {
            yield return new WaitForFixedUpdate();
            if (LittleBiologist_HUD.instance != null)
            {
                LittleBiologist_HUD.instance.slatedForDeletion = true;
                LittleBiologist_HUD.instance = null;
            }
        }

        public delegate void OnUpdate();
    }
}
