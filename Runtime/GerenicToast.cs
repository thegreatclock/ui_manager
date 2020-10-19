#pragma warning disable 649

using UnityEngine;
using UnityEngine.UI;
using System;
using GreatClock.Common.Tweens;
using GreatClock.Common.Utils;

namespace GreatClock.Common.UI {

	public class GerenicToast : MonoBehaviour {

		[SerializeField]
		private Text m_Content;
		[SerializeField]
		private TweenComponentBase m_ToastCanvas;
		[SerializeField]
		private Vector2 m_MinSize = new Vector2(400f, 48f);
		[SerializeField]
		private Vector2 m_Padding = new Vector2(160f, 20f);

		private RectTransform mTrans;

		private Action<GerenicToast> mOnBeginClose;
		private Action<GerenicToast> mOnClosed;
		private ulong mCdId;

		private Timer.TimerDelegate mOnTweenFinish;

		private int mCheckResize;

		void Awake() {
			mTrans = transform as RectTransform;
			mOnTweenFinish = OnTweenFinish;
		}

		public void Show(string content, float duration, Action<GerenicToast> onBeginClose, Action<GerenicToast> onClosed) {
			mOnBeginClose = onBeginClose;
			mOnClosed = onClosed;
			mCheckResize = 2;
			m_Content.text = content;
			mCdId = RealTimeTimer.Register(duration, Close);
		}

		public void Close() {
			RealTimeTimer.Unregister(mCdId);
			if (mOnBeginClose != null) { mOnBeginClose(this); }
			mOnBeginClose = null;
			float dur = m_ToastCanvas.PlayGroup("out");
			Timer.Register(Mathf.Min(dur, 1), mOnTweenFinish);
		}

		private void OnTweenFinish() {
			if (mOnClosed != null) {
				mOnClosed(this);
				mOnClosed = null;
			}
		}

		void LateUpdate() {
			if (mCheckResize > 0) {
				mCheckResize--;
				Vector2 size = m_Content.rectTransform.sizeDelta;
				size += m_Padding;
				size = new Vector2(Mathf.Max(Mathf.Ceil(size.x), m_MinSize.x), Mathf.Max(Mathf.Ceil(size.y), m_MinSize.y));
				mTrans.sizeDelta = size;
			}
		}

	}

}
