using UnityEngine;
using System.Collections.Generic;


public class InputManager : MonoBehaviour
{
    
    public LayerMask touchLayerMask;
    public Camera mainCamera;
    public GameObject tileParent;

    private List<GameObject> touchList = new List<GameObject>();
    private GameObject[] touchesOld;
    private RaycastHit hit;

    private Vector3 lastMousePosition;
    private Vector3 lastTouchPosition;

    Vector3 vel = new Vector3(0f, 0f, 0f);
    float smoothTime = 0.01f;

    void Update()
    {

#if UNITY_EDITOR
        if (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
        {
            
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, touchLayerMask))
            {
                GameObject hitObject = hit.transform.gameObject;
                InputBehavior inputBehavior = hitObject.GetComponent<InputBehavior>();

                if (inputBehavior == null)
                {
                    return;
                }
                
                if (Input.GetMouseButtonDown(0))
                {
                    inputBehavior.OnTouchDown(hit.point);
                    lastMousePosition = Input.mousePosition;
                    Debug.Log("TOUCH DOWN");
                }

                if (Input.GetMouseButton(0) && Input.mousePosition == lastMousePosition)
                {
                    inputBehavior.OnTouchStationary(hit.point);
                    lastMousePosition = Input.mousePosition;
                    Debug.Log("TOUCH STAY");
                }

                else if (Input.GetMouseButton(0) && Input.mousePosition != lastMousePosition)
                {
                    inputBehavior.OnTouchMoved(hit.point);
                    UpdateCameraPosition(lastMousePosition,Input.mousePosition);
                    
                    lastMousePosition = Input.mousePosition;
                    Debug.Log("TOUCH MOVE");
                }
                
                if (Input.GetMouseButtonUp(0))
                {
                    inputBehavior.OnTouchExit(hit.point);
                    Debug.Log("TOUCH EXIT");
                }
            }
        }
        
#endif

        if (Input.touchCount > 0) {

            touchesOld = new GameObject[touchList.Count];
            touchList.CopyTo(touchesOld);
            touchList.Clear();

            foreach (Touch touch in Input.touches) {
                Ray ray = mainCamera.ScreenPointToRay(touch.position);

                if (Physics.Raycast(ray, out hit, touchLayerMask))
                {
                    GameObject hitObject = hit.transform.gameObject;
                    InputBehavior inputBehavior = hitObject.GetComponent<InputBehavior>();

                    if (inputBehavior == null)
                    {
                        continue;
                    }

                    if (touch.phase == TouchPhase.Began)
                    {
                        inputBehavior.OnTouchDown(hit.point);
                        lastTouchPosition = touch.position;
                    }

                    if (touch.phase == TouchPhase.Stationary)
                    {
                        inputBehavior.OnTouchStationary(hit.point);
                        lastTouchPosition = touch.position;
                    }

                    if (touch.phase == TouchPhase.Moved)
                    {
                        inputBehavior.OnTouchMoved(hit.point);
                        UpdateCameraPosition(lastTouchPosition, touch.position);
                        lastTouchPosition = touch.position;
                    }
                    
                    if (touch.phase == TouchPhase.Ended)
                    {
                        inputBehavior.OnTouchExit(hit.point);
                    }

                    if (touch.phase == TouchPhase.Canceled)
                    {
                        inputBehavior.OnTouchCanceled(hit.point);
                    }

                }
            }   
        }
    }

    void UpdateCameraPosition(Vector3 previousInputPos, Vector3 currentInputPos) {
        //Vector3 moveDelta = (currentInputPos - previousInputPos) * 0.03f;
        //previousInputPos = mainCamera.ScreenToWorldPoint(previousInputPos);
        //currentInputPos = mainCamera.ScreenToWorldPoint(currentInputPos);
        Debug.Log(previousInputPos + " --> " + currentInputPos);
        Debug.Log(mainCamera.ScreenToWorldPoint(previousInputPos) + " --> " + mainCamera.ScreenToWorldPoint(currentInputPos));

        //Vector3 targetPos = Vector3.SmoothDamp(previousInputPos, currentInputPos, ref vel, smoothTime);


        Vector3 moveDelta = (currentInputPos - previousInputPos);
        
        mainCamera.transform.position = new Vector3(
                mainCamera.transform.position.x - moveDelta.x,
                mainCamera.transform.position.y - moveDelta.y,
                mainCamera.transform.position.z
            );
        
    }
}
