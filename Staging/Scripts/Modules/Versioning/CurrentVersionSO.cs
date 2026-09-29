using UnityEngine;

namespace UnityX.Versioning {
    public class CurrentVersionSO : ScriptableObject {
        private static CurrentVersionSO _Instance;
        public static CurrentVersionSO Instance {
            get {
                if(_Instance == null) _Instance = Resources.Load<CurrentVersionSO>(typeof(CurrentVersionSO).Name);
                if(_Instance == null){
                    Debug.LogWarning("No instance of " + typeof(CurrentVersionSO).Name + " found, using default values");
                    _Instance = CreateInstance<CurrentVersionSO>();
                }
                return _Instance;
            }
        }
        public Version version;
        
        protected virtual void OnEnable() {
            if( _Instance == null )
                _Instance = this;
        }

        protected virtual void OnDisable () {
            if( _Instance == this )
                _Instance = null;
        }
    }
}