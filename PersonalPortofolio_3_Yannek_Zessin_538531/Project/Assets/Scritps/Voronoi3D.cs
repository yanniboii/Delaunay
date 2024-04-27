using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class Voronoi3D : MonoBehaviour
{
    [SerializeField] float size = 0;
    [SerializeField] float cellSize;
    [SerializeField] int regionAmount = 0;
    [SerializeField] float tolerance;
    [SerializeField] bool reverse = false;
    [SerializeField] GameObject Cube;
    List<Vector3> points = new List<Vector3>();
    List<GameObject> cubes = new List<GameObject>();
    private float lastCellSize;
    private int lastRegionAmount = 0;
    private float lastTolerance;
    private bool lastReverse = false;

    // Start is called before the first frame update
    void Start()
    {
        GenVoronoi3D();
        VoxelizeVoronoi();
        lastTolerance = tolerance;
        lastCellSize = size;
        lastRegionAmount = regionAmount;
        lastReverse = reverse;
        //List<Tetrahedron> triangles = DelaunayTriangulation();
        //DrawDelaunayLines(triangles);
        //ShootRays();
    }

    // Update is called once per frame
    void Update()
    {
        if(lastReverse != reverse ||  lastCellSize != cellSize || lastTolerance != tolerance)
        {

            for (int i = cubes.Count()-1; i > 1; i--)
            {
                Destroy(cubes[i]);
            }
            VoxelizeVoronoi();
            lastTolerance = tolerance;
            lastCellSize = cellSize;
            lastReverse = reverse;
        }
        if(lastRegionAmount != regionAmount)
        {
            points.Clear();
            GenVoronoi3D();
            lastRegionAmount = regionAmount;
            for (int i = cubes.Count() - 1; i > 1; i--)
            {
                Destroy(cubes[i]);
            }
            VoxelizeVoronoi();
            lastTolerance = tolerance;
            lastCellSize = cellSize;
            lastReverse = reverse;
        }
    }

    void GenVoronoi3D()
    {
        for (int i = 0; i < regionAmount; i++)
        {
            points.Add(new Vector3(Random.Range(-size, size), Random.Range(-size, size), Random.Range(-size, size)));
        }
    }

    void VoxelizeVoronoi()
    {
        float effectiveCellSize = Mathf.Abs(cellSize);

        for (int  x = (int)-size; x < size; x++)
        {
            for (int y = (int)-size; y < size; y++)
            {
                for (int z = (int)-size; z < size; z++)
                {

                    float distance = float.MaxValue;

                    for (int i = 0; i < regionAmount; i++)
                    {
                        float pointDistance = Vector3.Distance(new Vector3(x, y, z), points[i]);
                        distance = Mathf.Min(distance, pointDistance);
                    }



                    float normalizedDistance = distance / effectiveCellSize;

                    if (reverse)
                    {
                        if (normalizedDistance > tolerance)
                        {
                            GameObject go =Instantiate(Cube, new Vector3(x, y, z), Quaternion.identity);
                            cubes.Add(go);
                        }
                    }
                    else
                    {
                        if (normalizedDistance < tolerance)
                        {
                            GameObject go = Instantiate(Cube, new Vector3(x, y, z), Quaternion.identity);
                            cubes.Add(go);
                        }
                    }

                }
            }
        }
    }

    void ShootRays()
    {
        List<Vector3> pointsToRemove = new List<Vector3>();

        Physics.queriesHitBackfaces = true;

        for (int i = 0; i < points.Count(); i++)
        {

            Vector3 randomDirection = Random.rotation.eulerAngles;

            RaycastHit[] hits = Physics.RaycastAll(points[i], randomDirection);

            int hitCount = 0;
            Debug.DrawRay(points[i], randomDirection, Color.red, Mathf.Infinity);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.gameObject == gameObject)
                {
                    hitCount++;
                }
            }

            if (hitCount % 2 == 0)
            {
                Debug.Log("Point is outside the mesh");
                pointsToRemove.Add(points[i]);
            }
            else
            {
                Debug.Log("Point is inside the mesh");
            }
        }
        foreach (Vector3 pointToRemove in pointsToRemove)
        {
            points.Remove(pointToRemove);
        }
    }

    List<Tetrahedron> DelaunayTriangulation()
    {
        List<Vector3> pointList = new List<Vector3>(points);
        List<Tetrahedron> triangleList = new List<Tetrahedron>();

        Vector3[] verts = new Vector3[8];

        verts[0] = new Vector3(-1, -1, -1);
        verts[1] = new Vector3(1, -1, -1);
        verts[2] = new Vector3(1, 1, -1);
        verts[3] = new Vector3(-1, 1, -1);
        verts[4] = new Vector3(1, 1, 1);
        verts[5] = new Vector3(1, -1, 1);
        verts[6] = new Vector3(-1, -1, 1);
        verts[7] = new Vector3(-1, 1, 1);

        for(int i = 0; i < verts.Length; i++)
        {
            verts[i] *= (size);
        }
        pointList.AddRange(verts);


        float minX = pointList[0].x, minY = pointList[0].y, minZ = pointList[0].z, maxX = pointList[0].x, maxY = pointList[0].y, maxZ = pointList[0].z;
        for (int i = 1; i < pointList.Count; i++)
        {
            if (pointList[i].x < minX) minX = pointList[i].x;
            if (pointList[i].y < minY) minY = pointList[i].y;
            if (pointList[i].z < minZ) minZ = pointList[i].z;
            if (pointList[i].x > maxX) maxX = pointList[i].x;
            if (pointList[i].y > maxY) maxY = pointList[i].y;
            if (pointList[i].z > maxZ) maxZ = pointList[i].z;
        }

        float dx = maxX - minX;
        float dy = maxY - minY;
        float dz = maxZ - minZ;
        float deltaMax = Mathf.Max(dx, dy, dz);
        Vector3 mid = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, (minZ + maxZ) / 2f);

        Vector3 p1 = new Vector3(mid.x - 2 * deltaMax, mid.y - 2 * deltaMax, mid.z - 2 * deltaMax);
        Vector3 p2 = new Vector3(mid.x, mid.y + 4 * deltaMax, mid.z);
        Vector3 p3 = new Vector3(mid.x + 2 * deltaMax, mid.y - 2 * deltaMax, mid.z + 2 * deltaMax);
        Vector3 p4 = new Vector3(mid.x, mid.y, mid.z - 4 * deltaMax);

        Tetrahedron superTriangle = new Tetrahedron(p1, p2, p3, p4);
        triangleList.Add(superTriangle);

        //for(int i = 0; i < 6; i++)
        //{
        //    triangleList.Add(new Triangle3(verts[i], verts[i+1], p3));
        //}

        while (pointList.Count > 0)
        {
            Vector3 point = pointList[0];
            pointList.RemoveAt(0); // Remove the processed point from the list

            List<Tetrahedron> badTriangles = new List<Tetrahedron>();

            foreach (Tetrahedron triangle in triangleList)
            {
                if (triangle.CircumsphereContains(point))
                {
                    badTriangles.Add(triangle);
                }
            }

            List<Edge3> polygon = new List<Edge3>();

            foreach (Tetrahedron triangle in badTriangles)
            {
                foreach (Edge3 edge in triangle.GetEdges())
                {
                    bool shared = false;
                    foreach (Tetrahedron other in badTriangles)
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

            foreach (Tetrahedron triangle in badTriangles)
            {
                triangleList.Remove(triangle);
            }

            foreach (Edge3 edge in polygon)
            {
                Vector3 centroid = (edge.point1 + edge.point2 + point) / 3f;

                triangleList.Add(new Tetrahedron(edge.point1, edge.point2, point, centroid));
            }
        }

        return triangleList;
    }

    void DrawDelaunayLines(List<Tetrahedron> triangles)
    {
        foreach (Tetrahedron triangle in triangles)
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
            Gizmos.DrawSphere(point, 0.2f);
        }
    }
}


public struct Tetrahedron
{
    public Vector3 point1;
    public Vector3 point2;
    public Vector3 point3;
    public Vector3 point4;

    public Tetrahedron(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        point1 = p1;
        point2 = p2;
        point3 = p3;
        point4 = p4;
    }

    public bool HasVertex(Vector3 vertex)
    {
        return vertex == point1 || vertex == point2 || vertex == point3 || vertex == point4;
    }

    public bool IsDifferentFrom(Tetrahedron other)
    {
        return !((point1 == other.point1 || point1 == other.point2 || point1 == other.point3 || point1 == other.point4) &&
                 (point2 == other.point1 || point2 == other.point2 || point2 == other.point3 || point2 == other.point4) &&
                 (point3 == other.point1 || point3 == other.point2 || point3 == other.point3 || point3 == other.point4) &&
                 (point4 == other.point1 || point4 == other.point2 || point4 == other.point3 || point4 == other.point4));
    }

    public List<Edge3> GetEdges()
    {
        List<Edge3> edges = new List<Edge3>
        {
            new Edge3(point1, point2),
            new Edge3(point1, point3),
            new Edge3(point1, point4),
            new Edge3(point2, point3),
            new Edge3(point2, point4),
            new Edge3(point3, point4)
        };
        return edges;
    }

    public bool CircumsphereContains(Vector3 point)
    {
        float ab = (point1.x * point1.x) + (point1.y * point1.y) + (point1.z * point1.z);
        float cd = (point2.x * point2.x) + (point2.y * point2.y) + (point2.z * point2.z);
        float ef = (point3.x * point3.x) + (point3.y * point3.y) + (point3.z * point3.z);

        float circumX = (ab * (point2.y - point3.y) + cd * (point3.y - point1.y) + ef * (point1.y - point2.y)) / (2f * (point1.x * (point2.y - point3.y) - point1.y * (point2.x - point3.x) + point2.x * point3.y - point3.x * point2.y));
        float circumY = (ab * (point3.x - point2.x) + cd * (point1.x - point3.x) + ef * (point2.x - point1.x)) / (2f * (point1.x * (point2.y - point3.y) - point1.y * (point2.x - point3.x) + point2.x * point3.y - point3.x * point2.y));
        float circumZ = (ab * (point2.z - point3.z) + cd * (point3.z - point1.z) + ef * (point1.z - point2.z)) / (2f * (point1.x * (point2.y - point3.y) - point1.y * (point2.x - point3.x) + point2.x * point3.y - point3.x * point2.y));
        Vector3 circumcenter = new Vector3(circumX, circumY, circumZ);

        float radiusSquared = Mathf.Pow(point1.x - circumcenter.x, 2) + Mathf.Pow(point1.y - circumcenter.y, 2) + Mathf.Pow(point1.z - circumcenter.z, 2);
        float distSquared = Mathf.Pow(point.x - circumcenter.x, 2) + Mathf.Pow(point.y - circumcenter.y, 2) + Mathf.Pow(point.z - circumcenter.z, 2);

        return distSquared <= radiusSquared;
    }
}

public struct Edge3
{
    public Vector3 point1;
    public Vector3 point2;

    public Edge3(Vector3 p1, Vector3 p2)
    {
        point1 = p1;
        point2 = p2;
    }

    public bool Equals(Edge3 other)
    {
        return (point1 == other.point1 && point2 == other.point2) ||
               (point1 == other.point2 && point2 == other.point1);
    }
}