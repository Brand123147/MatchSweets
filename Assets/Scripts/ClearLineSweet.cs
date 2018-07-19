using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//继承清除脚本的类，包括属性方法
public class ClearLineSweet : ClearedSweet {
    public bool isRow;
    public override void Clear()
    {
        base.Clear();
        if (isRow)
        {
            sweet.gameManager.ClearRow(sweet.Y);
        }
        else
        {
            sweet.gameManager.ClearColumn(sweet.X);
        }
    }
}
