﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using GameTools.Graph;
using Microsoft.Xna.Framework;

namespace GameTools.Noise3D
{
    public class FastPerlinNoise3D
    {
        private const int premutationSize = 100;
        private float[] flatPremutationList;

        private Random rng;

        private Dictionary<Vector3, FastPerlinInterpolatedNoise3D> calcLookup;

        private PerlinNoiseSettings3D settings;

        public FastPerlinNoise3D(PerlinNoiseSettings3D settings)
        {
            this.settings = settings;
            rng = new Random(settings.seed);

            populatePremutations();
        }
        public void FillWithPerlinNoise3D(float[] toFill)
        {
            int width = (int)settings.size.X;
            int height = (int)settings.size.Y;
            int length = (int)settings.size.Z;

            int effectiveX;
            int effectiveY;
            int effectiveZ;

            calcLookup = new Dictionary<Vector3, FastPerlinInterpolatedNoise3D>();

            for(int x = 0; x < 0 + width; x++)
            {
                for(int y = 0; y < 0 + height; y++)
                {
                    for(int z = 0; z < 0 + length; z++)
                    {
                        effectiveX = x + (int)settings.startingPoint.X;
                        effectiveY = y + (int)settings.startingPoint.Y;
                        effectiveZ = z + (int)settings.startingPoint.Z;

                        toFill[x * height * length + y * length + z] = GetPerlinNoise3D(effectiveX, effectiveY, effectiveZ);
                    }
                }
            }
        }
        public float GetPerlinNoise3D(float x, float y, float z)
        {
            float result;

            float frequency = 1;
            float amplitude = 1;

            x /= settings.zoom;
            y /= settings.zoom;
            z /= settings.zoom;

            result = 0;
            for(int oct = 0; oct < settings.octaves; oct++)
            {
                result += amplitude * GenInterpolatedNoise(x * frequency, y * frequency, z * frequency);

                frequency *= settings.frequencyMulti;
                amplitude *= settings.persistence;
            }

            return result;
        }
        private float GenInterpolatedNoise(float x, float y, float z)
        {
            int floorX = (x > 0) ? (int)x : (int)x - 1;
            int floorY = (y > 0) ? (int)y : (int)y - 1;
            int floorZ = (z > 0) ? (int)z : (int)z - 1;

            float fractionalX = x - floorX;
            float fractionalY = y - floorY;
            float fractionalZ = z - floorZ;

            float centerInter, bottomInter, belowInter, aboveInter;

            FastPerlinInterpolatedNoise3D noise;

            Vector3 key = new Vector3(floorX, floorY, floorZ);

            calcLookup.TryGetValue(key, out noise);

            if(noise == null)
            {
                noise = new FastPerlinInterpolatedNoise3D();
                noise.center = GenSmoothNoise(floorX, floorY, floorZ);
                noise.bottom = GenSmoothNoise(floorX, floorY + 1, floorZ);
                noise.centerRight = GenSmoothNoise(floorX + 1, floorY, floorZ);
                noise.bottomRight = GenSmoothNoise(floorX + 1, floorY + 1, floorZ);

                noise.centerAbove = GenSmoothNoise(floorX, floorY, floorZ + 1);
                noise.bottomAbove = GenSmoothNoise(floorX, floorY + 1, floorZ + 1);
                noise.centerRightAbove = GenSmoothNoise(floorX + 1, floorY, floorZ + 1);
                noise.bottomRightAbove = GenSmoothNoise(floorX + 1, floorY + 1, floorZ + 1);

                calcLookup.Add(key, noise);
            }

            centerInter = GraphMath.LinearInterpolateFloat(noise.center, noise.centerRight, fractionalX);
            bottomInter = GraphMath.LinearInterpolateFloat(noise.bottom, noise.bottomRight, fractionalX);
            belowInter = GraphMath.LinearInterpolateFloat(centerInter, bottomInter, fractionalY);

            centerInter = GraphMath.LinearInterpolateFloat(noise.centerAbove, noise.centerRightAbove, fractionalX);
            bottomInter = GraphMath.LinearInterpolateFloat(noise.bottomAbove, noise.bottomRightAbove, fractionalX);
            aboveInter = GraphMath.LinearInterpolateFloat(centerInter, bottomInter, fractionalY);

            return GraphMath.LinearInterpolateFloat(belowInter, aboveInter, fractionalZ);
        }
        private float GenSmoothNoise(int x, int y, int z)
        {
            float corners, sides, center;

            int adjustedX = x;
            int adjustedY = y;
            int adjustedZ = z;

            adjustedX = adjustedX % (premutationSize - 1);
            adjustedY = adjustedY % (premutationSize - 1);
            adjustedZ = adjustedZ % (premutationSize - 1);

            while(adjustedX < 1)
                adjustedX = premutationSize + adjustedX - 2;
            while(adjustedY < 1)
                adjustedY = premutationSize + adjustedY - 2;
            while(adjustedZ < 1)
                adjustedZ = premutationSize + adjustedZ - 2;


            center = GetCenter(adjustedX, adjustedY, adjustedZ);
            sides = GetSides(adjustedX, adjustedY, adjustedZ);
            corners = GetCorners(adjustedX, adjustedY, adjustedZ);

            Debug.Assert(Math.Abs(center) < 5 / 8.0);
            Debug.Assert(Math.Abs(sides) < 1 / 4.0);
            Debug.Assert(Math.Abs(corners) < 1 / 8.0);

            return corners + sides + center;
        }

        private float GetCenter(int adjustedX, int adjustedY, int adjustedZ)
        {
            return 5 * ThreeIndexIntoArray(adjustedX, adjustedY, adjustedZ) / 8;
        }
        private float GetSides(int adjustedX, int adjustedY, int adjustedZ)
        {
            float right = ThreeIndexIntoArray(adjustedX + 1, adjustedY, adjustedZ);
            float left = ThreeIndexIntoArray(adjustedX - 1, adjustedY, adjustedZ);
            float up = ThreeIndexIntoArray(adjustedX, adjustedY + 1, adjustedZ);
            float down = ThreeIndexIntoArray(adjustedX, adjustedY - 1, adjustedZ);

            float forward = ThreeIndexIntoArray(adjustedX, adjustedY, adjustedZ + 1);
            float back = ThreeIndexIntoArray(adjustedX, adjustedY, adjustedZ - 1);

            return (right + left + up + down + forward + back) / 24f;
        }
        private float GetCorners(int adjustedX, int adjustedY, int adjustedZ)
        {
            float rightUpForward = ThreeIndexIntoArray(adjustedX + 1, adjustedY + 1, adjustedZ + 1);
            float rightUpBack = ThreeIndexIntoArray(adjustedX + 1, adjustedY + 1, adjustedZ - 1);
            float rightDownForward = ThreeIndexIntoArray(adjustedX + 1, adjustedY - 1, adjustedZ + 1);
            float rightDownBack = ThreeIndexIntoArray(adjustedX + 1, adjustedY - 1, adjustedZ - 1);

            float leftUpForward = ThreeIndexIntoArray(adjustedX - 1, adjustedY + 1, adjustedZ + 1);
            float leftUpBack = ThreeIndexIntoArray(adjustedX - 1, adjustedY + 1, adjustedZ - 1);
            float leftDownForward = ThreeIndexIntoArray(adjustedX - 1, adjustedY - 1, adjustedZ + 1);
            float leftDownBack = ThreeIndexIntoArray(adjustedX - 1, adjustedY - 1, adjustedZ - 1);

            return (rightUpForward + rightUpBack + rightDownForward + rightDownBack +
                    leftUpForward + leftUpBack + leftDownForward + leftDownBack) / 64;
        }
        private float ThreeIndexIntoArray(int x, int y, int z)
        {
            return flatPremutationList[x * premutationSize * premutationSize + y * premutationSize + z];
        }
        private void populatePremutations()
        {
            flatPremutationList = new float[premutationSize * premutationSize * premutationSize];
            int index;
            for(int x = 0; x < premutationSize; x++)
            {
                for(int y = 0; y < premutationSize; y++)
                {
                    for(int z = 0; z < premutationSize; z++)
                    {
                        index = x * premutationSize * premutationSize + y * premutationSize + z;
                        flatPremutationList[index] = SimpleNoise3D.GenFloatNoise(rng.Next(), rng.Next(), rng.Next(), settings.seed);
                    }
                }
            }
        }
    }
}

