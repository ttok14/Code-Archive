using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public struct ColorFadeConfiguration
{
    public List<Material> mats;
    public COLOR_FADE_SCHEME scheme;
    public COLOR_FADE_FINALIZE_OP finalizeOption;
    public float duration;

    public string shaderColorPropertyName;
    public string shaderBrightnessPropertyName;
    public string key;

    public Color resetColor, colorFrom, colorTo;
    public float resetBrightness, brightnessFrom, brightnessTo;

    public const float defaultMin = 0.2f;

    public Action<string> onEnd;

    public ColorFadeConfiguration(int matCnt)
    {
        mats = new List<Material>(matCnt);
        scheme = COLOR_FADE_SCHEME.MATERIAL_COLOR;
        duration = 1;
        key = shaderBrightnessPropertyName = shaderColorPropertyName = string.Empty;
        resetColor = colorFrom = Color.white;
        colorTo = new Color(defaultMin, defaultMin, defaultMin, 1);
        resetBrightness = brightnessFrom = 1;
        brightnessTo = defaultMin;
        onEnd = null;
        finalizeOption = COLOR_FADE_FINALIZE_OP.DO_NOTHING;
    }

    public void Release(bool discardMatList)
    {
        if (discardMatList)
        {
            if (mats != null)
            {
                for (int i = 0; i < mats.Count; i++)
                {
                    mats[i] = null;
                }

                mats.Clear();
                mats = null;
            }
        }

        key = null;
    }

    public void ClearMaterial()
    {
        if (mats != null)
        {
            mats.Clear();
        }
    }

    public ColorFadeConfiguration Build()
    {
        return this;
    }

    public ColorFadeConfiguration AddMaterial(Material mat)
    {
        mats.Add(mat);
        return this;
    }
    public ColorFadeConfiguration SetScheme(COLOR_FADE_SCHEME scheme)
    {
        this.scheme = scheme;
        return this;
    }
    public ColorFadeConfiguration SetDuration(float duration)
    {
        this.duration = duration;
        return this;
    }
    public ColorFadeConfiguration SetOnlyColor(Color from, Color to)
    {
        colorFrom = from;
        colorTo = to;
        return this;
    }
    public ColorFadeConfiguration SetBaseColor(Color from, Color to)
    {
        scheme = COLOR_FADE_SCHEME.MATERIAL_COLOR;
        colorFrom = from;
        colorTo = to;
        return this;
    }
    public ColorFadeConfiguration SetShaderColor(string propertyName, Color from, Color to)
    {
        scheme = COLOR_FADE_SCHEME.SHADER_COLOR;
        shaderColorPropertyName = propertyName;
        colorFrom = from;
        colorTo = to;
        return this;
    }
    public ColorFadeConfiguration SetOnlyBrightness(float from, float to)
    {
        brightnessFrom = from;
        brightnessTo = to;
        return this;
    }
    public ColorFadeConfiguration SetShaderBrightness(string propertyName, float from, float to)
    {
        scheme = COLOR_FADE_SCHEME.SHADER_BRIGHTNESS;
        shaderBrightnessPropertyName = propertyName;
        brightnessFrom = from;
        brightnessTo = to;
        return this;
    }
    public ColorFadeConfiguration AddFinalizer(Action<string> onEnd, string key = "")
    {
        this.onEnd = onEnd;
        this.key = key;
        return this;
    }
    public ColorFadeConfiguration AddKey(string key)
    {
        this.key = key;
        return this;
    }
    public ColorFadeConfiguration SetResetColor(Color color)
    {
        resetColor = color;
        return this;
    }
    public ColorFadeConfiguration SetResetBrightness(float brightness)
    {
        resetBrightness = brightness;
        return this;
    }
    public ColorFadeConfiguration SetFinalizeOption(COLOR_FADE_FINALIZE_OP option)
    {
        finalizeOption = option;
        return this;
    }
}

public class ColorFadeEntity
{
    public ColorFadeConfiguration config;
    public float curTimeCheck;
}

public class ColorFadeManager : MonoBehaviour
{
    static ColorFadeManager instance;
    static public ColorFadeManager Instance
    {
        get
        {
            if (instance == null)
                new GameObject("ColorFadeManager").AddComponent<ColorFadeManager>();
            return instance;
        }
    }

    List<ColorFadeEntity> entities;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else Destroy(gameObject);

        entities = new List<ColorFadeEntity>();
    }

    public void Activate(params ColorFadeConfiguration[] config)
    {
        System.Array.ForEach(config, t =>
        {
            if (t.mats == null ||
            t.mats.Count == 0)
            {
                Debug.LogError("NO MATERIAL EXISTS");
            }
            else
            {
                var entity = new ColorFadeEntity();
                entity.config = t;
                entities.Add(entity);
            }
        });
    }

    public void Remove(params string[] keys)
    {
        foreach (var key in keys)
        {
            var target = FindEntity(key);

            if (target != null)
            {
                ReleaseEntity(target);
                entities.Remove(target);
            }
        }
    }

    ColorFadeEntity FindEntity(string key)
    {
        ColorFadeEntity result = null;

        for (int i = 0; i < entities.Count; i++)
        {
            if(entities[i].config.key.Equals(key, System.StringComparison.OrdinalIgnoreCase))
            {
                result = entities[i];
                break;
            }
        }

        return result;
    }

    private void Update()
    {
        foreach (var entity in entities)
        {
            UpdateFade(entity);
        }

        entities.RemoveAll(t =>
        {
            bool remove = t.curTimeCheck >= 1;

            if (remove)
            {
                ReleaseEntity(t);
            }

            return remove;
        });
    }

    void UpdateFade(ColorFadeEntity entity)
    {
        entity.curTimeCheck += Time.deltaTime / entity.config.duration;

        Color color = Color.clear;
        float brightness = 0;

        switch (entity.config.scheme)
        {
            case COLOR_FADE_SCHEME.MATERIAL_COLOR:
                color = Color.Lerp(entity.config.colorFrom, entity.config.colorTo, entity.curTimeCheck);

                entity.config.mats.ForEach(t =>
                {
                    SetMatColor(t, color);
                });
                break;
            case COLOR_FADE_SCHEME.SHADER_COLOR:
                color = Color.Lerp(entity.config.colorFrom, entity.config.colorTo, entity.curTimeCheck);

                entity.config.mats.ForEach(t =>
                {
                    SetMatShaderColor(t, entity.config.shaderColorPropertyName, color);
                });
                break;
            case COLOR_FADE_SCHEME.SHADER_BRIGHTNESS:
                brightness = Mathf.Lerp(entity.config.brightnessFrom, entity.config.brightnessTo, entity.curTimeCheck);

                entity.config.mats.ForEach(t =>
                {
                    SetMatShaderBrightness(t, entity.config.shaderBrightnessPropertyName, brightness);
                });
                break;
            default:
                Debug.LogError("NO TYPE MATCHING, PLEASE ADD");
                break;
        }
    }

    public void Reset(ColorFadeConfiguration config)
    {
        switch (config.scheme)
        {
            case COLOR_FADE_SCHEME.MATERIAL_COLOR:
                config.mats.ForEach(t => t.color = config.resetColor);
                break;
            case COLOR_FADE_SCHEME.SHADER_COLOR:
                config.mats.ForEach(t => t.SetColor(config.shaderColorPropertyName, config.resetColor));
                break;
            case COLOR_FADE_SCHEME.SHADER_BRIGHTNESS:
                config.mats.ForEach(t => t.SetFloat(config.shaderBrightnessPropertyName, config.resetBrightness));
                break;
            default:
                break;
        }
    }

    void SetMatColor(Material mat, Color col)
    {
        mat.color = col;
    }

    void SetMatShaderColor(Material mat, string propertyName, Color col)
    {
        mat.SetColor(propertyName, col);
    }

    void SetMatShaderBrightness(Material mat, string propertyName, float brightness)
    {
        mat.SetFloat(propertyName, brightness);
    }

    public void ReleaseEntity(ColorFadeEntity entity)
    {
        if (entity.config.onEnd != null)
        {
            entity.config.onEnd(entity.config.key);
        }

        switch (entity.config.finalizeOption)
        {
            case COLOR_FADE_FINALIZE_OP.DO_NOTHING:
                break;
            case COLOR_FADE_FINALIZE_OP.DISCARD_MATERIAL_LIST:
                entity.config.Release(true);
                break;
            default:
                Debug.LogError("PLEASE ADD OPTION");
                break;
        }
    }
}