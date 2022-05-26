using LittleBiologist.LBio_Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static LittleBiologist.LBio_Const;

namespace LittleBiologist.LBio_Navigations
{
    public class LBio_NaviHUD
    {
        public static bool mouseOverAnyElement => LBio_NaviTypeHolder.currentMouseOverHolder != null || LBio_NaviHodler.currentMouseOverHolder != null;
        public static LBio_NaviHUD instance;
        public static SortedDictionary<CreatureTemplate.Type, List<LBio_NaviHodler>> allHolders = new SortedDictionary<CreatureTemplate.Type, List<LBio_NaviHodler>>();

        public List<LBio_NaviTypeHolder> allTypeHolders = new List<LBio_NaviTypeHolder>();
        public LBio_NaviHUD()
        {
            instance = this;
            Log("Init NaviHUD");
        }

        public void InitSprites()
        {

        }

        public void Draw()
        {
            for(int i = allTypeHolders.Count - 1;i>= 0; i--)
            {
                allTypeHolders[i].Draw();
            }
        }
        public void RemoveSprites()
        {
            Log("Destroy NaviHUD");
            for(int i = allTypeHolders.Count - 1; i >= 0; i--)
            {
                allTypeHolders[i].Destroy();
            }
            for (int i = LBio_NaviHodler.allHolders.Count - 1;i >= 0; i--)
            {
                LBio_NaviHodler.allHolders[i].Destroy();
            }
            instance = null;
        }

        public void AddNaviHolder(LBio_NaviHodler lBio_NaviHodler)
        {
            CreatureTemplate.Type key = lBio_NaviHodler.Type;
            if (!allHolders.ContainsKey(key))
            {
                allHolders.Add(key, new List<LBio_NaviHodler>());
                allTypeHolders.Add(new LBio_NaviTypeHolder(key,this));
                allTypeHolders.Sort((x,y) => ((int)x.Type).CompareTo((int)y.Type));
            }
            allHolders[key].Add(lBio_NaviHodler);
            allHolders[key].Sort((x, y) => x.ID.CompareTo(y.ID));
        }

        public void RemoveNaviHolder(LBio_NaviHodler lBio_NaviHodler)
        {
            try
            {
                var type = lBio_NaviHodler.Type;
                allHolders[type].Remove(lBio_NaviHodler);
                allHolders[type].Sort((x, y) => x.ID.CompareTo(y.ID));

                if (allHolders[type].Count == 0)
                {
                    for(int i = allTypeHolders.Count - 1;i >= 0; i--)
                    {
                        if(allTypeHolders[i].Type == type)
                        {
                            allTypeHolders[i].Destroy();
                        }
                    }
                    allHolders.Remove(type);
                }
            }
            catch
            {
                string allkey = "";

                foreach(var key in allHolders.Keys)
                {
                    allkey += key.ToString() + " ; ";
                }
                throw new KeyNotFoundException("\n Error happened with : " + lBio_NaviHodler.Type.ToString() + "\n" + allkey);
            }
        }

        /// <summary>
        /// 在Update中调用
        /// </summary>
        public static void UpdateMouseOperation()
        {
            if(instance != null)
            {
                LBio_NaviTypeHolder.UpdateMouseOperation();
                LBio_NaviHodler.UpdateMouseOperation();
            }
        }
    }

    public class LBio_NaviTypeHolder
    {
        public static LBio_NaviTypeHolder currentMouseOverHolder = null;
        public LBio_NaviTypeHolder(CreatureTemplate.Type type, LBio_NaviHUD owner)
        {
            this.owner = owner;
            this.Type = type;

            InitSprites();
        }

        public void InitSprites()
        {
            IconSymbol.IconSymbolData iconSymbol = new IconSymbol.IconSymbolData(Type, AbstractPhysicalObject.AbstractObjectType.Creature, 0);
            shadow = new FSprite("Futile_White", true) { shader = LBio_HUD.instance.hud.rainWorld.Shaders["FlatLight"], color = Color.black ,scale = 3f};
            icon = new FSprite(CreatureSymbol.SpriteNameOfCreature(iconSymbol), true) { color = CreatureSymbol.ColorOfCreature(iconSymbol) };

            LBio_HUD.AddNodeToContainer(shadow, 1);
            LBio_HUD.AddNodeToContainer(icon, 1);
        }

        void Start()
        {
            smoothPos = Pos;
            smoothPos.x = -20;
            lastPos = smoothPos;
        }

        public void Draw()
        {
            if (!started)
            {
                started = true;
                Start();
            }

            smoothPos = Vector2.Lerp(lastPos, Pos, 0.1f);
            lastPos = smoothPos;

            smoothAlpha = Mathf.Lerp(lastAlpha, alpha * life * (hideCounter == 0 ? 0.1f : 1f), 0.05f);
            lastAlpha = smoothAlpha;

            if (!slatedForDeletion)
            {
                icon.SetPosition(smoothPos);
                shadow.SetPosition(smoothPos);
            }
            else
            {
                life -= 0.01f;
            }

            if(life <= 0 && slatedForDeletion)
            {
                RemoveSprites();
            }

            icon.alpha = smoothAlpha;
            shadow.alpha = smoothAlpha;

            if (!slatedForDeletion)
            {
                for (int i = LBio_NaviHUD.allHolders[Type].Count - 1; i >= 0; i--)
                {
                    LBio_NaviHUD.allHolders[Type][i].Draw();
                }
            }
        }

        public void Destroy()
        {
            slatedForDeletion = true;
        }

        public void RemoveSprites()
        {
            LBio_HUD.RemoveNodeFromContainer(icon);
            LBio_HUD.RemoveNodeFromContainer(shadow);

            owner.allTypeHolders.Remove(this);
        }

        public static void UpdateMouseOperation()
        {
            hideCounter = Mathf.Clamp(hideCounter - 1, 0, 360);
            GetSettle();

            bool MouseOverOneTypeHolder = false;
            for(int i = LBio_NaviHUD.instance.allTypeHolders.Count - 1;i >= 0; i--)
            {
                LBio_NaviTypeHolder currentTypeHolder = LBio_NaviHUD.instance.allTypeHolders[i];
                if (currentTypeHolder.icon.GetLocalMousePosition().x > -40f && currentTypeHolder.icon.GetLocalMousePosition().x < 5f && Mathf.Abs(currentTypeHolder.icon.GetLocalMousePosition().y) < 10f)
                {
                    currentMouseOverHolder = currentTypeHolder;
                    MouseOverOneTypeHolder = true;
                    break;
                }
            }
            if(!MouseOverOneTypeHolder && currentMouseOverHolder != null)
            {
                currentMouseOverHolder = null;
            }

            if (Input.GetMouseButton(0) && LBio_NaviHodler.currentMouseOverHolder == null)
            {
                for (int i = LBio_NaviHUD.instance.allTypeHolders.Count - 1; i >= 0; i--)
                {
                    LBio_NaviHUD.instance.allTypeHolders[i].Selected = false;
                }
                if (MouseOverOneTypeHolder)
                {
                    currentMouseOverHolder.Selected = true;
                }
            }
        }

        Vector2 _Pos;
        Vector2 Pos
        {
            get
            {
                if (owner.allTypeHolders.Contains(this) && LBio_NaviHUD.allHolders.ContainsKey(Type))
                {
                    _Pos = new Vector2(40 + (Selected ? 10 : 0) + (currentMouseOverHolder == this ? 8 : 0), 768 - Index * 30 - 40f);
                }
                return _Pos;
            }
        }
        Vector2 lastPos;
        Vector2 smoothPos;

        public float alpha = 1f;
        float lastAlpha = 0f;
        float smoothAlpha = 0f;

        public FSprite icon;
        public FSprite shadow;

        bool started = false;
        bool slatedForDeletion = false;
        float life = 1f;
        public bool Selected = false;

        public LBio_NaviHUD owner;
        public CreatureTemplate.Type Type;
        public List<LBio_NaviHodler> MyHolders => LBio_NaviHUD.allHolders[Type];
        public int Index => owner.allTypeHolders.IndexOf(this);

        public static int hideCounter = 0;

        public static bool GetSettle()
        {
            if (currentMouseOverHolder != null || LBio_NaviHodler.currentMouseOverHolder != null)
            {
                hideCounter = 360;
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class LBio_NaviHodler
    {
        public static LBio_NaviHodler currentMouseOverHolder = null;
        public static LBio_NaviHodler selecetdHolder = null;
        public static List<LBio_NaviHodler> allHolders = new List<LBio_NaviHodler>();
        public LBio_NaviHodler(AbstractCreature abstractCreature)
        {
            basciName = abstractCreature.ToString();
            AbCreature = abstractCreature;
            Type = AbCreature.creatureTemplate.type;
            ID = AbCreature.ID.number;

            allHolders.Add(this);
            LBio_NaviHUD.instance.AddNaviHolder(this);
            owner = LBio_NaviHUD.instance.allTypeHolders.Find(x => x.Type == Type);
            Log("Add Holder for " + basciName);
            InitSprtes();
        }

        public void InitSprtes()
        {
            fLabel = new FLabel("font", ID.ToString()) { alpha = 0f };
            LBio_HUD.AddNodeToContainer(fLabel, 1);
        }

        public void Start()
        {
            smoothPos = owner.icon.GetPosition();
            lastPos = owner.icon.GetPosition();
        }

        public void Draw()
        {
            if (!Started)
            {
                Start();
                Started = true;
            }

            if(ShouldUpdate)
            {
                smoothPos = Vector2.Lerp(lastPos, pos, 0.1f);
                lastPos = smoothPos;

                smoothAlpha = Mathf.Lerp(lastAlpha, alpha, 0.1f);
                lastAlpha = smoothAlpha;

                smoothScale = Mathf.Lerp(lastScale, scale, 0.1f);
                lastScale = smoothScale;


                fLabel.SetPosition(smoothPos);
                fLabel.alpha = smoothAlpha;
                fLabel.scale = smoothScale;
            }
        }

        public void Destroy()
        {
            allHolders.Remove(this);
            RemoveSprites();
            LBio_NaviHUD.instance.RemoveNaviHolder(this);
            if(selecetdHolder == this)
            {
                selecetdHolder = null;
            }
            if(currentMouseOverHolder == this)
            {
                currentMouseOverHolder = null;
            }
            Log("Destroy Holder for " + basciName);
        }

        public void RemoveSprites()
        {
            LBio_HUD.RemoveNodeFromContainer(fLabel);
        }

        public static void UpdateMouseOperation()
        {
            currentMouseOverHolder = null;
            for (int i = allHolders.Count - 1;i >= 0; i--)
            {
                if (Mathf.Abs(allHolders[i].fLabel.GetLocalMousePosition().x) < 15f && Mathf.Abs(allHolders[i].fLabel.GetLocalMousePosition().y) < 10f && allHolders[i].ShouldUpdate)
                {
                    currentMouseOverHolder = allHolders[i];

                    if (Input.GetMouseButtonDown(2))
                    {
                        if(allHolders[i].AbCreature != null)
                        {
                            LBio_Safari.ChangeCameraFollow(allHolders[i].AbCreature);
                            
                            foreach (var holder in LBio_NaviHUD.instance.allTypeHolders)
                            {
                                holder.Selected = false;
                            }
                        }
                    }
                    break;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (selecetdHolder != null)
                {
                    if(currentMouseOverHolder != selecetdHolder)
                    {
                        if(currentMouseOverHolder != null)
                        {
                            selecetdHolder = currentMouseOverHolder;
                        }
                    }
                    else
                    {
                        selecetdHolder = null;
                    }
                }
                else
                {
                    if(currentMouseOverHolder != null)
                    {
                        selecetdHolder = currentMouseOverHolder;
                    }
                    else
                    {
                        selecetdHolder = null;
                    }
                }
            }
        }

        float alpha => (selecetdHolder == this ? 1f : (owner.Selected ? 1f : 0f)) * (LBio_NaviTypeHolder.hideCounter == 0 ? 0.1f : 1f);
        float smoothAlpha = 0f;
        float lastAlpha = 0f;

        float scale => (currentMouseOverHolder == this ? 1.2f : 1f) * (selecetdHolder == this ? 1f : smoothAlpha);
        float smoothScale = 1f;
        float lastScale = 1f;

        public bool ShouldUpdate => owner != null && (smoothAlpha > 0.01f || owner.Selected || selecetdHolder == this);

        Vector2 pos => selecetdHolder == this ? new Vector2(20f,768f - 15f): (owner.Selected ? new Vector2(80f, 768 - 40f - LBio_NaviHUD.allHolders[Type].IndexOf(this) * 18f) : owner.icon.GetPosition());
        Vector2 smoothPos;
        Vector2 lastPos;

        bool Started = false;

        public string basciName;
        public int ID;

        public FLabel fLabel;

        public CreatureTemplate.Type Type;
        public LBio_NaviTypeHolder owner;
        WeakReference _abCreature;
        public AbstractCreature AbCreature
        {
            get
            {
                if (_abCreature.Target == null || !_abCreature.IsAlive)
                {
                    Destroy();
                    return null;
                }
                else
                {
                    AbstractCreature creature = _abCreature.Target as AbstractCreature;
                    if (creature.InSameRegionWithCamera())
                    {
                        return _abCreature.Target as AbstractCreature;
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
                _abCreature = new WeakReference(value);
            }
        }
    }
}
