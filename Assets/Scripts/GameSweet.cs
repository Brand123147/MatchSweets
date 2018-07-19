using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSweet : MonoBehaviour {
	private int x;
	private int y;
	public int X{
		get{
			return x;
		}
		set{
			if(CanMove()){
				x = value;
			}
		}
	}
	public int Y{
		get{
			return y;
		}
		set{
			if(CanMove()){
				y = value;
			}
		}
	}

	private SweetType type;
	public SweetType Type{
		get{
			return type;
		}
	}


	[HideInInspector]
	public GameManager gameManager;  //在inspector面板中隐藏

	private MovedSweet movedComponent;
	public MovedSweet MovedComponent{
		get{ return movedComponent; }
	}

	private ColorSweet coloredComponent;
	public ColorSweet ColoredComponent{
		get{ return coloredComponent; }
	}

    private ClearedSweet clearedComponent;
    public ClearedSweet ClearedComponent
    {
        get
        {
            return clearedComponent;
        }

        set
        {
            clearedComponent = value;
        }
    }


    //是否可更改甜品颜色
    public bool CanColor(){
		return coloredComponent != null;
	}
    //是否可移动
    public bool CanMove()
    {
        return movedComponent != null;
    }
    //是否可清除
    public bool CanClear()
    {
        return clearedComponent != null;
    }


    void Awake(){
		movedComponent = GetComponent<MovedSweet>();
		coloredComponent = GetComponent<ColorSweet>();
        clearedComponent = GetComponent<ClearedSweet>();
	}

	public void Init(int _x,int _y,GameManager _gameManager, SweetType _type){
		x = _x;
		y = _y;
		gameManager = _gameManager;
		type = _type;
	}




    private void OnMouseDown()
    {
        gameManager.PressSweet(this);
    }
    private void OnMouseEnter()
    {
        gameManager.EnterSweet(this);
    }
    private void OnMouseUp()
    {
        gameManager.ReleaseSweet();
    }
}
