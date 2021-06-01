using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileSwipeInput : MonoBehaviour
{
    private bool swipeUp, swipeDown;
    private float startSwipeTime;
    private Vector2 startSwipePosition;
    private float swipeTime;
    private float swipeDistance;

    private float endSwipeTime;
    private Vector2 endSwipePosition;

    public float maxSwipeTime;
    public float minSwipeDistance;

    void CheckSwipe()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                startSwipePosition = touch.position;
                startSwipeTime = Time.time;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                endSwipePosition = touch.position;
                endSwipeTime = Time.time;
                swipeTime = endSwipeTime - startSwipeTime;
                swipeDistance = (endSwipePosition - startSwipePosition).magnitude;
                if (swipeTime < maxSwipeTime && swipeDistance > minSwipeDistance)
                {
                    SwipeControl();
                }
            }
        }
        else
        {
            swipeUp = false;
            swipeDown = false;
        }
    }

    void SwipeControl()
    {
        Vector2 Distance = endSwipePosition - startSwipePosition;
        float xDistance = Mathf.Abs(Distance.x);
        float yDistance = Mathf.Abs(Distance.y);

        if (xDistance > yDistance)
        {
            if (Distance.x > 0)
            {
                //MoveLane(false);
            }
            else if (Distance.x < 0)
            {
                //MoveLane(true);
            }
            swipeUp = false;
            swipeDown = false;
        }
        else if (xDistance < yDistance)
        {
            if (Distance.y > 0)
            {
                Debug.Log("Jump");
                swipeUp = true;
            }
            if (Distance.y < 0)
            {
                swipeDown = true;
            }
        }
    }
}
