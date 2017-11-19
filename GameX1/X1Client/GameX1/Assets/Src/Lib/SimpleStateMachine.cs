using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

public class SimpleStateMachine<T> {
    private Dictionary<T, SimpleStateMachineState<T>> states = new Dictionary<T, SimpleStateMachineState<T>>();
    private SimpleStateMachineState<T> _curState;
    public T curState;
    public T preState;
    public bool firstUpdate = true;
    public bool firstFixedUpdate = true;
    public void Register(T state, UnityAction enter, UnityAction exit, UnityAction update) {
        states[state] = new SimpleStateMachineState<T>(state, enter, exit, update);
    }
    public void Register(T state, UnityAction enter, UnityAction exit, UnityAction update, UnityAction fixedUpdate) {
        states[state] = new SimpleStateMachineState<T>(state, enter, exit, update, fixedUpdate);
    }
    public void Register(T state, ISimpleStateMachineStateBehaviour stateBehaviour) {
        states[state] = new SimpleStateMachineState<T>(state, stateBehaviour.OnEnter, stateBehaviour.OnExit, stateBehaviour.OnUpdate, stateBehaviour.OnFixedUpdate);
    }

    public void ChangeState(T state) {
        if(_curState != null && _curState.exit != null) {
            _curState.exit();
        }

        //Debug.Log(curState + " => " + state);
        preState = curState;
        curState = state;
        _curState = null;
        firstUpdate = true;
        firstFixedUpdate = true;
        if(states.TryGetValue(state, out _curState)) {
            if(_curState.enter != null) {
                _curState.enter();
            }
        }
    }

    public void Update() {
        if(_curState != null && _curState.update != null) {
            _curState.update();
        }
        firstUpdate = false;
    }

    public void FixedUpdate() {
        if(_curState != null && _curState.fixedUpdate != null) {
            _curState.fixedUpdate();
        }
        firstFixedUpdate = false;
    }
}

public class SimpleStateMachineState<T> {
    public T state;
    public UnityAction enter;
    public UnityAction exit;
    public UnityAction update;
    public UnityAction fixedUpdate;

    public SimpleStateMachineState(T state, UnityAction enter, UnityAction exit, UnityAction update) {
        this.state = state;
        this.enter = enter;
        this.exit = exit;
        this.update = update;
    }

    public SimpleStateMachineState(T state, UnityAction enter, UnityAction exit, UnityAction update, UnityAction fixedUpdate) {
        this.state = state;
        this.enter = enter;
        this.exit = exit;
        this.update = update;
        this.fixedUpdate = fixedUpdate;
    }
}

public interface ISimpleStateMachineStateBehaviour {
    void OnEnter();
    void OnExit();
    void OnUpdate();
    void OnFixedUpdate();
}
