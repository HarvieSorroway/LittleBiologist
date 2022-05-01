using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HUD;

namespace LittleBiologist
{
    public class LittleBiologist_HUD : HudPart
    {
        public static LittleBiologist_HUD instance;
        public static List<Label_HUDPart> label_HUDParts_ToAdd = new List<Label_HUDPart>();

        public  List<Label_HUDPart> label_HUDParts = new List<Label_HUDPart>();
        List<FNode> fNodes = new List<FNode>();
        public LittleBiologist_HUD(HUD.HUD hud) : base(hud)
        {
            instance = this;
            hud.AddPart(this);

            foreach(var part in label_HUDParts_ToAdd)
            {
                AddPart(part);
            }
        }

        public void Destroy(bool ignorePlayer = false)
        {
            label_HUDParts_ToAdd.Clear();
            for (int i = LittleBiologist_Label.labels.Count - 1; i >= 0; i--)
            {
                if(ignorePlayer && LittleBiologist_Label.labels[i].abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
                {
                    label_HUDParts_ToAdd.Add(LittleBiologist_Label.labels[i].label);
                    continue;
                }
                LittleBiologist_Label.labels[i].Destory();
            }
            
            slatedForDeletion = true;
            instance = null;
        }

        public override void Draw(float timeStacker)
        {
            base.Draw(timeStacker);
            if(label_HUDParts.Count > 0)
            {
                foreach(var part in label_HUDParts)
                {
                    part.Draw(timeStacker);
                }
            }
        }

        public void RemoveNode(FNode node)
        {
            hud.fContainers[1].RemoveChild(node);
            fNodes.Remove(node);
        }

        public void AddNode(FNode node)
        {
            hud.fContainers[1].AddChild(node);
            fNodes.Add(node);
        }

        public void ClearNodes()
        {
            foreach(var node in fNodes)
            {
                hud.fContainers[1].RemoveChild(node);
            }
            fNodes.Clear();
        }

        public void AddPart(Label_HUDPart part)
        {
            label_HUDParts.Add(part);
            part.InitSprite();
        }
        public void RemovePart(Label_HUDPart part)
        {
            label_HUDParts.Remove(part);
        }

        public static void AddPartToInstance(Label_HUDPart part)
        {
            if(instance != null)
            {
                instance.AddPart(part);
            }
            else
            {
                label_HUDParts_ToAdd.Add(part);
            } 
        }
    }
}
