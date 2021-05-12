using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneratorController : MonoBehaviour
{
    [Header("Templates")]
    public List<TerrainTemplateController> terrainTemplates;
    public float terrainTemplateWidth;

    [Header("Generaor Area")]
    public Camera gameCamera;
    public float areaStartOffset;
    public float areaEndOffset;

    [Header("Force Early Template")]
    public List<TerrainTemplateController> earlyTerrainTemplates;

    private const float debugLineHeight = 10.0f;

    List<GameObject> spawnedTerrain;

    float lastGereratedPositionX;
    float lastRemovedPositionX;

    Dictionary<string, List<GameObject>>pool;


    private void Start()
    {
        pool = new Dictionary<string, List<GameObject>>();

        spawnedTerrain = new List<GameObject>();

        lastGereratedPositionX = GetHorizontalPositionStart();
        lastRemovedPositionX = lastGereratedPositionX - terrainTemplateWidth;

        foreach (TerrainTemplateController terrain in earlyTerrainTemplates)
        {
            GenerateTerrain(lastGereratedPositionX, terrain);
            lastGereratedPositionX += terrainTemplateWidth;
        }

        while (lastGereratedPositionX < GetHorizontalPositionEnd())
        {
            GenerateTerrain(lastGereratedPositionX);
            lastGereratedPositionX += terrainTemplateWidth;
        }
    }

    private void Update()
    {
        while (lastGereratedPositionX < GetHorizontalPositionEnd())
        {
            GenerateTerrain(lastGereratedPositionX);
            lastGereratedPositionX += terrainTemplateWidth;
        }
        while(lastRemovedPositionX + terrainTemplateWidth < GetHorizontalPositionStart())
        {
            lastRemovedPositionX += terrainTemplateWidth;
            RemoveTerrain(lastRemovedPositionX);
        }
    }

    private GameObject GenerateFromPool(GameObject item, Transform parent)
    {
        if (pool.ContainsKey(item.name))
        {
            if(pool[item.name].Count > 0)
            {
                GameObject newItemFromPool = pool[item.name][0];
                pool[item.name].Remove(newItemFromPool);
                newItemFromPool.SetActive(true);
                return newItemFromPool;
            }
        }
        else
        {
            pool.Add(item.name, new List<GameObject>());
        }

        GameObject newItem = Instantiate(item, parent);
        newItem.name = item.name;
        return newItem;
    }

    private void ReturnToPool(GameObject item)
    {
        if (!pool.ContainsKey(item.name))
        {
            Debug.LogError("Invalid pool item");
        }

        pool[item.name].Add(item);
        item.SetActive(false);
    }

    private void RemoveTerrain(float posX)
    {
        GameObject terrainRemove = null;

        foreach(GameObject item in spawnedTerrain)
        {
            if(item.transform.position.x <= posX)
            {
                terrainRemove = item;
                break;
            }
        }

        if(terrainRemove != null)
        {
            spawnedTerrain.Remove(terrainRemove);
            //Destroy(terrainRemove);
            ReturnToPool(terrainRemove);
        }
    }

    private void GenerateTerrain(float posX, TerrainTemplateController forceTerrain = null)
    {
        //GameObject newTerrain = Instantiate(terrainTemplates[Random.Range(0, terrainTemplates.Count)].gameObject, transform);

        GameObject newTerrain = null;

        if (forceTerrain == null)
        {
            newTerrain = GenerateFromPool(terrainTemplates[Random.Range(0, terrainTemplates.Count)].gameObject, transform);
        }
        else
        {
            newTerrain = GenerateFromPool(forceTerrain.gameObject, transform);
        }

        newTerrain.transform.position = new Vector2(posX, 0f);

        spawnedTerrain.Add(newTerrain);
    }

    private float GetHorizontalPositionStart()
    {
        return gameCamera.ViewportToWorldPoint(new Vector2(0f, 0f)).x + areaStartOffset;
    }

    private float GetHorizontalPositionEnd()
    {
        return gameCamera.ViewportToWorldPoint(new Vector2(1f, 0f)).x + areaEndOffset;
    }

    private void OnDrawGizmos()
    {
        Vector3 areaStartPosition = transform.position;
        Vector3 areaEndPosition = transform.position;

        areaStartPosition.x = GetHorizontalPositionStart();
        areaEndPosition.x = GetHorizontalPositionEnd();

        Debug.DrawLine(areaStartPosition + Vector3.up * debugLineHeight / 2, areaStartPosition + Vector3.down * debugLineHeight / 2, Color.red);
        Debug.DrawLine(areaEndPosition + Vector3.up * debugLineHeight / 2, areaEndPosition + Vector3.down * debugLineHeight / 2, Color.red);
    }
}
