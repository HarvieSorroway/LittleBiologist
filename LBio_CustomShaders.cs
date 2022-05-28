using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static LittleBiologist.LBio_Const;


namespace LittleBiologist
{
    public static class LBio_CustomShaders
    {
        public static void Patch()
        {
            On.RainWorld.Start += RainWorld_Start;
        }

        private static void RainWorld_Start(On.RainWorld.orig_Start orig, RainWorld self)
        {
            orig.Invoke(self);

            var shader_Text = LBio_Res.CompiledShader;

            Material material = new Material(shader_Text);
            Shader shader = material.shader;

            self.Shaders.Add("HoloGridMod",FShader.CreateShader("HoloGridMod", shader));
        }
    }
}
