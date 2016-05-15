using UnityEngine;
using System.Collections;

public abstract class InputBehavior : MonoBehaviour {

    public virtual void OnTouchDown(Vector3 position) { }

    public virtual void OnTouchUp(Vector3 position) { }

    public virtual void OnTouchMoved(Vector3 point) { }

    public virtual void OnTouchStationary(Vector3 point) { }

    public virtual void OnTouchCanceled(Vector3 point) { }

    public virtual void OnTouchExit(Vector3 point) { }

    public virtual bool CancleSwipe { get { return false; } }
}
