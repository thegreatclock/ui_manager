using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GreatClock.Common.UI {

    public interface IUIParameterHandler {
        void SetParameter(object para);
	}

    public interface IUIFocusHandler {
        void OnFocus();
        void OnLoseFocus();
        void OnClose();
	}

    public interface IUIKeyHandler {
        void OnKey(KeyCode key);
	}
        
}
