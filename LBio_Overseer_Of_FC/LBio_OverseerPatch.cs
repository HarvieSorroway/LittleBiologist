using CoralBrain;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static LittleBiologist.LBio_Const;

using System.Reflection;
using MonoMod.RuntimeDetour;


namespace LittleBiologist
{
    public static class LBio_OverseerPatch
    {
        public static void OnEnable_Patch()
        {
            OverseerColorModify.OnEnable_Patch();
            OverseerColorModify.AddColor(806, new Color(0.32f, 0.38f, 0.66f));
        }
    }

    public static class OverseerColorModify
    {
        public delegate Color orig_MainColor(OverseerGraphics self);
        static BindingFlags propFlags = BindingFlags.Instance | BindingFlags.Public;
        static BindingFlags myMethodFlags = BindingFlags.Static | BindingFlags.Public;

        public static Color OverseerGraphics_get_MainColor(orig_MainColor orig, OverseerGraphics self)
        {
            int iterator = (self.overseer.abstractCreature.abstractAI as OverseerAbstractAI).ownerIterator;
            Color? newCol = GetColor(iterator);
            if(newCol != null)
            {
                return newCol.Value;
            }
            else
            {
                return orig.Invoke(self);
            }
        }

        public static void OnEnable_Patch()
        {
            Hook overseerGraphics_get_MainColor_Hook = new Hook(typeof(OverseerGraphics).GetProperty("MainColor", propFlags).GetGetMethod(), typeof(OverseerColorModify).GetMethod("OverseerGraphics_get_MainColor", myMethodFlags));
        }
        public static void AddColor(int ownIterator, Color color)
        {
            try
            {
                IteratorAndColor.Add(ownIterator, color);
            }
            catch
            {
                Log("Color of iterator already exists", ownIterator.ToString());
            }
        }

        public static Color? GetColor(int iterator)
        {
            if (IteratorAndColor.ContainsKey(iterator))
            {
                return IteratorAndColor[iterator];
            }
            return null;
        }
        
        static Dictionary<int, Color> IteratorAndColor = new Dictionary<int, Color>();
    }
}
