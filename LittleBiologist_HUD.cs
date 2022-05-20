using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HUD;
using RWCustom;
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
            connectLine = new CustomFSprite("Futile_White");
            background = new FSprite("pixel", true) { scaleX = text.Length * 7f, scaleY = 10f, color = Color.black };

            IconSymbol.IconSymbolData iconSymbol = new IconSymbol.IconSymbolData(creature.abstractCreature.creatureTemplate.type, creature.abstractCreature.type, 0);
            icon = new FSprite(CreatureSymbol.SpriteNameOfCreature(iconSymbol), true) { color = CreatureSymbol.ColorOfCreature(iconSymbol)};
            iconFlash = new FSprite("Futile_White", true) { shader = rCam.game.rainWorld.Shaders["FlatLight"], color = CreatureSymbol.ColorOfCreature(iconSymbol), alpha = 0.6f, scale = 3 };

            LBio_HUD.AddNodeToContainer(connectLine, 0);
            LBio_HUD.AddNodeToContainer(background , 0);
            LBio_HUD.AddNodeToContainer(iconFlash  , 0);
            LBio_HUD.AddNodeToContainer(icon       , 0);

            for (int i = 0; i < fLabel.Length; i++)
            {
                fLabel[i] = new FLabel("font", text + " - " + i.ToString()) {alpha = 0f, scaleX = 0f };
                LBio_HUD.AddNodeToContainer(fLabel[i], 0);
            }

            InitLabelPages();

            if (LBio_InfoMemories.ContainsKey(basicName))
            {
                LBio_InfoMemory lBio_InfoMemory = LBio_InfoMemories[basicName];
                Log("Get LBio_InfoMemory for " + basicName, lBio_InfoMemory.screenPos, lBio_InfoMemory.isShowing, lBio_InfoMemory.isHanging, lBio_InfoMemory.indexer);

                isHanging = lBio_InfoMemory.isHanging;

                if (isHanging)
                {
                    ScreenPos = lBio_InfoMemory.screenPos;
                }
                else
                {
                    creaturePos = creature.mainBodyChunk.pos;
                }
                lastCreaturePos = creaturePos;
                smoothCreaturePos = creaturePos;

                alpha = isShowing ? 1 : 0;
                lastAlpha = alpha;
                smoothAlpha = alpha;

                if (isHanging)
                {
                    isShowing = lBio_InfoMemory.isShowing;
                    indexer = lBio_InfoMemory.indexer;
                    smoothInderxer = indexer;
                    lastInderxer = indexer;
                }

                lBio_LabelPages[indexer].SetLocalPage(lBio_InfoMemory.localPageIndex);
            }
            else
            {
                creaturePos = creature.mainBodyChunk.pos;
                lastCreaturePos = creaturePos;
                smoothCreaturePos = creaturePos;

                LBio_InfoMemories.Add(basicName, new LBio_InfoMemory());
            }

            lastBackgroundColor = lBio_LabelPages[indexer].GetColor();
            smoothBackgroundColor = lBio_LabelPages[indexer].GetColor();
        }

        public void InitLabelPages()
        {
            lBio_LabelPages.Add(new LBioPage_ShowID(this));
            lBio_LabelPages.Add(new LBioPage_ShowPersonality(this));
        }
        /// <summary>
        /// 绘制，由LBio_HUDPart调用
        /// </summary>
        public void Draw()
        {
            if (rCam != null || slatedForDeletion)
            {
                CollisionDetection();

                //smooth caculate
                smoothCreaturePos = Vector2.Lerp(lastCreaturePos, creaturePos + (isHanging ? Vector2.zero : pushForce), 0.1f);
                lastCreaturePos = smoothCreaturePos;
                pushForce = Vector2.Lerp(pushForce, Vector2.zero, 0.1f);

                smoothAlpha = Mathf.Lerp(lastAlpha, alpha, 0.1f);
                lastAlpha = smoothAlpha;
                
                smoothInderxer = Mathf.Lerp(lastInderxer, (float)indexer, 0.1f);
                lastInderxer = smoothInderxer;

                smoothSize = Vector2.Lerp(lastSize, size, 0.1f);
                lastSize = smoothSize;

                smoothShowingZoom = Mathf.Lerp(lastShowingZoom, showingZoom, 0.1f);
                lastShowingZoom = smoothShowingZoom;

                smoothAlpha *= smoothShowingZoom;
                size = fLabel[Mathf.RoundToInt(smoothInderxer)].textRect.size + Vector2.one * 10f * zoom;

                smoothPlayerRelativeVector = Vector2.Lerp(lastPlayerRelativeVector, playerRelativeVector, 0.1f);
                lastPlayerRelativeVector = smoothPlayerRelativeVector;

                //get background color
                background.scaleX = (smoothSize.x + icon.localRect.size.x + 2f) * smoothShowingZoom;
                background.scaleY = (smoothSize.y + icon.localRect.size.y + 2f) * smoothShowingZoom;

                Color aimColor = Color.Lerp(Color.gray,lBio_LabelPages[indexer].GetColor(),0.4f);
                aimColor.a = aimColor.a * smoothAlpha * smoothShowingZoom;

                if (rCam != null)
                {
                    Color reverseCol = rCam.PixelColorAtCoordinate(background.GetPosition() + rCam.pos);
                    reverseCol.a = aimColor.a;

                    reverseCol = Color.Lerp(reverseCol, new Color(1 - reverseCol.r, 1 - reverseCol.g, 1 - reverseCol.b, aimColor.a), reverseCol.grayscale * reverseCol.grayscale);
                    aimColor = Color.Lerp(aimColor, reverseCol, 0.65f);
                };
                
                smoothBackgroundColor = Color.Lerp(lastBackgroundColor, aimColor, 0.1f);
                lastBackgroundColor = smoothBackgroundColor;

                icon.scale = smoothShowingZoom;
                iconFlash.scale = icon.localRect.size.x / 5 * smoothShowingZoom;

                //背景更新
                if (!slatedForDeletion)
                {
                    localTextChangeProcess += 1 / 30f;
                    localTextChangeProcess = Mathf.Clamp(localTextChangeProcess, 0, 0.5f);

                    if (!isHanging)
                    {
                        playerRelativeVector = Vector2.zero;
                        if (rCam.room.game.Players.Count > 0)
                        {
                            foreach (var player in rCam.room.game.Players)
                            {
                                if (player.realizedCreature != null && player.Room == rCam.room.abstractRoom && player.realizedCreature != creature)
                                {
                                    playerRelativeVector += (smoothCreaturePos - player.realizedCreature.mainBodyChunk.pos).normalized;
                                }
                                else
                                {
                                    playerRelativeVector = Vector2.up;
                                }
                            }
                            playerRelativeVector = playerRelativeVector.normalized;
                        }
                        else
                        {
                            playerRelativeVector = Vector2.up;
                        }

                        float length = Mathf.Max(background.scaleX, background.scaleY) + 30f;
                        playerRelativeVector *= length * smoothShowingZoom;
                    }

                    background.SetPosition(ScreenPos + Vector2.up * ((icon.localRect.size.y + 2f) / 2) * smoothShowingZoom + Vector2.right * (-(icon.localRect.size.x + 2f) / 2) * smoothShowingZoom + smoothPlayerRelativeVector);
                    icon.SetPosition(new Vector2(background.x - background.scaleX / 2f + icon.localRect.size.x / 2f + 2f, background.y + background.scaleY / 2f - icon.localRect.size.y / 2 - 2f));
                    iconFlash.SetPosition(new Vector2(icon.x + 1f, icon.y - 1.5f));

                    //Caculate lineConnection
                    Vector2 temp = (creature.mainBodyChunk.pos - rCam.pos - new Vector2(background.x, background.y)).normalized;
                    Vector2 perpendicularTemp = Custom.PerpendicularVector(temp) * 2f * smoothShowingZoom;


                    connectLine.MoveVertice(0, Vector2.Lerp(creature.mainBodyChunk.lastPos, creature.mainBodyChunk.pos, 0.5f) - rCam.pos - perpendicularTemp);
                    connectLine.MoveVertice(1, Vector2.Lerp(creature.mainBodyChunk.lastPos, creature.mainBodyChunk.pos, 0.5f) - rCam.pos + perpendicularTemp);
                    connectLine.MoveVertice(2, background.GetPosition() - perpendicularTemp);
                    connectLine.MoveVertice(3, background.GetPosition() + perpendicularTemp);

                    if (LBio_InfoMemories.ContainsKey(basicName))
                    {
                        LBio_InfoMemories[basicName].indexer = indexer;
                        LBio_InfoMemories[basicName].isHanging = isHanging;
                        LBio_InfoMemories[basicName].isShowing = isShowing;
                        LBio_InfoMemories[basicName].screenPos = ScreenPos + smoothPlayerRelativeVector;
                        LBio_InfoMemories[basicName].localPageIndex = lBio_LabelPages[indexer].localPageIndex;
                    }
                }

                background.alpha = smoothAlpha;
                icon.alpha = smoothAlpha;
                iconFlash.alpha = 0.6f * smoothAlpha;
                connectLine.alpha = smoothAlpha;

                background.color = smoothBackgroundColor;

                connectLine.verticeColors[0] = new Color(smoothBackgroundColor.r, smoothBackgroundColor.g, smoothBackgroundColor.b, 0);
                connectLine.verticeColors[1] = new Color(smoothBackgroundColor.r, smoothBackgroundColor.g, smoothBackgroundColor.b, 0);

                connectLine.verticeColors[2] = smoothBackgroundColor;
                connectLine.verticeColors[3] = smoothBackgroundColor;

                if (flashing == 2)
                {
                    iconFlash.isVisible = false;
                    flashing--;
                }
                else if(flashing == 1)
                {
                    iconFlash.isVisible = true;
                    flashing--;
                }



                for (int i = 0; i < fLabel.Length; i++)
                {
                    if (Mathf.Abs(i - smoothInderxer) < 1f)
                    {
                        if (!slatedForDeletion && i == indexer && localTextChangeProcess > 0.25f)
                        {
                            fLabel[i].text = lBio_LabelPages[indexer].GetText();
                        }

                        float scaleCoeffcient = Mathf.Sin(Mathf.Lerp(0f, Mathf.PI, smoothInderxer - i + 0.5f));
                        float posCoefficient = Mathf.Cos(Mathf.Lerp(0f, Mathf.PI, smoothInderxer - i + 0.5f));

                        fLabel[i].scaleX = scaleCoeffcient * smoothShowingZoom;
                        fLabel[i].scaleY = smoothShowingZoom;
                        fLabel[i].alpha = smoothAlpha * localTextChangeAlpha * smoothShowingZoom;

                        if (!slatedForDeletion)
                        {
                            fLabel[i].SetPosition(ScreenPos + Vector2.right * (size.x / 2) * posCoefficient * smoothShowingZoom + smoothPlayerRelativeVector);
                        }
                    }
                }

                if (DeletionProcessComplete)
                {
                    RealDestroy();
                }
            }
        }

        /// <summary>
        /// 切换标签页
        /// </summary>
        /// <param name="index">需要切换的序号，-1表示显示开关</param>
        public void SwitchPages(int index)
        {
            if (isHanging && (currentMouseOverLabel != this))
            {
                return;
            }
            if(currentMouseOverLabel != null && currentMouseOverLabel != this)
            {
                return;
            }
            if(index == -1)
            {
                isShowing = !isShowing;
            }
            else
            {
                if(index == indexer)
                {
                    lBio_LabelPages[indexer].SwitchLocalPage();
                }
                indexer = index;
            }
            Flash();
          }

        /// <summary>
        /// 标签灯闪烁
        /// </summary>
        public void Flash()
        {
            if (flashing == 0)
            {
                flashing = 2;
            }
        }
        /// <summary>
        /// 碰撞处理
        /// </summary>
        public void CollisionDetection()
        {
            foreach(var otherLabel in lBio_CreatureLabels)
            {
                if(otherLabel != this && Vector2.Distance(otherLabel.smoothCreaturePos, smoothCreaturePos) < (otherLabel.background.scaleX + otherLabel.background.scaleY + background.scaleX + background.scaleY))
                {
                    Vector2 forceDir = (smoothCreaturePos - otherLabel.smoothCreaturePos).normalized;
                    float coef = 100 / (smoothCreaturePos - otherLabel.smoothCreaturePos).magnitude;
                    Vector2 force = forceDir * Mathf.Clamp(coef * coef, 0f, 500f);
                    pushForce += force;
                }
            }
        }
        /// <summary>
        /// 标签删除时调用，移除容器中的精灵
        /// </summary>
        public void RemoveSprites()
        {
            for (int i = 0; i < fLabel.Length; i++)
            {
                LBio_HUD.RemoveNodeFromContainer(fLabel[i]);
            }
            LBio_HUD.RemoveNodeFromContainer(background);
            LBio_HUD.RemoveNodeFromContainer(icon);
            LBio_HUD.RemoveNodeFromContainer(iconFlash);
            LBio_HUD.RemoveNodeFromContainer(connectLine);

        }

        List<LBio_LabelPage> lBio_LabelPages = new List<LBio_LabelPage>();


        //生物的平滑位置坐标
        public Vector2 creaturePos = new Vector2(0, 0);
        Vector2 lastCreaturePos = new Vector2(0, 0);
        Vector2 smoothCreaturePos = new Vector2(0, 0);

        Vector2 _ScreenPos = new Vector2(0, 0);
        Vector2 ScreenPos
        {
            get => isHanging ? _ScreenPos : smoothCreaturePos - rCam.pos;
            set
            {
                _ScreenPos = value;

                if (isHanging)
                {
                    creaturePos = _ScreenPos + rCam.pos;
                }
            }
        }

        //总透明度
        public float alpha = 1;
        float lastAlpha = 0f;
        float smoothAlpha = 0f;

        Color lastBackgroundColor = Color.black;
        Color smoothBackgroundColor = Color.black;

        //标签序号
        int _indexer = totalIndexer;
        public int indexer
        {
            get => _indexer;
            set
            {
                _indexer = (int)Mathf.Clamp(value, 0, lBio_LabelPages.Count - 1 > 0 ? lBio_LabelPages.Count - 1 : 0);
            }
        }
        float lastInderxer = totalIndexer;
        float smoothInderxer = totalIndexer;

        public float showingZoom => isShowing && !slatedForDeletion ? 1f : 0f;
        float lastShowingZoom = 0f;
        float smoothShowingZoom = 0f;

        Vector2 playerRelativeVector = Vector2.zero;
        Vector2 smoothPlayerRelativeVector = Vector2.zero;
        Vector2 lastPlayerRelativeVector = Vector2.zero;

        //背景大小
        Vector2 size = Vector2.zero;
        Vector2 lastSize = Vector2.zero;
        Vector2 smoothSize = Vector2.zero;

        Vector2 pushForce = Vector2.zero;
        

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

        //闪烁计数器
        int flashing = 0;

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
                if (value != Reveal)
                {
                    alpha = value ? 1 : 0;
                }
            }
        }

        //标签页局部文本切换过程
        internal float localTextChangeProcess = 0.5f;
        float localTextChangeAlpha => 1 - Mathf.Sin((float)Math.PI * 2f * localTextChangeProcess);
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
        /// <summary>
        /// 所有的标签页
        /// </summary>
        FLabel[] fLabel = new FLabel[6];
        /// <summary>
        /// 背景
        /// </summary>
        FSprite background;
        /// <summary>
        /// 连线
        /// </summary>
        CustomFSprite connectLine;
        /// <summary>
        /// 生物图标
        /// </summary>
        FSprite icon;
        /// <summary>
        /// 图标闪光
        /// </summary>
        FSprite iconFlash;

        #region hanging
        /// <summary>
        /// 表示鼠标悬停或者选中时的缩放
        /// </summary>
        float zoom
        {
            get
            {
                return (isHanging? 1.5f : 1f) * (currentMouseOverLabel == this ? 1.5f : 1f);
            }
        }

        bool _isHanging;//是否悬挂
        public bool isHanging
        {
            get => _isHanging;
            set
            {
                _isHanging = value;
                if (value)
                {
                    ScreenPos = creaturePos - rCam.pos;
                }
            }
        }
        public bool isShowing = totalShowing; //是否显示
        #endregion


        //静态变量
        public static Dictionary<string, LBio_InfoMemory> LBio_InfoMemories = new Dictionary<string, LBio_InfoMemory>();//用于记忆标签状态

        public static LBio_CreatureLabel currentMouseOverLabel;
        static Vector2 mousePosLastFrame = Input.mousePosition;
        static float clickTime = 0f;

        static bool _totalShow = true;
        public static bool totalShowing
        {
            get => _totalShow;
            set
            {
                if(value != _totalShow)
                {
                    for(int i = 0;i < lBio_CreatureLabels.Count; i++)
                    {
                        lBio_CreatureLabels[i].isShowing = value;
                    }
                }
                _totalShow = value;
            }
        }

        static int _totalIndexer = 0;
        public static int totalIndexer
        {
            get => _totalIndexer;
            set
            {
                if(value != totalIndexer)
                {
                    for (int i = 0; i < lBio_CreatureLabels.Count; i++)
                    {
                        lBio_CreatureLabels[i].SwitchPages(value);
                    }
                    _totalIndexer = value;
                }
            }
        }

        /// <summary>
        /// 获取鼠标悬停或者被点击的鼠标，由Update调用。
        /// </summary>
        public static void GetHanging_or_MouseOver_Label()
        {
            if(lBio_CreatureLabels.Count == 0)
            {
                return;
            }

            bool getOneMouseOverLabel = false;
            Vector2 mousePos = Input.mousePosition;
            
            foreach (var label in lBio_CreatureLabels)
            {
                Vector2 localMousePos = label.background.GetLocalMousePosition();
                //Log(label.basicName, localMousePos.x, localMousePos.y);
                if (localMousePos.x < 0.5f && localMousePos.x > -0.5f && localMousePos.y > -0.5f && localMousePos.y < 0.5f)
                {
                    currentMouseOverLabel = label;
                    getOneMouseOverLabel = true;
                    break;
                }
            }
            
            //free haning for all
            if(!getOneMouseOverLabel && Input.GetMouseButton(1))
            {
                foreach (var label in lBio_CreatureLabels)
                {
                    label.isHanging = false;
                }
            }

            //hanging state setting
            if (!getOneMouseOverLabel && (!Input.GetMouseButton(0) || Input.GetMouseButtonUp(0)))
            {
                currentMouseOverLabel = null;

                if (Input.GetMouseButtonDown(0))
                {
                    foreach (var label in lBio_CreatureLabels)
                    {
                        label.isHanging = false;
                    }
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    mousePosLastFrame = mousePos;
                    clickTime = Time.time;
                }

                if (Input.GetMouseButton(0))
                {
                    if(Time.time - clickTime > 0.15f)
                    {
                        currentMouseOverLabel.isHanging = true;
                        Vector2 deltaMousePos = mousePos - mousePosLastFrame;
                        currentMouseOverLabel.ScreenPos += deltaMousePos;
                        mousePosLastFrame = mousePos;
                    }
                }

                if (Input.GetMouseButtonUp(0))
                {
                    float span = Time.time - clickTime;
                    if(span < 0.15f)
                    {
                        currentMouseOverLabel.isHanging = !currentMouseOverLabel.isHanging;
                    }
                }
            }


            bool switching = false;
            int newIndex = 0;
            #region getInput
            if (Input.GetKeyDown(KeyCode.F1))
            {
                newIndex = 0;
                switching = true;
            }
            else if (Input.GetKeyDown(KeyCode.F2))
            {
                newIndex = 1;
                switching = true;
            }
            else if (Input.GetKeyDown(KeyCode.F3))
            {
                newIndex = 2;
                switching = true;
            }
            else if (Input.GetKeyDown(KeyCode.F4))
            {
                newIndex = 3;
                switching = true;
            }
            else if (Input.GetKeyDown(KeyCode.F5))
            {
                newIndex = 4;
                switching = true;
            }
            else if (Input.GetKeyDown(KeyCode.F6))
            {
                newIndex = 5;
                switching = true;
            }
            else if (Input.GetKeyDown(KeyCode.CapsLock))
            {
                newIndex = -1;
                switching = true;
            }
            #endregion
            //showing or page setting

            if (switching)
            {
                if (getOneMouseOverLabel)
                {
                    currentMouseOverLabel.SwitchPages(newIndex);
                }
                else
                {
                    if (newIndex == -1)
                    {
                        totalShowing = !totalShowing;
                    }
                    else
                    {
                        totalIndexer = newIndex;
                    }
                }
            }

            if (getOneMouseOverLabel)
            {
                currentMouseOverLabel.Flash();
            }
        }

        public class LBio_InfoMemory
        {
            public bool isHanging;
            public bool isShowing;
            public Vector2 screenPos;
            public int indexer;
            public int localPageIndex;
        }
    }
}
