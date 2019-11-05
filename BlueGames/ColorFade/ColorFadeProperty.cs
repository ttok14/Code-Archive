using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum COLOR_FADE_SCHEME
{
    MATERIAL_COLOR, // Material 기본 멤버변수 color
    SHADER_COLOR, // 쉐이더의 Color property (fixed4)
    SHADER_BRIGHTNESS // 쉐이더의 float property
}

public enum COLOR_FADE_FINALIZE_OP
{
    DO_NOTHING,
   DISCARD_MATERIAL_LIST,
}

public struct ColorFadeConfiguration
{
    // 페이드시킬 매터리얼 리스트
    public List<Material> mats;
    // 
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