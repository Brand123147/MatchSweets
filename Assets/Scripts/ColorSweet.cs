using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ColorType
{  //具体什么糖果，是彩虹糖还是棒棒糖
    YELLOW,
    PURPLE,
    RED,
    BLUE,
    GREEN,
    PINK,
    COLORS,
    COUNT
}

public class ColorSweet : MonoBehaviour
{

    public int NumColors
    {   //有多少种颜色数量糖果
        get { return ColorSprites.Count; }
    }

    [System.Serializable]
    public struct ColorSprite
    {
        public ColorType color;
        public Sprite sprite;
    }
    public List<ColorSprite> ColorSprites;
    private Dictionary<ColorType, Sprite> colorSpriteDict;//声明字典

    private SpriteRenderer sprite; //声明sprite为SpriteRenderer

    private void Awake()
    {
        sprite = transform.Find("BlueSweet_sp").GetComponent<SpriteRenderer>();//实例化精灵
        colorSpriteDict = new Dictionary<ColorType, Sprite>();//实例化字典
        for (int i = 0; i < ColorSprites.Count; i++)
        {   //初始化字典
            if (!colorSpriteDict.ContainsKey(ColorSprites[i].color))
            {
                colorSpriteDict.Add(ColorSprites[i].color, ColorSprites[i].sprite);
            }
        }
    }


    private ColorType color;//设置ColorTpye
    public ColorType Color
    {
        get { return color; }
        set { SetColor(value); }
    }
    public void SetColor(ColorType newColor)
    {  //保护设置_color
        color = newColor;
        if (colorSpriteDict.ContainsKey(newColor))
        {
            //如果已经包含newColor
            sprite.sprite = colorSpriteDict[newColor];  //就把这个渲染出来，即字典中的value
        }
    }
}
