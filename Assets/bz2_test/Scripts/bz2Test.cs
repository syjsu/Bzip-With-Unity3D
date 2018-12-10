using UnityEngine;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#if NETFX_CORE
    #if UNITY_WSA_10_0
        using System.IO.IsolatedStorage;
        using static System.IO.Directory;
        using static System.IO.File;
        using static System.IO.FileStream;
    #endif
#endif

public class bz2Test : MonoBehaviour
{
#if !UNITY_WEBPLAYER

	//we use some integer to get error codes from the lzma library (look at lzma.cs for the meaning of these error codes)
	private int bzres=0, bzres2=0;
	
	//for counting the time taken to decompress the 7z file.
	private float t1, t, t2;
	
	private string myFile;
	private WWW www;
	
	private string ppath;
	
	private bool compressionStarted;
	private bool downloadDone;

    private byte[] bt, reusableBuffer, reusableBuffer2;
    private bool pass1, pass2;
	

	void Start(){
		ppath = Application.persistentDataPath;
		
		#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
		ppath=".";
		#endif
		
		Debug.Log(ppath);

        reusableBuffer = new byte[0];
        reusableBuffer2 = new byte[0];
		
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		//下载文件做测试
		if (!File.Exists(ppath + "/" + myFile)) StartCoroutine(Downloadbz2File());
	}
	
	
	IEnumerator Downloadbz2File(){
		
		Debug.Log("starting download");

        myFile = "picture.jpg.bz2";

		if (File.Exists(ppath + "/" + myFile)) File.Delete(ppath + "/" + myFile);
		
		//从七牛云下载测试文件
		www = new WWW("https://dn-littlesmallsu.qbox.me/"+ myFile);
        Debug.Log("www is"+www.url);
        yield return www;
		if (www.error != null) Debug.Log(www.error);
		downloadDone = true;
		
		//write the downloaded 7z file to the ppath directory so we can have access to it
		//depending on the Install Location you have set for your app, set the Write Access accordingly!
		FileStream fs = new FileStream(ppath + "/" + myFile, FileMode.Create);
		fs.Write(www.bytes, 0, www.size);
#if !NETFX_CORE
        fs.Close();
#endif
		fs.Dispose();
		
		www.Dispose();
		www = null;
	
	}

    public Texture2D myTex = null;
    IEnumerator LoadPicture(string myFile) {
        string spath =
	#if UNITY_EDITOR_WIN
		"file://" + ppath;
	#elif UNITY_ANDROID
		"jar:file://" + ppath;
	#else
		ppath;
	#endif

	www=new WWW(spath+"/"+myFile);
        Debug.Log("www is"+www.url);
        yield return www;
        if(www.error!=null)
            Debug.Log(www.error);
        if(www.isDone)
            myTex=www.texture;
    }


     void DoDecompression(){

        //BZ2 FILE COMPRESSION/DECOMPRESSION
        //decompress the downloaded file
        t = Time.realtimeSinceStartup;
        bzres = lbz2.decompressBz2(ppath + "/" + myFile, ppath +"/uncompressed.dat");
        t1 = Time.realtimeSinceStartup - t;

        StartCoroutine(LoadPicture("uncompressed.dat"));

        //recompress it to test compression
        t = Time.realtimeSinceStartup;
        bzres2 = lbz2.compressBz2(ppath + "/uncompressed.dat", ppath + "/RC" + myFile, 9);
        t2 = Time.realtimeSinceStartup - t;


        //BZ2 BUFFERS COMPRESSION/DECOMPRESSION
        //read the uncompressed file into a byte buffer to test buffer2buffer compression/decompression
        bt = File.ReadAllBytes(ppath + "/uncompressed.dat");

        //compress the byte buffer into the reusableBuffer
        if (lbz2.bz2CompressBuffer(bt, ref reusableBuffer, 9, true)) pass1 = true;

        //write the compressed buffer into a file for checking purposes
        File.WriteAllBytes(ppath + "/recompressed2.bz2", reusableBuffer);

        //decompress the reusableBuffer into the reusableBuffer2
        if (lbz2.bz2DecompressBuffer(reusableBuffer,  ref reusableBuffer2, true) == 0) pass2 = true;

        //write the uncompressed buffer into a file to check if everything went fine.
         File.WriteAllBytes(ppath + "/uncompressed2.dat", reusableBuffer2);

	}
	

	void Update(){
		if (Input.GetKeyDown(KeyCode.Escape)) { Application.Quit(); }
	}
	
	
	void OnGUI(){
		
		if (downloadDone == true){
			GUI.Label(new Rect(10, 0, 250, 30), "压缩包以被下载,准备开始测试");
			GUI.Label(new Rect(10, 50, 650, 100), ppath);

		    if (GUI.Button(new Rect(10, 110, 240, 50), "开始测试")){
			    compressionStarted = true;
			    DoDecompression();
		    }
		
		}
		
		
		if (compressionStarted){
			//if the return code is 1 then the decompression was succesful.
            GUI.Label(new Rect(50, 180, 350, 50), "解压文件 - 返回状态码: " + bzres.ToString());
			//if the return code is 1 then the compression was succesful.
            GUI.Label(new Rect(50, 210, 350, 50), "压缩文件 - 返回状态码: "+ bzres2.ToString());
			//time took to decompress
			GUI.Label(new Rect(50, 250, 250, 50), "解压文件 - 使用时间: "+ t1.ToString());
			// time took to compress
            GUI.Label(new Rect(50, 280, 250, 50), "压缩文件 - 使用时间: "+ t2.ToString());
            GUI.Label(new Rect(50, 320, 250, 50), "buffer compress    : "+pass1.ToString());
            GUI.Label(new Rect(50, 360, 250, 50), "buffer deCompress: "+pass2.ToString());

            GUI.Box(new Rect(50,400,800,600),myTex);
		}	
		
		
	}
#else
    void Start()
    {
        Debug.Log("Does not work in Webplayer!");
    }
#endif	
}
