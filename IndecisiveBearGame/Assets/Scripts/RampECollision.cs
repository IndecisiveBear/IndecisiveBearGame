using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RampECollision : object
{
    private static float _rampFactor = 0f;
    private static float _rampFactor2 = 0.9f;
    /// <summary>
    /// <c>DetectCollisionRampE</c> handles collision detection logic between two BoxColliders, one being 
    /// the player, and the other being a east facing ramp, from a lower level to a higher level. 
    /// It also warps player to nearest acceptable location if they are inside of a ramp.
    /// </summary>
    /// <returns>
    /// Whether or not a collision was detected.
    /// </returns>
    public static bool DetectCollision(GameObject object1, GameObject object2)
    {
        BoxCollider2D body1 = object1.GetComponent<BoxCollider2D>();
        BoxCollider2D body2 = object2.GetComponent<BoxCollider2D>();
        Collideable script1 = body1.GetComponent<Collideable>();

        if (script1.GetStartClimbingRamp() == null)
        {
            if (body1.bounds.center.x + body1.bounds.extents.x >= body2.bounds.center.x - body2.bounds.extents.x &&
            body1.bounds.center.x - body1.bounds.extents.x <= body2.bounds.center.x + body2.bounds.extents.x &&
            body1.bounds.center.y + body1.bounds.extents.y >= body2.bounds.center.y - body2.bounds.extents.y &&
            body1.bounds.center.y - body1.bounds.extents.y <= body2.bounds.center.y + body2.bounds.extents.y)
            {
                // Detect the edge with which we are colliding.
                if (body1.bounds.center.x > body2.bounds.center.x &&
                    Mathf.Abs(body1.bounds.center.y - body2.bounds.center.y) < 1.5f * body2.bounds.extents.y)
                {
                    // Colliding with right edge of body2
                    script1.SetPositionX(body2.bounds.center.x + body2.bounds.extents.x + body1.bounds.extents.x);
                    script1.SetTransformPosition(script1.GetPosition());
                }
                if (body1.bounds.center.x < body2.bounds.center.x &&
                    body1.bounds.center.x + body1.bounds.extents.x >
                    body2.bounds.center.x - body2.bounds.extents.x * _rampFactor2 &&
                    Mathf.Abs(body1.bounds.center.y - body2.bounds.center.y) < 1f * body2.bounds.extents.y)
                {
                    // Colliding with left edge of body2
                    script1.SetStartClimbingRamp(body2);
                }
                if (body1.bounds.center.y > body2.bounds.center.y &&
                    Mathf.Abs(body1.bounds.center.x - body2.bounds.center.x) < 1.5f * body2.bounds.extents.x)
                {
                    // Colliding with top edge of body2
                    script1.SetPositionY(body2.bounds.center.y + body2.bounds.extents.y + body1.bounds.extents.y);
                    script1.SetTransformPosition(script1.GetPosition());
                }
                if (body1.bounds.center.y < body2.bounds.center.y &&
                    Mathf.Abs(body1.bounds.center.x - body2.bounds.center.x) < 1.5f * body2.bounds.extents.x)
                {
                    // Colliding with bottom edge of body2
                    script1.SetPositionY(body2.bounds.center.y - body2.bounds.extents.y - body1.bounds.extents.y);
                    script1.SetTransformPosition(script1.GetPosition());
                }
                return true;
            }
        }
        else if (script1.GetStartClimbingRamp() == body2)
        {
            if (body1.bounds.center.x + body1.bounds.extents.x >= body2.bounds.center.x + body2.bounds.extents.x)
            {
                // Exiting with top edge of body2
                script1.MoveUpGrid();
                script1.SetStartClimbingRamp(null);
            }
            if (
            !(
            body1.bounds.center.x + body1.bounds.extents.x >=
            body2.bounds.center.x - body2.bounds.extents.x &&
            body1.bounds.center.x - body1.bounds.extents.x <=
            body2.bounds.center.x + body2.bounds.extents.x &&
            body1.bounds.center.y + body1.bounds.extents.y * _rampFactor >=
            body2.bounds.center.y - body2.bounds.extents.y * _rampFactor &&
            body1.bounds.center.y - body1.bounds.extents.y * _rampFactor <=
            body2.bounds.center.y + body2.bounds.extents.y * _rampFactor
            ))
            {
                // Detect the edge with which we are exiting collision
                if (body1.bounds.center.y - body1.bounds.extents.y * _rampFactor >
                    body2.bounds.center.y + body2.bounds.extents.y * _rampFactor)
                {
                    // Exiting with top edge of body2
                    script1.SetPositionY(body2.bounds.center.y + body2.bounds.extents.y * _rampFactor +
                        body1.bounds.extents.y * _rampFactor);
                    script1.SetTransformPosition(script1.GetPosition());
                }
                if (body1.bounds.center.y + body1.bounds.extents.y * _rampFactor <
                    body2.bounds.center.y - body2.bounds.extents.y * _rampFactor)
                {
                    // Exiting with bottom edge of body2
                    script1.SetPositionY(body2.bounds.center.y - body2.bounds.extents.y * _rampFactor -
                        body1.bounds.extents.y * _rampFactor);
                    script1.SetTransformPosition(script1.GetPosition());
                }
                if (body1.bounds.center.x + body1.bounds.extents.x * 0.9 <
                    body2.bounds.center.x - body2.bounds.extents.x)
                {
                    // Exiting with right edge of body2
                    script1.SetStartClimbingRamp(null);
                }
            }
        }
        return false;
    }

    /// <summary>
    /// <c>DetectCollisionRampEDown</c> handles collision detection logic between two BoxColliders, one being 
    /// the player, and the other being a east facing ramp, from a higher level to a lower level. 
    /// It also warps player to nearest acceptable location if they are inside of a ramp.
    /// </summary>
    /// <returns>
    /// Whether or not a collision was detected.
    /// </returns>
    public static bool DetectCollisionDown(GameObject object1, GameObject object2)
    {
        BoxCollider2D body1 = object1.GetComponent<BoxCollider2D>();
        BoxCollider2D body2 = object2.GetComponent<BoxCollider2D>();
        Collideable script1 = body1.GetComponent<Collideable>();

        if (script1.GetStartClimbingRamp() == null) {
            if (body1.bounds.center.x + body1.bounds.extents.x >= body2.bounds.center.x - body2.bounds.extents.x &&
            body1.bounds.center.x - body1.bounds.extents.x <= body2.bounds.center.x + body2.bounds.extents.x &&
            body1.bounds.center.y + body1.bounds.extents.y >= body2.bounds.center.y - body2.bounds.extents.y &&
            body1.bounds.center.y - body1.bounds.extents.y <= body2.bounds.center.y + body2.bounds.extents.y)
            {
                // Detect the edge with which we are colliding.
                if (body1.bounds.center.x > body2.bounds.center.x &&
                    Mathf.Abs(body1.bounds.center.y - body2.bounds.center.y) < 1.5f * body2.bounds.extents.y)
                {
                    // Colliding with right edge of body2
                    script1.SetStartClimbingRamp(body2);
                }
                if (body1.bounds.center.x < body2.bounds.center.x &&
                    Mathf.Abs(body1.bounds.center.y - body2.bounds.center.y) < 1.5f * body2.bounds.extents.y)
                {
                    // Colliding with left edge of body2
                    script1.SetPositionX(body2.bounds.center.x - body2.bounds.extents.x - body1.bounds.extents.x);
                    script1.SetTransformPosition(script1.GetPosition());
                }
                if (body1.bounds.center.y > body2.bounds.center.y &&
                    Mathf.Abs(body1.bounds.center.x - body2.bounds.center.x) < 1.5f * body2.bounds.extents.x)
                {
                    // Colliding with top edge of body2
                    script1.SetPositionY(body2.bounds.center.y + body2.bounds.extents.y + body1.bounds.extents.y);
                    script1.SetTransformPosition(script1.GetPosition());
                }
                if (body1.bounds.center.y < body2.bounds.center.y &&
                    Mathf.Abs(body1.bounds.center.x - body2.bounds.center.x) < 1.5f * body2.bounds.extents.x)
                {
                    // Colliding with bottom edge of body2
                    script1.SetPositionY(body2.bounds.center.y - body2.bounds.extents.y - body1.bounds.extents.y);
                    script1.SetTransformPosition(script1.GetPosition());
                }
                return true;
            }
        }
        else if (script1.GetStartClimbingRamp() == body2)
        {
            if (body1.bounds.center.x - body1.bounds.extents.x <= body2.bounds.center.x - body2.bounds.extents.x &&
                body1.bounds.center.y > body2.bounds.center.y - body2.bounds.extents.y &&
                body1.bounds.center.y < body2.bounds.center.y + body2.bounds.extents.y)
            {
                // Exiting with right edge of body2
                script1.MoveDownGrid();
                script1.SetStartClimbingRamp(null);
            }
            if (
            !(
            body1.bounds.center.x + body1.bounds.extents.x >=
            body2.bounds.center.x - body2.bounds.extents.x &&
            body1.bounds.center.x - body1.bounds.extents.x <=
            body2.bounds.center.x + body2.bounds.extents.x &&
            body1.bounds.center.y + body1.bounds.extents.y >=
            body2.bounds.center.y - body2.bounds.extents.y &&
            body1.bounds.center.y - body1.bounds.extents.y <=
            body2.bounds.center.y + body2.bounds.extents.y
            ))
            {
                script1.SetStartClimbingRamp(null);
            }
        }
        return false;
    }
}
