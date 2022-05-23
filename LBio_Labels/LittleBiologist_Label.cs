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
            this.Creature = creature;
            lBio_CreatureLabels.Add(this);
            basicName = creature.ToString();
            Log("Add Creature",creature);

            InitSprites();
        }

        
        #region 基础信息部分
        public void Update()
        {
            if (!IsHanging)
            {
                creaturePos = Creature.mainBodyChunk.pos;
            }
            Reveal = !Creature.inShortcut;
            lBio_LabelPages[Indexer].UpdateText();
        }

        public void Destroy()
        {
            SlatedForDeletion = true;
            Creature = null;
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
        public Creature Creature
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
                return owner.Creature.abstractCreature.ID.ToString();
            }

            public virtual Color GetColor()
            {
                return Color.gray;
            }
            public virtual void SwitchLocalPage()
            {
                int temp = localPageIndex;
                localPageIndex++;
                if(localPageIndex > MaxLocalPageIndex)
                {
                    localPageIndex = 0;
                }
                if(temp != localPageIndex)
                {
                    owner.localTextChangeProcess = 0f;
                }
            }

            public virtual void ResetLocal()
            {
                if (!owner.IsHanging)
                {
                    localPageIndex = 0;
                }
            }

            public virtual void SetLocalPage(int value)
            {
                if(value <= MaxLocalPageIndex && value >= 0)
                {
                    localPageIndex = value;
                }
            }

            public int localPageIndex = 0;
            public virtual int MaxLocalPageIndex => 0;
        }
    }
}
