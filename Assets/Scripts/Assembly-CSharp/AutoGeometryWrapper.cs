using System;
using System.Runtime.InteropServices;
using UnityEngine;

internal static class AutoGeometryWrapper //Same as chipmunk i guess
{

#if UNITY_IPHONE || UNITY_STANDALONE_OSX
    private const string lookFrom = "__Internal";
#else
    private const string lookFrom = "chipmunk";
#endif

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr agSimpleSamplerNew(int width, int height, cpBB outputRect, byte[] data);

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    public static extern void agSimpleSamplerFree(IntPtr sampler);

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    public static extern void agSimpleSamplerSetProperties(IntPtr sampler, float marchTreshold);

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    public static extern int agCachedTileGetShapeCount(IntPtr tile);

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    public static extern void agCachedTileGetShapeList(IntPtr tile, IntPtr[] shapeListArray);

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr agCachedTileGetPolylineSet(IntPtr tile);

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    public static extern cpBB agCachedTileGetBB(IntPtr tile);

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    public static extern void agCachedTileSetDirty(IntPtr tile, bool dirty);

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    public static extern int agPolylineSetGetLineCount(IntPtr set);

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    public static extern void agPolylineSetGetLines(IntPtr set, IntPtr[] polylineArray);

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    public static extern int agPolylineGetVertCount(IntPtr polyline);

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    public static extern void agPolyLineGetVertices(IntPtr polyline, Vector2[] vertexArray);

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool agPolylineIsLooped(IntPtr polyline);

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr agBasicTileCacheNew(IntPtr sampler, IntPtr space, float tileSize, float samplesPerTile, int cacheSize, bool createShapes, bool extendedDirtyList);

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    public static extern void agBasicTileCacheFree(IntPtr cache);

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    public static extern void agBasicTileCacheResetCache(IntPtr cache);

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    public static extern void agBasicTileCacheMarkDirtyRect(IntPtr cache, cpBB bounds);

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    public static extern void agBasicTileCacheEnsureRect(IntPtr cache, cpBB bounds);

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    public static extern int agBasicTileCacheGetTileCountInRect(IntPtr cache, cpBB bounds);

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    public static extern void agBasicTileCacheGetTilesInRect(IntPtr cache, cpBB bounds, IntPtr[] tileListArray);

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    public static extern void agBasicTileCacheSetOffsets(IntPtr cache, Vector2 worldOffset, Vector2 tileOffset);

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    public static extern void agBasicTileCacheSetMarchProperties(IntPtr cache, bool marchHard, float simplifyTreshold);

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    public static extern void agBasicTileCacheSetSegmentProperties(IntPtr cache, float segmentRadius, float segmentFriction, float segmentElasticity, uint segmentGroup, uint segmentLayers, uint segmentCollisionType);

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    public static extern int agBasicTileCacheGetDirtyTileCount(IntPtr cache);

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    public static extern void agBasicTileCacheGetDirtyTileList(IntPtr cache, IntPtr[] tileListArray);
}