using OverseerHolograms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LittleBiologist.LBio_Navigations;
using System.Collections;
using UnityEngine;

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
        }

        public static void Update(Player player)
        {
            if (player == null) return;

            if(LBio_NaviHodler.selecetdHolder != null && LBio_NaviHodler.selecetdHolder.AbCreature != null)
            {
                //bring NaviGuide to player
                BringOverseerToPlayer(player);

                if(guideOverseer.realizedCreature != null)
                {
                    Overseer guide = guideOverseer.realizedCreature as Overseer;
                    if(!(guide.hologram is LBio_NaviHologram))
                    {
                        guide.TryAddHologram(EnumExt_LBioOverseer.LBio_NaviHologram, player, float.MaxValue);
                    }
                }
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

            if(guideOverseer.abstractAI.denPosition != player.coord.WashNode())
            {
                guideOverseer.abstractAI.SetDestination(player.coord.WashNode());
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
            if (message != EnumExt_LBioOverseer.LBio_NaviHologram)
            {
                orig.Invoke(self, message, communicateWith, importance);
                return;
            }
            if (self.dead)
            {
                return;
            }
            if (self.hologram != null)
            {
                if (self.hologram.message == message)
                {
                    return;
                }
                if (self.hologram.importance >= importance && importance != float.MaxValue)
                {
                    return;
                }
                self.hologram.stillRelevant = false;
                self.hologram = null;
            }

            if(LBio_NaviHodler.selecetdHolder != null && LBio_NaviHodler.selecetdHolder.AbCreature != null)
            {
                try
                {
                    self.hologram = new LBio_NaviHologram(self, communicateWith, importance);
                    self.room.AddObject(self.hologram);
                }
                catch(Exception e)
                {
                    Debug.LogError(e);
                    Debug.LogError("player : " + (communicateWith != null));
                }
            }
            else
            {
                orig.Invoke(self, OverseerHologram.Message.None, communicateWith, importance);
            }
        }

        public static AbstractCreature guideOverseer;
    }
}
public static class EnumExt_LBioOverseer
{
    public static OverseerHolograms.OverseerHologram.Message LBio_NaviHologram;
}