using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 안전영역 조정용 (현재사용안함 이스크립트는)
 * */
public class SafeAreaAdjustor : MonoBehaviour
{
    public enum Dir_Horizontal
    {
        LEFT,
        RIGHT
    }
    public enum Dir_Vertical
    {
        UP,
        DOWN
    }

    int lastAppliedWidth = -1;

    float lastAppliedWidthRatio;

    public bool moveHorizontal;
    public bool moveVertical;
    bool hasSetNativePos;

    public Dir_Horizontal dirHorizontal;
    public Dir_Vertical dirVertical;

    Vector2 nativeLocalPos;

    SafeAreaManager mgr;

    private void Start()
    {
        if (GetComponent<Animation>() != null)
        {
            Debug.LogWarning("SafeAreaAdjustor may not work with animation");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (mgr == null)
        {
            mgr = SafeAreaManager.Instance;

            if (mgr == null)
                return;
        }

        if (mgr.UseSafeArea == false ||
            mgr.LastAppliedWidth == lastAppliedWidth)
            return;

        if (hasSetNativePos == false)
        {
            hasSetNativePos = true;
            nativeLocalPos = transform.localPosition;
        }

        UpdateLayout(mgr.SafeAreaWidthRatio, mgr.SafeAreaHeightRatio, nativeLocalPos);

        lastAppliedWidth = mgr.LastAppliedWidth;
    }

    private void UpdateLayout(
        float widthRatio,
        float heightRatio,
        Vector3 nativeLocalPos)
    {
        int widthOffset = 0;
        int heightOffset = 0;

        if (moveHorizontal)
        {
            widthOffset = (int)(Screen.width * widthRatio);

            if (dirHorizontal == Dir_Horizontal.LEFT)
            {
                widthOffset *= -1;
            }
        }

        if (moveVertical)
        {
            heightOffset = (int)(Screen.height * heightRatio);

            if (dirVertical == Dir_Vertical.DOWN)
            {
                heightOffset *= -1;
            }
        }

        Vector3 offset = new Vector3(widthOffset, heightOffset, 0);

        transform.localPosition = nativeLocalPos + offset;
    }
}
