using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 사용은 2 가지 방법으로 
 * 1. Mgr 에게 등록할 Configuration 응 세팅해야함 
 *      - 인스펙터에서 컴포넌트 부착 후 관련 값 세팅 후 configureOnAwake 변수 체크하여 자동으로 Configure . 
 *      - AddComponent 로 부착 후 SetVariables() 를 통해 값 세팅하여 Configure.
 *      
 *  2. Configuration 이 된 후에는 사용 가능한 상태임 
 *  3. Activate() 를 이용해서 from -> to 로 작동시킬지 to -> from 으로 작동시킬지를 정할 수 있음 
 *      ColorFadeManager 그럼 내부적으로 미리 setting 돼있는 Configuration 을 가지고 Manager 에게 Activate 요청 . 
 * 
 * */

// WARNING : KEY 값 오브젝트 hashCode 로 구분함 . 고로 두개이상의 컴포넌트 안됨. 
[DisallowMultipleComponent]
// renderer 에서 material 을 긁어오기때문에 renderer 가 필요함 
[RequireComponent(typeof(Renderer))]
public class ColorFadeController : MonoBehaviour
{
    public enum FromToDir
    {
        Forward,
        Backward
    }

    // Scene 에서 컴포넌트를 직접 붙여 쓰는 경우 true 로 하면 자동 configure
    // 스크립트에서 addComponent 를 하는 경우는 ,
    [Header("! WARNING : Enable this when attached on Insepector (default:false) !")]
    public bool configureOnAwake = false;

    Renderer targetRenderer;

    public COLOR_FADE_SCHEME scheme;

    public float duration;

    public string shaderPropertyName;
    public string shaderPropertyFallbackName;

    string key = "N/S";

    public Color colorFrom = Color.white;
    public Color colorTo = Color.white;
    public float brightnessFrom, brightnessTo;

    Color colorFromInternal, colorToInternal;
    float brightnessFromInternal, brightnessToInternal;

    ColorFadeConfiguration config;

    private void Awake()
    {
        Init();

        if (configureOnAwake)
        {
            RefreshConfigs();
        }
    }

    private void OnEnable()
    {
        Init();
    }

    void Init()
    {
        if (key.Equals("N/S"))
        {
            key = this.GetHashCode().ToString();
            targetRenderer = GetComponent<Renderer>();
        }
    }

    public void SetVariables(
        COLOR_FADE_SCHEME scheme,
        float duration,
        string shaderPropertyName,
        string shaderPropertyFallbackName,
        Color colorFrom,
        Color colorTo,
        float brightnessFrom,
        float brightnessTo)
    {
        this.scheme = scheme;
        this.duration = duration;
        this.shaderPropertyName = shaderPropertyName;
        this.shaderPropertyFallbackName = shaderPropertyFallbackName;

        switch (scheme)
        {
            case COLOR_FADE_SCHEME.MATERIAL_COLOR:
            case COLOR_FADE_SCHEME.SHADER_COLOR:
                this.colorFrom = colorFrom;
                this.colorTo = colorTo;
                break;
            case COLOR_FADE_SCHEME.SHADER_BRIGHTNESS:
                this.brightnessFrom = brightnessFrom;
                this.brightnessTo = brightnessTo;
                break;
            default:
                Debug.LogError("PLEASE ADD TYPE !");
                break;
        }

        RefreshConfigs();
    }

    // config 를 전체적으로 싹다 초기화해서 새로 업데이트함 
    public void RefreshConfigs()
    {
        if (targetRenderer == null || targetRenderer.materials.Length == 0)
            return;

        ReleaseConfig();

        config = new ColorFadeConfiguration(targetRenderer.materials.Length)
            .AddKey(key)
            .SetBaseColor(colorFrom, colorTo)
            .SetDuration(duration);

        foreach (var mat in targetRenderer.materials)
        {
            config.AddMaterial(mat);

            if (string.IsNullOrEmpty(shaderPropertyFallbackName) == false)
            {
                if(mat.HasProperty(shaderPropertyName) == false)
                {
                    shaderPropertyName = shaderPropertyFallbackName;

                    if(mat.HasProperty(shaderPropertyFallbackName) == false)
                    {
                        Debug.LogError("** Fade Shader Property does not exist **");
                    }
                }
            }
        }

        shaderPropertyFallbackName = string.Empty;

        switch (scheme)
        {
            case COLOR_FADE_SCHEME.MATERIAL_COLOR:
                config.SetBaseColor(colorFrom, colorTo);
                break;
            case COLOR_FADE_SCHEME.SHADER_COLOR:
                config.SetShaderColor(shaderPropertyName, colorFrom, colorTo);
                break;
            case COLOR_FADE_SCHEME.SHADER_BRIGHTNESS:
                config.SetShaderBrightness(shaderPropertyName, brightnessFrom, brightnessTo);
                break;
            default:
                break;
        }
    }

    void SetConfigByDirection(FromToDir direction)
{
    switch (scheme)
    {
        case COLOR_FADE_SCHEME.MATERIAL_COLOR:
        case COLOR_FADE_SCHEME.SHADER_COLOR:
            if (direction == FromToDir.Forward)
            {
                colorFromInternal = colorFrom;
                colorToInternal = colorTo;
            }
            else
            {
                colorFromInternal = colorTo;
                colorToInternal = colorFrom;
            }

            config.SetOnlyColor(colorFromInternal, colorToInternal);
            break;
        case COLOR_FADE_SCHEME.SHADER_BRIGHTNESS:
            if (direction == FromToDir.Forward)
            {
                brightnessFromInternal = brightnessFrom;
                brightnessToInternal = brightnessTo;
            }
            else
            {
                brightnessFromInternal = brightnessTo;
                brightnessToInternal = brightnessFrom;
            }

            config.SetOnlyBrightness(brightnessFromInternal, brightnessToInternal);
            break;
        default:
            Debug.LogError("NO MATCH Please Add");
            break;
    }
}

void ReleaseConfig()
{
    config.ClearMaterial();
}

public void Activate(FromToDir direction)
{
    if (targetRenderer == null)
    {
        return;
    }

    RemoveFromManager();

    SetConfigByDirection(direction);

    ColorFadeManager.Instance.Activate(config);
}

void RemoveFromManager()
{
    ColorFadeManager.Instance.Remove(key);
}

public void Reset_()
{
    RemoveFromManager();

    if (targetRenderer != null)
    {
        Array.ForEach(targetRenderer.materials, t => ColorFadeManager.Instance.Reset(config));
    }
}
}