using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static LittleBiologist.LBio_Const;

namespace LittleBiologist
{
    public static class LBio_Patch
    { 
        public static LBio_CreatureLabel GetModule(this Creature creature)
        {
            
            if (creature.InSameRoomWithCamera())
            {
                for(int i = LBio_CreatureLabel.lBio_CreatureLabels.Count - 1;i >= 0; i--)
                {
                    if(LBio_CreatureLabel.lBio_CreatureLabels[i].creature == creature)
                    {
                        
                        return LBio_CreatureLabel.lBio_CreatureLabels[i];
                    }
                }
                if(LBio_HUD.instance != null)
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
    }
}
