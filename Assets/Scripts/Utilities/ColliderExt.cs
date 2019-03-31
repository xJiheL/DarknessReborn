using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ColliderExt
{
    public static Vector3 ClosestPointExt(this Collider collider, Vector3 to)
    {
        if (collider is BoxCollider ||
            collider is SphereCollider ||
            collider is CapsuleCollider)
        {
            return collider.ClosestPoint(to);
        }

        if (collider is MeshCollider meshCollider)
        {
            if (meshCollider.convex)
            {
                return meshCollider.ClosestPoint(to);
            }
            else
            {
                // TODO expensive!!
                Vector3[] vertices = meshCollider.sharedMesh.vertices.Select(collider.transform.TransformPoint).ToArray();
                int[] triangles = meshCollider.sharedMesh.triangles;
                
                Debug.Assert(triangles.Length > 0);
                Debug.Assert(triangles.Length % 3 == 0);

                float distance = float.MaxValue;
                Vector3 closestPoint = Vector3.zero;
                
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    Vector3 newClosestPoint = ClosestPtPointTriangle(
                        vertices[triangles[i]], 
                        vertices[triangles[i + 1]],
                        vertices[triangles[i + 2]], 
                        to);

                    float newDistance = Vector3.Distance(to, newClosestPoint);

                    if (newDistance < distance)
                    {
                        distance = newDistance;
                        closestPoint = newClosestPoint;
                    }
                }

                return closestPoint;
            }
        }
/*
        if (collider is TerrainCollider terrainCollider)
        {
            return ClosestPointOnSurface(terrainCollider, to, radius, false);
        }*/

        throw new Exception($"{collider.GetType()} does not have an implementation for ClosestPoint; GameObject.Name='{collider.gameObject.name}'");
    }

    // TODO move
    private static Vector3 ClosestPtPointTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
    {
        // Real-Time Collision Detection
        // https://books.google.fr/books?id=WGpL6Sk9qNAC
        // Page 141
        
        // Check if P in vertex region outside A
        Vector3 ab = b - a;
        Vector3 ac = c - a;
        Vector3 ap = p - a;

        float d1 = Vector3.Dot(ab, ap);
        float d2 = Vector3.Dot(ac, ap);

        if (d1 <= 0f && d2 <= 0f)
        {
            return a; // barycentric coordinates (1,0,0)
        }
        
        // Check if P in vertex region outside B
        Vector3 bp = p - b;
        float d3 = Vector3.Dot(ab, bp);
        float d4 = Vector3.Dot(ac, bp);

        if (d3 >= 0f && d4 <= d3)
        {
            return b; // barycentric coordinates (0,1,0)
        }
        
        // Check if P in edge region of AB, if so return projection of P onto AB
        float vc = d1 * d4 - d3 * d2;
        if (vc <= 0f && d1 >= 0f && d3 <= 0f)
        {
            float v = d1 / (d1 - d3);
            return a + v * ab; // barycentric coordinates (1-v,v,0)
        }
        
        // Check if P in vertex region outside C
        Vector3 cp = p - c;
        float d5 = Vector3.Dot(ab, cp);
        float d6 = Vector3.Dot(ac, cp);

        if (d6 >= 0f && d5 <= d6)
        {
            return c; // barycentric coordinates (0,0,1)
        }
        
        // Check if P in edge region of AC, if so return projection of P onto AC
        float vb = d5 * d2 - d1 * d6;
        if (vb <= 0f && d2 >= 0f && d6 <= 0f)
        {
            float w = d2 / (d2 - d6);
            return a + w * ac; // barycentric coordinates (1-w,0,w)
        }
        
        // Check if P in edge region of BC, if so return projection of P onto BC
        float va = d3 * d6 - d5 * d4;
        if (va <= 0f && (d4 - d3) >= 0f && (d5 - d6) >= 0f)
        {
            float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
            return b + w * (c - b); // barycentric coordinates (0,1-w,w)
        }

        {
            // P inside face region. Compute Q through its barycentric coordinates (u,v,w)
            float denom = 1f / (va + vb + vc);
            float v = vb * denom;
            float w = vc * denom;
            return a + ab * v + ac * w; // = u*a + v*b + w*c, u = va * denom = 1f - v - w 
        }
    }
    
    /*
    private static Vector3 ClosestPointOnSurface(TerrainCollider collider, Vector3 to, float radius, bool debug=false)
    {
        var terrainData = collider.terrainData;

        var local = collider.transform.InverseTransformPoint(to);

        // Calculate the size of each tile on the terrain horizontally and vertically
        float pixelSizeX = terrainData.size.x / (terrainData.heightmapResolution - 1);
        float pixelSizeZ = terrainData.size.z / (terrainData.heightmapResolution - 1);

        var percentZ = Mathf.Clamp01(local.z / terrainData.size.z);
        var percentX = Mathf.Clamp01(local.x / terrainData.size.x);

        float positionX = percentX * (terrainData.heightmapResolution - 1);
        float positionZ = percentZ * (terrainData.heightmapResolution - 1);

        // Calculate our position, in tiles, on the terrain
        int pixelX = Mathf.FloorToInt(positionX);
        int pixelZ = Mathf.FloorToInt(positionZ);

        // Calculate the distance from our point to the edge of the tile we are in
        float distanceX = (positionX - pixelX) * pixelSizeX;
        float distanceZ = (positionZ - pixelZ) * pixelSizeZ;

        // Find out how many tiles we are overlapping on the X plane
        float radiusExtentsLeftX = radius - distanceX;
        float radiusExtentsRightX = radius - (pixelSizeX - distanceX);

        int overlappedTilesXLeft = radiusExtentsLeftX > 0 ? Mathf.FloorToInt(radiusExtentsLeftX / pixelSizeX) + 1 : 0;
        int overlappedTilesXRight = radiusExtentsRightX > 0 ? Mathf.FloorToInt(radiusExtentsRightX / pixelSizeX) + 1 : 0;

        // Find out how many tiles we are overlapping on the Z plane
        float radiusExtentsLeftZ = radius - distanceZ;
        float radiusExtentsRightZ = radius - (pixelSizeZ - distanceZ);

        int overlappedTilesZLeft = radiusExtentsLeftZ > 0 ? Mathf.FloorToInt(radiusExtentsLeftZ / pixelSizeZ) + 1 : 0;
        int overlappedTilesZRight = radiusExtentsRightZ > 0 ? Mathf.FloorToInt(radiusExtentsRightZ / pixelSizeZ) + 1 : 0;

        // Retrieve the heights of the pixels we are testing against
        int startPositionX = pixelX - overlappedTilesXLeft;
        int startPositionZ = pixelZ - overlappedTilesZLeft;

        int numberOfXPixels = overlappedTilesXRight + overlappedTilesXLeft + 1;
        int numberOfZPixels = overlappedTilesZRight + overlappedTilesZLeft + 1;

        // Account for if we are off the terrain
        if (startPositionX < 0)
        {
            numberOfXPixels -= Mathf.Abs(startPositionX);
            startPositionX = 0;
        }

        if (startPositionZ < 0)
        {
            numberOfZPixels -= Mathf.Abs(startPositionZ);
            startPositionZ = 0;
        }

        if (startPositionX + numberOfXPixels + 1 > terrainData.heightmapResolution)
        {
            numberOfXPixels = terrainData.heightmapResolution - startPositionX - 1;
        }

        if (startPositionZ + numberOfZPixels + 1 > terrainData.heightmapResolution)
        {
            numberOfZPixels = terrainData.heightmapResolution - startPositionZ - 1;
        }

        // Retrieve the heights of the tile we are in and all overlapped tiles
        var heights = terrainData.GetHeights(startPositionX, startPositionZ, numberOfXPixels + 1, numberOfZPixels + 1);

        // Pre-scale the heights data to be world-scale instead of 0...1
        for (int i = 0; i < numberOfXPixels + 1; i++)
        {
            for (int j = 0; j < numberOfZPixels + 1; j++)
            {
                heights[j, i] *= terrainData.size.y;
            }
        }

        // Find the shortest distance to any triangle in the set gathered
        float shortestDistance = float.MaxValue;

        Vector3 shortestPoint = Vector3.zero;

        for (int x = 0; x < numberOfXPixels; x++)
        {
            for (int z = 0; z < numberOfZPixels; z++)
            {
                // Build the set of points that creates the two triangles that form this tile
                Vector3 a = new Vector3((startPositionX + x) * pixelSizeX, heights[z, x], (startPositionZ + z) * pixelSizeZ);
                Vector3 b = new Vector3((startPositionX + x + 1) * pixelSizeX, heights[z, x + 1], (startPositionZ + z) * pixelSizeZ);
                Vector3 c = new Vector3((startPositionX + x) * pixelSizeX, heights[z + 1, x], (startPositionZ + z + 1) * pixelSizeZ);
                Vector3 d = new Vector3((startPositionX + x + 1) * pixelSizeX, heights[z + 1, x + 1], (startPositionZ + z + 1) * pixelSizeZ);

                Vector3 nearest;

                BSPTree.ClosestPointOnTriangleToPoint(ref a, ref d, ref c, ref local, out nearest);

                float distance = (local - nearest).sqrMagnitude;

                if (distance <= shortestDistance)
                {
                    shortestDistance = distance;
                    shortestPoint = nearest;
                }

                BSPTree.ClosestPointOnTriangleToPoint(ref a, ref b, ref d, ref local, out nearest);

                distance = (local - nearest).sqrMagnitude;

                if (distance <= shortestDistance)
                {
                    shortestDistance = distance;
                    shortestPoint = nearest;
                }

                if (debug)
                {
                    DebugExt.DrawTriangle(a, d, c, Color.cyan);
                    DebugExt.DrawTriangle(a, b, d, Color.red);
                }
            }
        }

        return collider.transform.TransformPoint(shortestPoint);
    }*/
}