using System;
using UnityEngine;

namespace GreatClock.Common.UI {

	public interface IUILoader {
		
		void LoadWindow(string window, Action<GameObject> onLoaded);
		void LoadPopup(string popup, Action<GameObject> onLoaded);
		void LoadLoadingMask(string loadingMask, Action<GameObject> onLoaded);

		void UnloadInstance(GameObject go);

	}

}
