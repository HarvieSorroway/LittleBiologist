using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LittleBiologist
{
    public static class LBio_ModulePatch
    {
        public static bool InSameRoomWithCamera(this Creature creature)
        {
            if (creature == null || creature.room == null)
            {
                return false;
            }
            return creature.abstractCreature.Room == creature.room.game.cameras[0].room.abstractRoom;
        }
    }
}
