using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearColorSweet : ClearedSweet {

    private ColorType clearColor;

    public ColorType ClearColor
    {
        get
        {
            return clearColor;
        }

        set
        {
            clearColor = value;
        }
    }

    public override void Clear()
    {
        base.Clear();
        sweet.gameManager.ClearColor(clearColor);
    }
}
