
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;

public class RecentSelectionHistory : EditorWindow
{

	public const int historyMaxItems = 50; // change this to increase/decrease max stored history items

    List<Object> recordedActions;
	public Object selectedObject;
	public Object previousSelectedObject = null;
	
	Vector2 historyScrollView;

	public enum FileAction {
		OpenAndHighlight,
		Open,
		Highlight
	}

	public FileAction fileAction = FileAction.OpenAndHighlight;

	public bool pauseRecording = false;
	
	[MenuItem("Window/Recent Selection History...")]
    static void OpenCustomEditor()
    {

        RecentSelectionHistory window = (RecentSelectionHistory)GetWindow(typeof(RecentSelectionHistory));
		
        window.position     = new Rect(6, 100, 320, 200);    // initial window position
		window.titleContent = new GUIContent("Recent Selection History"); // window title

		// handle resizing
		window.minSize = new Vector2(320, 100);
        window.maxSize = new Vector2(320, 300);
		
		window.Show();	// show the window
		window.Focus(); // keyboard focus the window
		
    }

    void OnGUI()
    {

        Color originalColor = GUI.color;

        selectedObject = Selection.activeObject;

		if (recordedActions == null) {
			recordedActions = new List<Object>();
		}

		if (selectedObject != null) {

			if (selectedObject != previousSelectedObject && !pauseRecording) {

				if (AssetDatabase.IsValidFolder( AssetDatabase.GetAssetPath(selectedObject) )) {

					// found asset folder
					recordedActions.Insert(0, selectedObject as Object);
					previousSelectedObject = selectedObject;

				}

				else if (AssetDatabase.Contains( selectedObject )) {

					// found asset file
					recordedActions.Insert(0, selectedObject as Object);
					previousSelectedObject = selectedObject;

				} else {

					// found gameObject
					recordedActions.Insert(0, selectedObject as GameObject);
					previousSelectedObject = selectedObject;

				}

			}

			
		} else {
			previousSelectedObject = null;
		}

		// window contents
		
		historyScrollView = EditorGUILayout.BeginScrollView(historyScrollView);

		if (recordedActions != null) {

			if (recordedActions.Count > 0) {

				foreach (Object recordedAction in recordedActions) {

					if (recordedAction != null) {
						
						if (AssetDatabase.Contains( recordedAction )) {

							if (AssetDatabase.IsValidFolder((AssetDatabase.GetAssetPath(recordedAction)))) {

								GUI.color = new Color32(255, 85, 163, 255);

								if (GUILayout.Button(recordedAction.name)) {
									EditorGUIUtility.PingObject(recordedAction);
								}

							} else {

								GUI.color = new Color32(0, 185, 251, 255);

								if (GUILayout.Button(recordedAction.name)) {
								
									if (fileAction == FileAction.Highlight || fileAction == FileAction.OpenAndHighlight) {
										EditorGUIUtility.PingObject(recordedAction);
									}
									
									if (fileAction == FileAction.OpenAndHighlight || fileAction == FileAction.Open) {

										if (AssetDatabase.GetMainAssetTypeAtPath(AssetDatabase.GetAssetPath(recordedAction)) == typeof(SceneAsset)) {
											
											// about to open a scene, let's ask to save changes first
											EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
											
										}
										AssetDatabase.OpenAsset(recordedAction);

									}

								}

							}


							

						} else {

							GUI.color = new Color32(255, 170, 29, 255);
							
							if (GUILayout.Button(recordedAction.name)) {
								EditorGUIUtility.PingObject(recordedAction);
								Selection.activeGameObject = recordedAction as GameObject;
							}

						}
					}

				}

			}

		}

		EditorGUILayout.EndScrollView();

		if (recordedActions.Count > historyMaxItems) {
			recordedActions.RemoveRange(historyMaxItems, recordedActions.Count - historyMaxItems);
		}
		
		// update window contents
		this.Repaint();
	
		GUI.color = originalColor;

		EditorGUILayout.Space();

		EditorGUILayout.BeginVertical();

		GUIStyle guiSeparator = new GUIStyle("box");

		// --- separator ---
        guiSeparator.border.top     = guiSeparator.border.bottom  = 1;
        guiSeparator.margin.top     = guiSeparator.margin.bottom  = 5;
        guiSeparator.margin.bottom  = guiSeparator.margin.top     = 5;
        guiSeparator.padding.top    = guiSeparator.padding.bottom = 1;
        GUILayout.Box("", guiSeparator, GUILayout.ExpandWidth(true), GUILayout.Height(1));

		pauseRecording = EditorGUILayout.Toggle("Pause Recording", pauseRecording);
		fileAction = (FileAction)EditorGUILayout.EnumPopup("File Action", fileAction, EditorStyles.popup);

		EditorGUILayout.EndVertical();

		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();

		if (GUILayout.Button("Clear")) {
			recordedActions.Clear();
		}
		if (GUILayout.Button("Close")) {
			this.Close();
		}

		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();

	}

}