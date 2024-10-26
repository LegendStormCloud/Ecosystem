using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureSpawner : MonoBehaviour
{
    [Header("Basic Stats")]
    public bool canSpawnFood = true;
    public float radius;
    [Min(1)] public int creatureNumber;
    [Min(1)] public int foodNumber;

    [Header("Prefabs")]
    public GameObject creature;
    public GameObject food;

    public Transform platform;

    [Header("Lists")]
    public List<GameObject> creaturesGO;
    public List<GameObject> foodGO;

    public int foodCount;

    public Transform creatureParent { get; private set; }
    Transform foodParent;

    private void Awake()
    {
        creatureParent = new GameObject("creatureFolder").transform;
        foodParent = new GameObject("foodFolder").transform;
        foodCount = foodNumber;
    }

    public void SpawnCreatures()
    {
        if (creaturesGO.Count > 0 && creaturesGO != null)
        {
            foreach (GameObject go in creaturesGO)
            {
                Destroy(go);
            }
        }

        creaturesGO = new(creatureNumber);

        for (int i = 0; i < creatureNumber; i++)
        {
            //place at equally angles
            float angle = (2f * Mathf.PI) / creatureNumber * i;
            Vector3 pos = new (Mathf.Cos(angle), 0, Mathf.Sin(angle));

            GameObject cGO = SpawnAndEvolveOneCreature (pos * radius,
                                                        Quaternion.LookRotation((platform.position - pos).normalized, Vector3.up),
                                                        null);
            creaturesGO.Add(cGO);
        }

        EcosystemManager.instance.SetInfoTMP();
    }


    public GameObject SpawnAndEvolveOneCreature(Vector3 pos, Quaternion rotation, CreatureTrait parentTrait)
    {
        GameObject go = Instantiate(creature, pos, rotation);
        go.transform.parent = creatureParent;

        MeshRenderer mr = go.GetComponentInChildren<MeshRenderer>();

        mr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));

        go.GetComponent<Creature>().EvolveTrait(parentTrait);

        return go;
    }

    public void SpawnFood()
    {
        if(!canSpawnFood)
        {
            return;
        }

        if (foodGO.Count > 0 && foodGO != null)
        {
            foreach (GameObject go in foodGO)
            {
                Destroy(go);
            }
        }

        foodGO = new(foodNumber);

        for (int i = 0; i < foodNumber; i++)
        {
            Vector3 pos = Random.insideUnitSphere * (radius - .1f);
            pos.y = 0;
            foodGO.Add(Instantiate(food, pos, Quaternion.identity));
            foodGO[i].transform.parent = foodParent;
        }
    }
}
