using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CollisionManager : object
{
    public static void DetectCollision(GameObject object1, GameObject object2)
    {
        string name = object2.tag;
        
        switch (name)
        {
            case "Wall":
                WallCollision.DetectCollision(object1, object2);
                break;
            case "RampN":
                RampNCollision.DetectCollision(object1, object2);
                break;
            case "RampNDown":
                RampNCollision.DetectCollisionDown(object1, object2);
                break;
            case "RampS":
                RampSCollision.DetectCollision(object1, object2);
                break;
            case "RampSDown":
                RampSCollision.DetectCollisionDown(object1, object2);
                break;
            case "RampW":
                RampWCollision.DetectCollision(object1, object2);
                break;
            case "RampWDown":
                RampWCollision.DetectCollisionDown(object1, object2);
                break;
            case "RampE":
                RampECollision.DetectCollision(object1, object2);
                break;
            case "RampEDown":
                RampECollision.DetectCollisionDown(object1, object2);
                break;
            default:
                break;
        }
    }
}
