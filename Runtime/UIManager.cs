#pragma warning disable 649

using GreatClock.Common.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace GreatClock.Common.UI {

	[RequireComponent(typeof(Canvas))]
	public sealed class UIManager : MonoBehaviour {

		public static UIManager instance { get; private set; }

		[SerializeField]
		private RectTransform m_WindowRoot;
		[SerializeField]
		private RectTransform m_PopupRoot;
		[SerializeField]
		private LoadingMaskBase m_DefaultLoadingMask;
		[SerializeField]
		private GerenicToast m_ToastTemplate;

		void Awake() {
			if (instance != null) {
				throw new System.InvalidOperationException();
			}
			instance = this;
			mOnHideLoadingMaskEnd = OnHideLoadingMaskEnd;
			WindowProcess.window_root = m_WindowRoot;
			PopupProcess.popup_root = m_PopupRoot;
			ToastProcess.toast_template = m_ToastTemplate;
			WindowProcess.on_start_load_window = OnStartLoadWindow;
			WindowProcess.on_end_load_window = OnEndLoadWindow;
			mLoadingMask = m_DefaultLoadingMask;
			mLoadingMask.gameObject.SetActive(false);
		}

		private System.Action mOnHideLoadingMaskEnd;

		private IUILoader mUILoader;
		private LoadingMaskBase mLoadingMask;

		public void Init(IUILoader uiLoader) {
			if (mUILoader != null) {
				throw new System.InvalidOperationException();
			}
			mUILoader = uiLoader;
		}

		private Canvas mCanvas;
		public Canvas Root {
			get {
				if (mCanvas == null) {
					mCanvas = GetComponent<Canvas>();
				}
				return mCanvas;
			}
		}

		#region window

		private WindowProcess mWindow;

		public void ShowWindow(string windowName, object param, System.Action loadedCallback) {
			if (mUILoader == null) { return; }
			if (mWindow != null) {
				RemoveFocus(mWindow, true);
				mWindow.Close();
				WindowProcess.Cache(mWindow);
			}
			mWindow = WindowProcess.Get(windowName, param);
			AddFocus(mWindow, true, true);
			mWindow.Start(mUILoader, loadedCallback);
		}

		public void CloseWindow() {
			if (mUILoader == null) { return; }
			if (mWindow != null) {
				RemoveFocus(mWindow, true);
				mWindow.Close();
				WindowProcess.Cache(mWindow);
				mWindow = null;
			}
		}

		private void OnStartLoadWindow() {
			ShowLoadingMask("load_window");
		}

		private void OnEndLoadWindow() {
			HideLoadingMask("load_window");
		}

		private class WindowProcess : IFocus {
			public static RectTransform window_root;
			public static System.Action on_start_load_window;
			public static System.Action on_end_load_window;
			public string name { get; private set; }
			public object param { get; private set; }
			public System.Action onClose { get; private set; }
			private bool mActive;
			private GameObject mGameObject;
			private List<MonoBehaviour> mBehaviours = new List<MonoBehaviour>();
			private IUILoader mUILoader;
			private System.Action<GameObject> mOnLoaded;
			private System.Action mOnLoadedCallback;
			private RealTimeTimer.TimerDelegate mStartLoad;
			private int mFocusState;
			private WindowProcess() {
				mOnLoaded = OnLoaded;
				mStartLoad = StartLoad;
			}
			public void Start(IUILoader uiLoader, System.Action callback) {
				mActive = true;
				mUILoader = uiLoader;
				mOnLoadedCallback = callback;
				if (on_start_load_window != null) {
					on_start_load_window();
					RealTimeTimer.Register(0.1f, mStartLoad);
				} else {
					StartLoad();
				}
			}
			public void Close() {
				mActive = false;
				if (mGameObject != null) {
					for (int i = 0, imax = mBehaviours.Count; i < imax; i++) {
						IUIFocusHandler handler = mBehaviours[i] as IUIFocusHandler;
						if (handler != null) {
							try { handler.OnClose(); } catch (System.Exception e) { Debug.LogException(e); }
						}
					}
					mUILoader.UnloadInstance(mGameObject);
					mGameObject = null;
					mBehaviours.Clear();
				}
			}
			public void GetFocus() {
				if (mGameObject != null) {
					for (int i = 0, imax = mBehaviours.Count; i < imax; i++) {
						IUIFocusHandler handler = mBehaviours[i] as IUIFocusHandler;
						if (handler != null) {
							try { handler.OnFocus(); } catch (System.Exception e) { Debug.LogException(e); }
						}
					}
					mFocusState = 2;
				} else {
					mFocusState = 1;
				}
			}
			public void LoseFocus() {
				if (mGameObject != null) {
					for (int i = 0, imax = mBehaviours.Count; i < imax; i++) {
						IUIFocusHandler handler = mBehaviours[i] as IUIFocusHandler;
						if (handler != null) {
							try { handler.OnLoseFocus(); } catch (System.Exception e) { Debug.LogException(e); }
						}
					}
				}
				mFocusState = 0;
			}
			public void HandleKey(KeyCode key) {
				for (int i = 0, imax = mBehaviours.Count; i < imax; i++) {
					IUIKeyHandler handler = mBehaviours[i] as IUIKeyHandler;
					if (handler != null) {
						try { handler.OnKey(key); } catch (System.Exception e) { Debug.LogException(e); }
					}
				}
			}
			private void StartLoad() {
				mUILoader.LoadWindow(name, mOnLoaded);
			}
			private void OnLoaded(GameObject go) {
				System.Action callback = mOnLoadedCallback;
				if (callback != null) {
					try { callback(); } catch (System.Exception e) { Debug.LogException(e); }
				}
				if (!mActive) {
					mUILoader.UnloadInstance(go);
					return;
				}
				mGameObject = go;
				RectTransform rectTrans = go.transform as RectTransform;
				rectTrans.SetParent(window_root, false);
				rectTrans.offsetMin = Vector2.zero;
				rectTrans.offsetMax = Vector2.zero;
				rectTrans.localRotation = Quaternion.identity;
				rectTrans.localScale = Vector3.one;
				mBehaviours.Clear();
				go.GetComponents<MonoBehaviour>(mBehaviours);
				bool paraSet = false;
				for (int i = 0, imax = mBehaviours.Count; i < imax; i++) {
					IUIParameterHandler handler = mBehaviours[i] as IUIParameterHandler;
					if (handler != null) {
						try { handler.SetParameter(param); } catch (System.Exception e) { Debug.LogError(e); }
						paraSet = true;
					}
				}
				if (param != null && !paraSet) {
					Debug.LogErrorFormat(go, "[UIManager] No parameter handler found for '{0]' !", name);
				}
				if (mFocusState == 1) {
					for (int i = 0, imax = mBehaviours.Count; i < imax; i++) {
						IUIFocusHandler handler = mBehaviours[i] as IUIFocusHandler;
						if (handler != null) {
							try { handler.OnFocus(); } catch (System.Exception e) { Debug.LogException(e); }
						}
					}
					mFocusState = 2;
				}
				if (on_end_load_window != null) { on_end_load_window(); }
			}
			private static Queue<WindowProcess> cached_instance = new Queue<WindowProcess>();
			public static WindowProcess Get(string name, object param) {
				WindowProcess win = null;
				if (cached_instance.Count > 0) {
					win = cached_instance.Dequeue();
				}
				if (win == null) { win = new WindowProcess(); }
				win.mActive = false;
				win.mGameObject = null;
				win.mUILoader = null;
				win.name = name;
				win.param = param;
				win.mFocusState = 0;
				return win;
			}
			public static void Cache(WindowProcess win) {
				if (win == null) { return; }
				win.name = null;
				win.param = null;
				win.mActive = false;
				win.mGameObject = null;
				win.mUILoader = null;
				win.mFocusState = 0;
				cached_instance.Enqueue(win);
			}
		}

		#endregion

		#region popup

		private static Stack<PopupProcess> temp_popups = new Stack<PopupProcess>();
		private Stack<PopupProcess> mPopups = new Stack<PopupProcess>();

		private PopupProcess.LoadedCallback mOnPopupLoaded;

		public void ShowPopup(string popupName, object param, System.Action onClose) {
			ClosePopupByName(popupName, true);
			if (mPopups.Count > 0) {
				RemoveFocus(mPopups.Peek(), true);
			}
			PopupProcess popup = PopupProcess.Get(popupName, param, onClose);
			mPopups.Push(popup);
			AddFocus(popup, false, true);
			if (mOnPopupLoaded == null) { mOnPopupLoaded = OnPopupLoaded; }
			popup.Start(mUILoader, mOnPopupLoaded);
		}

		public void ClosePopup(string popupName) {
			ClosePopupByName(popupName, false);
		}

		public void ClosePopup(GameObject popupObject) {
			temp_popups.Clear();
			PopupProcess target = null;
			while (mPopups.Count > 0) {
				PopupProcess popup = mPopups.Pop();
				if (popup.gameObject == popupObject) {
					target = popup;
					break;
				}
				temp_popups.Push(popup);
			}
			bool isTopClosed = true;
			while (temp_popups.Count > 0) {
				mPopups.Push(temp_popups.Pop());
				isTopClosed = false;
			}
			if (target != null) {
				RemoveFocus(target, true);
				target.Close(isTopClosed);
			}
			if (isTopClosed && mPopups.Count > 0) {
				PopupProcess popup = mPopups.Peek();
				if (!popup.visible) { popup.Show(); }
			}
		}

		private void OnPopupLoaded(PopupProcess popup, bool overlay) {
			if (mPopups.Peek() != popup) { return; }
			if (overlay) { return; }
			mPopups.Pop();
			if (mPopups.Count > 0) {
				mPopups.Peek().Hide();
			}
			mPopups.Push(popup);
		}

		private void ClosePopupByName(string popupName, bool internalCall) {
			temp_popups.Clear();
			PopupProcess target = null;
			while (mPopups.Count > 0) {
				PopupProcess popup = mPopups.Pop();
				if (popup.name == popupName) {
					target = popup;
					break;
				}
				temp_popups.Push(popup);
			}
			bool isTopClosed = true;
			while (temp_popups.Count > 0) {
				mPopups.Push(temp_popups.Pop());
				isTopClosed = false;
			}
			if (target != null) {
				RemoveFocus(target, !internalCall);
				target.Close(isTopClosed);
			}
			if (isTopClosed && mPopups.Count > 0) {
				PopupProcess popup = mPopups.Peek();
				if (!popup.visible) { popup.Show(); }
			}
		}

		private class PopupProcess : IFocus {
			public delegate void LoadedCallback(PopupProcess popup, bool overlay);
			public static RectTransform popup_root;
			public string name { get; private set; }
			public object param { get; private set; }
			public System.Action onClose { get; private set; }
			public GameObject gameObject { get; private set; }
			public bool visible { get; private set; }
			private List<MonoBehaviour> mBehaviours = new List<MonoBehaviour>();
			private bool mActive;
			private IUILoader mUILoader;
			private LoadedCallback mOnLoadedCallback;
			private System.Action<GameObject> mOnLoaded;
			private System.Action mOnCloseAnimCallback;
			private int mFocusState;
			private bool mClosing;
			private int mClosingCount;
			private PopupProcess() {
				mOnLoaded = OnLoaded;
				mOnCloseAnimCallback = OnCloseAnimCallback;
			}
			public void Start(IUILoader uiLoader, LoadedCallback callback) {
				mActive = true;
				visible = true;
				mUILoader = uiLoader;
				mOnLoadedCallback = callback;
				mUILoader.LoadPopup(name, mOnLoaded);
			}
			public void Show() {
				if (visible) { return; }
				visible = true;
				if (gameObject != null) { gameObject.SetActive(true); }
			}
			public void Hide() {
				if (!visible) { return; }
				visible = false;
				if (gameObject != null) { gameObject.SetActive(false); }
			}
			public void Close(bool anim) {
				if (gameObject != null) {
					mClosing = true;
					for (int i = 0, imax = mBehaviours.Count; i < imax; i++) {
						MonoBehaviour behaviour = mBehaviours[i];
						IUIFocusHandler focusHandler = behaviour as IUIFocusHandler;
						if (focusHandler != null) {
							try { focusHandler.OnClose(); } catch (System.Exception e) { Debug.LogException(e); }
						}
						if (anim) {
							IPopupCloseAnim closeAnim = behaviour as IPopupCloseAnim;
							if (closeAnim != null) {
								mClosingCount++;
								closeAnim.ExecuteClose(mOnCloseAnimCallback);
							}
						}
					}
					mClosing = false;
				}
				System.Action callback = onClose;
				if (callback != null) { try { callback(); } catch (System.Exception e) { Debug.LogException(e); } }
				if (mClosingCount > 0) {
					// TODO block ui
				} else {
					ClearInstance();
				}
			}
			public void GetFocus() {
				if (gameObject != null) {
					for (int i = 0, imax = mBehaviours.Count; i < imax; i++) {
						IUIFocusHandler handler = mBehaviours[i] as IUIFocusHandler;
						if (handler != null) {
							try { handler.OnFocus(); } catch (System.Exception e) { Debug.LogException(e); }
						}
					}
					mFocusState = 2;
				} else {
					mFocusState = 1;
				}
			}
			public void LoseFocus() {
				if (gameObject != null) {
					for (int i = 0, imax = mBehaviours.Count; i < imax; i++) {
						IUIFocusHandler handler = mBehaviours[i] as IUIFocusHandler;
						if (handler != null) {
							try { handler.OnLoseFocus(); } catch (System.Exception e) { Debug.LogException(e); }
						}
					}
				}
				mFocusState = 0;
			}
			public void HandleKey(KeyCode key) {

			}
			private void OnLoaded(GameObject go) {
				if (!mActive) {
					mUILoader.UnloadInstance(go);
					return;
				}
				gameObject = go;
				RectTransform rectTrans = go.transform as RectTransform;
				rectTrans.SetParent(popup_root, false);
				rectTrans.offsetMin = Vector2.zero;
				rectTrans.offsetMax = Vector2.zero;
				rectTrans.localRotation = Quaternion.identity;
				rectTrans.localScale = Vector3.one;
				bool overlay = false;
				mBehaviours.Clear();
				go.GetComponents<MonoBehaviour>(mBehaviours);
				bool paraSet = false;
				for (int i = 0, imax = mBehaviours.Count; i < imax; i++) {
					MonoBehaviour behaviour = mBehaviours[i];
					IUIParameterHandler handler = behaviour as IUIParameterHandler;
					if (handler != null) {
						try { handler.SetParameter(param); } catch (System.Exception e) { Debug.LogError(e); }
						paraSet = true;
					}
					IPopupOverlay popupOverlay = behaviour as IPopupOverlay;
					if (popupOverlay != null && popupOverlay.Overlay) { overlay = true; }
				}
				if (param != null && !paraSet) {
					Debug.LogErrorFormat(go, "[UIManager] No parameter handler found for '{0]' !", name);
				}
				if (mFocusState == 1) {
					for (int i = 0, imax = mBehaviours.Count; i < imax; i++) {
						IUIFocusHandler handler = mBehaviours[i] as IUIFocusHandler;
						if (handler != null) {
							try { handler.OnFocus(); } catch (System.Exception e) { Debug.LogException(e); }
						}
					}
					mFocusState = 2;
				}
				go.SetActive(visible);
				LoadedCallback callback = mOnLoadedCallback;
				mOnLoadedCallback = null;
				callback(this, overlay);
			}
			private void OnCloseAnimCallback() {
				mClosingCount--;
				if (mClosingCount == 0 && !mClosing) { ClearInstance(); }
			}
			private void ClearInstance() {
				if (gameObject != null) {
					mUILoader.UnloadInstance(gameObject);
					gameObject = null;
				}
				mBehaviours.Clear();
				Cache(this);
			}
			private static Queue<PopupProcess> cached_instances = new Queue<PopupProcess>();
			public static PopupProcess Get(string name, object param, System.Action onClose) {
				PopupProcess popup = null;
				if (cached_instances.Count > 0) {
					popup = cached_instances.Dequeue();
				}
				if (popup == null) { popup = new PopupProcess(); }
				popup.mActive = false;
				popup.visible = false;
				popup.gameObject = null;
				popup.mUILoader = null;
				popup.name = name;
				popup.param = param;
				popup.onClose = onClose;
				popup.mFocusState = 0;
				popup.mClosing = false;
				popup.mClosingCount = 0;
				return popup;
			}
			private static void Cache(PopupProcess popup) {
				if (popup == null) { return; }
				popup.mActive = false;
				popup.visible = false;
				popup.gameObject = null;
				popup.mUILoader = null;
				popup.name = null;
				popup.param = null;
				popup.onClose = null;
				popup.mFocusState = 0;
				popup.mClosing = false;
				popup.mClosingCount = 0;
				cached_instances.Enqueue(popup);
			}
		}

		#endregion

		#region toast

		private ToastProcess mCurrentToast;
		private System.Action<ToastProcess> mOnToastBeginClose;
		private System.Action<ToastProcess> mOnToastClosed;

		public void ShowToast(string content, float duration) {
			if (mCurrentToast != null) {
				mCurrentToast.Close();
				mCurrentToast = null;
			}
			if (mOnToastBeginClose == null) { mOnToastBeginClose = OnToastBeginClose; }
			if (mOnToastClosed == null) { mOnToastClosed = OnToastClosed; }
			mCurrentToast = ToastProcess.Get(content, duration);
			mCurrentToast.Start(mOnToastBeginClose, mOnToastClosed);
		}

		private void OnToastBeginClose(ToastProcess toast) {
			if (mCurrentToast == toast) {
				mCurrentToast = null;
			}
		}

		private void OnToastClosed(ToastProcess toast) {
			toast.Clear();
			ToastProcess.Cache(toast);
		}

		private class ToastProcess {
			public static GerenicToast toast_template;
			private static Queue<GerenicToast> cached_toasts = new Queue<GerenicToast>();
			private string mContent;
			private float mDuration;
			private GerenicToast mToast;
			private System.Action<GerenicToast> mOnBeginClose;
			private System.Action<GerenicToast> mOnClosed;
			private System.Action<ToastProcess> mOnBeginCloseHandler;
			private System.Action<ToastProcess> mOnClosedHandler;
			private ToastProcess() {
				mOnBeginClose = OnBeginClose;
				mOnClosed = OnClosed;
			}
			public void Start(System.Action<ToastProcess> onBeginClose, System.Action<ToastProcess> onClosed) {
				mOnBeginCloseHandler = onBeginClose;
				mOnClosedHandler = onClosed;
				mToast = cached_toasts.Count > 0 ? cached_toasts.Dequeue() : null;
				if (mToast == null) {
					mToast = GameObject.Instantiate<GerenicToast>(toast_template);
				}
				GameObject go = mToast.gameObject;
				RectTransform rectTrans = go.transform as RectTransform;
				rectTrans.SetParent(toast_template.transform.parent, false);
				rectTrans.offsetMin = Vector2.zero;
				rectTrans.offsetMax = Vector2.zero;
				rectTrans.localRotation = Quaternion.identity;
				rectTrans.localScale = Vector3.one;
				mToast.Show(mContent, mDuration, mOnBeginClose, mOnClosed);
				go.SetActive(true);
			}
			public void Close() {
				if (mToast != null) {
					mToast.Close();
				} else if (mOnClosedHandler != null) {
					mOnClosedHandler(this);
				}
			}
			public void Clear() {
				if (mToast != null) {
					mToast.gameObject.SetActive(false);
					cached_toasts.Enqueue(mToast);
				}
				mToast = null;
				mOnBeginCloseHandler = null;
				mOnClosedHandler = null;
			}
			private void OnBeginClose(GerenicToast toast) {
				if (mOnBeginCloseHandler != null) { mOnBeginCloseHandler(this); }
			}
			private void OnClosed(GerenicToast toast) {
				if (mOnClosedHandler != null) { mOnClosedHandler(this); }
			}
			private static Queue<ToastProcess> cached_instances = new Queue<ToastProcess>();
			public static ToastProcess Get(string content, float duration) {
				ToastProcess toast = null;
				if (cached_instances.Count > 0) {
					toast = cached_instances.Dequeue();
				}
				if (toast == null) { toast = new ToastProcess(); }
				toast.mContent = content;
				toast.mDuration = duration;
				return toast;
			}
			public static void Cache(ToastProcess toast) {
				if (toast == null) { return; }
				cached_instances.Enqueue(toast);
			}
		}

		#endregion

		#region loading mask

		private class LoadingMaskFocus : IFocus {
			public void GetFocus() { }
			public void LoseFocus() { }
			public void HandleKey(KeyCode key) { }
		}

		private IFocus mLoadingMaskFocus = new LoadingMaskFocus();

		public void SetLoadingMask(string name) {
			if (string.IsNullOrEmpty(name)) { return; }
			mUILoader.LoadLoadingMask(name, (GameObject go) => {
				if (go == null) { return; }
				if (mLoadingKeys.Count > 0) {
					mUILoader.UnloadInstance(go);
					return;
				}
				LoadingMaskBase lm = go.GetComponent<LoadingMaskBase>();
				if (lm == null) {
					mUILoader.UnloadInstance(go);
					return;
				}
				RectTransform rectTrans = go.transform as RectTransform;
				rectTrans.SetParent(m_DefaultLoadingMask.transform.parent, false);
				rectTrans.offsetMin = Vector2.zero;
				rectTrans.offsetMax = Vector2.zero;
				rectTrans.localRotation = Quaternion.identity;
				rectTrans.localScale = Vector3.one;
				go.SetActive(false);
				if (mLoadingMask != m_DefaultLoadingMask) {
					mUILoader.UnloadInstance(mLoadingMask.gameObject);
				}
				mLoadingMask = lm;
			});
		}

		public bool ResetLoadingMask() {
			if (mLoadingKeys.Count > 0) { return false; }
			if (mLoadingMask == m_DefaultLoadingMask) { return false; }
			mUILoader.UnloadInstance(mLoadingMask.gameObject);
			mLoadingMask = m_DefaultLoadingMask;
			return true;
		}

		private Dictionary<string, int> mLoadingKeys = new Dictionary<string, int>();
		public void ShowLoadingMask(string key) {
			if (string.IsNullOrEmpty(key)) { return; }
			if (mLoadingKeys.ContainsKey(key)) { return; }
			if (mLoadingKeys.Count == 0) {
				mLoadingMask.Show();
				AddFocus(mLoadingMaskFocus, false, true);
			}
			mLoadingKeys.Add(key, 0);
		}

		public void HideLoadingMask(string key) {
			if (string.IsNullOrEmpty(key)) { return; }
			if (!mLoadingKeys.Remove(key)) { return; }
			if (mLoadingKeys.Count <= 0) {
				mLoadingMask.Close(mOnHideLoadingMaskEnd);
				RemoveFocus(mLoadingMaskFocus, true);
			}
		}

		private void OnHideLoadingMaskEnd() { }
		#endregion

		#region focus

		private IFocus mCurrentFocus;
		private List<IFocus> mFocuses = new List<IFocus>();

		private void AddFocus(IFocus focus, bool isBase, bool dispatch) {
			if (focus == null) { return; }
			if (isBase) {
				mFocuses.Insert(0, focus);
			} else {
				mFocuses.Add(focus);
			}
			if (dispatch) { CheckCurrentFocus(); }
		}

		private void RemoveFocus(IFocus focus, bool dispatch) {
			if (focus == null) { return; }
			if (!mFocuses.Remove(focus)) { return; }
			if (dispatch) { CheckCurrentFocus(); }
		}

		private void CheckCurrentFocus() {
			int n = mFocuses.Count;
			IFocus focus = n > 0 ? mFocuses[n - 1] : null;
			if (focus == mCurrentFocus) { return; }
			if (mCurrentFocus != null) { mCurrentFocus.LoseFocus(); }
			mCurrentFocus = focus;
			if (mCurrentFocus != null) { mCurrentFocus.GetFocus(); }
		}

		private interface IFocus {
			void GetFocus();
			void LoseFocus();
			void HandleKey(KeyCode key);
		}

		#endregion

	}

}
