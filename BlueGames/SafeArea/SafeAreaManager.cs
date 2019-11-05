using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 안전영역 조정 매니저(현재 이 스크립트 사용안함)
 * */
public class SafeAreaManager : MonoBehaviour
{
    static SafeAreaManager instance;
    public static SafeAreaManager Instance
    {
        get
        {
            return instance;
        }
    }
    
    public int LastAppliedWidth { get; private set; }
    public bool UseSafeArea { get; private set; }

    public float SafeAreaWidthRatio { get; private set; }
    public float SafeAreaHeightRatio { get; private set; }

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }

        UpdateSafeAreaSystem();
    }

    void Update()
    {
        UpdateSafeAreaSystem();
    }

    public void UpdateSafeAreaSystem()
    {
        if (LastAppliedWidth == Screen.width)
        {
            return;
        }

#if SIMULATE_SAFEAREA
        UseSafeArea = true;

        SafeAreaWidthRatio = GlobalVariable.safeAreaSimulationWidthRatio;
        SafeAreaHeightRatio = GlobalVariable.safeAreaSimulationHeightRatio;
#else
        UseSafeArea = Screen.safeArea.x > 0;

        SafeAreaWidthRatio = Screen.safeArea.x / Screen.width;
        SafeAreaHeightRatio = Screen.safeArea.y / Screen.height;
#endif

        LastAppliedWidth = Screen.width;
    }
}
