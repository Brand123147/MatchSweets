using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//甜品种类，是否会有特效，还是正常的糖果
public enum SweetType
{
    EMPTY,
    NORMAL,
    BARRIER,
    ROW_CLEAR,
    COLUMN_CLEAR,
    RAINBOWCANDY,
    COUNT,//标记类型
};

public class GameManager : MonoBehaviour
{
    #region 单例

    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            return _instance;
        }
        set
        {
            _instance = value;
        }
    }
    private void Awake()
    {
        _instance = this;
    }
    private void Destroy()
    {
        _instance = null;
    }

    #endregion



    //通过结构体序列化，让字典在unity中显示出来
    [System.Serializable]
    public struct sweetPrefab
    {
        public SweetType type;//作为字典的key
        public GameObject prefab;//作为字典的value
    }
    public sweetPrefab[] sweetPrefabs;//结构体类型的数组，之后传入字典

    //大网格行列数
    public int xColumn;
    public int yRow;

    //甜品预制体字典
    public Dictionary<SweetType, GameObject> sweetPrefabDict;
    public GameObject gridPrefab;//巧克力网格

    // 行列式实例化甜品
    public GameSweet[,] sweets;


    //填充时间
    public float fillTime;

    //可交换的两个甜品
    private GameSweet pressedSweet;
    private GameSweet enteredSweet;

    // Use this for initialization
    void Start()
    {
        //绘制网格
        for (int x = 0; x < xColumn; x++)
        {
            for (int y = 0; y < yRow; y++)
            {
                GameObject chocolate = Instantiate(gridPrefab, CorrectPosition(x, y), Quaternion.identity);
                chocolate.transform.SetParent(transform);

            }
        }
        //字典实例化
        sweetPrefabDict = new Dictionary<SweetType, GameObject>();
        //遍历一圈字典如果不存在则将类型和甜品都加到字典里
        for (int i = 0; i < sweetPrefabs.Length; i++)
        {
            if (!sweetPrefabDict.ContainsKey(sweetPrefabs[i].type))
            {
                sweetPrefabDict.Add(sweetPrefabs[i].type, sweetPrefabs[i].prefab);
            }
        }

        //实例化甜甜圈，将糖果渲染出来并不确定是彩虹糖还是棒棒糖
        sweets = new GameSweet[xColumn, yRow];
        for (int x = 0; x < xColumn; x++)
        {
            for (int y = 0; y < yRow; y++)
            {
                CreateNewSweet(x, y, SweetType.EMPTY);
            }
        }


        //添加饼干障碍
        CreateBarrier();

        StartCoroutine(AllFill());//填充,协同程序延迟掉落

    }


    // 视频绘制网格不在中间的处理方法
    public Vector3 CorrectPosition(int x, int y)
    {
        return new Vector3(transform.position.x - xColumn / 2f + x, transform.position.y + yRow / 2f - y);
    }


    //产生甜品的方法
    public GameSweet CreateNewSweet(int x, int y, SweetType type)
    {
        GameObject newSweet = Instantiate(sweetPrefabDict[type], CorrectPosition(x, y), Quaternion.identity);
        newSweet.transform.parent = transform;

        sweets[x, y] = newSweet.GetComponent<GameSweet>();
        sweets[x, y].Init(x, y, this, type);
        return sweets[x, y];
    }

    /// <summary>
    /// 填充，循环一轮
    /// </summary>
    public bool Fill()
    {
        bool filledNotFinished = false;//判断本次是否填充完成
        for (int y = yRow - 2; y >= 0; y--)
        {
            for (int x = 0; x < xColumn; x++)
            {
                GameSweet sweet = sweets[x, y];//得到当前元素位置的甜品对象
                if (sweet.CanMove())//如果可以移动
                {
                    GameSweet sweetBelow = sweets[x, y + 1];  //获取下一行位置
                    if (sweetBelow.Type == SweetType.EMPTY)//如果为空，则往下填充
                    {
                        //Destroy(sweetBelow.gameObject);可保护可不保护
                        sweet.MovedComponent.Move(x, y + 1, fillTime);
                        sweets[x, y + 1] = sweet;//下一行等于当前行的东西
                        CreateNewSweet(x, y, SweetType.EMPTY);//移动之后把自身置空
                        filledNotFinished = true;//自身格子判断填充未完成
                    }
                    else//否则向左下角，右下角移动
                    {
                        for (int down = -1; down <= 1; down++)  //-1左边 0当前 1右边
                        {
                            if (down != 0)
                            {
                                int downX = x + down;
                                if (downX >= 0 && downX < xColumn)//判断是否在最左跟最右列之间，是才移动
                                {
                                    GameSweet downSweet = sweets[downX, y + 1];
                                    if (downSweet.Type == SweetType.EMPTY)
                                    {
                                        bool canfill = true;//用来判断垂直填充是否可以满足填充要求
                                        for (int aboveY = y; aboveY >= 0; aboveY--)//当前y行  ，  从下往上遍历
                                        {
                                            GameSweet sweetAbove = sweets[downX, aboveY];
                                            if (sweetAbove.CanMove())
                                            {
                                                break;
                                            }
                                            else if (!sweetAbove.CanMove() && sweetAbove.Type != SweetType.EMPTY)
                                            {
                                                canfill = false;
                                                break;
                                            }
                                        }
                                        if (!canfill)//如果垂直填充不满足，则斜向填充
                                        {
                                            //Destroy(downSweet.gameObject);可保护可不保护
                                            sweet.MovedComponent.Move(downX, y + 1, fillTime);
                                            sweets[downX, y + 1] = sweet;//填充
                                            CreateNewSweet(x, y, SweetType.EMPTY);//将原来格子置空
                                            filledNotFinished = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                    }
                }

            }
        }

        //最上排的特殊情况
        for (int x = 0; x < xColumn; x++)
        {
            GameSweet sweet = sweets[x, 0];//获取最上行的甜品对象
            if (sweet.Type == SweetType.EMPTY)//最上行是否为空
            {
                GameObject newSweet = Instantiate(sweetPrefabDict[SweetType.NORMAL], CorrectPosition(x, -1), Quaternion.identity);//实例化-1行（生成）
                newSweet.transform.parent = transform;

                sweets[x, 0] = newSweet.GetComponent<GameSweet>();//当前行拿到组件
                sweets[x, 0].Init(x, -1, this, SweetType.NORMAL);//初始化
                sweets[x, 0].MovedComponent.Move(x, 0, fillTime);//移动到第一行，位置脚本
                sweets[x, 0].ColoredComponent.SetColor((ColorType)Random.Range(0, sweets[x, 0].ColoredComponent.NumColors));//随机设置什么颜色类型的甜品,由于参数是ColorSweet脚本组件的枚举类型
                filledNotFinished = true;
            }
        }
        return filledNotFinished;
    }


    /// <summary>
    /// 全部填充
    /// </summary>
    public IEnumerator AllFill()
    {
        bool needRefill = true;
        while (needRefill)
        {
            yield return new WaitForSeconds(fillTime);
            while (Fill())
            {
                yield return new WaitForSeconds(fillTime);
            }
            //清除所有我们已经匹配好的甜品
            needRefill = ClearAllMatchedSweet();
        }

    }


    //是否相邻
    private bool isFriend(GameSweet sweet1, GameSweet sweet2)
    {
        return ((sweet1.X == sweet2.X) && (Mathf.Abs(sweet1.Y - sweet2.Y) == 1)) || ((sweet1.Y == sweet2.Y) && (Mathf.Abs(sweet1.X - sweet2.X) == 1));
    }

    //交换两个甜品
    private void ExChange(GameSweet sweet1, GameSweet sweet2)
    {
        if (sweet1.CanMove() && sweet2.CanMove())
        {

            sweets[sweet1.X, sweet1.Y] = sweet2;
            sweets[sweet2.X, sweet2.Y] = sweet1;
            if (MatchSweets(sweet2, sweet1.X, sweet1.Y) != null || MatchSweets(sweet1, sweet2.X, sweet2.Y) != null
                || sweet1.Type == SweetType.RAINBOWCANDY || sweet2.Type == SweetType.RAINBOWCANDY)
            {
                //交换
                int tempX = sweet1.X;
                int tempY = sweet1.Y;


                sweet1.MovedComponent.Move(sweet2.X, sweet2.Y, fillTime);
                sweet2.MovedComponent.Move(tempX, tempY, fillTime);

                if (sweet1.Type == SweetType.RAINBOWCANDY && sweet1.CanClear() && sweet2.CanClear())
                {
                    ClearColorSweet clearColor = sweet1.GetComponent<ClearColorSweet>();
                    if (clearColor != null)
                    {
                        clearColor.ClearColor = sweet2.ColoredComponent.Color;
                    }
                    ClearSweet(sweet1.X, sweet1.Y);
                }

                if (sweet2.Type == SweetType.RAINBOWCANDY && sweet2.CanClear() && sweet1.CanClear())
                {
                    ClearColorSweet clearColor = sweet2.GetComponent<ClearColorSweet>();
                    if (clearColor != null)
                    {
                        clearColor.ClearColor = sweet1.ColoredComponent.Color;
                    }
                    ClearSweet(sweet2.X, sweet2.Y);
                }


                ClearAllMatchedSweet();//清除
                StartCoroutine(AllFill());

                //交换之后将这两个置空，方便下次再调用，不会出现bug
                pressedSweet = null;
                enteredSweet = null;
            }
            else//不交换
            {
                sweets[sweet1.X, sweet1.Y] = sweet1;
                sweets[sweet2.X, sweet2.Y] = sweet2;
            }
        }
    }

    #region 鼠标按键功能 

    public void PressSweet(GameSweet sweet)
    {
        if (UIManager.gameTime == 0)
        {
            return;
        }
        pressedSweet = sweet;
    }
    public void EnterSweet(GameSweet sweet)
    {
        if (UIManager.gameTime == 0)
        {
            return;
        }
        enteredSweet = sweet;
    }
    public void ReleaseSweet()
    {
        if (UIManager.gameTime == 0)
        {
            return;
        }
        if (isFriend(pressedSweet, enteredSweet))
        {
            ExChange(pressedSweet, enteredSweet);
        }
    }

    #endregion

    //匹配方法
    public List<GameSweet> MatchSweets(GameSweet sweet, int newX, int newY)
    {
        if (sweet.CanColor())
        {
            ColorType color = sweet.ColoredComponent.Color;
            List<GameSweet> matchRowSweet = new List<GameSweet>();//匹配行
            List<GameSweet> matchColumnSweet = new List<GameSweet>();//匹配列
            List<GameSweet> finishedMatchSweets = new List<GameSweet>();//完成匹配

            //行匹配，嵌套列匹配
            matchRowSweet.Add(sweet);//添加基准甜品
            //i=0往左，i=1往右遍历
            for (int i = 0; i <= 1; i++)
            {
                for (int xDistance = 1; xDistance < xColumn; xDistance++)
                {
                    int x;//偏移之后的坐标
                    if (i == 0)//往左
                    {
                        x = newX - xDistance;//向左走
                    }
                    else//i=1时
                    {
                        x = newX + xDistance;//向右走
                    }
                    if (x < 0 || x >= xColumn)//保护x范围
                    {
                        break;//结束循环遍历
                    }

                    if (sweets[x, newY].CanColor() && sweets[x, newY].ColoredComponent.Color == color)//是否一样颜色
                    {
                        matchRowSweet.Add(sweets[x, newY]);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            //先添加进完成匹配列表
            if (matchRowSweet.Count >= 3)
            {
                for (int i = 0; i < matchRowSweet.Count; i++)
                {
                    finishedMatchSweets.Add(matchRowSweet[i]);
                }
            }

            //检查当前行遍历列表元素>=3??
            if (matchRowSweet.Count >= 3)
            {
                for (int i = 0; i < matchRowSweet.Count; i++)//遍历逐个添加进finishedMatchSweets列表
                {
                    //已经满足行匹配，看是否还有没有满足列匹配的TL型消除
                    //0代表上   1代表下方
                    for (int j = 0; j <= 1; j++)
                    {
                        for (int yDistance = 1; yDistance < yRow; yDistance++)
                        {
                            int y;
                            if (j == 0)//向上遍历
                            {
                                y = newY - yDistance;
                            }
                            else
                            {
                                y = newY + yDistance;
                            }
                            if (y >= yRow || y < 0)//不超出范围
                            {
                                break;
                            }
                            if (sweets[matchRowSweet[i].X, y].CanColor() && sweets[matchRowSweet[i].X, y].ColoredComponent.Color == color)//行遍历跟列遍历之后是否相同颜色
                            {
                                matchColumnSweet.Add(sweets[matchRowSweet[i].X, y]);
                            }
                            else
                            {
                                break;
                            }
                        }

                    }

                    if (matchColumnSweet.Count < 2)
                    {
                        matchColumnSweet.Clear();
                    }
                    else
                    {
                        for (int j = 0; j < matchColumnSweet.Count; j++)
                        {
                            finishedMatchSweets.Add(matchColumnSweet[j]);
                        }
                        break;
                    }

                }
            }
            if (finishedMatchSweets.Count >= 3)//如果完成匹配列表元素大于3，则直接返回整个列表
            {
                return finishedMatchSweets;
            }

            //清空一下，接收下一次匹配
            matchRowSweet.Clear();
            matchColumnSweet.Clear();



            //列匹配，嵌套行匹配
            matchColumnSweet.Add(sweet);//添加基准甜品
                                        //i=0往左，i=1往右遍历
            for (int i = 0; i <= 1; i++)
            {
                for (int yDistance = 1; yDistance < yRow; yDistance++)
                {
                    int y;//偏移之后的坐标
                    if (i == 0)//往左
                    {
                        y = newY - yDistance;//向左走
                    }
                    else//i=1时
                    {
                        y = newY + yDistance;//向右走
                    }
                    if (y < 0 || y >= yRow)//保护x范围
                    {
                        break;//结束循环遍历
                    }

                    if (sweets[newX, y].CanColor() && sweets[newX, y].ColoredComponent.Color == color)//是否一样颜色
                    {
                        matchColumnSweet.Add(sweets[newX, y]);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            //先添加进完成匹配列表
            if (matchColumnSweet.Count >= 3)
            {
                for (int i = 0; i < matchColumnSweet.Count; i++)
                {
                    finishedMatchSweets.Add(matchColumnSweet[i]);    //如果超过3个就添加进最后匹配的列表
                }
            }

            //L T型匹配
            //检查当前列遍历列表元素>=3??
            if (matchColumnSweet.Count >= 3)
            {
                for (int i = 0; i < matchColumnSweet.Count; i++)//遍历逐个添加进finishedMatchSweets列表
                {
                    //已经满足列匹配，看是否还有没有满足列匹配的TL型消除
                    //0代表上   1代表下方
                    for (int j = 0; j <= 1; j++)
                    {
                        for (int xDistance = 1; xDistance < xColumn; xDistance++)
                        {
                            int x;
                            if (j == 0)//向上遍历
                            {
                                x = newX - xDistance;
                            }
                            else
                            {
                                x = newX + xDistance;
                            }
                            if (x >= xColumn || x < 0)
                            {
                                break;
                            }
                            if (sweets[x, matchColumnSweet[i].Y].CanColor() && sweets[x, matchColumnSweet[i].Y].ColoredComponent.Color == color)
                            {
                                matchRowSweet.Add(sweets[x, matchColumnSweet[i].Y]);
                            }
                            else
                            {
                                break;
                            }
                        }

                    }

                    if (matchRowSweet.Count < 2)
                    {
                        matchRowSweet.Clear();
                    }
                    else
                    {
                        for (int j = 0; j < matchRowSweet.Count; j++)
                        {
                            finishedMatchSweets.Add(matchRowSweet[j]);
                        }
                    }
                }
            }
            if (finishedMatchSweets.Count >= 3)//如果完成匹配列表元素大于3，则直接返回整个列表
            {
                return finishedMatchSweets;
            }
        }
        return null;
    }

    //清除方法
    public bool ClearSweet(int x, int y)
    {
        if (sweets[x, y].CanClear() && !sweets[x, y].ClearedComponent.IsClearing)//判断是否是可以清除的，并且没有正在执行的清除协同程序
        {
            sweets[x, y].ClearedComponent.Clear();//清除
            CreateNewSweet(x, y, SweetType.EMPTY);//置空

            ClearBarrier(x, y);
            return true;
        }
        return false;
    }


    /// <summary>
    /// 清除饼干的方法
    /// </summary>
    /// <param 甜品自身="x"></param>
    /// <param 甜品自身="y"></param>
    /// friendX,friendY为饼干障碍的位置
    private void ClearBarrier(int x, int y)
    {
        for (int friendX = x - 1; friendX <= x + 1; friendX++)//x轴方向
        {
            if (friendX != x && friendX >= 0 && friendX < xColumn)
            {
                if (sweets[friendX, y].Type == SweetType.BARRIER && sweets[friendX, y].CanClear())//判断是障碍类型，并且可以消除
                {
                    sweets[friendX, y].ClearedComponent.Clear();//调用消除
                    CreateNewSweet(friendX, y, SweetType.EMPTY);//置空
                }
            }
        }
        for (int friendY = y - 1; friendY <= y + 1; friendY++)//y轴方向
        {
            if (friendY != y && friendY >= 0 && friendY < yRow)
            {
                if (sweets[x, friendY].Type == SweetType.BARRIER && sweets[x, friendY].CanClear())
                {
                    sweets[x, friendY].ClearedComponent.Clear();
                    CreateNewSweet(x, friendY, SweetType.EMPTY);
                }
            }
        }
    }

    //清除全部完成匹配的甜品
    private bool ClearAllMatchedSweet()
    {
        bool needRefill = false;//是否重新填充

        //遍历场景是否存在可清除的甜品
        for (int y = 0; y < yRow; y++)
        {
            for (int x = 0; x < xColumn; x++)
            {
                if (sweets[x, y].CanClear())
                {
                    List<GameSweet> matchList = MatchSweets(sweets[x, y], x, y);//用一个列表去接收匹配成功的返回来的甜品列表
                    if (matchList != null)
                    {//如果不为空则遍历列表中的每一个甜品对象进行清除

                        SweetType specialSweetsType = SweetType.COUNT;//定义一个特殊类型的甜品
                        GameSweet randomSweet = matchList[Random.Range(0, matchList.Count)];//随机一个匹配成功之后的糖果选定一个,即3个选一个
                        int specialSweetX = randomSweet.X;
                        int specialSweetY = randomSweet.Y;


                        if (matchList.Count == 4)
                        {
                            //随机出一个(生成)行消除或者列消除的特殊甜品
                            specialSweetsType = (SweetType)Random.Range((int)SweetType.ROW_CLEAR, (int)SweetType.COLUMN_CLEAR + 1);
                        }
                        else if (matchList.Count >= 5)//大于4就产生彩虹糖
                        {
                            //随机出一个(生成)行消除或者列消除的特殊甜品
                            specialSweetsType = SweetType.RAINBOWCANDY;
                        }


                        for (int i = 0; i < matchList.Count; i++)
                        {
                            if (ClearSweet(matchList[i].X, matchList[i].Y))
                            {
                                needRefill = true;//是否重新填充
                            }
                        }

                        if (specialSweetsType != SweetType.COUNT)
                        {//有问题，需要填充，没有为true，还是昨晚想到的那个

                            //Destroy(sweets[specialSweetX, specialSweetY]);
                            GameSweet newSweet = CreateNewSweet(specialSweetX, specialSweetY, specialSweetsType);//生成特殊甜品对象

                            if (specialSweetsType == SweetType.ROW_CLEAR || specialSweetsType == SweetType.COLUMN_CLEAR && matchList[0].CanColor() && newSweet.CanColor())
                            {
                                newSweet.ColoredComponent.SetColor(matchList[0].ColoredComponent.Color);
                            }
                            if (specialSweetsType == SweetType.RAINBOWCANDY && newSweet.CanColor())
                            {
                                newSweet.ColoredComponent.SetColor(ColorType.COLORS);
                            }

                        }
                    }
                }
            }
        }
        return needRefill;
    }


    //清除行的方法
    public void ClearRow(int row)
    {
        for (int x = 0; x < xColumn; x++)
        {
            ClearSweet(x, row);
        }
    }

    //清除列的方法
    public void ClearColumn(int column)
    {
        for (int y = 0; y < yRow; y++)
        {
            ClearSweet(column, y);
        }
    }

    //彩虹糖清除所有同一颜色
    public void ClearColor(ColorType color)
    {
        for (int x = 0; x < xColumn; x++)
        {
            for (int y = 0; y < yRow; y++)
            {
                if (sweets[x, y].CanColor() && (sweets[x, y].ColoredComponent.Color == color || color == ColorType.COLORS))
                {
                    ClearSweet(x, y);
                }
            }
        }
    }

    //添加障碍
    private void CreateBarrier()
    {
        Destroy(sweets[4, 4].gameObject);
        CreateNewSweet(4, 4, SweetType.BARRIER);
        Destroy(sweets[6, 2].gameObject);
        CreateNewSweet(6, 2, SweetType.BARRIER);
        Destroy(sweets[2, 2].gameObject);
        CreateNewSweet(2, 2, SweetType.BARRIER);
        Destroy(sweets[2, 6].gameObject);
        CreateNewSweet(2, 6, SweetType.BARRIER);
        Destroy(sweets[6, 6].gameObject);
        CreateNewSweet(6, 6, SweetType.BARRIER);
    }
}
