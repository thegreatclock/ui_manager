#pragma warning disable 649

using UnityEngine;
using System;
using GreatClock.Common.Tweens;
using GreatClock.Common.Utils;

namespace GreatClock.Common.UI {

	public class DefaultLoadingMask : LoadingMaskBase {

		[SerializeField]
		private TweenComponentBase m_Tween;

		private bool mShowing = false;

		private Action mOnClosed;
		private Timer.TimerDelegate mTimeout;

		void Awake() {
			mTimeout = OnTweenFinish;
		}

		public override void Show() {
			mShowing = true;
			gameObject.SetActive(true);
			m_Tween.PlayGroup("show");
		}

		public override void Close(Action onClosed) {
			mShowing = false;
			mOnClosed = onClosed;
			float dur = m_Tween.PlayGroup("hide");
			Timer.Register(Mathf.Min(1f, dur), mTimeout);
		}

		private void OnTweenFinish() {
			if (mShowing) { return; }
			gameObject.SetActive(false);
			Action callback = mOnClosed;
			mOnClosed = null;
			if (callback != null) {
				try { callback(); } catch (Exception e) { Debug.LogException(e); }
			}
		}

	}

}
