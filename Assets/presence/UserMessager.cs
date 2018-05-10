using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UserMessager : MonoBehaviour {

   
   

  public  Text MessageText;
    float alpha=0;
    float deltaAlpha=0;
    Color TextColor;
    UnityEvent OnFadeOut,OnFadeIn,OnTimeOut;
    string nextMessage;
    float timeOut,messageDuration;
   // public Text UserMessageText;


	// Use this for initialization
	void Start () {
        
      //  MessageText=GetComponentInChildren<Text>();

        TextColor=MessageText.color; // Get from the editor.

        OnFadeOut=new UnityEvent();
        OnFadeIn=new UnityEvent();
        OnTimeOut=new UnityEvent();

	}
	
	// Update is called once per frame
	void Update () {
		
        TextColor.a=alpha;
        MessageText.color = TextColor;

        alpha+=deltaAlpha*Time.deltaTime;

        if (alpha<0){
            alpha=0;
            deltaAlpha=0;
            MessageText.text="";
            OnFadeOut.Invoke();

        }

        if (alpha>1){
            alpha=1;
            deltaAlpha=0;
            OnFadeIn.Invoke();
        }

        if (Time.time > timeOut){

            OnTimeOut.Invoke();

        }
 

	}

    void ShowNextMessage (){
        //Debug.Log("next message");
        OnFadeOut.RemoveListener(ShowNextMessage);
        ShowTextMessage(nextMessage,messageDuration);

    }

    void FadeOut(){

        deltaAlpha=-1;
        OnTimeOut.RemoveListener(FadeOut);

    }

    void SetTimeOut(){
        OnFadeIn.RemoveListener(SetTimeOut);
        //Debug.Log("set timeout");
        timeOut=Time.time+messageDuration;
        OnTimeOut.AddListener(FadeOut);

    }

    public void TextMessageOff(){

        deltaAlpha=-1;

    }


    public void ShowTextMessage (string message,float time=0){
        
       

        if (MessageText.text!="" ){
            
            // something still visible, fade out first.
            //Debug.Log("message visible, queueing "+message);
            nextMessage=message;

            if ( messageDuration==0) 
            deltaAlpha=-1;// if current message has no timeout we fade out. else we wait.

            OnFadeOut.AddListener(ShowNextMessage);
            messageDuration=time;     
           

        } else if (MessageText.text!=message) {

            //Debug.Log("displaying message "+message);

            messageDuration=time;     

            MessageText.text=message;
            deltaAlpha=1;
            if (messageDuration>0)
            OnFadeIn.AddListener(SetTimeOut);

        }


               


    }
     








}
