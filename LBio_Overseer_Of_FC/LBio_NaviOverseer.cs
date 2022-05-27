using OverseerHolograms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LittleBiologist.LBio_Navigations;
using System.Collections;
using UnityEngine;
using static LittleBiologist.LBio_Const;



namespace LittleBiologist
{
    public class LBio_NaviOverseer
    {
        public static void Patch()
        {
            On.RainWorldGame.Update += RainWorldGame_Update;
            On.RainWorldGame.ShutDownProcess += RainWorldGame_ShutDownProcess;
            On.Overseer.TryAddHologram += Overseer_TryAddHologram;
            On.OverseerAbstractAI.HowInterestingIsCreature += OverseerAbstractAI_HowInterestingIsCreature;
            On.Overseer.Update += Overseer_Update;
            //On.OverseerAI.HoverScoreOfTile += OverseerAI_HoverScoreOfTile;
        }

        private static void Overseer_Update(On.Overseer.orig_Update orig, Overseer self, bool eu)
        {
            orig.Invoke(self,eu);
            if(guideOverseer != null && self.abstractCreature != null &&  self.abstractCreature == guideOverseer)
            {
                if(LBio_NaviHodler.selecetdHolder != null && LBio_NaviHodler.selecetdHolder.AbCreature != null)
                {
                    if(self.room != null && self.room.world != null && self.room.world.game != null && self.room.world.game.Players != null && self.room.world.game.Players.Count > 0 && self.room.world.game.Players[0] != null && self.room.world.game.Players[0].realizedCreature != null)
                    {
                        self.TryAddHologram(EnumExt_LBioOverseer.LBio_NaviHologram, self.room.world.game.Players[0].realizedCreature, float.MaxValue);
                    }
                }
            }
        }

        private static float OverseerAI_HoverScoreOfTile(On.OverseerAI.orig_HoverScoreOfTile orig, OverseerAI self, RWCustom.IntVector2 testTile)
        {
            if(self.overseer.abstractCreature == guideOverseer && self.overseer.hologram != null && self.overseer.hologram is LBio_NaviHologram)
            {
                LBio_NaviHologram hologram = self.overseer.hologram as LBio_NaviHologram;
                return -(100000f - Vector2.Distance(testTile.ToVector2(), hologram.displayTile.ToVector2()) * 1000f);
            }
            else
            {
                return orig.Invoke(self, testTile);
            }
        }

        private static float OverseerAbstractAI_HowInterestingIsCreature(On.OverseerAbstractAI.orig_HowInterestingIsCreature orig, OverseerAbstractAI self, AbstractCreature testCrit)
        {
            if (self.parent == guideOverseer && testCrit != null && testCrit.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
            {
                return 0f;
            }
            return orig.Invoke(self, testCrit);
        }

        private static void RainWorldGame_ShutDownProcess(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame self)
        {
            orig.Invoke(self);
            guideOverseer = null;
        }

        private static void RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
        {
            orig.Invoke(self);
            if(self.Players.Count > 0)
            {
                Update(self.Players[0].realizedCreature as Player);
            }
            for(int i = LBio_NaviHodler.allHolders.Count - 1;i >= 0; i--)
            {
                AbstractCreature temp = LBio_NaviHodler.allHolders[i].AbCreature;
            }
        }

        public static void Update(Player player)
        {
            if (player == null) return;

            if(LBio_NaviHodler.selecetdHolder != null && LBio_NaviHodler.selecetdHolder.AbCreature != null)
            {
                //bring NaviGuide to player
                BringOverseerToPlayer(player);
            }
            else
            {
                BringOverseerAway();
            }
        }

        public static void BringOverseerToPlayer(Player player)
        {
            if (guideOverseer != null && guideOverseer.world != player.abstractCreature.world)
            {
                guideOverseer.Destroy();
                guideOverseer = null;
            }
            if (guideOverseer != null && guideOverseer.slatedForDeletion)
            {
                guideOverseer = null;
            }
            if (guideOverseer != null && guideOverseer.state.dead)
            {
                guideOverseer.Destroy();
                guideOverseer = null;
            }

            if(guideOverseer == null)
            {
                guideOverseer = new AbstractCreature(player.abstractCreature.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Overseer), null, new WorldCoordinate(player.room.world.offScreenDen.index, -1, -1, 0), player.room.game.GetNewID());
                player.room.world.offScreenDen.AddEntity(guideOverseer);
                ((OverseerAbstractAI)guideOverseer.abstractAI).ownerIterator = 806;
            }

            if(!((guideOverseer.abstractAI) as OverseerAbstractAI).goToPlayer)
            {
                ((guideOverseer.abstractAI) as OverseerAbstractAI).goToPlayer = true;
            }
        }

        public static void BringOverseerAway()
        {
            if(guideOverseer != null && (guideOverseer.abstractAI as OverseerAbstractAI).goToPlayer)
            {
                (guideOverseer.abstractAI as OverseerAbstractAI).goToPlayer = false;
                if (guideOverseer.abstractAI.RealAI != null)
                {
                    (guideOverseer.abstractAI.RealAI as OverseerAI).scaredDistance = 150f;
                }
            }
        }
        static IEnumerator bringAwayAfterSeconds(float second)
        {
            yield return new WaitForSeconds(second);
            if(guideOverseer != null && !(LBio_NaviHodler.selecetdHolder != null && LBio_NaviHodler.selecetdHolder.AbCreature != null))
            {
                (guideOverseer.abstractAI as OverseerAbstractAI).goToPlayer = false;
                (guideOverseer.abstractAI as OverseerAbstractAI).SetDestination(new WorldCoordinate(guideOverseer.world.offScreenDen.index,-1,-1,0));
            }
        }
        private static void Overseer_TryAddHologram(On.Overseer.orig_TryAddHologram orig, Overseer self, OverseerHologram.Message message, Creature communicateWith, float importance)
        {
            if(self == null)
            {
                return;
            }
            if(self.hologram != null && self.hologram is LBio_NaviHologram)
            {
                return;
            }
            if (message != EnumExt_LBioOverseer.LBio_NaviHologram)
            {
                if((self.abstractCreature.abstractAI as OverseerAbstractAI).ownerIterator != 806)
                {
                    orig.Invoke(self, message, communicateWith, importance);
                }
                return;
            }
            Log("Add holo - block1");
            if (self.dead)
            {
                return;
            }
            Log("Add holo - block2");
            if (self.hologram != null)
            {
                if (self.hologram.message == message)
                {
                    Log("Add holo - block3");
                    return;
                }
                if (self.hologram.importance >= importance && importance != float.MaxValue)
                {
                    Log("Add holo - block4");
                    return;
                }
                Log("Add holo - block5");
                self.hologram.stillRelevant = false;
                self.hologram = null;
            }

            if(LBio_NaviHodler.selecetdHolder != null && LBio_NaviHodler.selecetdHolder.AbCreature != null && LBio_NaviHodler.selecetdHolder.AbCreature.Room != null)
            {
                Log("Add holo - block6");
                self.hologram = new LBio_NaviHologram(self, communicateWith, importance);
                Log("Add holo - block7");
                if (self.hologram != null)
                {
                    self.room.AddObject(self.hologram);
                    Log("Add holo - block8");
                }
            }
        }

        public static AbstractCreature guideOverseer;
    }
}
public static class EnumExt_LBioOverseer
{
    public static OverseerHolograms.OverseerHologram.Message LBio_NaviHologram;
}