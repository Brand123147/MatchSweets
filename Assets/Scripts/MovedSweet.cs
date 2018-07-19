using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovedSweet : MonoBehaviour {
	private GameSweet sweet;
    private IEnumerator moveCoroutine;//这样得到其他指令的时候我们可以终止这个协同程序
	private void Awake(){
		sweet = GetComponent<GameSweet>();
	}

    //public void Move(int newX, int newY){
    //	sweet.X = newX;
    //	sweet.Y = newY;
    //       // 一般来说这里移动应该设置好移动之后的位置的
    //       //补充：原来之前是用世界坐标所以不用localposition，直接position就可以了 
    //       sweet.transform.position = GameManager.Instance.CorrectPosition(newX, newY);
    //       sweet.gameManager.CorrectPosition(newX, newY);
    //   }

    //开始或者结束一个协同程序
    public void Move(int newX,int newY,float time)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        moveCoroutine = MoveCoroutine(newX, newY, time); //控制协同
        StartCoroutine(moveCoroutine);//开启协同
    }

    //负责移动的协同程序
    private IEnumerator MoveCoroutine(int newX, int newY,float time)
    {
        //移动后的位置
        sweet.X = newX;
        sweet.Y = newY;

        //每一帧移动一点点
        Vector3 startPos = transform.position;//移动之前的位置
        Vector3 endPos = sweet.gameManager.CorrectPosition(newX, newY);//移动之后的位置

        for (float t = 0; t < time; t+=Time.deltaTime)
        {
            //给它一个总时间，每一帧移动时间比总时间，第3个参数是比例因数
            sweet.transform.position = Vector3.Lerp(startPos, endPos, t / time);//插值
            yield return 0;
        }
        sweet.transform.position = endPos;//做一个保护，万一没有移动到想要的endPos位置时，直接置为endPos。
    }
}
