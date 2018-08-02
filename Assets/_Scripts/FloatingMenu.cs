﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingMenu : MonoBehaviour
{

    bool expanded = false;
    public Vector3 init;
    public Vector3 target;
    RectTransform rect;

    // Use this for initialization
    void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Switch()
    {
        expanded = !expanded;
    }

    private void FixedUpdate()
    {

        float currRatio = (rect.position - init).magnitude / (target - init).magnitude;
        if (expanded && currRatio < 1)
        {
            rect.position = Vector3.Lerp(init, target, currRatio + 0.05f);
        }
        if (!expanded && currRatio > 0)
        {
            rect.position = Vector3.Lerp(init, target, currRatio - 0.05f);
        }
    }
}
