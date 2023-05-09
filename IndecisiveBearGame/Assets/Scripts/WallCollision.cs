using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WallCollision : object
{
    /// <summary>
    /// <c>DetectCollision</c> handles collision detection logic between two BoxColliders. 
    /// It also warps player to nearest acceptable location if they are inside of a wall.
    /// </summary>
    /// <returns>
    /// Whether or not a collision was detected.
    /// </returns>
    public static bool DetectCollision(GameObject object1, GameObject object2)
    {
        BoxCollider2D body1 = object1.GetComponent<BoxCollider2D>();
        BoxCollider2D body2 = object2.GetComponent<BoxCollider2D>();
        Collideable script1 = body1.GetComponent<Collideable>();

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
        return false;
    }
}
