using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Androids
{
    public static class ExtraRendering
    {
        public static void DrawAdvanced(this Graphic graphic, Vector3 loc, Rot4 rot, float rotY, ThingDef thingDef, Thing thing)
        {
            Mesh mesh = graphic.MeshAt(rot);
            Quaternion rotation = graphic.GetQuatFromRot(rot) * Quaternion.AngleAxis(rotY, Vector3.up);
            Material material = graphic.MatAt(rot, thing);
            Graphics.DrawMesh(mesh, loc, rotation, material, 0);
            if (graphic.ShadowGraphic != null)
            {
                //graphic.ShadowGraphic.DrawWorker(loc, rot, thingDef, thing);
            }
        }

        public static Quaternion GetQuatFromRot(this Graphic graphic, Rot4 rot)
        {
            if (graphic.data != null && !graphic.data.drawRotated)
            {
                return Quaternion.identity;
            }
            if (graphic.ShouldDrawRotated)
            {
                return rot.AsQuat;
            }

            return Quaternion.identity;
        }
    }
}
