﻿using System;
using UnityEngine;

namespace OpenRelativity.ConformalMaps
{
    public class Schwarzschild : ConformalMap
    {
        public float radius = 1;
        public float radiusCutoff = 1;

        override public Matrix4x4 GetConformalFactor(Vector4 stpiw)
        {
            Vector4 origin = transform.position;

            //We assume all input space-time-position-in-world vectors are Cartesian.
            //The Schwarzschild metric is most naturally expressed in spherical coordinates.
            //So, let's just convert to spherical to get the conformal factor:
            Vector4 cartesianPos = stpiw - origin;
            Vector4 sphericalPos = cartesianPos.CartesianToSpherical();
            //Assume that spherical transform of input are Lemaître coordinates, since they "co-move" with the gravitational pull of the black hole:
            double rho = cartesianPos.magnitude;
            double tau = cartesianPos.w;

            //Convert to usual Schwarzschild solution r:
            double r = Math.Pow(Math.Pow(3.0 / 2.0 * (rho - tau), 2) * radius, 1.0 / 3.0);

            //At the center of the coordinate system is a singularity, at the Schwarzschild radius is an event horizon,
            // so we need to cut-off the interior metric at some point, for numerical sanity
            if (r <= radiusCutoff)
            {
                return Matrix4x4.identity;
            }
            else
            {
                double schwarzFac = 1 - radius / r;

                //Here's the value of the conformal factor at this distance in spherical coordinates with the orig at zero:
                Matrix4x4 sphericalConformalFactor = Matrix4x4.zero;
                //(For the metric, rather than the conformal factor, the time coordinate would have its sign flipped relative to the spatial components,
                // either positive space and negative time, or negative time and positive space.)
                sphericalConformalFactor[3, 3] = (float)(schwarzFac);
                sphericalConformalFactor[0, 0] = (float)(-1.0 / schwarzFac);
                sphericalConformalFactor[1, 1] = (float)(-r * r);
                sphericalConformalFactor[2, 2] = (float)(-r * r * Mathf.Pow(Mathf.Sin(sphericalPos.y), 2));

                //A particular useful "tensor" (which we can think of loosely here as "just a matrix") called the "Jacobian"
                // lets us convert the "metric tensor" (and other tensors) between coordinate systems, like from spherical back to Cartesian:
                Matrix4x4 jacobian  = Matrix4x4.identity;
                double x = cartesianPos.x;
                double y = cartesianPos.y;
                double z = cartesianPos.z;
                rho = Math.Sqrt(x * x + y * y + z * z);
                double sqrtXSqrYSqr = Math.Sqrt(x * x + y * y);
                // This is the Jacobian from spherical to Cartesian coordinates:
                jacobian.m00 = (float)(x / rho);
                jacobian.m01 = (float)(y / rho);
                jacobian.m02 = (float)(z / rho);
                jacobian.m10 = (float)(x * z / (rho * rho * sqrtXSqrYSqr));
                jacobian.m11 = (float)(y * z / (rho * rho * sqrtXSqrYSqr));
                jacobian.m12 = (float)(-sqrtXSqrYSqr / (rho * rho));
                jacobian.m20 = (float)(-y / (x * x + y * y));
                jacobian.m21 = (float)(x / (x * x + y * y));
                jacobian.m22 = 0;
                jacobian.m33 = 1;

                //To convert the coordinate system of the metric (or the "conformal factor," in this case,) we multiply this way by the Jacobian and its transpose.
                //(*IMPORTANT NOTE: I'm assuming this "conformal factor" transforms like a true tensor, which not all matrices are. I need to do more research to confirm that
                // it transforms the same way as the metric, but given that the conformal factor maps from Minkowski to another metric, I think this is a safe bet.)
                return jacobian.transpose * sphericalConformalFactor * jacobian;
            }
        }

        override public Matrix4x4 GetMetric(Vector4 stpiw)
        {
            Vector4 origin = transform.position;

            //We assume all input space-time-position-in-world vectors are Cartesian.
            //The Schwarzschild metric is most naturally expressed in spherical coordinates.
            //So, let's just convert to spherical to get the conformal factor:
            Vector4 cartesianPos = stpiw - origin;
            Vector4 sphericalPos = cartesianPos.CartesianToSpherical();
            //Assume that spherical transform of input are Lemaître coordinates, since they "co-move" with the gravitational pull of the black hole:
            double rho = cartesianPos.magnitude;
            double tau = cartesianPos.w;

            //Convert to usual Schwarzschild solution r:
            double r = Math.Pow(Math.Pow(3.0 / 2.0 * (rho - tau), 2) * radius, 1.0 / 3.0);
            
            //At the center of the coordinate system is a singularity, at the Schwarzschild radius is an event horizon,
            // so we need to cut-off the interior metric at some point, for numerical sanity
            if (r <= radiusCutoff)
            {
                Matrix4x4 minkowski = Matrix4x4.identity;
                minkowski.m33 = SRelativityUtil.cSqrd;
                minkowski.m00 = -1;
                minkowski.m11 = -1;
                minkowski.m22 = -1;

                return minkowski;
            }
            else
            {
                double schwarzFac = 1 - radius / r;

                //Here's the value of the conformal factor at this distance in spherical coordinates with the orig at zero:
                Matrix4x4 sphericalMetric = Matrix4x4.zero;
                //(For the metric, rather than the conformal factor, the time coordinate would have its sign flipped relative to the spatial components,
                // either positive space and negative time, or negative time and positive space.)
                sphericalMetric[3, 3] = (float)(SRelativityUtil.cSqrd * schwarzFac);
                sphericalMetric[0, 0] = (float)(-1.0 / schwarzFac);
                sphericalMetric[1, 1] = (float)(-r * r);
                sphericalMetric[2, 2] = (float)(-r * r * Mathf.Pow(Mathf.Sin(sphericalPos.y), 2));

                //A particular useful "tensor" (which we can think of loosely here as "just a matrix") called the "Jacobian"
                // lets us convert the "metric tensor" (and other tensors) between coordinate systems, like from spherical back to Cartesian:
                Matrix4x4 jacobian = Matrix4x4.identity;
                double x = cartesianPos.x;
                double y = cartesianPos.y;
                double z = cartesianPos.z;
                rho = Math.Sqrt(x * x + y * y + z * z);
                double sqrtXSqrYSqr = Math.Sqrt(x * x + y * y);
                // This is the Jacobian from spherical to Cartesian coordinates:
                jacobian.m00 = (float)(x / rho);
                jacobian.m01 = (float)(y / rho);
                jacobian.m02 = (float)(z / rho);
                jacobian.m10 = (float)(x * z / (rho * rho * sqrtXSqrYSqr));
                jacobian.m11 = (float)(y * z / (rho * rho * sqrtXSqrYSqr));
                jacobian.m12 = (float)(-sqrtXSqrYSqr / (rho * rho));
                jacobian.m20 = (float)(-y / (x * x + y * y));
                jacobian.m21 = (float)(x / (x * x + y * y));
                jacobian.m22 = 0;
                jacobian.m33 = 1;

                //To convert the coordinate system of the metric (or the "conformal factor," in this case,) we multiply this way by the Jacobian and its transpose.
                //(*IMPORTANT NOTE: I'm assuming this "conformal factor" transforms like a true tensor, which not all matrices are. I need to do more research to confirm that
                // it transforms the same way as the metric, but given that the conformal factor maps from Minkowski to another metric, I think this is a safe bet.)
                return jacobian.transpose * sphericalMetric * jacobian;
            }
        }
    }
}
