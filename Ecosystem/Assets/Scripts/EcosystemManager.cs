using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EcosystemManager : MonoBehaviour
{
    const float baseTimeScale = 1;
    const int baseCreatureNumber = 100;
    const float baseFoodRatio = 2;

    const float maxTimeScale = 10;
    const int maxCreatureNumber = 500;
    const float maxFoodRatio = 5;

    public static EcosystemManager instance;

    public CreatureSpawner creatureSpawner { get; private set; }
    public CameraRotation cameraRotation { get; private set; }

    private void Awake()
    {
        instance = this;

        creatureSpawner = GetComponent<CreatureSpawner>();
        cameraRotation = Camera.main.GetComponent<CameraRotation>();

        timeScaleSlider.maxValue = maxTimeScale;
        timeScaleSlider.value = timeScale;

        automaticToggle.isOn = true;

        creatureNumberSlider.maxValue = maxCreatureNumber;
        creatureNumberSlider.value = baseCreatureNumber;

        foodRatioSlider.maxValue = maxFoodRatio;
        foodRatioSlider.value = baseFoodRatio;

        ChangeSettingsActivationState(true);

        timerFSSeconds = foodSearchingTimerMinutes * 60f;
        timerRTESeconds = returnToEdgeTimerMinutes * 60f;
    }

    private void OnEnable()
    {
        if (instance == null) instance = this;
    }

    [Header("Simulation Infos")]
    int generationCount = 0;
    public TextMeshProUGUI info_tmp;
    public TextMeshProUGUI timer_tmp;

    [Header("Time Settings")]
    public Slider timeScaleSlider;
    public TextMeshProUGUI timeScale_tmp;
    public float timeScale { get; private set; } = 1;

    [Header("Automatic Sim Settings")]
    public Toggle automaticToggle;
    bool automatic = false;

    [Header("Creature Number Settings")]
    public Slider creatureNumberSlider;
    public TextMeshProUGUI creatureNum_tmp;

    [Header("Food Ratio Settings")]
    public Slider foodRatioSlider;
    public TextMeshProUGUI foodRation_tmp;

    [Header("Timer Settings")]
    public float foodSearchingTimerMinutes;
    float timerFSSeconds;

    public float returnToEdgeTimerMinutes;
    float timerRTESeconds;
    bool beginRTECountdown;

    bool isGenRunning;
    bool alreadyEvolved = false;
    bool firstTime = true;

    bool canSimulationRun = true;

    bool canChangeSettings = true;

    public void ResetTimeScale()
    {
        timeScaleSlider.value = baseTimeScale;
        timeScale = baseTimeScale;
        timeScale_tmp.SetText("01,000");
    }

    public void StopTime()
    {
        timeScaleSlider.value = 0;
        timeScale = 0;
        timeScale_tmp.SetText("00.000");
    }

    public void OnTimeScaleSliderValueChanged()
    {
        float t = Mathf.Floor(timeScaleSlider.value * 1000f) / 1000f;
        timeScale = t;
        timeScale_tmp.SetText(t.ToString("00.000"));
    }

    public void OnAutomaticToggleValueChanged()
    {
        if (!canChangeSettings)
        {
            return;
        }

        automatic = automaticToggle.isOn;
    }

    public void OnCreatureNumberSliderValueChanged()
    {
        if (!canChangeSettings)
        {
            return;
        }

        int a = (int)creatureNumberSlider.value;
        creatureSpawner.creatureNumber = a;
        creatureNum_tmp.SetText(a.ToString());

        if (foodRatioSlider.value == 0)
        {
            creatureSpawner.canSpawnFood = false;
            foodRation_tmp.SetText("0,0");
        }
        else
        {
            creatureSpawner.canSpawnFood = true;

            float t = foodRatioSlider.value / 2f;
            creatureSpawner.foodNumber = Mathf.RoundToInt(a * t);
            creatureSpawner.foodCount = creatureSpawner.foodNumber;
            foodRation_tmp.SetText(t.ToString("0.0") + " (" + creatureSpawner.foodNumber + ")");
        }
    }

    public void ResetCreatureNumber()
    {
        if (!canChangeSettings)
        {
            return;
        }

        creatureNumberSlider.value = baseCreatureNumber;
        creatureSpawner.creatureNumber = baseCreatureNumber;

        if (foodRatioSlider.value == 0)
        {
            creatureSpawner.canSpawnFood = false;
            foodRation_tmp.SetText("0,0");
        }
        else
        {
            creatureSpawner.canSpawnFood = true;

            float t = foodRatioSlider.value / 2f;
            creatureSpawner.foodNumber = Mathf.RoundToInt(baseCreatureNumber * t);
            creatureSpawner.foodCount = creatureSpawner.foodNumber;
            foodRation_tmp.SetText(t.ToString("0.0") + " (" + creatureSpawner.foodNumber + ")");
        }
    }

    public void OnFoodRatioSliderValueChanged()
    {
        if (!canChangeSettings)
        {
            return;
        }

        if (foodRatioSlider.value == 0)
        {
            creatureSpawner.canSpawnFood = false;
            foodRation_tmp.SetText("0,0");
        }
        else
        {
            creatureSpawner.canSpawnFood = true;

            float t = foodRatioSlider.value / 2f;
            creatureSpawner.foodNumber = Mathf.RoundToInt(creatureSpawner.creatureNumber * t);
            creatureSpawner.foodCount = creatureSpawner.foodNumber;
            foodRation_tmp.SetText(t.ToString("0.0") + " (" + creatureSpawner.foodNumber + ")");
        }
    }

    public void ResetFoodRatio()
    {
        if (!canChangeSettings)
        {
            return;
        }

        foodRatioSlider.value = baseFoodRatio;
        float t = foodRatioSlider.value / 2f;
        creatureSpawner.foodNumber = Mathf.RoundToInt(creatureSpawner.creatureNumber * t);
        creatureSpawner.foodCount = creatureSpawner.foodNumber;
        foodRation_tmp.SetText(t.ToString("0.0") + " (" + creatureSpawner.foodNumber + ")");
    }

    void ChangeSettingsActivationState(bool v)
    {
        creatureNumberSlider.interactable = v;
        foodRatioSlider.interactable = v;
        automaticToggle.interactable = v;

        canChangeSettings = v;
    }

    private void Update()
    {
        if(!canSimulationRun)
        {
            cameraRotation.canRotate = false;
            return;
        }

        if(creatureSpawner.creaturesGO.Count == 0 && !firstTime)
        {
            canSimulationRun = false;
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            creatureSpawner.creaturesGO[0].GetComponent<Creature>().debugEnabled = true;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(automatic && !firstTime)
            {
                return;
            }

            if(firstTime)
            {
                ChangeSettingsActivationState(false);

                creatureSpawner.SpawnCreatures();
                creatureSpawner.SpawnFood();

                StartNewGeneration();

                firstTime = false;
            }
            else
            {
                if(!isGenRunning)
                {
                    if (!alreadyEvolved)
                    {
                        ReproduceAndEvolve();
                    }
                    else
                    {
                        StartNewGeneration();
                    }
                }
            }
        }

        if(automatic && !firstTime)
        {
            if (!isGenRunning)
            {
                StartCoroutine(AutomaticEvolutionAndNewGeneration());
            }
        }

        #region Simulation Running
        if (!isGenRunning)
        {
            return;
        }

        if (beginRTECountdown)
        {
            timerRTESeconds -= Time.deltaTime * timeScale;
            if (timerRTESeconds <= 0)
            {
                timerRTESeconds = 0;
                StartCoroutine(StopCurrentGeneration());
            }
            timer_tmp.SetText("RTE timer: " + timerRTESeconds.ToString("00.00"));
        }
        else
        {
            timerFSSeconds -= Time.deltaTime * timeScale;
            if (timerFSSeconds <= 0)
            {
                ReturnAllCreaturesToEdge();
                beginRTECountdown = true;
            }
            timer_tmp.SetText("FS timer: " + timerFSSeconds.ToString("00.00"));
        }

        if (creatureSpawner.foodCount == 0)
        {
            ReturnAllCreaturesToEdge(true);
        }
        #endregion
    }

    IEnumerator AutomaticEvolutionAndNewGeneration()
    {
        List<GameObject> newObj = new();

        for (int i = 0; i < creatureSpawner.creaturesGO.Count; i++)
        {
            Creature c = creatureSpawner.creaturesGO[i].GetComponent<Creature>();

            if (c.foodConsumed == 2)
            {
                Vector3 pos = creatureSpawner.creaturesGO[i].transform.position + Random.onUnitSphere;
                pos.y = 0;
                pos = pos.normalized * creatureSpawner.radius;

                Vector3 platformDir = creatureSpawner.platform.position - creatureSpawner.creaturesGO[i].transform.position;

                GameObject cGO = creatureSpawner.SpawnAndEvolveOneCreature(pos,
                                                                            Quaternion.LookRotation(platformDir.normalized, Vector3.up),
                                                                            creatureSpawner.creaturesGO[i].GetComponent<Creature>().trait);
                newObj.Add(cGO);
            }
        }

        foreach (GameObject go in newObj)
        {
            creatureSpawner.creaturesGO.Add(go);
        }

        yield return new WaitForEndOfFrame();

        cameraRotation.canRotate = true;

        ResetAllFlags();

        if (!firstTime)
        {
            creatureSpawner.SpawnFood();
        }

        for (int i = 0; i < creatureSpawner.creaturesGO.Count; i++)
        {
            Creature c = creatureSpawner.creaturesGO[i].GetComponent<Creature>();
            c.Initialize();
            c.m_enabled = true;
        }

        generationCount++;
        SetInfoTMP();
    }

    void StartNewGeneration()
    {
        cameraRotation.canRotate = true;

        ResetAllFlags();

        if (!firstTime)
        {
            creatureSpawner.SpawnFood();
        }

        for (int i = 0; i < creatureSpawner.creaturesGO.Count; i++)
        {
            Creature c = creatureSpawner.creaturesGO[i].GetComponent<Creature>();
            c.Initialize();
            c.m_enabled = true;
        }

        generationCount++;
        SetInfoTMP();
    }

    void ReproduceAndEvolve()
    {
        alreadyEvolved = true;

        List<GameObject> newObj = new();

        for (int i = 0; i < creatureSpawner.creaturesGO.Count; i++) 
        {
            Creature c = creatureSpawner.creaturesGO[i].GetComponent<Creature>();

            if(c.foodConsumed == 2)
            {
                Vector3 pos = creatureSpawner.creaturesGO[i].transform.position + Random.onUnitSphere;
                pos.y = 0;
                pos = pos.normalized * creatureSpawner.radius;

                Vector3 platformDir = creatureSpawner.platform.position - creatureSpawner.creaturesGO[i].transform.position;

                GameObject cGO = creatureSpawner.SpawnAndEvolveOneCreature (pos,
                                                                            Quaternion.LookRotation(platformDir.normalized, Vector3.up),
                                                                            creatureSpawner.creaturesGO[i].GetComponent<Creature>().trait);
                newObj.Add(cGO);
            }
        }

        foreach(GameObject go in newObj)
        {
            creatureSpawner.creaturesGO.Add(go);
        }

        SetInfoTMP(false, "press space to start new generation");
    }

    void ResetAllFlags()
    {
        beginRTECountdown = false;

        timerFSSeconds = foodSearchingTimerMinutes * 60f;
        timerRTESeconds = returnToEdgeTimerMinutes * 60f;

        isGenRunning = true;
        alreadyEvolved = false;
    }

    IEnumerator StopCurrentGeneration()
    {
        isGenRunning = false;

        for (int i = 0; i < creatureSpawner.creaturesGO.Count; i++) 
        {
            Creature c = creatureSpawner.creaturesGO[i].GetComponent<Creature>();
            c.m_enabled = false;
        }

        for (int i = 0; i < creatureSpawner.creaturesGO.Count; i++)
        {
            Creature c = creatureSpawner.creaturesGO[i].GetComponent<Creature>();

            if (!c.returned)
            {
                Destroy(creatureSpawner.creaturesGO[i]);
            }
        }

        foreach (GameObject go in creatureSpawner.foodGO)
        {
            Destroy(go);
        }

        yield return new WaitForEndOfFrame();

        creatureSpawner.creaturesGO.RemoveAll(x => x == null);
        alreadyEvolved = false;
        cameraRotation.canRotate = false;
        SetInfoTMP(false, "press space to evolve creatures");
    }

    void ReturnAllCreaturesToEdge(bool obligated = false)
    {
        for (int i = 0; i < creatureSpawner.creaturesGO.Count; i++)
        {
            Creature c = creatureSpawner.creaturesGO[i].GetComponent<Creature>();
            if (c.foodConsumed == 0 && !obligated)
            {
                continue;
            }

            c.ReturnToEdge();
        }
    }

    public void SetInfoTMP(bool overrideText = false, string addBottomString = "medium trait values above")
    {
        if (overrideText)
        {

            return;
        }

        float mediumSpeed = 0;
        float mediumSize = 0;
        float mediumSense = 0;
        float mediumVision = 0;

        for (int i = 0; i < creatureSpawner.creaturesGO.Count; i++)
        {
            Creature c = creatureSpawner.creaturesGO[i].GetComponent<Creature>();
            mediumSpeed += c.trait.speed * 10000;
            mediumSize += c.trait.size * 10000;
            mediumSense += c.trait.sense * 10000;
            mediumVision += c.trait.vision * 10000;
        }

        mediumSpeed /= creatureSpawner.creaturesGO.Count * 10000f;
        mediumSize /= creatureSpawner.creaturesGO.Count * 10000f;
        mediumSense /= creatureSpawner.creaturesGO.Count * 10000f;
        mediumVision /= creatureSpawner.creaturesGO.Count * 10000f;

        info_tmp.SetText(
                        "generation number: " + generationCount + "\n" + "\n" +
                        "creature number: " + creatureSpawner.creaturesGO.Count + "\n" +
                        "medium speed: " + mediumSpeed + "\n" +
                        "medium size: " + mediumSize + "\n" +
                        "medium sense: " + mediumSense + "\n" +
                        "medium vision: " + mediumVision + "\n" + "\n" +
                        addBottomString);
    }
}
