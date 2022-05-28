using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LittleBiologist.LBio_Navigations;

namespace LittleBiologist
{
    public static class LBio_ModulePatch
    {
        public static LBio_CreatureLabel GetModule(this Creature creature)
        {
            if (creature.InSameRoomWithCamera())
            {
                for (int i = LBio_CreatureLabel.lBio_CreatureLabels.Count - 1; i >= 0; i--)
                {
                    if (LBio_CreatureLabel.lBio_CreatureLabels[i].Creature == creature)
                    {

                        return LBio_CreatureLabel.lBio_CreatureLabels[i];
                    }
                }
                if (LBio_HUD.instance != null)
                {
                    return new LBio_CreatureLabel(creature);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public static LBio_NaviHodler GetModule(this AbstractCreature abstractCreature)
        {
            if (abstractCreature.InSameRegionWithCamera() && LBio_NaviHUD.instance != null)
            {
                for (int i = LBio_NaviHodler.allHolders.Count - 1; i >= 0; i--)
                {
                    if (LBio_NaviHodler.allHolders[i].AbCreature == abstractCreature)
                    {
                        return LBio_NaviHodler.allHolders[i];
                    }
                }
                return new LBio_NaviHodler(abstractCreature);
            }
            return null;
        }

        public static bool InSameRoomWithCamera(this Creature creature)
        {
            if (creature == null || creature.room == null)
            {
                return false;
            }
            return creature.abstractCreature.Room == creature.room.game.cameras[0].room.abstractRoom;
        }

        public static bool InSameRegionWithCamera(this AbstractCreature abstractCreature)
        {
            if(abstractCreature == null || abstractCreature.Room == null || abstractCreature.Room.world == null)
            {
                return false;
            }
            return abstractCreature.Room.world.region == abstractCreature.Room.world.game.cameras[0].room.world.region && abstractCreature.Room.entities.Contains(abstractCreature) && !abstractCreature.slatedForDeletion;
        }
    }
}
