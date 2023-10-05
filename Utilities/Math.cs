using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleGame
{
    public static class Acceleration
    {
        public const float LargeNumber = 69420f;

        /// <summary>
        /// If <paramref name="time"/> is 0 it returns <see cref="LargeNumber"/> to avoid divison by zero
        /// </summary>
        public static float CalculateAcceleration(float initialVelocity, float topVelocity, float time)
        {
            if (time == 0f) return LargeNumber;
            return (topVelocity - initialVelocity) / time;
        }

        public static float SpeedAfterTime(float velocity, float acceleration, float time)
        {
            return velocity + (acceleration * time);
        }

        public static float SpeedAfterDistance(float velocity, float acceleration, float distance)
        {
            if (acceleration == 0f) return velocity;
            if (distance == 0f) return velocity;

            float valueUnderSqr = (2 * acceleration * distance + (velocity * velocity));
            if (valueUnderSqr <= 0f) return 0f;

            return MathF.Sqrt(valueUnderSqr);
        }

        /// <summary>
        /// <b>v * t + ½ * a * t²</b> <br/><br/>
        /// 
        /// v: <paramref name="velocity"/> <br/>
        /// a: <paramref name="acceleration"/> <br/>
        /// t: <paramref name="time"/> <br/>
        /// </summary>
        public static float DistanceAfterTime(float velocity, float acceleration, float time)
        {
            return (velocity * time) + ((acceleration / 2) * (time * time));
        }

        /// <summary>
        /// <b>Δv / a</b> <br/>
        /// or <br/>
        /// <b>(v - vₒ) / a</b> <br/><br/>
        /// 
        /// If <paramref name="targetVelocity"/> can't be reached, it returns <see cref="LargeNumber"/> to avoid divison by zero. <br/><br/>
        /// 
        /// v: <paramref name="targetVelocity"/> <br/>
        /// vₒ: <paramref name="initialVelocity"/> <br/>
        /// a: <paramref name="acceleration"/> <br/>
        /// </summary>
        public static float TimeToReachVelocity(float initialVelocity, float targetVelocity, float acceleration)
        {
            if (acceleration == 0f) return LargeNumber;
            if (initialVelocity < targetVelocity && acceleration < 0f) return LargeNumber;
            if (initialVelocity > targetVelocity && acceleration > 0f) return LargeNumber;

            return (targetVelocity - initialVelocity) / acceleration;
        }

        /// <summary>
        /// <b>-vₒ / a</b> <br/><br/>
        /// 
        /// If 0 velocity can't be reached, it returns <see cref="LargeNumber"/> to avoid divison by zero. <br/><br/>
        /// 
        /// vₒ: <paramref name="initialVelocity"/> <br/>
        /// a: <paramref name="acceleration"/> <br/>
        /// </summary>
        public static float TimeToStop(float initialVelocity, float acceleration)
        {
            if (acceleration == 0f) return LargeNumber;
            if (initialVelocity < 0f && acceleration < 0f) return LargeNumber;
            if (initialVelocity > 0f && acceleration > 0f) return LargeNumber;

            return (-initialVelocity) / acceleration;
        }

        /// <summary>
        /// <b>vₒ * t + ½ * a * t²</b> <br/><br/>
        /// 
        /// v: <paramref name="targetVelocity"/> <br/>
        /// vₒ: <paramref name="initialVelocity"/> <br/>
        /// a: <paramref name="acceleration"/> <br/>
        /// t: <see cref="TimeToReachVelocity(float, float, float)"/> <br/>
        /// </summary>
        public static float DistanceToReachVelocity(float initialVelocity, float targetVelocity, float acceleration)
        {
            float time = TimeToReachVelocity(initialVelocity, targetVelocity, acceleration);

            if (time == 0f) return 0f;

            return DistanceAfterTime(initialVelocity, acceleration, time);
        }

        /// <summary>
        /// <b>vₒ * t + ½ * -a * t²</b> <br/><br/>
        /// 
        /// vₒ: <paramref name="velocity"/> <br/>
        /// a: -<paramref name="braking"/> <br/>
        /// t: <see cref="TimeToStop(float, float)"/> <br/>
        /// </summary>
        public static float DistanceToStop(float velocity, float braking)
        {
            float time = TimeToStop(velocity, -braking);

            if (time == 0f) return 0f;

            return DistanceAfterTime(velocity, -braking, time);
        }

        /// <summary>
        /// <b>(vₒ + v)/2 * t</b> <br/><br/>
        /// 
        /// vₒ: <paramref name="initialVelocity"/> <br/>
        /// vₒ: <paramref name="topVelocity"/> <br/>
        /// t: <paramref name="time"/> <br/>
        /// </summary>
        public static float CalculateDistanceFromSpeed(float initialVelocity, float topVelocity, float time)
        {
            return ((initialVelocity + topVelocity) / 2) * time;
        }

        public static float CalculateTime(float initialVelocity, float topVelocity, float timeToSpeedUp, float distance, float acceleration)
        {
            float distanceTravelledUntilMaxSpeed = DistanceAfterTime(initialVelocity, acceleration, timeToSpeedUp);
            float timeWithMaxVelocity = Velocity.CalculateTime(distance - distanceTravelledUntilMaxSpeed, topVelocity);
            return timeToSpeedUp + timeWithMaxVelocity;
        }

        /// <returns>Aim offset</returns>
        public static Vector CalculateInterceptCourse(Vector targetPosition, Vector targetVelocity, Vector projectilePosition, float projectileVelocity, float projectileAcceleration)
        {
            float distance;
            float time = 0f;

            int iterations = 3;
            for (int i = 0; i < iterations; i++)
            {
                distance = Vector.Distance(projectilePosition, targetPosition + (targetVelocity * time));
                float speedAfterThis = SpeedAfterDistance(projectileVelocity, projectileAcceleration, distance);
                time = TimeToReachVelocity(projectileVelocity, speedAfterThis, projectileAcceleration);
            }

            return targetVelocity * time;
        }

        /// <returns>Aim offset</returns>
        public static Vector CalculateInterceptCourse(Vector targetPosition, Vector targetVelocity, Vector targetAcceleration, Vector projectilePosition, float projectileVelocity, float projectileAcceleration)
        {
            Vector targetOriginalVelocity = targetVelocity;
            float distance;
            float time = 0f;

            int iterations = 4;
            for (int i = 0; i < iterations; i++)
            {
                distance = Vector.Distance(projectilePosition, targetPosition + (targetVelocity * time));
                float speedAfterThis = SpeedAfterDistance(projectileVelocity, projectileAcceleration, distance);
                time = TimeToReachVelocity(projectileVelocity, speedAfterThis, projectileAcceleration);
                targetVelocity = targetOriginalVelocity.Normalized * SpeedAfterTime(targetOriginalVelocity.Magnitude, targetAcceleration.Magnitude, time);
            }

            return targetVelocity * time;
        }

        /// <returns>Aim offset</returns>
        public static Vector CalculateInterceptCourse(Vector targetPosition, Vector targetVelocity, Vector targetAcceleration, Vector projectilePosition, float projectileVelocity)
        {
            Vector targetOriginalVelocity = targetVelocity;
            float distance;
            float time = 0f;

            int iterations = 4;
            for (int i = 0; i < iterations; i++)
            {
                distance = Vector.Distance(projectilePosition, targetPosition + (targetVelocity * time));
                time = Velocity.CalculateTime(distance, projectileVelocity);
                targetVelocity = targetOriginalVelocity.Normalized * SpeedAfterTime(targetOriginalVelocity.Magnitude, targetAcceleration.Magnitude, time);
            }

            return targetVelocity * time;
        }

        public static float? RequiredSpeedToReachDistance(float acceleration, float distance)
        {
            if (acceleration == 0f ) return null;
            if (distance == 0f) return 0f;

            float valueUnderSqr = -(2 * acceleration * distance);

            if (valueUnderSqr < 0f) return null;

            return MathF.Sqrt(valueUnderSqr);
        }
    }

    public static class Velocity
    {
        public static float CalculateTime(Vector pointA, Vector pointB, float speed)
        {
            return CalculateTime(Vector.Distance(pointA, pointB), speed);
        }

        public static float CalculateSpeed(float distance, float time)
        {
            if (time == 0f) return 0f;
            return (distance / time);
        }

        public static float CalculateDistance(float velocity, float time)
        {
            return velocity * time;
        }

        public static float CalculateTime(float distance, float velocity)
        {
            if (velocity == 0f) return 0f;
            return distance / velocity;
        }

        /// <returns>Aim offset</returns>
        public static Vector CalculateInterceptCourse(Vector targetPosition, Vector targetVelocity, Vector projectilePosition, float projectileVelocity)
        {
            float distance;
            float time = 0f;

            int iterations = 3;
            for (int i = 0; i < iterations; i++)
            {
                distance = Vector.Distance(projectilePosition, targetPosition + (targetVelocity * time));
                time = CalculateTime(distance, projectileVelocity);
            }

            return targetVelocity * time;
        }
    }

}
