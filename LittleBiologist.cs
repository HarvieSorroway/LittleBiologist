using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using System.Reflection;
using UnityEngine;
using System.Collections;
using static LittleBiologist.LBio_Const;


namespace LittleBiologist
{
    [BepInPlugin("Harvie.LittleBiologist", "LittleBiologist", "2.3.0")]
    public class LittleBiologist : BaseUnityPlugin
    {
        public static LittleBiologist instance;
        public static RoomCamera currentCamera;
        public static HUD.HUD currentHUD;

        public void Start()
        {
            instance = this;

            //camera hooks
            On.RoomCamera.ctor += RoomCamera_ctor;
            On.RoomCamera.ChangeRoom += RoomCamera_ChangeRoom;

            On.HUD.HUD.InitMultiplayerHud += HUD_InitMultiplayerHud;
            On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;

            On.Creature.Update += Creature_Update;

            LBio_LabelConfig.SetupConfig();
        }

        public void Update()
        {
            LBio_CreatureLabel.GetHanging_or_MouseOver_Label();
        }
        private void Creature_Update(On.Creature.orig_Update orig, Creature self, bool eu)
        {
            orig.Invoke(self,eu);

            LBio_CreatureLabel lBio_CreatureModule = self.GetModule();
            if(lBio_CreatureModule != null)
            {
                lBio_CreatureModule.Update();
            }
        }

        private void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
        {
            orig.Invoke(self, cam);
            self.AddPart(new LBio_HUD(self));
        }

        private void HUD_InitMultiplayerHud(On.HUD.HUD.orig_InitMultiplayerHud orig, HUD.HUD self, ArenaGameSession session)
        {
            orig.Invoke(self,session);
            self.AddPart(new LBio_HUD(self));
        }
        #region camrea_patch
        //重新展现标签
        private void RoomCamera_ChangeRoom(On.RoomCamera.orig_ChangeRoom orig, RoomCamera self, Room newRoom, int cameraPosition)
        {
            try
            {
                if(newRoom.abstractRoom.world.region.name != self.room.abstractRoom.world.region.name)
                {
                    LBio_CreatureLabel.DestroyAll();
                }
            }
            catch { }
            orig.Invoke(self, newRoom, cameraPosition);
        }

        //绑定相机
        private void RoomCamera_ctor(On.RoomCamera.orig_ctor orig, RoomCamera self, RainWorldGame game, int cameraNumber)
        {
            orig.Invoke(self,game,cameraNumber);
            currentCamera = self;
        }
        #endregion
    }
}
