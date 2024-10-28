using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CreatureTrait
{
    [Min(0)] public float speed = 1f;
    [Min(0.1f)] public float size = 1f;
    [Min(0.1f)] public float sense = 1f;
    [Min(0.05f)] public float vision = .15f;
    [Min(0)] public float maxEnergy = 50f;
}

public class Creature : MonoBehaviour
{
    public bool m_enabled;

    public CreatureTrait trait;
    public Gradient speedGradient;

    float energyConsumed;
    float currEnergy;
    float halvedAngle;
    public bool debugEnabled;
    public int segments;

    public bool returned { get; private set; }
    bool returning;
    bool destinationReached = true;
    public int foodConsumed { get; private set; }

    Vector3 dest;
    Vector3 dir;
    Transform targetedFood;
    public LayerMask foodLayer;

    private void OnDrawGizmos()
    {
        if(!debugEnabled)
        {
            return;
        }

        Gizmos.color = Color.red;

        float halvedAngle = Mathf.PI * trait.vision * Mathf.Rad2Deg;

        Vector3 central = transform.forward * trait.sense;
        Vector3 rotA = Quaternion.AngleAxis(halvedAngle, Vector3.up) * central;

        Vector3 rotB = Quaternion.AngleAxis(-halvedAngle, Vector3.up) * central;

        Gizmos.DrawLine(transform.position, transform.position + rotA);

        Gizmos.color = Color.black;
        Gizmos.DrawLine(transform.position, transform.position + rotB);

        Vector3 oldPoint = transform.position + rotA;

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            
            Vector3 point = transform.position + Quaternion.AngleAxis(-t * halvedAngle * 2, Vector3.up) * rotA;

            Gizmos.DrawLine(oldPoint, point);

            oldPoint = point;
        }

        Gizmos.DrawLine(dest + Vector3.up, dest + Vector3.down);
        Gizmos.DrawLine(transform.position, dest);
    } 

    public void EvolveTrait(CreatureTrait parent)
    {
        if(parent == null)
        {
            CreatureTrait _trait = new CreatureTrait()
            {
                speed = 1f,
                size = 1f,
                sense = 1f,
                vision = .15f
            };

            trait = _trait;
        }
        else
        {
            CreatureTrait _trait = new CreatureTrait()
            {
                speed =  CalculateNewTraitStat(parent.speed, 0.1f, 5f),
                size = CalculateNewTraitStat(parent.size, 0.1f, 5f),
                sense = CalculateNewTraitStat(parent.sense, 0.1f, 5f),
                vision = CalculateNewTraitStat(parent.vision, 0.05f, 1f)
            };

            trait = _trait;
        }

        static float CalculateNewTraitStat(float baseStat, float clampMin, float clampMax, float percentage = 0.05f, float minRange = -10000, float maxRange = 10000)
        {
            float mod = Random.Range(minRange, maxRange) / (Mathf.Abs(minRange) + maxRange)/2f;
            float rdm = Random.value;
            float _new = rdm < percentage ? baseStat + mod : baseStat;

            return Mathf.Clamp(_new, clampMin, clampMax);
        }

        MeshRenderer mr = GetComponentInChildren<MeshRenderer>();
        mr.sharedMaterial.color = speedGradient.Evaluate(trait.speed / 5f);
        
        transform.localScale = Vector3.one * trait.size;
    }

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        returning = returned = false;

        targetedFood = null;
        foodConsumed = 0;

        currEnergy = trait.maxEnergy;
        transform.localScale = Vector3.one * trait.size;
        halvedAngle = Mathf.PI * trait.vision * Mathf.Rad2Deg;
        energyConsumed = trait.size * trait.size * trait.size * trait.speed * trait.speed + trait.sense + trait.vision;
    }

    private void Update()
    {
        if (!m_enabled)
        {
            return;
        }

        if (returned)
        {
            return;
        }

        if (foodConsumed == 2)
        {
            ReturnToEdge();
        }

        if(currEnergy <= 0)
        {
            EcosystemManager.instance.creatureSpawner.creaturesGO.Remove(gameObject);
            Destroy(gameObject);
        }

        if (returning)
        {
            MoveAndRotate();

            if (dir.sqrMagnitude <= 0.001f)
            {
                returned = true;
            }

            return;
        }

        if (targetedFood == null)
        {
            targetedFood = null; //if missing object
            if (destinationReached)
            {
                SearchRandomPointInFOV();
            }
            else
            {
                MoveAndRotate();

                if (dir.sqrMagnitude <= 0.001f)
                {
                    destinationReached = true;

                    return;
                }
            }
        }
        else
        {
            MoveAndRotate();

            if (dir.sqrMagnitude <= 0.001f)
            {
                destinationReached = true;
                Destroy(targetedFood.gameObject);

                targetedFood = null;

                foodConsumed++;

                return;
            }
        }

        SearchNearestFoodInFOV();
    }

    private void SearchRandomPointInFOV()
    {
        Vector3 central = transform.forward * trait.sense;
        Vector3 rotA = Quaternion.AngleAxis(halvedAngle, Vector3.up) * central;

        float t = Random.value;

        Vector3 point = transform.position + Quaternion.AngleAxis(Mathf.Lerp(0, -halvedAngle * 2, t), Vector3.up) * rotA;

        while (!InsideOfPlatform(point))
        {
            transform.RotateAround(transform.position, Vector3.up, halvedAngle);
            
            central = transform.forward * trait.sense;
            rotA = Quaternion.AngleAxis(halvedAngle, Vector3.up) * central;

            t = Random.value;

            point = transform.position + Quaternion.AngleAxis(Mathf.Lerp(0, -halvedAngle * 2, t), Vector3.up) * rotA;
        }

        dest = point;
        transform.forward = (point - transform.position).normalized;

        destinationReached = false;
    }

    float CalculateAngleFromVector(Vector3 vec)
    {
        float angle = 0;
        float z = vec.z;
        float x = vec.x;

        if (x == 0 && z < 0)
        {
            angle = 180;
        }
        else
        {
            angle = Mathf.Atan(z / x) * Mathf.Rad2Deg;

            if (x < 0)
            {
                angle += 180;
            }

            if (z < 0 && x > 0)
            {
                angle += 360;
            }
        }

        return angle;
    }

    private void SearchNearestFoodInFOV()
    {
        if(targetedFood != null)
        {
            return;
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, trait.sense, foodLayer);

        float distance = float.MinValue;
        foreach (Collider c in colliders)
        {
            Vector3 objPos = c.transform.position;

            Vector3 dirFromObj = objPos - transform.position;
            dirFromObj.y = 0;

            if (dirFromObj.sqrMagnitude <= trait.sense * trait.sense)
            {
                Vector3 central = transform.forward * trait.sense;
                Vector3 rotA = Quaternion.AngleAxis(halvedAngle, Vector3.up) * central;
                Vector3 rotB = Quaternion.AngleAxis(-halvedAngle, Vector3.up) * central;

                float angleObj = CalculateAngleFromVector(dirFromObj);
                float angA = CalculateAngleFromVector(rotA) ;
                float angB = CalculateAngleFromVector(rotB);

                if (angB < angA)
                {
                    angB += 360;
                    angleObj += 360;
                }

                if (angA <= angleObj && angleObj <= angB)
                {
                    if (dirFromObj.sqrMagnitude > distance)
                    {
                        distance = dirFromObj.sqrMagnitude;
                        targetedFood = c.transform;
                    }
                }
            }
        }

        if (targetedFood != null)
        {
            dest = targetedFood.transform.position;
            transform.forward = (targetedFood.transform.position - transform.position).normalized;
        }
    }

    private bool InsideOfPlatform(Vector3 pos)
    {
        return pos.sqrMagnitude <= EcosystemManager.instance.creatureSpawner.radius * EcosystemManager.instance.creatureSpawner.radius - .32f;
    }

    private void MoveAndRotate()
    {
        destinationReached = false;
        dir = dest - transform.position;

        transform.position += EcosystemManager.instance.timeScale * Time.deltaTime * trait.speed * dir.normalized;

        currEnergy -= energyConsumed * Time.deltaTime * EcosystemManager.instance.timeScale;
    }

    public void ReturnToEdge()
    {
        returning = true;
        dest = transform.position.normalized * EcosystemManager.instance.creatureSpawner.radius;
    }
}
