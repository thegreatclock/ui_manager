using System;
using UnityEngine;

namespace GreatClock.Common.UI {

	public interface IUILoader {

		void LoadInstance(string path, Action<GameObject> onLoaded);

		void UnloadInstance(GameObject go);

	}

}
