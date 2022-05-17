using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HUD;
using UnityEngine;
using static LittleBiologist.LBio_Const;
using Random = UnityEngine.Random;

namespace LittleBiologist
{
    public class LBio_HUD : HudPart
    {
        public static LBio_HUD instance;

        public LBio_HUD(HUD.HUD hud) : base(hud)
        {
            baseColor = new Color(Random.value, Random.value, Random.value);
            instance = this;
        }

        public override void Draw(float timeStacker)
        {
            base.Draw(timeStacker);
            for(int i = lBio_CreatureLabels.Count - 1;i >= 0; i--)
            {
                lBio_CreatureLabels[i].Draw();
            }
        }

        public override void ClearSprites()
        {
            instance = null;

            LBio_CreatureLabel.RealDestroyAll();

            base.ClearSprites();
        }

        public static void AddNodeToContainer(FNode fNode,int container)
        {
            if(instance != null)
            {
                fNode.RemoveFromContainer();
                instance.hud.fContainers[container].AddChild(fNode);
            }
        }
        public static void RemoveNodeFromContainer(FNode fNode)
        {
            if(instance != null)
            {
                fNode.isVisible = false;
                fNode.RemoveFromContainer();
            }
        }


        public static Color baseColor = new Color(1,1,1);
        List<LBio_CreatureLabel> lBio_CreatureLabels => LBio_CreatureLabel.lBio_CreatureLabels;
    }

    public partial class LBio_CreatureLabel
    {
        /// <summary>
        /// 初始化精灵
        /// </summary>
        public void InitSprites()
        {
            string text = lBio_CreatureLabels.IndexOf(this).ToString() + " - " + creature.abstractCreature.ID.number.ToString();
            fSprite = new FSprite("pixel", true) { scaleX = text.Length * 7f, scaleY = 10f, color = Color.black };
            fLabel = new FLabel("font", text) { color = LBio_HUD.baseColor };

            LBio_HUD.AddNodeToContainer(fSprite, 0);
            LBio_HUD.AddNodeToContainer(fLabel, 0);

            creaturePos = creature.mainBodyChunk.pos;
            lastCreaturePos = creaturePos;
            smoothCreaturePos = creaturePos;
        }

        /// <summary>
        /// 绘制，由LBio_HUDPart调用
        /// </summary>
        public void Draw()
        {
            if (rCam != null)
            {
                //smooth caculate
                smoothCreaturePos = Vector2.Lerp(lastCreaturePos, creaturePos, 0.1f);
                lastCreaturePos = smoothCreaturePos;
                smoothAlpha = Mathf.Lerp(lastAlpha, alpha, 0.1f);
                lastAlpha = smoothAlpha;

                fLabel.SetPosition(smoothCreaturePos - rCam.pos + Vector2.up * 30);
                fLabel.text = lBio_CreatureLabels.IndexOf(this).ToString() + " - " + creature.abstractCreature.ID.number.ToString();
                fSprite.scaleX = fLabel.textRect.x;
                fSprite.scaleY = fLabel.textRect.y;
                fSprite.SetPosition(smoothCreaturePos - rCam.pos + Vector2.up * 30);

                fLabel.alpha = smoothAlpha;
                fSprite.alpha = smoothAlpha;
            }
            else if (slatedForDeletion)
            {
                smoothAlpha = Mathf.Lerp(lastAlpha, alpha, 0.1f);
                lastAlpha = smoothAlpha;

                fLabel.alpha = smoothAlpha;
                fSprite.alpha = smoothAlpha;

                if (DeletionProcessComplete)
                {
                    RealDestroy();
                }
            }
        }

        /// <summary>
        /// 标签删除时调用，移除容器中的精灵
        /// </summary>
        public void RemoveSprites()
        {
            LBio_HUD.RemoveNodeFromContainer(fLabel);
            LBio_HUD.RemoveNodeFromContainer(fSprite);
        }

        //生物的平滑位置坐标
        public Vector2 creaturePos = new Vector2(0, 0);
        Vector2 lastCreaturePos = new Vector2(0, 0);
        Vector2 smoothCreaturePos = new Vector2(0, 0);

        //总透明度
        public float alpha = 1;
        float lastAlpha = 0f;
        float smoothAlpha = 0f;


        bool _slatedForDeletion = false;
        /// <summary>
        /// 设置标签状态为删除
        /// </summary>
        public bool slatedForDeletion
        {
            get => _slatedForDeletion;
            set 
            {
                if (value)
                {
                    _slatedForDeletion = true;
                    alpha = 0;
                }
            }
        }

        //当这个值为True时才会真正的移除标签
        bool DeletionProcessComplete => smoothAlpha < 0.01f && slatedForDeletion;

        public bool Reveal
        {
            get
            {
                return alpha == 1;
            }
            set
            {
                if(value != Reveal)
                {
                    alpha = value ? 1 : 0;
                }
            }
        }

        //基础变量
        public RoomCamera rCam
        {
            get
            {
                if (creature.InSameRoomWithCamera())
                {
                    return creature.room.game.cameras[0];
                }
                else
                {
                    return null;
                }
            }
        }
        FLabel fLabel;
        FSprite fSprite;
    }
}
