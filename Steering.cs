using System.Collections.Generic;
using UnityEngine;

namespace SteeringCalcs
{
    [System.Serializable]
    public class AvoidanceParams
    {
        public bool Enable;
        public LayerMask ObstacleMask;
        // Note: As mentioned in the spec, you're free to add extra parameters to AvoidanceParams.
    }

    public class Steering
    {
        // PLEASE NOTE:
        // You do not need to edit any of the methods in the HelperMethods region.
        // In Visual Studio, you can collapse the HelperMethods region by clicking
        // the "-" to the left.
        #region HelperMethods

        // Helper method for rotating a vector by an angle (in degrees).
        public static Vector2 rotate(Vector2 v, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;

            return new Vector2(
                v.x * Mathf.Cos(radians) - v.y * Mathf.Sin(radians),
                v.x * Mathf.Sin(radians) + v.y * Mathf.Cos(radians)
            );
        }

        // Converts a desired velocity into a steering force, as will
        // be explained in class (Week 2).
        public static Vector2 DesiredVelToForce(Vector2 desiredVel, Rigidbody2D rb, float accelTime, float maxAccel)
        {
            Vector2 accel = (desiredVel - rb.velocity) / accelTime;

            if (accel.magnitude > maxAccel)
            {
                accel = accel.normalized * maxAccel;
            }

            // F = ma
            return rb.mass * accel;
        }

        // In addition to separation, cohesion and alignment, the flies also have
        // an "anchor" force applied to them while flocking, to keep them within
        // the game arena. This is already implemented for you.
        public static Vector2 GetAnchor(Vector2 currentPos, Vector2 anchorDims)
        {
            Vector2 desiredVel = Vector2.zero;

            if (Mathf.Abs(currentPos.x) > anchorDims.x)
            {
                desiredVel -= new Vector2(currentPos.x, 0.0f);
            }

            if (Mathf.Abs(currentPos.y) > anchorDims.y)
            {
                desiredVel -= new Vector2(0.0f, currentPos.y);
            }

            return desiredVel;
        }

        // This "parent" seek method toggles between SeekAndAvoid and BasicSeek
        // depending on whether obstacle avoidance is enabled. Do not edit this.
        public static Vector2 Seek(Vector2 currentPos, Vector2 targetPos, float maxSpeed, AvoidanceParams avoidParams)
        {
            if (avoidParams.Enable)
            {
                return SeekAndAvoid(currentPos, targetPos, maxSpeed, avoidParams);
            }
            else
            {
                return BasicSeek(currentPos, targetPos, maxSpeed);
            }
        }

        // Seek is already implemented for you. Do not edit this method.
        public static Vector2 BasicSeek(Vector2 currentPos, Vector2 targetPos, float maxSpeed)
        {
            Vector2 offset = targetPos - currentPos;
            Vector2 desiredVel = offset.normalized * maxSpeed;
            return desiredVel;
        }

        // Do not edit this method. To implement obstacle avoidance, the only method
        // you need to edit is GetAvoidanceTarget.
        public static Vector2 SeekAndAvoid(Vector2 currentPos, Vector2 targetPos, float maxSpeed, AvoidanceParams avoidParams)
        {
            targetPos = GetAvoidanceTarget(currentPos, targetPos, avoidParams);

            return BasicSeek(currentPos, targetPos, maxSpeed);
        }

        // This "parent" arrive method toggles between ArriveAndAvoid and BasicArrive
        // depending on whether obstacle avoidance is enabled. Do not edit this.
        public static Vector2 Arrive(Vector2 currentPos, Vector2 targetPos, float radius, float maxSpeed, AvoidanceParams avoidParams)
        {
            if (avoidParams.Enable)
            {
                return ArriveAndAvoid(currentPos, targetPos, radius, maxSpeed, avoidParams);
            }
            else
            {
                return BasicArrive(currentPos, targetPos, radius, maxSpeed);
            }
        }

        // Do not edit this method. To implement obstacle avoidance, the only method
        // you need to edit is GetAvoidanceTarget.
        public static Vector2 ArriveAndAvoid(Vector2 currentPos, Vector2 targetPos, float radius, float maxSpeed, AvoidanceParams avoidParams)
        {
            targetPos = GetAvoidanceTarget(currentPos, targetPos, avoidParams);

            return BasicArrive(currentPos, targetPos, radius, maxSpeed);
        }

        #endregion

        // Below are all the methods that you *do* need to edit.
        #region MethodsToImplement

        // See the spec for a detailed explanation of how GetAvoidanceTarget is expected to work.
        // You're expected to use Physics2D.CircleCast (https://docs.unity3d.com/ScriptReference/Physics2D.CircleCast.html)
        // You'll also probably want to use the rotate() method declared above.
        public static Vector2 GetAvoidanceTarget(Vector2 currentPos, Vector2 targetPos, AvoidanceParams avoidParams)
        {
            Vector2 newTarget = targetPos;

            if (avoidParams.Enable)
            {
                // TODO: Add logic here for calculating the new target position.
            }

            return newTarget;
        }

        public static Vector2 BasicFlee(Vector2 currentPos, Vector2 predatorPos, float maxSpeed)
        {
            // TODO: Implement proper flee logic.
            // The method should return the character's *desired velocity*, not a steering force.
            return Vector2.zero;
        }

        public static Vector2 BasicArrive(Vector2 currentPos, Vector2 targetPos, float radius, float maxSpeed)
        {
         // Calculate the distance to the target
            float distance = Vector2.Distance(currentPos, targetPos);

            // If outside the radius, seek towards the target
            if (distance >= radius)
            {
                return BasicSeek(currentPos, targetPos, maxSpeed);
            }
            else
            {
                // Scale the speed based on the distance within the radius
                float speed = maxSpeed * (distance / radius);
                speed = Mathf.Min(speed, maxSpeed);

                // Calculate the desired velocity to arrive at the target
                Vector2 desiredVelocity = (targetPos - currentPos).normalized * speed;
                return desiredVelocity;
            }
        }

        public static Vector2 GetSeparation(Vector2 currentPos, List<Transform> neighbours, float maxSpeed)
        {
            Vector2 separationVector = Vector2.zero;
            foreach (Transform neighbour in neighbours)
            {
                Vector2 towardsMe = currentPos - (Vector2)neighbour.position;
                if (towardsMe.magnitude > 0) // Avoid division by zero
                {
                    separationVector += towardsMe.normalized / towardsMe.magnitude;
                }
            }

            return separationVector.normalized * maxSpeed;
        }

        public static Vector2 GetCohesion(Vector2 currentPos, List<Transform> neighbours, float maxSpeed)
        {
            if (neighbours.Count == 0) return Vector2.zero;

            Vector2 averagePos = Vector2.zero;
            foreach (Transform neighbour in neighbours)
            {
                averagePos += (Vector2)neighbour.position;
            }
            averagePos /= neighbours.Count;

            Vector2 towardsAverage = averagePos - currentPos;
            return towardsAverage.normalized * maxSpeed;
        }

        public static Vector2 GetAlignment(List<Transform> neighbours, float maxSpeed)
        {
            if (neighbours.Count == 0) return Vector2.zero;

            Vector2 averageVel = Vector2.zero;
            foreach (Transform neighbour in neighbours)
            {
                Rigidbody2D rb = neighbour.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    averageVel += rb.velocity;
                }
            }
            averageVel /= neighbours.Count;

            return averageVel.normalized * maxSpeed;
        }

        #endregion
    }
}
