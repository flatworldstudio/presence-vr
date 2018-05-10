using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StoryEngine;

public class ConfineMent : MonoBehaviour {


	//private void OnCollisionEnter()
	//{
 //       Debug.Log("ENTER");

 //       GENERAL.UserInConfinedArea=false;
	//}


    //private void OnCollisionExit()
    //{

    //    Debug.Log("EXIT");

    //    GENERAL.UserInConfinedArea=true;

    //}

	void OnTriggerEnter() {
        //Debug.Log("ENTER");

        GENERAL.UserInConfinedArea=false;

    }
    void OnTriggerExit () {
        //Debug.Log("EXIT");

        GENERAL.UserInConfinedArea=true;

    }
}
