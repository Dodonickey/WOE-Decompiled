using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class GpcWrapper
{
    private enum gpc_op
    {
        GPC_DIFF = 0,
        GPC_INT = 1,
        GPC_XOR = 2,
        GPC_UNION = 3
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct gpc_vertex
    {
        public double x;
        public double y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct gpc_vertex_list
    {
        public int num_vertices;
        public IntPtr vertex;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct gpc_polygon
    {
        public int num_contours;
        public IntPtr hole;
        public IntPtr contour;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct gpc_tristrip
    {
        public int num_strips;
        public IntPtr strip;
    }

#if UNITY_IPHONE || UNITY_STANDALONE_OSX
    private const string lookFrom = "__Internal";
#else
    private const string lookFrom = "chipmunk";
#endif

    public static Tristrip PolygonToTristrip(Polygon polygon)
    {
        gpc_tristrip tristrip = default(gpc_tristrip);
        gpc_polygon polygon2 = PolygonTo_gpc_polygon(polygon);
        gpc_polygon_to_tristrip(ref polygon2, ref tristrip);
        Tristrip result = gpc_strip_ToTristrip(tristrip);
        Free_gpc_polygon(polygon2);
        gpc_free_tristrip(ref tristrip);
        return result;
    }

    public static Tristrip ClipToTristrip(GpcOperation operation, Polygon subject_polygon, Polygon clip_polygon)
    {
        gpc_tristrip result_tristrip = default(gpc_tristrip);
        gpc_polygon subject_polygon2 = PolygonTo_gpc_polygon(subject_polygon);
        gpc_polygon clip_polygon2 = PolygonTo_gpc_polygon(clip_polygon);
        gpc_tristrip_clip(operation, ref subject_polygon2, ref clip_polygon2, ref result_tristrip);
        Tristrip result = gpc_strip_ToTristrip(result_tristrip);
        Free_gpc_polygon(subject_polygon2);
        Free_gpc_polygon(clip_polygon2);
        gpc_free_tristrip(ref result_tristrip);
        return result;
    }

    public static Polygon Clip(GpcOperation operation, Polygon subject_polygon, Polygon clip_polygon)
    {
        gpc_polygon result_polygon = default(gpc_polygon);
        gpc_polygon subject_polygon2 = PolygonTo_gpc_polygon(subject_polygon);
        gpc_polygon clip_polygon2 = PolygonTo_gpc_polygon(clip_polygon);
        gpc_polygon_clip(operation, ref subject_polygon2, ref clip_polygon2, ref result_polygon);
        Polygon result = gpc_polygon_ToPolygon(result_polygon);
        Free_gpc_polygon(subject_polygon2);
        Free_gpc_polygon(clip_polygon2);
        gpc_free_polygon(ref result_polygon);
        return result;
    }

    private static gpc_polygon PolygonTo_gpc_polygon(Polygon polygon)
    {
        gpc_polygon result = new gpc_polygon
        {
            num_contours = polygon.NofContours
        };

        int contourCount = polygon.NofContours;
        if (contourCount == 0)
        {
            return result;
        }

        int[] array = new int[contourCount];
        for (int i = 0; i < contourCount; i++)
        {
            array[i] = polygon.ContourIsHole[i] ? 1 : 0;
        }

        result.hole = Marshal.AllocCoTaskMem(contourCount * Marshal.SizeOf(typeof(int)));
        Marshal.Copy(array, 0, result.hole, contourCount);

        int vlSize = Marshal.SizeOf(typeof(gpc_vertex_list));
        int vSize = Marshal.SizeOf(typeof(gpc_vertex));

        result.contour = Marshal.AllocCoTaskMem(contourCount * vlSize);
        IntPtr contourPtr = result.contour;

        for (int j = 0; j < contourCount; j++)
        {
            int vertexCount = polygon.Contour[j].NofVertices;
            gpc_vertex_list vl = new gpc_vertex_list
            {
                num_vertices = vertexCount,
                vertex = Marshal.AllocCoTaskMem(vertexCount * vSize)
            };

            IntPtr vertexPtr = vl.vertex;
            for (int k = 0; k < vertexCount; k++)
            {
                gpc_vertex v = new gpc_vertex
                {
                    x = polygon.Contour[j].Vertex[k].x,
                    y = polygon.Contour[j].Vertex[k].y
                };
                Marshal.StructureToPtr(v, vertexPtr, false);
                vertexPtr = new IntPtr(vertexPtr.ToInt64() + vSize);
            }

            Marshal.StructureToPtr(vl, contourPtr, false);
            contourPtr = new IntPtr(contourPtr.ToInt64() + vlSize);
        }

        return result;
    }

    private static Polygon gpc_polygon_ToPolygon(gpc_polygon gpc_polygon)
    {
        Polygon polygon = new Polygon();
        int contourCount = gpc_polygon.num_contours;
        polygon.NofContours = contourCount;
        polygon.ContourIsHole = new bool[contourCount];
        polygon.Contour = new VertexList[contourCount];

        if (contourCount == 0)
        {
            return polygon;
        }

        int vlSize = Marshal.SizeOf(typeof(gpc_vertex_list));
        int vSize = Marshal.SizeOf(typeof(gpc_vertex));

        int[] array = new int[contourCount];
        Marshal.Copy(gpc_polygon.hole, array, 0, contourCount);

        for (int i = 0; i < contourCount; i++)
        {
            polygon.ContourIsHole[i] = array[i] != 0;
        }

        IntPtr contourPtr = gpc_polygon.contour;
        for (int j = 0; j < contourCount; j++)
        {
            gpc_vertex_list vl = (gpc_vertex_list)Marshal.PtrToStructure(contourPtr, typeof(gpc_vertex_list));
            polygon.Contour[j] = new VertexList();
            polygon.Contour[j].NofVertices = vl.num_vertices;
            polygon.Contour[j].Vertex = new Vector2[vl.num_vertices];

            IntPtr vertexPtr = vl.vertex;
            for (int k = 0; k < vl.num_vertices; k++)
            {
                gpc_vertex v = (gpc_vertex)Marshal.PtrToStructure(vertexPtr, typeof(gpc_vertex));
                polygon.Contour[j].Vertex[k].x = (float)v.x;
                polygon.Contour[j].Vertex[k].y = (float)v.y;
                vertexPtr = new IntPtr(vertexPtr.ToInt64() + vSize);
            }

            contourPtr = new IntPtr(contourPtr.ToInt64() + vlSize);
        }

        return polygon;
    }

    private static Tristrip gpc_strip_ToTristrip(gpc_tristrip gpc_strip)
    {
        Tristrip tristrip = new Tristrip();
        int stripCount = gpc_strip.num_strips;
        tristrip.NofStrips = stripCount;
        tristrip.Strip = new VertexList[stripCount];

        if (stripCount == 0)
        {
            return tristrip;
        }

        int vlSize = Marshal.SizeOf(typeof(gpc_vertex_list));
        int vSize = Marshal.SizeOf(typeof(gpc_vertex));

        IntPtr stripPtr = gpc_strip.strip;
        for (int i = 0; i < stripCount; i++)
        {
            tristrip.Strip[i] = new VertexList();
            gpc_vertex_list vl = (gpc_vertex_list)Marshal.PtrToStructure(stripPtr, typeof(gpc_vertex_list));
            tristrip.Strip[i].NofVertices = vl.num_vertices;
            tristrip.Strip[i].Vertex = new Vector2[vl.num_vertices];

            IntPtr vertexPtr = vl.vertex;
            for (int j = 0; j < vl.num_vertices; j++)
            {
                gpc_vertex v = (gpc_vertex)Marshal.PtrToStructure(vertexPtr, typeof(gpc_vertex));
                tristrip.Strip[i].Vertex[j].x = (float)v.x;
                tristrip.Strip[i].Vertex[j].y = (float)v.y;
                vertexPtr = new IntPtr(vertexPtr.ToInt64() + vSize);
            }

            stripPtr = new IntPtr(stripPtr.ToInt64() + vlSize);
        }

        return tristrip;
    }

    private static void Free_gpc_polygon(gpc_polygon gpc_pol)
    {
        if (gpc_pol.hole != IntPtr.Zero)
        {
            Marshal.FreeCoTaskMem(gpc_pol.hole);
        }

        if (gpc_pol.contour != IntPtr.Zero)
        {
            int vlSize = Marshal.SizeOf(typeof(gpc_vertex_list));
            IntPtr contourPtr = gpc_pol.contour;

            for (int i = 0; i < gpc_pol.num_contours; i++)
            {
                gpc_vertex_list vl = (gpc_vertex_list)Marshal.PtrToStructure(contourPtr, typeof(gpc_vertex_list));
                if (vl.vertex != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(vl.vertex);
                }
                contourPtr = new IntPtr(contourPtr.ToInt64() + vlSize);
            }

            Marshal.FreeCoTaskMem(gpc_pol.contour);
        }
    }

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gpc_polygon_to_tristrip(ref gpc_polygon polygon, ref gpc_tristrip tristrip);

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gpc_polygon_clip(GpcOperation set_operation, ref gpc_polygon subject_polygon, ref gpc_polygon clip_polygon, ref gpc_polygon result_polygon);

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gpc_tristrip_clip(GpcOperation set_operation, ref gpc_polygon subject_polygon, ref gpc_polygon clip_polygon, ref gpc_tristrip result_tristrip);

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gpc_free_tristrip(ref gpc_tristrip tristrip);

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gpc_free_polygon(ref gpc_polygon polygon);

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gpc_read_polygon(IntPtr fp, int read_hole_flags, ref gpc_polygon polygon);

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gpc_write_polygon(IntPtr fp, int write_hole_flags, ref gpc_polygon polygon);
}