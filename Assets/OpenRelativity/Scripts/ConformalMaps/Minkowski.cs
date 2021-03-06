﻿using UnityEngine;

namespace OpenRelativity.ConformalMaps
{
    public class Minkowski : ConformalMap
    {
        override public Vector4 ComoveOptical(float properTDiff, Vector3 piw)
        {
            Vector4 piw4 = piw;
            piw4.w = properTDiff;
            return piw4;
        }

        override public Vector3 GetRindlerAcceleration(Vector3 piw)
        {
            return Vector3.zero;
        }
    }
}
