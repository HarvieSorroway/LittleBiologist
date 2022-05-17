using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Reflection;
using static LittleBiologist.LBio_Const;

namespace LittleBiologist
{

    /// <summary>
    /// LBio_CreatureLable的基础信息部分，绘制部分位于HUD
    /// </summary>
    public partial class LBio_CreatureLabel
    {
        public static List<LBio_CreatureLabel> lBio_CreatureLabels = new List<LBio_CreatureLabel>();

        public LBio_CreatureLabel(Creature creature)
        {
            this.creature = creature;
            lBio_CreatureLabels.Add(this);
            basicName = creature.ToString();
            Log("Add Creature",creature);
            InitSprites();
        }

        
        #region 基础信息部分
        public void Update()
        {
            creaturePos = creature.mainBodyChunk.pos;
            Reveal = !creature.inShortcut;
        }

        public void Destroy()
        {
            slatedForDeletion = true;
            creature = null;
        }

        public void RealDestroy()
        {
            Log("Remove creature " + basicName);
            lBio_CreatureLabels.Remove(this);
            RemoveSprites();
        }

        public static void DestroyAll()
        {
            for(int i = lBio_CreatureLabels.Count - 1;i >= 0; i--)
            {
                lBio_CreatureLabels[i].Destroy();
            }
        }

        public static void RealDestroyAll()
        {
            for (int i = lBio_CreatureLabels.Count - 1; i >= 0; i--)
            {
                lBio_CreatureLabels[i].RealDestroy();
            }
        }

        string basicName;
        WeakReference _creature;
        public Creature creature
        {
            get
            {
                if(_creature.Target == null || !_creature.IsAlive)
                {
                    Destroy();
                    return null;
                }
                else
                {
                    Creature creature = _creature.Target as Creature;
                    if (creature.InSameRoomWithCamera())
                    {
                        return _creature.Target as Creature;
                    }
                    else
                    {
                        Destroy();
                        return null;
                    }
                }
            }
            set
            {
                _creature = new WeakReference(value);
            }
        }

        #endregion
    }
}
