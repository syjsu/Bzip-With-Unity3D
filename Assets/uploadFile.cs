using UnityEngine;
using System.Collections;

public class uploadFile : MonoBehaviour {

	void OnGUI(){
		if (GUILayout.Button ("上传文件")) {
			StartCoroutine (UploadFile());		
		}
	}

	public Texture2D tex;
	IEnumerator UploadFile(){
		byte[] bs = tex.EncodeToPNG ();

		WWWForm f = new WWWForm ();
		f.AddBinaryData ("file", bs,"test.png", "image/png");
		WWW www = new WWW ("http://littlesmallsu.applinzi.com/public/file.php",f);

		yield return www;
		if(www.error != null){
			Debug.Log (www.error);
			yield return null;
		}
		if(www.isDone){
			Debug.Log (www.text);
		}
	}
}
