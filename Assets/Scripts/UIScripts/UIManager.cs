using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    const int happytime = 100;
    const int happyScore = 0;
	public static float gameTime = happytime;//游戏时间
	public static float gameScore = happyScore;//游戏得分
	public Text playerTime;
	public Text playerScore;
    public Text finalScore;//最后结算面板得分

	//分数逐渐增加显示
	private float updateScoreTime ;
	private float currentScore ;

    //结算面板
    public GameObject Panel_GameOVer;


    public void OnValueVoice()

    {
        AudioSource voice = GetComponent<AudioSource>();

        Transform scro = transform.Find("Scrollbar_voice");

        Scrollbar voiceValue = scro.GetComponent<Scrollbar>();
        
        voice.volume = voiceValue.value;
    }
    //声音关闭按钮
    public void OnClickVoiceClose()
    {
        AudioSource voiceClose = GetComponent<AudioSource>();
        voiceClose.volume=0;
    }
    //声音开启
    public void OnClickVoiceOpen()
    {
        AudioSource voiceOpen = GetComponent<AudioSource>();
        voiceOpen.volume=1;
    }
    //返回按钮
    public void OnClickBack(){
        SceneManager.LoadScene(0);
	}
    //重玩按钮
    public void OnClickReplay()
    {
        SceneManager.LoadScene(1);
        gameTime = happytime;
        gameScore = happyScore;
    }

	
	// Update is called once per frame
	void Update () {

		//游戏倒计时
		gameTime -= Time.deltaTime;
		if(gameTime <= 0){
			gameTime = 0;
            finalScore.text = playerScore.text;
            Panel_GameOVer.SetActive(true);
            return;
        }
		playerTime.text = gameTime.ToString("0");
        


        //分数更新		
        updateScoreTime += Time.deltaTime;
		if(updateScoreTime >= 0.1f && currentScore < gameScore){			
			currentScore++;
			playerScore.text = currentScore.ToString();
			updateScoreTime=0;
            return;
		}
	}
}
