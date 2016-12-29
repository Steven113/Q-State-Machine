using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;
using System;
using UnityEngine.UI;
using System.IO;

public class TeamEditorManager : MonoBehaviour
{

	public List<QConjectureLearner> conjectureLearners;
	public List<MeshCollider> colliderToHitInOrderToSelect;
	public List<MeshRenderer> meshRendererToEnableUponSelection;
	public List<RobotType> robotTypes = new List<RobotType> ();
	public List<string> AITextAssetLocations;
	public List<Transform> prefabSpawnPoints;
	public List<RobotPrefabMapping> prefabMap;
	public TextAsset referenceForPlayerData;
	public float scaleForModel = 3f;
	RaycastHit hit;
	Ray ray;
	public float learningRate = 0.1f;
	public float discountFactor = 0.4f;
	public List<string> possibleStates;
	public List<string> possibleActions;
	public Text robotLabel;
	int selected = -1;
	public GameObject objectToSpawnOnBroadcast;
	public string qFileName = "QConLearnerDelta.qsf";

	// Use this for initialization

	void Start ()
	{
		/*"" means the player data has not been initialized
		 * If player data has not been initialized we should be taken to the team creation screen instead of here.
		 */
		Debug.Assert (referenceForPlayerData.text != ""); 
		for (int i = 0; i<meshRendererToEnableUponSelection.Count; ++i) {
			meshRendererToEnableUponSelection [i].enabled = false;
		}
		string [] robotTypeStrings = (referenceForPlayerData.text.Split (Environment.NewLine.ToCharArray ()) [0].Split (" ".ToCharArray ()));

		for (int i = 0; i<robotTypeStrings.Length; ++i) {
			//TextAsset tempTAsset = Resources.Load(AITextAssetLocations[i]) as TextAsset;
			//if (tempTAsset.text!=Environment.NewLine && tempTAsset.text!=""){
			QConjectureLearner tempQ = null;
			if (!Utils.DeserializeFile ("Assets/Resources/" + AITextAssetLocations [i], ref tempQ)) {
				Debug.Assert (Utils.DeserializeFile ("Assets/Resources/" + qFileName, ref tempQ));
			}
			conjectureLearners.Add (tempQ);
			//} else {
			//QConjectureLearner tempQ = null;
				
			//conjectureLearners.Add(tempQ);
			//}
		}

		for (int i = 0; i<robotTypeStrings.Length; ++i) {
			bool hasSpawned = false;
			for (int j = 0; j<prefabMap.Count && !hasSpawned; ++j) {
				if (prefabMap [j].robotType.ToString () == robotTypeStrings [i]) {
					GameObject temp = (GameObject)GameObject.Instantiate (prefabMap [j].prefabToSpawn, prefabSpawnPoints [i].position, prefabSpawnPoints [i].rotation, prefabSpawnPoints [i]);
					robotTypes.Add (prefabMap [j].robotType);
					hasSpawned = true;
				}
			}
		}
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		for (int i = 0; i<Input.touchCount; ++i) {
			if (Input.touches [i].phase == TouchPhase.Began) {
				ray = Camera.main.ScreenPointToRay (Input.touches [i].position);
				if (Physics.Raycast (ray, out hit)) {
					bool hitASelectionSphere = false;
					for (int j = 0; j<colliderToHitInOrderToSelect.Count; ++j) {
						if (hit.collider == colliderToHitInOrderToSelect [j]) {
							hitASelectionSphere = true;
							for (int k = 0; k<meshRendererToEnableUponSelection.Count; ++k) {
								meshRendererToEnableUponSelection [k].enabled = k == j;
							}

							selected = j;
							robotLabel.text = robotTypes [j].ToString ();
						}
					}

					if (hitASelectionSphere)
						break;
				}
			}
		}

		if (Input.GetMouseButtonDown (0)) {
			ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			if (Physics.Raycast (ray, out hit)) {
				bool hitASelectionSphere = false;
				for (int j = 0; j<colliderToHitInOrderToSelect.Count; ++j) {
					if (hit.collider == colliderToHitInOrderToSelect [j]) {
						hitASelectionSphere = true;
						for (int k = 0; k<meshRendererToEnableUponSelection.Count; ++k) {
							meshRendererToEnableUponSelection [k].enabled = k == j;
						}
						
						selected = j;
						robotLabel.text = robotTypes [j].ToString ();
					}
				}
				

			}
		}
	}

	public void BroadCastAI ()
	{
		if (selected != -1) {
			for (int i = 0; i<conjectureLearners.Count; ++i) {
				if (i != selected && robotTypes [i] == robotTypes [selected]) {
					conjectureLearners [i] = new QConjectureLearner (conjectureLearners [selected]);
				}
			}
		}
	}

	public void ResetAI ()
	{
		if (selected != -1) {
			QConjectureLearner tempQ = null;
			Debug.Assert (Utils.DeserializeFile ("Assets/Resources/" + qFileName, ref tempQ));
			conjectureLearners [selected] = tempQ;
		}
	}

	public void LoadScene (string scene)
	{
		if (QConjectureLearner.numConstructorsLoading == 0) {
			Application.LoadLevel (scene);
		}
	}

//	public string FileName; // This contains the name of the file. Don't add the ".txt"
//	// Assign in inspector
//	private TextAsset asset; // Gets assigned through code. Reads the file.
//	private StreamWriter writer; // This is the writer that writes to the file
//	void AppendString(string appendString) {
//		asset = Resources.Load(FileName + ".txt") as TextAsset;
//		writer = new StreamWriter("Resources/" + FileName + ".txt"); // Does this work?
//		writer.WriteLine(appendString);
//	}

	public void OnDestroy ()
	{

		for (int i = 0; i<AITextAssetLocations.Count; ++i) {
//			File.WriteAllText("Assets/Resources/"+AITextAssetLocations[i]+".txt", String.Empty);
//			using (StreamWriter streamForTextAssets = new StreamWriter("Assets/Resources/"+AITextAssetLocations[i])){
//			streamForTextAssets.WriteLine(conjectureLearners[i].ToFileFormat());
//			}
			QConjectureLearner tempQ = conjectureLearners [i];
			Utils.SerializeFile ("Assets/Resources/" + AITextAssetLocations [i], ref tempQ, ".txt");
		}

	}

}
