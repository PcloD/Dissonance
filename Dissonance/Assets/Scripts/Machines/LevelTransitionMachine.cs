using UnityEngine;
using System.Collections;

public class LevelTransitionMachine : MonoBehaviour {
    // For now, this will just load the next level

    [SerializeField]
    Trigger2D[] _triggers;

    void OnEnable () {
        for (int i = 0; i < _triggers.Length; i++) {
            _triggers[i].ActiveHook += TriggerChanged;
        }
    }

    void OnDisable () {
        for (int i = 0; i < _triggers.Length; i++) {
            _triggers[i].ActiveHook -= TriggerChanged;
        }
    }

    void TriggerChanged () {
        for (int i = 0; i < _triggers.Length; i++) {
            if (!_triggers[i].IsActive) { return; }
        }
        Application.LoadLevel(Application.loadedLevel + 1);
    }
}
