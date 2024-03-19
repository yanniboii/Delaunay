using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Voronoi3D : MonoBehaviour
{
    [SerializeField] int size = 0;
    [SerializeField] int regionAmount = 0;
    Vector3[] points;

    // Start is called before the first frame update
    void Start()
    {
        GenVoronoi3D(out points);
        List<Triangle3> triangles = DelaunayTriangulation(points.ToList());
        DrawDelaunayLines(triangles);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void GenVoronoi3D(out Vector3[] points)
    {
        points = new Vector3[regionAmount];

        for (int i = 0; i < regionAmount; i++)
        {
            points[i] = new Vector3(Random.Range(0, size), Random.Range(0, size), Random.Range(0, size));
        }
    }

    List<Triangle3> DelaunayTriangulation(List<Vector3> points)
    {
        List<Vector3> pointList = new List<Vector3>(points);
        List<Triangle3> triangleList = new List<Triangle3>();

        // Add super triangle large enough to contain all points
        float minX = points[0].x, minY = points[0].y, minZ = points[0].z, maxX = points[0].x, maxY = points[0].y, maxZ = points[0].z;
        for (int i = 1; i < points.Count; i++)
        {
            if (points[i].x < minX) minX = points[i].x;
            if (points[i].y < minY) minY = points[i].y;
            if (points[i].z < minZ) minZ = points[i].z;
            if (points[i].x > maxX) maxX = points[i].x;
            if (points[i].y > maxY) maxY = points[i].y;
            if (points[i].z > maxZ) maxZ = points[i].z;
        }
        float dx = maxX - minX;
        float dy = maxY - minY;
        float dz = maxZ - minZ;
        float deltaMax = Mathf.Max(dx, dy, dz);
        Vector3 mid = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, (minZ + maxZ) / 2f);

        Vector3 p1 = new Vector3(mid.x - 2 * deltaMax, mid.y - 2 * deltaMax, mid.z - 2 * deltaMax);
        Vector3 p2 = new Vector3(mid.x, mid.y + 4 * deltaMax, mid.z);
        Vector3 p3 = new Vector3(mid.x + 2 * deltaMax, mid.y - 2 * deltaMax, mid.z + 2 * deltaMax);

        Triangle3 superTriangle = new Triangle3(p1, p2, p3);
        triangleList.Add(superTriangle);

        for (int p = 0; p < points.Count; p++)
        {
            List<Triangle3> badTriangles = new List<Triangle3>();

            foreach (Triangle3 triangle in triangleList)
            {
                if (triangle.CircumcircleContains(points[p]))
                {
                    badTriangles.Add(triangle);
                }
            }

            List<Edge3> polygon = new List<Edge3>();

            foreach (Triangle3 triangle in badTriangles)
            {
                foreach (Edge3 edge in triangle.GetEdges())
                {
                    bool shared = false;
                    foreach (Triangle3 other in badTriangles)
                    {
                        if (other.IsDifferentFrom(triangle) && other.HasVertex(edge.point1) && other.HasVertex(edge.point2))
                        {
                            shared = true;
                            break;
                        }
                    }

                    if (!shared)
                    {
                        polygon.Add(edge);
                    }
                }
            }

            foreach (Triangle3 triangle in badTriangles)
            {
                triangleList.Remove(triangle);
            }

            foreach (Edge3 edge in polygon)
            {
                triangleList.Add(new Triangle3(edge.point1, edge.point2, points[p]));
            }
        }

        // Remove triangles that share vertices with the super triangle
        triangleList.RemoveAll(triangle => triangle.HasVertex(p1) || triangle.HasVertex(p2) || triangle.HasVertex(p3));

        return triangleList;
    }

    void DrawDelaunayLines(List<Triangle3> triangles)
    {
        foreach (Triangle3 triangle in triangles)
        {
            Debug.DrawLine(new Vector3(triangle.point1.x, triangle.point1.y, triangle.point1.z), triangle.point2, Color.red, Mathf.Infinity);
            Debug.DrawLine(new Vector3(triangle.point2.x, triangle.point2.y, triangle.point2.z), triangle.point3, Color.red, Mathf.Infinity);
            Debug.DrawLine(new Vector3(triangle.point3.x, triangle.point3.y, triangle.point3.z), triangle.point1, Color.red, Mathf.Infinity);

        }


    }

    private void OnDrawGizmos()
    {
        foreach (Vector3 point in points)
        {
            Gizmos.DrawSphere(point, 2);
        }
    }
}
