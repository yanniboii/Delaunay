using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class PlaneVoronoiColor : MonoBehaviour
{
    int seed = 0;
    enum type
    {
        colored,
        blackToWhite,
        whiteToBlack
    }

    [SerializeField] int size = 0;
    [SerializeField] int regionAmount = 0;
    [SerializeField] int regionColorAmount = 0;
    [SerializeField] float cellSize;

    [SerializeField] type interpolationType = type.blackToWhite;

    [SerializeField] bool generate = false;
    [SerializeField] bool addRigidbody  = false;

    Vector2[] points;


    public Material material;

    // Start is called before the first frame update
    void Start()
    {
        if (interpolationType == type.colored)
        {
            SetColors(out points);
            List<Triangle> triangles = DelaunayTriangulation(points.ToList());
            DrawDelaunayLines(triangles);
        }
        else if(interpolationType == type.blackToWhite)
        {
            SetBlackToWhite(out points);
            List<Triangle> triangles = DelaunayTriangulation(points.ToList());
            DrawDelaunayLines(triangles);
        }
        else if (interpolationType == type.whiteToBlack)
        {
            SetWhiteToBlack(out points);
        }


    }

    // Update is called once per frame
    void Update()
    {
        if (generate)
        {
            List<Triangle> triangles = DelaunayTriangulation(points.ToList());
            DrawDelaunayLines(triangles);
            GenerateNewMeshes(triangles);
        }
    }

    List<Triangle> DelaunayTriangulation(List<Vector2> points)
    {
        List<Triangle> triangleList = new List<Triangle>();
        Vector3[] verts = GetComponent<MeshFilter>().mesh.vertices;
        Vector2[] verts2 = new Vector2[verts.Length];
        for(int i = 0; i < verts.Length; i++)
        {
            verts2[i] = new Vector2(verts[i].x*25.6f, verts[i].z * 25.6f);
            verts2[i] += new Vector2(size / 2, size / 2);
        }
        points.AddRange(verts2);
        points[0] += new Vector2(this.transform.position.x - size/2, this.transform.position.z - size / 2);
        float minX = points[0].x,
              minY = points[0].y, 
              maxX = points[0].x, 
              maxY = points[0].y;
        for (int i = 1; i < points.Count; i++)
        {
            points[i] += new Vector2(this.transform.position.x - size / 2, this.transform.position.z - size / 2);
            if (points[i].x < minX) minX = points[i].x;
            if (points[i].y < minY) minY = points[i].y;
            if (points[i].x > maxX) maxX = points[i].x;
            if (points[i].y > maxY) maxY = points[i].y;
        }
        float dx = maxX - minX;
        float dy = maxY - minY;
        float deltaMax = Mathf.Max(dx, dy);
        Vector2 mid = new Vector2((minX + maxX) / 2f, (minY + maxY) / 2f);

        Vector2 p1 = new Vector2(mid.x - 2 * deltaMax, mid.y - deltaMax);
        Vector2 p2 = new Vector2(mid.x, mid.y + 2 * deltaMax);
        Vector2 p3 = new Vector2(mid.x + 2 * deltaMax, mid.y - deltaMax);

        Triangle superTriangle = new Triangle(p1, p2, p3);
        triangleList.Add(superTriangle);

        for (int p = 0; p < points.Count; p++)
        {
            List<Triangle> badTriangles = new List<Triangle>();

            foreach (Triangle triangle in triangleList)
            {
                if (triangle.CircumcircleContains(points[p]))
                {
                    badTriangles.Add(triangle);
                }
            }

            List<Edge> polygon = new List<Edge>();

            foreach (Triangle triangle in badTriangles)
            {
                foreach (Edge edge in triangle.GetEdges())
                {
                    bool shared = false;
                    foreach (Triangle other in badTriangles)
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

            foreach (Triangle triangle in badTriangles)
            {
                triangleList.Remove(triangle);
            }

            foreach (Edge edge in polygon)
            {
                triangleList.Add(new Triangle(edge.point1, edge.point2, points[p]));
            }
        }

        triangleList.RemoveAll(triangle => triangle.HasVertex(p1) || triangle.HasVertex(p2) || triangle.HasVertex(p3));

        return triangleList;
    }

    void DrawDelaunayLines(List<Triangle> triangles)
    {
        foreach (Triangle triangle in triangles)
        {
            Debug.DrawLine(new Vector3(triangle.point1.x, 0, triangle.point1.y), new Vector3(triangle.point2.x, 0, triangle.point2.y), Color.red, Mathf.Infinity);
            Debug.DrawLine(new Vector3(triangle.point2.x, 0, triangle.point2.y), new Vector3(triangle.point3.x, 0, triangle.point3.y), Color.red, Mathf.Infinity);
            Debug.DrawLine(new Vector3(triangle.point3.x, 0, triangle.point3.y), new Vector3(triangle.point1.x, 0, triangle.point1.y), Color.red, Mathf.Infinity);

        }
    }

    void GenerateNewMeshes(List<Triangle> delaunayTriangles)
    {
        for(int i = 0; i <  delaunayTriangles.Count; i++)
        {
            GameObject gameObject = new GameObject("broken");
            gameObject.transform.localScale = new Vector3(1, 0.1f, 1);
            Mesh mesh = new Mesh();
            Vector3[] vertices = new Vector3[3];
            int[] triangles = new int[3];

            vertices[0] = new Vector3(delaunayTriangles[i].point1.x, 5, delaunayTriangles[i].point1.y);
            vertices[1] = new Vector3(delaunayTriangles[i].point2.x, 5, delaunayTriangles[i].point2.y);
            vertices[2] = new Vector3(delaunayTriangles[i].point3.x, 5, delaunayTriangles[i].point3.y);
            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;
            if (vertices[0] == vertices[1] || vertices[0] == vertices[2] || vertices[1] == vertices[2])
            {
                Destroy(gameObject);
                continue;
            }
            MeshFilter filter = gameObject.AddComponent<MeshFilter>();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            filter.mesh = mesh;
            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = material;
            MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.convex = true;
            if(addRigidbody)
            {
                Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();

            }
            //rigidbody.isKinematic = true;
        }
        Destroy(this.gameObject);
    }

    void SetColors(out Vector2[] points)
    {
        points = new Vector2[regionAmount];

        Color[] regionColors = new Color[regionColorAmount];


        Color[] colors = new Color[size*size];

        for(int i = 0; i < regionAmount; i++)
        {
            points[i] = new Vector2(Random.Range(0,size),Random.Range(0,size));
        }

        for (int i = 0; i < regionAmount; i++)
        {
            regionColors[i] = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        }

        for (int y = 0; y < size; y++)
        {
            for(int x = 0; x < size; x++)
            {
                float distance = float.MaxValue;
                int value = 0;
                for(int i = 0; i < regionAmount; i++)
                {
                    if(Vector2.Distance(new Vector2(x, y), points[i]) < distance)
                    {
                        distance = Vector2.Distance(new Vector2(x, y), points[i]);
                        value = i;
                    }
                }
                // Calculate the distance percentage
                float distancePercentage = (distance / size) * 100f;

                colors[x + y * size] = regionColors[value%regionColorAmount];
            }
        }
        Texture2D voronoiTexture = new Texture2D(size,size);
        voronoiTexture.SetPixels(colors);
        voronoiTexture.Apply();
        GetComponent<MeshRenderer>().material.SetTexture("_Texture2D", voronoiTexture);
    }

    void SetBlackToWhite(out Vector2[] points)
    {
        points = new Vector2[regionAmount];

        Color[] colors = new Color[size * size];

        for (int i = 0; i < regionAmount; i++)
        {
            points[i] = new Vector2(Random.Range(0, size), Random.Range(0, size));
        }

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = float.MaxValue;
                for (int i = 0; i < regionAmount; i++)
                {
                    if (Vector2.Distance(new Vector2(x, y), points[i]) < distance)
                    {
                        distance = Vector2.Distance(new Vector2(x, y), points[i]);
                    }
                }
                float normalizedDistance = Mathf.Clamp01(distance / cellSize);

                colors[x + y * size] = Color.Lerp(Color.black, Color.white, normalizedDistance);
                if(normalizedDistance == 0)
                {
                    Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube),new Vector3(x,0,y),Quaternion.identity, this.transform);
                }
            }
        }
        Texture2D voronoiTexture = new Texture2D(size, size);
        voronoiTexture.SetPixels(colors);
        voronoiTexture.Apply();
        GetComponent<MeshRenderer>().material.SetTexture("_Texture2D", voronoiTexture);
    }

    void SetWhiteToBlack(out Vector2[] points)
    {
        points = new Vector2[regionAmount];

        Color[] colors = new Color[size * size];

        for (int i = 0; i < regionAmount; i++)
        {
            points[i] = new Vector2(Random.Range(0, size), Random.Range(0, size));
        }

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = float.MaxValue;
                for (int i = 0; i < regionAmount; i++)
                {
                    if (Vector2.Distance(new Vector2(x, y), points[i]) < distance)
                    {
                        distance = Vector2.Distance(new Vector2(x, y), points[i]);
                    }
                }

                float normalizedDistance = Mathf.Clamp01(distance / cellSize);

                colors[x + y * size] = Color.Lerp(Color.white, Color.black, normalizedDistance);
            }
        }
        Texture2D voronoiTexture = new Texture2D(size, size);
        voronoiTexture.SetPixels(colors);
        voronoiTexture.Apply();
        GetComponent<MeshRenderer>().material.SetTexture("_Texture2D", voronoiTexture);
    }
}

public struct Triangle
{
    public Vector2 point1;
    public Vector2 point2;
    public Vector2 point3;

    public Triangle(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        point1 = p1;
        point2 = p2;
        point3 = p3;
    }

    public bool HasVertex(Vector2 vertex)
    {
        return vertex == point1 || vertex == point2 || vertex == point3;
    }

    public bool IsDifferentFrom(Triangle other)
    {
        return !((point1 == other.point1 || point1 == other.point2 || point1 == other.point3) &&
                 (point2 == other.point1 || point2 == other.point2 || point2 == other.point3) &&
                 (point3 == other.point1 || point3 == other.point2 || point3 == other.point3));
    }

    public List<Edge> GetEdges()
    {
        List<Edge> edges = new List<Edge>
        {
            new Edge(point1, point2),
            new Edge(point2, point3),
            new Edge(point3, point1)
        };
        return edges;
    }

    public bool CircumcircleContains(Vector2 point)
    {
        float ab = (point1.x * point1.x) + (point1.y * point1.y);
        float cd = (point2.x * point2.x) + (point2.y * point2.y);
        float ef = (point3.x * point3.x) + (point3.y * point3.y);

        float circumX = (ab * (point2.y - point3.y) + cd * (point3.y - point1.y) + ef * (point1.y - point2.y)) / (2f * (point1.x * (point2.y - point3.y) - point1.y * (point2.x - point3.x) + point2.x * point3.y - point3.x * point2.y));
        float circumY = (ab * (point3.x - point2.x) + cd * (point1.x - point3.x) + ef * (point2.x - point1.x)) / (2f * (point1.x * (point2.y - point3.y) - point1.y * (point2.x - point3.x) + point2.x * point3.y - point3.x * point2.y));
        Vector2 circumcenter = new Vector2(circumX, circumY);

        float radiusSquared = Mathf.Pow(point1.x - circumcenter.x, 2) + Mathf.Pow(point1.y - circumcenter.y, 2);
        float distSquared = Mathf.Pow(point.x - circumcenter.x, 2) + Mathf.Pow(point.y - circumcenter.y, 2);

        return distSquared <= radiusSquared;
    }
}


public struct Edge
{
    public Vector2 point1;
    public Vector2 point2;

    public Edge(Vector2 p1, Vector2 p2)
    {
        point1 = p1;
        point2 = p2;
    }

    public bool Equals(Edge other)
    {
        return (point1 == other.point1 && point2 == other.point2) ||
               (point1 == other.point2 && point2 == other.point1);
    }
}


