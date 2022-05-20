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
            if (!isHanging)
            {
                creaturePos = creature.mainBodyChunk.pos;
            }
            Reveal = !creature.inShortcut;
            lBio_LabelPages[indexer].UpdateText();
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
            LBio_InfoMemories.Clear();
        }

        internal string basicName;
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

        public class LBio_LabelPage
        {
            public LBio_LabelPage(LBio_CreatureLabel owner)
            {
                this.owner = owner;
            }

            public LBio_CreatureLabel owner;

            public virtual void UpdateText()
            {

            }

            public virtual string GetText()
            {
                return owner.creature.abstractCreature.ID.number.ToString();
            }

            public virtual Color GetColor()
            {
                return Color.gray;
            }
            public virtual void SwitchLocalPage()
            {
                int temp = localPageIndex;
                localPageIndex++;
                if(localPageIndex > maxLocalPageIndex)
                {
                    localPageIndex = 0;
                }
                if(temp != localPageIndex)
                {
                    owner.localTextChangeProcess = 0f;
                }
            }

            public virtual void SetLocalPage(int value)
            {
                if(value <= maxLocalPageIndex && value >= 0)
                {
                    localPageIndex = value;
                }
            }

            public int localPageIndex = 0;
            public virtual int maxLocalPageIndex => 0;
        }
    }
}
