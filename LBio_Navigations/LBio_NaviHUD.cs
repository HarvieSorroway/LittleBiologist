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
        }

        public void RemoveNaviHolder(LBio_NaviHodler lBio_NaviHodler)
        {
            try
            {
                var type = lBio_NaviHodler.Type;
                allHolders[type].Remove(lBio_NaviHodler);
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
            catch(Exception e)
            {
                string allkey = "";

                foreach(var key in allHolders.Keys)
                {
                    allkey += key.ToString() + " ; ";
                }
                throw new KeyNotFoundException("\n Error happened with : " + lBio_NaviHodler.Type.ToString() + "\n" + allkey);
            }
        }
    }

    public class LBio_NaviTypeHolder
    {
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
            smoothPos = pos;
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

            smoothPos = Vector2.Lerp(lastPos, pos, 0.1f);
            lastPos = smoothPos;

            smoothAlpha = Mathf.Lerp(lastAlpha, alpha * life, 0.05f);
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


        Vector2 pos => new Vector2(40, 768 - index * 30 - 20f);
        Vector2 lastPos;
        Vector2 smoothPos;

        float alpha = 1f;
        float lastAlpha = 0f;
        float smoothAlpha = 0f;

        public FSprite icon;
        public FSprite shadow;

        bool started = false;
        bool slatedForDeletion = false;
        float life = 1f;

        public LBio_NaviHUD owner;
        public CreatureTemplate.Type Type;
        public List<LBio_NaviHodler> myHolders => LBio_NaviHUD.allHolders[Type];
        public int index => owner.allTypeHolders.IndexOf(this);
    }

    public class LBio_NaviHodler
    {
        public static List<LBio_NaviHodler> allHolders = new List<LBio_NaviHodler>();
        public LBio_NaviHodler(AbstractCreature abstractCreature)
        {
            basciName = abstractCreature.ToString();
            abCreature = abstractCreature;
            Type = abCreature.creatureTemplate.type;

            allHolders.Add(this);
            LBio_NaviHUD.instance.AddNaviHolder(this);
            Log("Add Holder for " + basciName);
        }

        public void Update()
        {

        }

        public void Draw()
        {

        }

        public void Destroy()
        {
            allHolders.Remove(this);
            LBio_NaviHUD.instance.RemoveNaviHolder(this);
            Log("Destroy Holder for " + basciName);
        }

        public string basciName;
        public CreatureTemplate.Type Type;
        WeakReference _abCreature;
        public AbstractCreature abCreature
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
