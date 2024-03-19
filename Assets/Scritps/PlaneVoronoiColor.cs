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


    public Material material;

    // Start is called before the first frame update
    void Start()
    {
        if (interpolationType == type.colored)
        {
            Vector2[] points;
            SetColors(out points);
            List<Triangle> triangles = DelaunayTriangulation(points.ToList());
            DrawDelaunayLines(triangles);
        }
        else if(interpolationType == type.blackToWhite)
        {
            Vector2[] points;
            SetBlackToWhite(out points);
            List<Triangle> triangles = DelaunayTriangulation(points.ToList());
            DrawDelaunayLines(triangles);
        }
        else if (interpolationType == type.whiteToBlack)
        {
            Vector2[] points;
            SetWhiteToBlack(out points);
            List<Triangle> triangles = DelaunayTriangulation(points.ToList());
            DrawDelaunayLines(triangles);
        }


    }

    // Update is called once per frame
    void Update()
    {

    }

    List<Triangle> DelaunayTriangulation(List<Vector2> points)
    {
        List<Vector2> pointList = new List<Vector2>(points);
        List<Triangle> triangleList = new List<Triangle>();

        // Add super triangle large enough to contain all points
        float minX = points[0].x, minY = points[0].y, maxX = points[0].x, maxY = points[0].y;
        for (int i = 1; i < points.Count; i++)
        {
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

        // Remove triangles that share vertices with the super triangle
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
                // Normalize the distance to be between 0 and 1
                float normalizedDistance = Mathf.Clamp01(distance / cellSize);

                // Interpolate between black and white based on normalized distance
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

                // Normalize the distance to be between 0 and 1
                float normalizedDistance = Mathf.Clamp01(distance / cellSize);

                // Interpolate between black and white based on normalized distance
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

public struct Triangle3
{
    public Vector3 point1;
    public Vector3 point2;
    public Vector3 point3;

    public Triangle3(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        point1 = p1;
        point2 = p2;
        point3 = p3;
    }

    public bool HasVertex(Vector3 vertex)
    {
        return vertex == point1 || vertex == point2 || vertex == point3;
    }

    public bool IsDifferentFrom(Triangle3 other)
    {
        return !((point1 == other.point1 || point1 == other.point2 || point1 == other.point3) &&
                 (point2 == other.point1 || point2 == other.point2 || point2 == other.point3) &&
                 (point3 == other.point1 || point3 == other.point2 || point3 == other.point3));
    }

    public List<Edge3> GetEdges()
    {
        List<Edge3> edges = new List<Edge3>
        {
            new Edge3(point1, point2),
            new Edge3(point2, point3),
            new Edge3(point3, point1)
        };
        return edges;
    }

    public bool CircumcircleContains(Vector3 point)
    {
        float ab = (point1.x * point1.x) + (point1.y * point1.y) + (point1.z * point1.z);
        float cd = (point2.x * point2.x) + (point2.y * point2.y) + (point2.z * point2.z);
        float ef = (point3.x * point3.x) + (point3.y * point3.y) + (point3.z * point3.z);

        float circumX = (ab * (point2.y - point3.y) + cd * (point3.y - point1.y) + ef * (point1.y - point2.y)) / (2f * (point1.x * (point2.y - point3.y) - point1.y * (point2.x - point3.x) + point2.x * point3.y - point3.x * point2.y));
        float circumY = (ab * (point3.x - point2.x) + cd * (point1.x - point3.x) + ef * (point2.x - point1.x)) / (2f * (point1.x * (point2.y - point3.y) - point1.y * (point2.x - point3.x) + point2.x * point3.y - point3.x * point2.y));
        float circumZ = (ab * (point2.z - point3.z) + cd * (point3.z - point1.z) + ef * (point1.z - point2.z)) / (2f * (point1.x * (point2.y - point3.y) - point1.y * (point2.x - point3.x) + point2.x * point3.y - point3.x * point2.y));
        Vector3 circumcenter = new Vector3(circumX, circumY,circumZ);

        float radiusSquared = Mathf.Pow(point1.x - circumcenter.x, 2) + Mathf.Pow(point1.y - circumcenter.y, 2) + Mathf.Pow(point1.z - circumcenter.z, 2);
        float distSquared = Mathf.Pow(point.x - circumcenter.x, 2) + Mathf.Pow(point.y - circumcenter.y, 2) + Mathf.Pow(point.z - circumcenter.z, 2);

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
