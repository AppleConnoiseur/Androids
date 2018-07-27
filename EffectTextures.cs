using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Androids
{
    [StaticConstructorOnStartup]
    public static class EffectTextures
    {
        //public static Graphic IKArm = GraphicDatabase.Get<Graphic_Single>("Effects/IKArm2");
        public static string Eyeglow_Front_Path = "Effects/Eyeglow_front";
        public static string Eyeglow_Side_Path = "Effects/Eyeglow_side";

        public static Dictionary<Pair<bool, Color>, Graphic> eyeCache = new Dictionary<Pair<bool, Color>, Graphic>();

        static EffectTextures()
        {
            
        }

        public static Graphic GetEyeGraphic(bool isFront, Color color)
        {
            //Construct request.
            Pair<bool, Color> req = new Pair<bool, Color>(isFront, color);

            if(eyeCache.ContainsKey(req))
            {
                return eyeCache[req];
            }

            if(isFront)
                eyeCache[req] = GraphicDatabase.Get<Graphic_Single>(Eyeglow_Front_Path, ShaderDatabase.MoteGlow, Vector2.one, color);
            else
                eyeCache[req] = GraphicDatabase.Get<Graphic_Single>(Eyeglow_Side_Path, ShaderDatabase.MoteGlow, Vector2.one, color);

            return eyeCache[req];
        }
    }
}
