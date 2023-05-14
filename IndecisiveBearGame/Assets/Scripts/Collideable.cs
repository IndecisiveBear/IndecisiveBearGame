using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Collideable
{
    public Vector2 GetPosition();
    public void SetPositionX(float x);
    public void SetPositionY(float y);
    public void SetTransformPosition(Vector2 pos);
    public void MoveUpGrid();
    public void MoveDownGrid();
    public BoxCollider2D GetStartClimbingRamp();
    public void SetStartClimbingRamp(BoxCollider2D body);
}