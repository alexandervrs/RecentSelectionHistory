
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Animations;

public class RecentSelectionHistory : EditorWindow
{

	public static int historyMaxItems = 100;

    public static List<Object> recordedActions;
	public static Object selectedObject = null;
	public static Object previousSelectedObject = null;
	
	Vector2 historyScrollView;

	public enum FileAction {
		Highlight,
		HighlightButOnlyOpenScripts,
		OpenAndHighlight,
		Open
	}

	public FileAction fileAction = FileAction.Highlight;

	public static bool pauseRecording = false;
	
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

		EditorApplication.update -= UpdateList;
		EditorApplication.update += UpdateList;
		
    }

	static void UpdateList()
	{

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

	}

    void OnGUI()
    {

        Color originalColor = GUI.color;

		UpdateList();

		// window contents
		
		historyScrollView = EditorGUILayout.BeginScrollView(historyScrollView);

		if (recordedActions != null) {

			if (recordedActions.Count > 0) {

				foreach (Object recordedAction in recordedActions) {

					if (recordedAction != null) {
						
						if (AssetDatabase.Contains( recordedAction )) {

							if (AssetDatabase.IsValidFolder((AssetDatabase.GetAssetPath(recordedAction)))) {


								
								Texture2D buttonIcon = EditorGUIUtility.IconContent("Folder Icon").image as Texture2D;
								GUIContent buttonContent = new GUIContent(" "+recordedAction.name, buttonIcon);
								buttonContent.tooltip = AssetDatabase.GetAssetPath(recordedAction);
								GUI.skin.button.alignment = TextAnchor.MiddleLeft;

								if (GUILayout.Button(buttonContent, new GUILayoutOption[] { GUILayout.Height(24) })) {
									EditorGUIUtility.PingObject(recordedAction);
								}



							} else {

								bool isScriptAsset = false;

								Texture2D buttonIcon = null;

								//Debug.Log(recordedAction.GetType());
								
								if (AssetDatabase.GetMainAssetTypeAtPath(AssetDatabase.GetAssetPath(recordedAction)) == typeof(SceneAsset)) {
									buttonIcon = EditorGUIUtility.IconContent("SceneAsset Icon").image as Texture2D;
								} else if (AssetDatabase.GetMainAssetTypeAtPath(AssetDatabase.GetAssetPath(recordedAction)) == typeof(MonoScript)) {
									buttonIcon = EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D;
									isScriptAsset = true;
								} else if (AssetDatabase.GetMainAssetTypeAtPath(AssetDatabase.GetAssetPath(recordedAction)) == typeof(AudioClip)) {
									buttonIcon = EditorGUIUtility.IconContent("AudioSource Gizmo").image as Texture2D;
								} else if (AssetDatabase.GetMainAssetTypeAtPath(AssetDatabase.GetAssetPath(recordedAction)) == typeof(Shader)) {
									buttonIcon = EditorGUIUtility.IconContent("Shader Icon").image as Texture2D;
									isScriptAsset = true;
								} else if (AssetDatabase.GetMainAssetTypeAtPath(AssetDatabase.GetAssetPath(recordedAction)) == typeof(Material)) {
									buttonIcon = EditorGUIUtility.IconContent("Material Icon").image as Texture2D;
								} else if (AssetDatabase.GetMainAssetTypeAtPath(AssetDatabase.GetAssetPath(recordedAction)) == typeof(GameObject)) {
									buttonIcon = EditorGUIUtility.IconContent("Prefab Icon").image as Texture2D;
								} else if (AssetDatabase.GetMainAssetTypeAtPath(AssetDatabase.GetAssetPath(recordedAction)) == typeof(ScriptableObject)) {
									buttonIcon = EditorGUIUtility.IconContent("ScriptableObject Icon").image as Texture2D;
									isScriptAsset = true;
								} else if (AssetDatabase.GetMainAssetTypeAtPath(AssetDatabase.GetAssetPath(recordedAction)) == typeof(Texture2D)) {
									buttonIcon = EditorGUIUtility.IconContent("Texture2D Icon").image as Texture2D;
								} else if (AssetDatabase.GetMainAssetTypeAtPath(AssetDatabase.GetAssetPath(recordedAction)) == typeof(Texture)) {
									buttonIcon = EditorGUIUtility.IconContent("Texture Icon").image as Texture2D;
								} else if (AssetDatabase.GetMainAssetTypeAtPath(AssetDatabase.GetAssetPath(recordedAction)) == typeof(AnimationClip)) {
									buttonIcon = EditorGUIUtility.IconContent("AnimationClip Icon").image as Texture2D;
								} else if (AssetDatabase.GetMainAssetTypeAtPath(AssetDatabase.GetAssetPath(recordedAction)) == typeof(AnimatorController)) {
									buttonIcon = EditorGUIUtility.IconContent("AnimatorController Icon").image as Texture2D;
								} else if (AssetDatabase.GetMainAssetTypeAtPath(AssetDatabase.GetAssetPath(recordedAction)) == typeof(Font)) {
									buttonIcon = EditorGUIUtility.IconContent("Font Icon").image as Texture2D;
								} else {
									buttonIcon = EditorGUIUtility.IconContent("DefaultAsset Icon").image as Texture2D;
								}

								GUIContent buttonContent = new GUIContent(" "+recordedAction.name, buttonIcon);
								buttonContent.tooltip = AssetDatabase.GetAssetPath(recordedAction);
								GUI.skin.button.alignment = TextAnchor.MiddleLeft;

								if (GUILayout.Button(buttonContent, new GUILayoutOption[] { GUILayout.Height(24) })) {

									if (fileAction == FileAction.Highlight || fileAction == FileAction.OpenAndHighlight || fileAction == FileAction.HighlightButOnlyOpenScripts) {
										EditorGUIUtility.PingObject(recordedAction);
										Selection.activeObject = recordedAction;
									}

									if ((fileAction != FileAction.Open || fileAction != FileAction.OpenAndHighlight) && (isScriptAsset && fileAction == FileAction.HighlightButOnlyOpenScripts)) {
											AssetDatabase.OpenAsset(recordedAction);
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
							

								Texture2D buttonIcon = EditorGUIUtility.IconContent("GameObject Icon").image as Texture2D;
								GUIContent buttonContent = new GUIContent(" "+recordedAction.name, buttonIcon);
								buttonContent.tooltip = AssetDatabase.GetAssetPath(recordedAction);
								GUI.skin.button.alignment = TextAnchor.MiddleLeft;

								if (GUILayout.Button(buttonContent, new GUILayoutOption[] { GUILayout.Height(24) })) {

									EditorGUIUtility.PingObject(recordedAction);
									Selection.activeGameObject = recordedAction as GameObject;

								}
	


						}
					}

				}

			}

		}

		EditorGUILayout.EndScrollView();

		GUI.skin.button.alignment = TextAnchor.MiddleCenter;

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

		historyMaxItems = EditorGUILayout.IntSlider("Max History Items", historyMaxItems, 10, 3000);

		EditorGUILayout.EndVertical();

		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();

		if (GUILayout.Button("Clear")) {
			recordedActions.Clear();
		}

		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();

	}

}