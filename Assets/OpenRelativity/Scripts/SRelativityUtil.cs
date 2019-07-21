﻿using System;
using System.Reflection;
using UnityEngine;

namespace OpenRelativity
{
    public static class SRelativityUtil
    {
        public static float c { get { return (float)srCamera.SpeedOfLight; } }
        public static float cSqrd { get { return (float)srCamera.SpeedOfLightSqrd; } }
        public static float maxVel { get { return (float)srCamera.MaxSpeed; } }

        public static Matrix4x4 GetWorldCoordMetric(Vector4 stpiw)
        {
            if (srCamera.conformalMap)
            {
                return srCamera.conformalMap.WorldCoordMetric(stpiw);
            } else
            {
                return Matrix4x4.identity;
            }
        }

        public static Matrix4x4 GetPlayerCooordMetric(Vector4 stpiw, Vector4 pstpiw)
        {
            if (srCamera.conformalMap)
            {
                return srCamera.conformalMap.WorldToLocal(pstpiw) * GetWorldCoordMetric(stpiw);
            } else
            {
                return Matrix4x4.identity;
            }
        }

        public static Matrix4x4 GetConformalMap(Vector4 stpiw, Vector4 pstpiw)
        {
            if (srCamera.conformalMap)
            {
                return srCamera.conformalMap.WorldToLocal(pstpiw) * srCamera.conformalMap.LocalToWorld(stpiw);
            } else
            {
                return Matrix4x4.identity;
            }
        }

        private static GameState _srCamera;
        private static GameState srCamera
        {
            get
            {
                if (_srCamera == null)
                {
                    GameObject cameraGO = GameObject.FindGameObjectWithTag(Tags.player);
                    _srCamera = cameraGO.GetComponent<GameState>();
                }

                return _srCamera;
            }
        }

        private static Vector3 GetSpatial(this Vector4 st)
        {
            return new Vector3(st.x, st.y, st.z);
        }

        private static Vector4 MakeVel(this Vector3 spatial)
        {
            return new Vector4(spatial.x, spatial.y, spatial.z, c);

        }

        public static Vector3 AddVelocity(this Vector3 orig, Vector3 toAdd)
        {
            Vector3 parra = Vector3.Project(toAdd, orig);
            Vector3 perp = toAdd - parra;
            perp = orig.InverseGamma() * perp / (1.0f + Vector3.Dot(orig, parra) / cSqrd);
            parra = (parra + orig) / (1.0f + Vector3.Dot(orig, parra) / cSqrd);
            return parra + perp;
        }

        public static Vector4 AddVelocity(this Vector4 orig, Vector4 toAdd)
        {
            Vector3 new3Vel = orig.GetSpatial().AddVelocity(toAdd.GetSpatial());
            return new Vector4(new3Vel.x, new3Vel.y, new3Vel.z, c);
        }

        public static Vector3 RelativeVelocityTo(this Vector3 myWorldVel, Vector3 otherWorldVel)
        {
            float speedSqr = myWorldVel.sqrMagnitude / cSqrd;
            //Get player velocity dotted with velocity of the object.
            float vuDot = Vector3.Dot(myWorldVel, otherWorldVel) / cSqrd;
            Vector3 vr;
            //If our speed is zero, this parallel velocity component will be NaN, so we have a check here just to be safe
            if (speedSqr != 0)
            {
                //Get the parallel component of the object's velocity
                Vector3 uparra = (vuDot / speedSqr) * myWorldVel / c;
                //Get the perpendicular component of our velocity, just by subtraction
                Vector3 uperp = otherWorldVel / c - uparra;
                //relative velocity calculation
                vr = (myWorldVel / c - uparra - (Mathf.Sqrt(1 - speedSqr)) * uperp) / (1 + vuDot);
            }
            //If our speed is nearly zero, it could lead to infinities.
            else
            {
                //relative velocity calculation
                vr = -otherWorldVel / c;
            }

            return vr * c;
        }

        public static Vector4 RelativeVelocityTo(this Vector4 myWorldVel, Vector4 otherWorldVel)
        {
            Vector3 new3Vel = myWorldVel.GetSpatial().RelativeVelocityTo(otherWorldVel.GetSpatial());
            return new Vector4(new3Vel.x, new3Vel.y, new3Vel.z, c);
        }


        public static Vector4 ContractLengthBy(this Vector4 interval, Vector4 velocity)
        {
            float sqrMag = velocity.sqrMagnitude;
            if (sqrMag == 0.0f)
            {
                return interval;
            }
            float invGamma = Mathf.Sqrt(1.0f + sqrMag / cSqrd);
            Quaternion rot = Quaternion.FromToRotation(1.0f / sqrMag * velocity, Vector3.right);
            Vector3 rotInt = rot * new Vector3(interval.x, interval.y, interval.z);
            rotInt = new Vector3(rotInt.x * invGamma, rotInt.y, rotInt.z);
            rotInt = Quaternion.Inverse(rot) * rotInt;
            return new Vector4(rotInt.x, rotInt.y, rotInt.z, interval.w / invGamma);
        }
        public static Vector4 InverseContractLengthBy(this Vector4 interval, Vector4 velocity)
        {
            float sqrMag = velocity.sqrMagnitude;
            if (sqrMag == 0.0f)
            {
                return interval;
            }
            float invGamma = Mathf.Sqrt(1.0f + sqrMag / cSqrd);
            Quaternion rot = Quaternion.FromToRotation(1.0f / sqrMag * velocity, Vector3.right);
            Vector3 rotInt = rot * new Vector3(interval.x, interval.y, interval.z);
            rotInt = new Vector3(rotInt.x / invGamma, rotInt.y, rotInt.z);
            rotInt = Quaternion.Inverse(rot) * rotInt;
            return new Vector4(rotInt.x, rotInt.y, rotInt.z, interval.w * invGamma);
        }

        public static Vector3 ContractLengthBy(this Vector3 interval, Vector3 velocity)
        {
            float speedSqr = velocity.sqrMagnitude;
            if (speedSqr == 0.0)
            {
                return interval;
            }
            float invGamma = Mathf.Sqrt(1.0f - speedSqr / cSqrd);
            Quaternion rot = Quaternion.FromToRotation(velocity / Mathf.Sqrt(speedSqr), Vector3.forward);
            Vector3 rotInt = rot * interval;
            rotInt = new Vector3(rotInt.x, rotInt.y, rotInt.z * invGamma);
            return Quaternion.Inverse(rot) * rotInt;
        }

        public static Vector3 InverseContractLengthBy(this Vector3 interval, Vector3 velocity)
        {
            float speedSqr = velocity.sqrMagnitude;
            if (speedSqr == 0.0)
            {
                return interval;
            }
            float invGamma = Mathf.Sqrt(1.0f - speedSqr / cSqrd);
            Quaternion rot = Quaternion.FromToRotation(velocity / Mathf.Sqrt(speedSqr), Vector3.forward);
            Vector3 rotInt = rot * interval;
            rotInt = new Vector3(rotInt.x, rotInt.y, rotInt.z / invGamma);
            return Quaternion.Inverse(rot) * rotInt;
        }

        public const float divByZeroCutoff = 1e-8f;

        public static Matrix4x4 GetLorentzTransformMatrix(Vector3 vpc)
        {
            float beta = vpc.magnitude;
            float gamma = 1.0f / Mathf.Sqrt(1 - beta * beta);
            Matrix4x4 vpcLorentzMatrix = Matrix4x4.identity;
            if (beta > 0)
            {
                Vector4 vpcTransUnit = -vpc / beta;
                vpcTransUnit.w = 1;
                Vector4 spatialComp = (gamma - 1) * vpcTransUnit;
                spatialComp.w = -gamma * beta;
                Vector4 tComp = -gamma * (new Vector4(beta, beta, beta, -1));
                tComp.Scale(vpcTransUnit);
                vpcLorentzMatrix.SetColumn(3, tComp);
                vpcLorentzMatrix.SetColumn(0, vpcTransUnit.x * spatialComp);
                vpcLorentzMatrix.SetColumn(1, vpcTransUnit.y * spatialComp);
                vpcLorentzMatrix.SetColumn(2, vpcTransUnit.z * spatialComp);
                vpcLorentzMatrix.m00 += 1;
                vpcLorentzMatrix.m11 += 1;
                vpcLorentzMatrix.m22 += 1;
            }

            return vpcLorentzMatrix;
        }

        public static Matrix4x4 GetRindlerMetric(Vector4 riw, Vector4 pap, Vector3 avp)
        {
            //Find metric based on player acceleration and rest frame:
            Vector3 angFac = Vector3.Cross(avp, riw) / c;
            float linFac = Vector3.Dot(pap, riw) / cSqrd;
            linFac = ((1 + linFac) * (1 + linFac) - angFac.sqrMagnitude) * cSqrd;
            angFac *= -2;

            Matrix4x4 metric = new Matrix4x4(
                new Vector4(-1, 0, 0, angFac.x),
                new Vector4(0, -1, 0, angFac.y),
                new Vector4(0, 0, -1, angFac.z),
                new Vector4(angFac.x, angFac.y, angFac.z, linFac)
            );

            return metric;
        }

        public static Matrix4x4 GetPlayerLocalAcceleratedMetric(this Vector4 stpiw, Vector3 origin, Vector3 playerVel, Vector4 pap, Vector3 avp)
        {
            Vector3 vpc = -playerVel / c;

            //riw = location in world, for reference
            Vector4 riw = stpiw - (Vector4)origin;//Position that will be used in the output
            Matrix4x4 vpcLorentzMatrix = GetLorentzTransformMatrix(vpc);
            // Boost to rest frame of player
            Vector4 riwForMetric = vpcLorentzMatrix * riw;
            //Find metric based on player acceleration and rest frame:
            Matrix4x4 metric = GetRindlerMetric(riwForMetric, pap, avp);
            //Lorentz boost back to world frame:
            vpcLorentzMatrix = vpcLorentzMatrix.inverse;
            // The Lorentz transformation is just a coordinate transformation:
            metric = vpcLorentzMatrix.transpose * metric * vpcLorentzMatrix;

            return metric;
        }

        public static float GetTisw(this Vector4 stpiw, Vector3 velocity, Vector3 origin, Vector3 playerVel, Vector4 pap, Vector3 avp, Vector4 aiw)
        {
            Vector3 vpc = -playerVel / c;
            Vector3 viw = velocity / c;

            //riw = location in world, for reference
            Vector4 riw = stpiw - (Vector4)origin;
 
            Matrix4x4 vpcLorentzMatrix = GetLorentzTransformMatrix(vpc);
            // Boost to rest frame of player
            Vector4 riwForMetric = vpcLorentzMatrix * riw;
            //Find metric based on player acceleration and rest frame:
            Matrix4x4 metric = GetRindlerMetric(riwForMetric, pap, avp);

            if (!srCamera.isMinkowski)
            {
                Matrix4x4 ltp = GetConformalMap(stpiw, pap);
                metric = ltp.transpose * metric * ltp;
            }

            //Lorentz boost back to world frame:
            vpcLorentzMatrix = vpcLorentzMatrix.inverse;
            // The Lorentz transformation is just a coordinate transformation:
            metric = vpcLorentzMatrix.transpose * metric * vpcLorentzMatrix;

            //We'll also Lorentz transform the vectors:
            Matrix4x4 viwLorentzMatrix = GetLorentzTransformMatrix(viw);

            //Apply Lorentz transform:
            Vector4 riwTransformed = viwLorentzMatrix * riw;
            Vector4 aiwTransformed = viwLorentzMatrix * aiw;

            //We need these values:
            float tisw = riwTransformed.w;
            riwTransformed.w = 0;
            aiwTransformed.w = 0;
            float riwDotRiw = -Vector4.Dot(riwTransformed, metric * riwTransformed);
            float aiwDotAiw = -Vector4.Dot(aiwTransformed, metric * aiwTransformed);
            float riwDotAiw = -Vector4.Dot(riwTransformed, metric * aiwTransformed);

            float sqrtArg = riwDotRiw * (cSqrd - riwDotAiw + aiwDotAiw * riwDotRiw / (4 * cSqrd)) / ((cSqrd - riwDotAiw) * (cSqrd - riwDotAiw));
            float t2 = 0;
            if (sqrtArg > 0)
            {
                t2 = -Mathf.Sqrt(sqrtArg);
            }
            tisw += t2;

            return tisw;
        }

        public static Vector3 WorldToOptical(this Vector4 stpiw, Vector3 velocity, Vector3 origin, Vector3 playerVel, Vector4 pap, Vector3 avp, Vector4? aiw = null, Matrix4x4? vpcLorentzMatrix = null, Matrix4x4? viwLorentzMatrix = null)
        {
            Vector3 vpc = -playerVel / c;
            Vector3 viw = velocity / c;

            //riw = location in world, for reference
            Vector4 riw = stpiw - (Vector4)origin;//Position that will be used in the output

            if (vpcLorentzMatrix == null)
            {
                vpcLorentzMatrix = GetLorentzTransformMatrix(vpc);
            }
            // Boost to rest frame of player
            Vector4 riwForMetric = vpcLorentzMatrix.Value * riw;

            //Find metric based on player acceleration and rest frame:
            Matrix4x4 metric = GetRindlerMetric(riwForMetric, pap, avp);

            if (!srCamera.isMinkowski)
            {
                Matrix4x4 ltp = GetConformalMap(stpiw, pap);
                metric = ltp.transpose * metric * ltp;
            }

            //Lorentz boost back to world frame;
            vpcLorentzMatrix = vpcLorentzMatrix.Value.inverse;
            metric = vpcLorentzMatrix.Value.transpose * metric * vpcLorentzMatrix.Value;

            //We'll also Lorentz transform the vectors:
            if (viwLorentzMatrix == null)
            {
                viwLorentzMatrix = GetLorentzTransformMatrix(viw);
            }

            //Remember that relativity is time-translation invariant.
            //The above metric gives the numerically correct result if the time coordinate of riw is zero,
            //(at least if the "conformal factor" or "mixed [indices] metric" is the identity).
            //We are free to translate our position in time such that this is the case.

            //Apply Lorentz transform;
            //metric = mul(transpose(viwLorentzMatrix), mul(metric, viwLorentzMatrix));
            Vector4 aiwTransformed = viwLorentzMatrix.Value * aiw.Value;
            aiwTransformed.w = 0;
            Vector4 riwTransformed = viwLorentzMatrix.Value * riw;
            //Translate in time:
            float tisw = riwTransformed.w;
            riwForMetric.w = 0;
            riw = vpcLorentzMatrix.Value * riwForMetric;
            riwTransformed = viwLorentzMatrix.Value * riw;
            riwTransformed.w = 0;

            //(When we "dot" four-vectors, always do it with the metric at that point in space-time, like we do so here.)
            float riwDotRiw = -Vector4.Dot(riwTransformed, metric * riwTransformed);
            float aiwDotAiw = -Vector4.Dot(aiwTransformed, metric * aiwTransformed);
            float riwDotAiw = -Vector4.Dot(riwTransformed, metric * aiwTransformed);

            float sqrtArg = riwDotRiw * (cSqrd - riwDotAiw + aiwDotAiw * riwDotRiw / (4 * cSqrd)) / ((cSqrd - riwDotAiw) * (cSqrd - riwDotAiw));
            float aiwMag = aiwTransformed.magnitude;
            float t2 = 0;
            if (sqrtArg > 0)
            {
                t2 = -Mathf.Sqrt(sqrtArg);
            }
            //else
            //{
            //    //Unruh effect?
            //    //Seems to happen with points behind the player.
            //    bool putBreakPointHere = true;
            //}
            tisw += t2;
            //add the position offset due to acceleration
            if (aiwMag > divByZeroCutoff)
            {
                riwTransformed = riwTransformed - aiwTransformed / aiwMag * cSqrd * (Mathf.Sqrt(1 + (aiwMag * t2 / c) * (aiwMag * t2 / c)) - 1);
            }
            riwTransformed.w = tisw;
            //Inverse Lorentz transform the position:
            viwLorentzMatrix = viwLorentzMatrix.Value.inverse;
            riw = viwLorentzMatrix.Value * riwTransformed;
            tisw = riw.w;
            riw = (Vector3)riw + tisw * velocity;

            float speed = viw.magnitude;
            if (speed > 0)
            {
                float newz = speed * c * tisw;
                Vector4 vpcUnit = vpc / speed;
                newz = (Vector4.Dot(riw, vpcUnit) + newz) / Mathf.Sqrt(1 - (speed * speed));
                riw = riw + (newz - Vector4.Dot(riw, vpcUnit)) * vpcUnit;
            }

            riw = (Vector3)riw + origin;

            return riw;
        }

        const int defaultOpticalToWorldMaxIterations = 5;
        const float defaultOpticalToWorldSqrErrorTolerance = 0.0001f;

        public static Vector3 OpticalToWorld(this Vector4 opticalPos, Vector3 velocity, Vector3 origin, Vector3 playerVel, Vector4 pap, Vector3 avp, Vector4 aiw, Matrix4x4? vpcLorentzMatrix = null, Matrix4x4? viwLorentzMatrix = null)
        {
            Vector3 vpc = -playerVel / c;// srCamera.PlayerVelocityVector;

            //riw = location in world, for reference
            Vector4 riw = opticalPos - (Vector4)origin; //Position that will be used in the output
            Vector4 pos = (Vector3)riw;

            //Transform fails and is unecessary if relative speed is zero:
            float newz;
            float tisw = -pos.magnitude / c;

            float speed = vpc.magnitude;

            if (speed > divByZeroCutoff)
            {
                Vector4 vpcUnit = vpc / speed;
                newz = Vector4.Dot((Vector3)riw, vpcUnit) * Mathf.Sqrt(1 - (speed * speed));
                riw = riw + (newz - Vector4.Dot((Vector3)riw, vpcUnit)) * vpcUnit;
                newz = speed * c * tisw;
                riw = riw - newz * vpcUnit;
            }

            //Rotate all our vectors so that velocity is entirely along z direction:
            Vector3 viw = velocity / c;
            Quaternion viwToZRot = Quaternion.FromToRotation(viw, Vector3.forward);
            Vector4 riwTransformed = viwToZRot * ((Vector3)riw - velocity * tisw);
            riwTransformed.w = tisw;
            Vector3 avpTransformed = viwToZRot * avp;
            Vector3 aiwTransformed = viwToZRot * aiw;

            //We'll also Lorentz transform the vectors:
            float beta = viw.sqrMagnitude;
            float gamma = 1.0f / Mathf.Sqrt(1 - beta);

            if (viwLorentzMatrix == null)
            {
                viwLorentzMatrix = GetLorentzTransformMatrix(viw);
            }

            //Apply Lorentz transform;
            //metric = viwLorentzMatrix.transpose * metric * viwLorentzMatrix;
            riwTransformed = viwLorentzMatrix.Value * riwTransformed;
            avpTransformed = viwLorentzMatrix.Value * avpTransformed;
            aiwTransformed = viwLorentzMatrix.Value * aiwTransformed;

            tisw = riwTransformed.w;

            if (aiw.sqrMagnitude > divByZeroCutoff)
            {
                float aiwMag = aiwTransformed.magnitude;
                //add the position offset due to acceleration
                riwTransformed += (Vector4)(aiwTransformed) / aiwMag * c * c * (Mathf.Sqrt(1 + (aiwMag * tisw / c) * (aiwMag * tisw / c)) - 1);
            }

            //Inverse Lorentz transform the position:
            riwTransformed = viwLorentzMatrix.Value.inverse * riwTransformed;
            riw = Quaternion.Inverse(viwToZRot) * riwTransformed;

            riw = (Vector3)riw + origin;

            return riw;
        }

        public static Vector3 OpticalToWorldHighPrecision(this Vector4 opticalPos, Vector3 velocity, Vector3 origin, Vector3 playerVel, Vector4 pap, Vector3 avp, Vector4 aiw, Matrix4x4? vpcLorentz = null, Matrix4x4? viwLorentz = null)
        {
            Vector4 startPoint = opticalPos;
            Vector3 est, offset, newEst;
            est = opticalPos.OpticalToWorld(velocity, origin, playerVel, Vector4.zero, Vector3.zero, Vector4.zero);
            offset = (Vector3)opticalPos - ((Vector4)est).WorldToOptical(velocity, origin, playerVel, pap, avp, aiw, vpcLorentz, viwLorentz);

            float sqrError = offset.sqrMagnitude;
            float oldSqrError = sqrError + 1.0f;
            float iterations = 1;
            while ((iterations < defaultOpticalToWorldMaxIterations)
                && (sqrError > defaultOpticalToWorldSqrErrorTolerance)
                && (sqrError < oldSqrError))
            {
                iterations++;
                startPoint += (Vector4)offset / 2.0f;
                newEst = startPoint.OpticalToWorld(velocity, origin, playerVel, pap, avp, aiw);
                offset = (Vector3)startPoint - ((Vector4)newEst).WorldToOptical(velocity, origin, playerVel, pap, avp, aiw, vpcLorentz, viwLorentz);
                oldSqrError = sqrError;
                sqrError = ((Vector3)opticalPos - ((Vector4)newEst).WorldToOptical(velocity, origin, playerVel, pap, avp, aiw, vpcLorentz, viwLorentz)).sqrMagnitude;
                if (sqrError < oldSqrError)
                {
                    est = newEst;
                }
            }

            return est;
        }

        public static float Gamma(this Vector3 velocity)
        {
            return 1.0f / Mathf.Sqrt(1.0f - velocity.sqrMagnitude / cSqrd);
        }

        public static float InverseGamma(this Vector3 velocity)
        {
            return 1.0f / Mathf.Sqrt(1.0f + velocity.sqrMagnitude / cSqrd);
        }

        public static Vector3 RapidityToVelocity(this Vector3 rapidity, float? altMag = null)
        {
            float mag = altMag ?? rapidity.magnitude;
            if (mag == 0.0f)
            {
                return Vector3.zero;
            }
            return (float)(c * Math.Tanh(mag / c) / mag) * rapidity;
        }

        public static Vector3 GetGravity(Vector3 origin)
        {
            return Physics.gravity;
        }

        public static Vector4 LorentzTransform(this Vector4 pos4, Vector3 vel3)
        {
            float gamma = vel3.Gamma();
            Vector3 pos3 = pos4;
            Vector3 parra = Vector3.Project(pos3, vel3.normalized);
            float tnew = gamma * (pos4.w - Vector3.Dot(parra, vel3) / cSqrd);
            pos3 = gamma * (parra - vel3 * pos4.w) + (pos3 - parra);
            return new Vector4(pos3.x, pos3.y, pos3.z, tnew);
        }

        public static Vector4 InverseLorentzTransform(this Vector4 pos4, Vector3 vel3)
        {
            float gamma = vel3.Gamma();
            Vector3 pos3 = pos4;
            Vector3 parra = Vector3.Project(pos3, vel3.normalized);
            float tnew = gamma * (pos4.w + Vector3.Dot(parra, vel3) / cSqrd);
            pos3 = gamma * (parra + vel3 * pos4.w) + (pos3 - parra);
            return new Vector4(pos3.x, pos3.y, pos3.z, tnew);
        }

        //http://answers.unity3d.com/questions/530178/how-to-get-a-component-from-an-object-and-add-it-t.html
        public static T GetCopyOf<T>(this Component comp, T other) where T : Component
        {
            Type type = comp.GetType();
            if (type != other.GetType()) return null; // type mis-match
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default;
            PropertyInfo[] pinfos = type.GetProperties(flags);
            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                    }
                    catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }
            FieldInfo[] finfos = type.GetFields(flags);
            foreach (var finfo in finfos)
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }
            return comp as T;
        }

        public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
        {
            return go.AddComponent<T>().GetCopyOf(toAdd) as T;
        }

        public static Vector3 SphericalToCartesian(this Vector3 spherical)
        {
            float radius = spherical.x;
            float elevation = spherical.y;
            float polar = spherical.z;
            Vector3 outCart;

            float a = radius * Mathf.Cos(elevation);
            outCart.x = a * Mathf.Cos(polar);
            outCart.y = radius * Mathf.Sin(elevation);
            outCart.z = a * Mathf.Sin(polar);

            return outCart;
        }

        public static Vector4 Spherical4ToCartesian4(this Vector4 spherical)
        {
            float radius = spherical.x;
            float elevation = spherical.y;
            float polar = spherical.z;
            float time = spherical.w;
            Vector4 outCart;

            float a = radius * Mathf.Cos(elevation);
            outCart.x = a * Mathf.Cos(polar);
            outCart.y = radius * Mathf.Sin(elevation);
            outCart.z = a * Mathf.Sin(polar);
            outCart.w = time;

            return outCart;
        }

        public static Vector3 CartesianToSpherical(this Vector3 cartesian)
        {
            float radius, polar, elevation;
            radius = cartesian.magnitude;
            if (radius == 0)
            {
                polar = 0;
                elevation = 0;
            }
            else
            {
                float sqrtXSqrYSqr = Mathf.Sqrt(cartesian.x * cartesian.x + cartesian.y * cartesian.y);
                if ((cartesian.z == 0) && (sqrtXSqrYSqr == 0))
                {
                    elevation = 0;
                }
                else
                {
                    elevation = Mathf.Atan2(sqrtXSqrYSqr, cartesian.z);
                }

                if ((cartesian.y == 0) && (cartesian.x == 0))
                {
                    polar = 0;
                }
                else
                {
                    polar = Mathf.Atan2(cartesian.y, cartesian.x);

                }
            }

            return new Vector3(radius, elevation, polar);
        }

        public static Vector4 Cartesian4ToSpherical4(this Vector4 cartesian)
        {
            float outTime = cartesian.w;
            float radius, polar, elevation;
            Vector3 spatial = new Vector3(cartesian.x, cartesian.y, cartesian.z);
            radius = spatial.magnitude;
            if (radius == 0)
            {
                polar = 0;
                elevation = 0;
            }
            else
            {
                float sqrtXSqrYSqr = Mathf.Sqrt(cartesian.x * cartesian.x + cartesian.y * cartesian.y);
                if ((cartesian.z == 0) && (sqrtXSqrYSqr == 0))
                {
                    elevation = 0;
                }
                else
                {
                    elevation = Mathf.Atan2(sqrtXSqrYSqr, cartesian.z);
                }

                if ((cartesian.y == 0) && (cartesian.x == 0))
                {
                    polar = 0;
                }
                else
                {
                    polar = Mathf.Atan2(cartesian.y, cartesian.x);

                }
            }

            return new Vector4(radius, elevation, polar, outTime);
        }

        public static Vector4 ToMinkowski4Viw(this Vector3 viw)
        {
            return new Vector4(viw.x, viw.y, viw.z, (float)(Math.Sqrt(c - viw.sqrMagnitude) / c));
        }

        public static Vector4 ProperToWorldAccel(this Vector3 propAccel, Vector3 viw)
        {
            float gammaSqrd = viw.Gamma();
            gammaSqrd *= gammaSqrd;
            float gammaFourthADotVDivCSqrd = Vector3.Dot(propAccel, viw) * gammaSqrd * gammaSqrd / cSqrd;
            Vector4 fourAccel = gammaSqrd * (Vector3)propAccel + gammaFourthADotVDivCSqrd * viw;
            fourAccel.w = gammaFourthADotVDivCSqrd * c;
            return fourAccel;
        }
    }
}