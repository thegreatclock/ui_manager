using GreatClock.Common.Tweens;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GreatClock.Common.UI {

	public class UIStructureCreator {

		private const string UI_ROOT_NAME = "UI Root";
		private const string UI_WINDOW_ROOT = "WindowRoot";
		private const string UI_POPUP_ROOT = "PopupRoot";
		private const string UI_MASK_ROOT = "MaskRoot";
		private const string UI_LOADING_MASK = "LoadingMask";
		private const string UI_TOAST_ROOT = "ToastRoot";

		[MenuItem("GreatClock/UIManager/Create UI Structure")]
		static void CreateUIStructure() {
			GameObject root = GameObject.Find(UI_ROOT_NAME);
			if (root != null && root.transform.parent == null && root.GetComponent<UIManager>() != null) {
				Debug.LogErrorFormat(root, "'{0}' is already exists !", UI_ROOT_NAME);
				return;
			}
			root = new GameObject(UI_ROOT_NAME);
			RectTransform rt = root.AddComponent<RectTransform>();
			rt.localPosition = Vector3.zero;
			rt.localRotation = Quaternion.identity;
			Canvas canvas = root.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			CanvasScaler cs = root.AddComponent<CanvasScaler>();
			cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			cs.referenceResolution = new Vector2(720f, 1280f);
			cs.matchWidthOrHeight = 0f;
			GraphicRaycaster gr = root.AddComponent<GraphicRaycaster>();
			gr.ignoreReversedGraphics = true;
			gr.blockingObjects = GraphicRaycaster.BlockingObjects.None;
			SerializedObject uiObj = new SerializedObject(root.AddComponent<UIManager>());
			uiObj.FindProperty("m_WindowRoot").objectReferenceValue = AddChild(rt, UI_WINDOW_ROOT, true);
			uiObj.FindProperty("m_PopupRoot").objectReferenceValue = AddChild(rt, UI_POPUP_ROOT, true);

			RectTransform maskRoot = AddChild(rt, UI_MASK_ROOT, true);
			RectTransform mask = AddChild(maskRoot, UI_LOADING_MASK, true);
			mask.offsetMin = new Vector2(-100f, -100f);
			mask.offsetMax = new Vector2(100f, 100f);
			CanvasGroup mcg = mask.gameObject.AddComponent<CanvasGroup>();
			mcg.interactable = true;
			mcg.blocksRaycasts = true;
			Type typeTweenBase = typeof(TweenComponentBase);
			Type typeTween = Type.GetType(typeTweenBase.Namespace + ".TweenComponent,Assembly-CSharp");
			Type typeTweenType = Type.GetType(typeTweenBase.Namespace + ".TweenComponent+eTweenType,Assembly-CSharp");
			TweenComponentBase tMask = mask.gameObject.AddComponent(typeTween) as TweenComponentBase;
			SerializedObject tmObj = new SerializedObject(tMask);
			SerializedProperty pts = tmObj.FindProperty("m_Tweens");
			pts.InsertArrayElementAtIndex(0);
			SerializedProperty pt1 = pts.GetArrayElementAtIndex(0);
			pt1.FindPropertyRelative("group").stringValue = "hide";
			pt1.FindPropertyRelative("autoPlay").boolValue = false;
			pt1.FindPropertyRelative("unscaled").boolValue = true;
			pt1.FindPropertyRelative("tweenType").intValue = (int)Enum.Parse(typeTweenType, "Alpha");
			pt1.FindPropertyRelative("delay").floatValue = 0.2f;
			pt1.FindPropertyRelative("duration").floatValue = 0.5f;
			pt1.FindPropertyRelative("easeDefinition").enumValueIndex = (int)TweenComponentBase.eEaseDefinition.EaseFunction;
			pt1.FindPropertyRelative("ease").enumValueIndex = (int)Ease.eEaseType.Linear;
			pt1.FindPropertyRelative("loop").enumValueIndex = (int)eLoopType.Once;
			pt1.FindPropertyRelative("floatValue").floatValue = 0f;
			pts.InsertArrayElementAtIndex(0);
			SerializedProperty pt0 = pts.GetArrayElementAtIndex(0);
			pt0.FindPropertyRelative("group").stringValue = "show";
			pt0.FindPropertyRelative("autoPlay").boolValue = false;
			pt0.FindPropertyRelative("unscaled").boolValue = true;
			pt0.FindPropertyRelative("tweenType").intValue = (int)Enum.Parse(typeTweenType, "Alpha");
			pt0.FindPropertyRelative("delay").floatValue = 0f;
			pt0.FindPropertyRelative("duration").floatValue = 0f;
			pt0.FindPropertyRelative("easeDefinition").enumValueIndex = (int)TweenComponentBase.eEaseDefinition.EaseFunction;
			pt0.FindPropertyRelative("ease").enumValueIndex = (int)Ease.eEaseType.Linear;
			pt0.FindPropertyRelative("loop").enumValueIndex = (int)eLoopType.Once;
			pt0.FindPropertyRelative("floatValue").floatValue = 1f;
			tmObj.ApplyModifiedProperties();
			Image maskImg = AddChild(mask, "Image", true).gameObject.AddComponent<Image>();
			maskImg.raycastTarget = true;
			maskImg.color = Color.black;
			RectTransform maskTxt = AddChild(mask, "Text", false);
			maskTxt.sizeDelta = new Vector2(400f, 100f);
			maskTxt.anchoredPosition = new Vector2(0f, 100f);
			Text maskText = maskTxt.gameObject.AddComponent<Text>();
			maskText.raycastTarget = true;
			maskText.alignment = TextAnchor.MiddleCenter;
			maskText.color = Color.white;
			maskText.fontSize = 32;
			maskText.text = "Loading ...";
			mask.gameObject.SetActive(false);
			DefaultLoadingMask dlm = mask.gameObject.AddComponent<DefaultLoadingMask>();
			SerializedObject dlmObj = new SerializedObject(dlm);
			dlmObj.FindProperty("m_Tween").objectReferenceValue = tMask;
			dlmObj.ApplyModifiedProperties();
			uiObj.FindProperty("m_DefaultLoadingMask").objectReferenceValue = dlm;

			RectTransform tr = AddChild(rt, UI_TOAST_ROOT, true);
			RectTransform tt = AddChild(tr, "ToastTemplate", false);
			tt.sizeDelta = new Vector2(400f, 48f);
			GerenicToast toast = tt.gameObject.AddComponent<GerenicToast>();
			SerializedObject tObj = new SerializedObject(toast);
			RectTransform to = AddChild(tt, "Canvas", true);
			CanvasGroup tcg = to.gameObject.AddComponent<CanvasGroup>();
			tcg.interactable = false;
			tcg.blocksRaycasts = false;
			TweenComponentBase tTween = to.gameObject.AddComponent(typeTween) as TweenComponentBase;
			SerializedObject twObj = new SerializedObject(tTween);
			SerializedProperty pTweens = twObj.FindProperty("m_Tweens");
			pTweens.InsertArrayElementAtIndex(0);
			SerializedProperty pTween3 = pTweens.GetArrayElementAtIndex(0);
			pTween3.FindPropertyRelative("group").stringValue = "out";
			pTween3.FindPropertyRelative("autoPlay").boolValue = false;
			pTween3.FindPropertyRelative("unscaled").boolValue = true;
			pTween3.FindPropertyRelative("tweenType").intValue = (int)Enum.Parse(typeTweenType, "AnchoredPosition");
			pTween3.FindPropertyRelative("delay").floatValue = 0f;
			pTween3.FindPropertyRelative("duration").floatValue = 0.3f;
			pTween3.FindPropertyRelative("easeDefinition").enumValueIndex = (int)TweenComponentBase.eEaseDefinition.EaseFunction;
			pTween3.FindPropertyRelative("ease").enumValueIndex = (int)Ease.eEaseType.QuartOut;
			pTween3.FindPropertyRelative("loop").enumValueIndex = (int)eLoopType.Once;
			pTween3.FindPropertyRelative("vector2Value").vector2Value = new Vector2(0f, 70f);
			pTweens.InsertArrayElementAtIndex(0);
			SerializedProperty pTween2 = pTweens.GetArrayElementAtIndex(0);
			pTween2.FindPropertyRelative("group").stringValue = "out";
			pTween2.FindPropertyRelative("autoPlay").boolValue = false;
			pTween2.FindPropertyRelative("unscaled").boolValue = true;
			pTween2.FindPropertyRelative("tweenType").intValue = (int)Enum.Parse(typeTweenType, "Alpha");
			pTween2.FindPropertyRelative("delay").floatValue = 0f;
			pTween2.FindPropertyRelative("duration").floatValue = 0.3f;
			pTween2.FindPropertyRelative("easeDefinition").enumValueIndex = (int)TweenComponentBase.eEaseDefinition.EaseFunction;
			pTween2.FindPropertyRelative("ease").enumValueIndex = (int)Ease.eEaseType.QuartOut;
			pTween2.FindPropertyRelative("loop").enumValueIndex = (int)eLoopType.Once;
			pTween2.FindPropertyRelative("floatValue").floatValue = 0f;
			pTweens.InsertArrayElementAtIndex(0);
			SerializedProperty pTween1 = pTweens.GetArrayElementAtIndex(0);
			pTween1.FindPropertyRelative("group").stringValue = "in";
			pTween1.FindPropertyRelative("autoPlay").boolValue = true;
			pTween1.FindPropertyRelative("unscaled").boolValue = true;
			pTween1.FindPropertyRelative("tweenType").intValue = (int)Enum.Parse(typeTweenType, "AnchoredPosition");
			pTween1.FindPropertyRelative("delay").floatValue = 0f;
			pTween1.FindPropertyRelative("duration").floatValue = 0.3f;
			pTween1.FindPropertyRelative("easeDefinition").enumValueIndex = (int)TweenComponentBase.eEaseDefinition.EaseFunction;
			pTween1.FindPropertyRelative("ease").enumValueIndex = (int)Ease.eEaseType.QuartOut;
			pTween1.FindPropertyRelative("loop").enumValueIndex = (int)eLoopType.Back;
			pTween1.FindPropertyRelative("vector2Value").vector2Value = new Vector2(0f, -20f);
			pTweens.InsertArrayElementAtIndex(0);
			SerializedProperty pTween0 = pTweens.GetArrayElementAtIndex(0);
			pTween0.FindPropertyRelative("group").stringValue = "in";
			pTween0.FindPropertyRelative("autoPlay").boolValue = true;
			pTween0.FindPropertyRelative("unscaled").boolValue = true;
			pTween0.FindPropertyRelative("tweenType").intValue = (int)Enum.Parse(typeTweenType, "Alpha");
			pTween0.FindPropertyRelative("delay").floatValue = 0f;
			pTween0.FindPropertyRelative("duration").floatValue = 0.3f;
			pTween0.FindPropertyRelative("easeDefinition").enumValueIndex = (int)TweenComponentBase.eEaseDefinition.EaseFunction;
			pTween0.FindPropertyRelative("ease").enumValueIndex = (int)Ease.eEaseType.QuartOut;
			pTween0.FindPropertyRelative("loop").enumValueIndex = (int)eLoopType.Back;
			pTween0.FindPropertyRelative("floatValue").floatValue = 0f;
			twObj.ApplyModifiedProperties();
			tObj.FindProperty("m_ToastCanvas").objectReferenceValue = tTween;
			Image tImg = AddChild(to, "Background", true).gameObject.AddComponent<Image>();
			tImg.color = new Color(0.8f, 0.8f, 0.8f, 0.7f);
			tImg.raycastTarget = false;
			RectTransform tc = AddChild(to, "Content", false);
			Text tTxt = tc.gameObject.AddComponent<Text>();
			tTxt.raycastTarget = false;
			tTxt.alignment = TextAnchor.MiddleCenter;
			tTxt.fontSize = 28;
			tObj.FindProperty("m_Content").objectReferenceValue = tTxt;
			ContentSizeFitter fitter = tc.gameObject.AddComponent<ContentSizeFitter>();
			fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			tTxt.text = "Toast Template";
			tt.gameObject.SetActive(false);
			tObj.ApplyModifiedProperties();

			uiObj.FindProperty("m_ToastTemplate").objectReferenceValue = toast;
			uiObj.ApplyModifiedProperties();

			GameObject ge = new GameObject("EventSystem");
			ge.AddComponent<EventSystem>();
			ge.AddComponent<StandaloneInputModule>();
		}

		private static RectTransform AddChild(RectTransform parent, string name, bool fullSize) {
			GameObject go = new GameObject(name);
			RectTransform t = go.AddComponent<RectTransform>();
			t.SetParent(parent);
			if (fullSize) {
				t.anchorMin = Vector2.zero;
				t.anchorMax = Vector2.one;
				t.sizeDelta = Vector2.zero;
			} else {
				t.anchorMin = Vector2.one * 0.5f;
				t.anchorMax = Vector2.one * 0.5f;
			}
			t.anchoredPosition = Vector2.zero;
			t.pivot = Vector2.one * 0.5f;
			return go.transform as RectTransform;
		}

	}

}
