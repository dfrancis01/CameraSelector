using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class UI_Controller : MonoBehaviour {

	public Dropdown drpdwn_CameraSelector;
	public Button btn_StartStop;
	public Button btn_Quit;
	public Text startStopText;

	private WebCamDevice[] devices;
	private Texture defualtCamInputScreenTexture;
	private WebCamTexture webCam;

	public RawImage camInputScreen;
	public AspectRatioFitter fitter;
	private int prevIndex=0;
	//Commit marker
	void Awake (){
		
		Debug.Log("Awake Called");
		//Load trigger event handlers
		drpdwn_CameraSelector.onValueChanged.AddListener(delegate {CameraSelector_Changed();});
		btn_StartStop.onClick.AddListener (delegate {StartStop_Clicked ();});
		btn_Quit.onClick.AddListener (delegate {Quit_Clicked ();});

	}
	private bool CameraAvailable(){
		try{
			return WebCamTexture.devices.Length > 0;
		}catch (Exception e){
			Debug.LogFormat ("Error Using WebCamTexture Utility: {0}",e.Message);
			return false;
		}
	}
	private void Start (){
		
		Debug.Log("Start Called");

		Initialize ();
	}
	private void Initialize(){
		defualtCamInputScreenTexture = camInputScreen.texture;
		devices = WebCamTexture.devices;
		if (CameraAvailable()) {
			
			StartCamera (drpdwn_CameraSelector.value);
		} else {
			//No camera was found

			Disable_UI_Buttons();
		}
		Populate_drpdwn_CameraSelector ();
	}
	private void Reinitialize(){
		//Something went wrong need to reinitialize
		Debug.Log("Reinitialize Called");
		Initialize();
	}
	private void Disable_UI_Buttons(){
		drpdwn_CameraSelector.interactable = false;
		btn_StartStop.interactable = false;
	}
	private void Populate_drpdwn_CameraSelector(){
		//2nd Method to code bellow
		//need to convert list of devices to open list for AddOptions
		//cameras = WebCamTexture.devices;
		//drpdwn_CameraSelector.AddOptions (cameras);

		int idx = 0;
		drpdwn_CameraSelector.options.Clear ();
		//Populate dropdown with list of available cameras from WebCamTexture.devices
		foreach (var camera in WebCamTexture.devices) {
			Debug.LogFormat ("Found Camera {0}: {1}", idx, camera.name);
			drpdwn_CameraSelector.options.Add (new Dropdown.OptionData () { text = camera.name });
			idx += 1;
		}
		drpdwn_CameraSelector.RefreshShownValue ();
	}

	//Quit the application.   If in Editor stop play.
	private void Quit_Clicked () {
		Debug.Log("Quit Button Clicked");
		#if UNITY_EDITOR 									//If we are in the Unity Development Environment
		UnityEditor.EditorApplication.isPlaying = false; 
		#else 												//If we are in a deployed environment
		Application.Quit (); 
		#endif  
	
	}

	private void StartStop_Clicked(){
		
		Debug.Log("StartStop Button Clicked");
		if (startStopText.text.Equals("Start Camera")){
			StartCamera (drpdwn_CameraSelector.value);

		}
		else{
			StopCamera(drpdwn_CameraSelector.value);

		}

	}

	private void CameraSelector_Changed(){
		try {
			var index = drpdwn_CameraSelector.value;
			StopCamera(prevIndex);
			StartCamera (index);
			prevIndex = index;
			Debug.LogFormat ("Camera {0} selected. New Index is: {1}", devices [index].name, index);
		}catch(IndexOutOfRangeException e){
			Debug.LogFormat("Error in Selector Change: {0}",e.Message);
			Reinitialize ();
		}catch (Exception e){
			Debug.LogFormat ("UnhandledException in CameraSelector_Changed(): {0}",e.Message);
			Quit_Clicked ();
		}

	}
	private void StopCamera(int index){
		webCam.Stop ();
		camInputScreen.texture = defualtCamInputScreenTexture;
		startStopText.text = "Start Camera";
	}

	private void StartCamera(int index){
		try{
		webCam = new WebCamTexture (WebCamTexture.devices [index].name);
		webCam.Play ();
		camInputScreen.texture = webCam;
		startStopText.text = "Stop Camera";
		}catch(IndexOutOfRangeException e){
			Debug.LogFormat("Error in Star Camera: {0}",e.Message);
			Reinitialize ();
		}catch (Exception e){
			Debug.LogFormat ("StartCamera in CameraSelector_Changed(): {0}",e.Message);
			Quit_Clicked ();
		}
	}

	private void Update (){
		if (!CameraAvailable())
			return;
		float ratio = (float)webCam.width / (float)webCam.height;
		fitter.aspectRatio = ratio;

		float scaleY = webCam.videoVerticallyMirrored ? -1f : 1f;
		camInputScreen.rectTransform.localScale = new Vector3 (1f, scaleY, 1f);

		int orient = -webCam.videoRotationAngle;
		camInputScreen.rectTransform.localEulerAngles = new Vector3 (0, 0, orient);
	}
}
