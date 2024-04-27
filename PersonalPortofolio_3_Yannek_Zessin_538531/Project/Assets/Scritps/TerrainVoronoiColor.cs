using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TerrainVoronoiColor : MonoBehaviour
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
            SetColors();
        }
        else if (interpolationType == type.blackToWhite)
        {
            SetBlackToWhite();
        }
        else if (interpolationType == type.whiteToBlack)
        {
            SetWhiteToBlack();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
        }
    }

    void SetColors()
    {
        Vector2[] points = new Vector2[regionAmount];

        Color[] regionColors = new Color[regionColorAmount];


        Color[] colors = new Color[size * size];

        for (int i = 0; i < regionAmount; i++)
        {
            points[i] = new Vector2(Random.Range(0, size), Random.Range(0, size));
        }

        for (int i = 0; i < regionAmount; i++)
        {
            regionColors[i] = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        }

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = float.MaxValue;
                int value = 0;
                for (int i = 0; i < regionAmount; i++)
                {
                    if (Vector2.Distance(new Vector2(x, y), points[i]) < distance)
                    {
                        distance = Vector2.Distance(new Vector2(x, y), points[i]);
                        value = i;
                    }
                }
                // Calculate the distance percentage
                float distancePercentage = (distance / size) * 100f;

                colors[x + y * size] = regionColors[value % regionColorAmount];
            }
        }
        Texture2D voronoiTexture = new Texture2D(size, size);
        voronoiTexture.SetPixels(colors);
        voronoiTexture.Apply();
        //GetComponent<MeshRenderer>().material.SetTexture("_Texture2D", voronoiTexture);
        GetComponent<Terrain>().materialTemplate.SetTexture("_MainTex", voronoiTexture);
    }

    void SetBlackToWhite()
    {
        Vector2[] points = new Vector2[regionAmount];

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
                if (normalizedDistance == 0)
                {
                    

                }

            }
        }
    }

    void SetWhiteToBlack()
    {
        Vector2[] points = new Vector2[regionAmount];

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
        GetComponent<Terrain>().materialTemplate.SetTexture("_MainTex", voronoiTexture);
    }


    private void OnApplicationQuit()
    {
    }
}