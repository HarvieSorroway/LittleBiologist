using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static LittleBiologist.LBio_Const;


namespace LittleBiologist.LBio_Tools
{
    public static class LBio_Safari
    {
        public static bool followPlayer = true;
        static Coroutine currentCoroutine;
        static bool loadFinished = false;

        public static void Patch()
        {
            On.Player.Die += Player_Die;
            On.Creature.Violence += Creature_Violence;
            On.Creature.Grab += Creature_Grab;
        }

        private static bool Creature_Grab(On.Creature.orig_Grab orig, Creature self, PhysicalObject obj, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
        {
            if(!followPlayer)
            {
                return false;
            }
            return orig.Invoke(self, obj, graspUsed, chunkGrabbed, shareability, dominance, overrideEquallyDominant, pacifying);
        }

        private static void Creature_Violence(On.Creature.orig_Violence orig, Creature self, BodyChunk source, UnityEngine.Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
        {
            if(hitChunk.owner is Player)
            {
                if(!followPlayer)
                {
                    return;
                }
            }
            orig.Invoke(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
        }

        private static void Player_Die(On.Player.orig_Die orig, Player self)
        {
            if(self.room.game.cameras[0].followAbstractCreature == self.abstractCreature)
            {
                followPlayer = true;
                orig.Invoke(self);
            }
            else
            {
                followPlayer = false;
            }
        }


        public static void ChangeCameraFollow(AbstractCreature abstractCreature)
        {
            RoomCamera roomCamera = abstractCreature.world.game.cameras[0];

            if(abstractCreature.Room.realizedRoom == null)
            {
                abstractCreature.Room.RealizeRoom(abstractCreature.world,abstractCreature.world.game);
            }

            followPlayer = abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat;

            if(currentCoroutine != null)
            {
                Log("Interupt moving camera");
                LittleBiologist.instance.StopCoroutine(currentCoroutine);
            }

            currentCoroutine = LittleBiologist.instance.StartCoroutine(WaitForCreatureRealizeAndMove(abstractCreature, roomCamera));
        }

        static IEnumerator WaitForCreatureRealizeAndMove(AbstractCreature abstractCreature,RoomCamera roomCamera)
        {
            loadFinished = false;
            while(abstractCreature.realizedCreature == null)
            {
                yield return new WaitForSeconds(0.1f);
            }

            roomCamera.followAbstractCreature = abstractCreature;

            if(roomCamera.room != abstractCreature.Room.realizedRoom)
            {
                roomCamera.MoveCamera(abstractCreature.Room.realizedRoom, 0);
            }

            currentCoroutine = null;
            loadFinished = true;
        }
    }
}
