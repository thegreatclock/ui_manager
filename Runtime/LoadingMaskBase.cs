using UnityEngine;
using UnityEngine.UI;
using System;

namespace GreatClock.Common.UI {

	public abstract class LoadingMaskBase : MonoBehaviour {

		public abstract void Show();

		public abstract void Close(Action onClosed);

	}

}
