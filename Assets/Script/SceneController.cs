using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class SceneController : MonoBehaviour {

	public void ChangeScene_Int (int SceneNumber){
		SceneManager.LoadScene (SceneNumber);
	}
	public void ChangeScene_String(string SceneName){
		SceneManager.LoadScene (SceneName);
	}

}
