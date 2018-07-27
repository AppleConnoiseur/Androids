using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Androids
{
    public static class ExtraMath
    {
        public static Vector3 RotatedBy(this Vector3 vector, Rot4 rot)
        {
            switch (rot.AsInt)
            {
                case 0:
                    return vector;
                case 1:
                    return new Vector3(vector.z, vector.y, -vector.x);
                case 2:
                    return new Vector3(-vector.x, vector.y, -vector.z);
                case 3:
                    return new Vector3(-vector.z, vector.y, vector.x);
                default:
                    return vector;
            }
        }

        public static double ToRad(this Single degrees)
        {
            return degrees / (180d / Math.PI);
        }
    }

    public struct IKSegment
    {
        public Vector3 offset;
        public double radian;

        public IKSegment(Vector3 offset, double radian)
        {
            this.offset = offset;
            this.radian = radian;
        }
    }
}
