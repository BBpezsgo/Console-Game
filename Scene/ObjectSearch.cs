namespace ConsoleGame
{
    public partial class Scene
    {
        public Entity[] ObjectsOfTag(int tags)
        {
            List<Entity> result = new();
            for (int i = Entities.Count - 1; i >= 0; i--)
            {
                Entity obj = Entities[i];
                if (obj.IsDestroyed) continue;
                if ((obj.Tags & tags) == 0) continue;
                result.Add(obj);
            }
            return result.ToArray();
        }

        public Entity? FirstObjectAt(Vector position, float distanceThreshold)
        {
            for (int i = Entities.Count - 1; i >= 0; i--)
            {
                Entity obj = Entities[i];
                if (obj.IsDestroyed) continue;
                Vector diff = Entities[i].Position - position;
                float diffSqrMag = diff.SqrMagnitude;
                if (diffSqrMag < distanceThreshold * distanceThreshold)
                {
                    return obj;
                }
            }
            return null;
        }
        public Entity? FirstObjectAt(Vector position, int tags, float distanceThreshold)
        {
            float sqrDistanceThreshold = distanceThreshold * distanceThreshold;

            for (int i = Entities.Count - 1; i >= 0; i--)
            {
                Entity obj = Entities[i];
                if (obj.IsDestroyed) continue;
                if ((obj.Tags & tags) == 0) continue;

                float diffSqrMag = (Entities[i].Position - position).SqrMagnitude;

                if (diffSqrMag < sqrDistanceThreshold)
                { return obj; }
            }
            return null;
        }

        public Entity? ClosestObject(Vector position, float radius)
        {
            Entity? result = null;
            float closestSqrDistance = float.PositiveInfinity;
            for (int i = Entities.Count - 1; i >= 0; i--)
            {
                Entity obj = Entities[i];
                if (obj.IsDestroyed) continue;
                Vector diff = Entities[i].Position - position;
                float sqrDistance = diff.SqrMagnitude;
                if (sqrDistance >= radius * radius) continue;

                if (sqrDistance < closestSqrDistance)
                {
                    result = obj;
                    closestSqrDistance = sqrDistance;
                }
            }
            return result;
        }
        public Entity? ClosestObject(Vector position, int tags, float radius)
        {
            float sqrRadius = radius * radius;

            Entity? result = null;
            float closestSqrDistance = float.PositiveInfinity;

            for (int i = Entities.Count - 1; i >= 0; i--)
            {
                Entity obj = Entities[i];
                if (obj.IsDestroyed) continue;
                if ((obj.Tags & tags) == 0) continue;

                float sqrDistance = (Entities[i].Position - position).SqrMagnitude;
                if (sqrDistance >= sqrRadius) continue;

                if (sqrDistance < closestSqrDistance)
                {
                    result = obj;
                    closestSqrDistance = sqrDistance;
                }
            }
            return result;
        }

        public Entity? ClosestObject(Vector position)
        {
            Entity? result = null;
            float closestSqrDistance = float.PositiveInfinity;
            for (int i = Entities.Count - 1; i >= 0; i--)
            {
                Entity obj = Entities[i];
                if (obj.IsDestroyed) continue;
                float sqrDistance = (Entities[i].Position - position).SqrMagnitude;

                if (sqrDistance < closestSqrDistance)
                {
                    result = obj;
                    closestSqrDistance = sqrDistance;
                }
            }
            return result;
        }
        public Entity? ClosestObject(Vector position, int tags)
        {
            Entity? result = null;
            float closestSqrDistance = float.PositiveInfinity;
            for (int i = Entities.Count - 1; i >= 0; i--)
            {
                Entity obj = Entities[i];
                if (obj.IsDestroyed) continue;
                if ((obj.Tags & tags) == 0) continue;

                float sqrDistance = (Entities[i].Position - position).SqrMagnitude;
                if (sqrDistance < closestSqrDistance)
                {
                    result = obj;
                    closestSqrDistance = sqrDistance;
                }
            }
            return result;
        }

        public Entity[] ObjectsAt(Vector position, float radius)
        {
            List<Entity> result = new();
            for (int i = Entities.Count - 1; i >= 0; i--)
            {
                Entity obj = Entities[i];
                if (obj.IsDestroyed) continue;
                Vector diff = Entities[i].Position - position;
                float diffSqrMag = diff.SqrMagnitude;
                if (diffSqrMag < radius * radius)
                {
                    result.Add(obj);
                }
            }
            return result.ToArray();
        }
        public Entity[] ObjectsAt(Vector position, int tags, float radius)
        {
            List<Entity> result = new();
            for (int i = Entities.Count - 1; i >= 0; i--)
            {
                Entity obj = Entities[i];
                if (obj.IsDestroyed) continue;
                if ((obj.Tags & tags) == 0) continue;
                Vector diff = Entities[i].Position - position;
                float diffSqrMag = diff.SqrMagnitude;
                if (diffSqrMag < radius * radius)
                {
                    result.Add(obj);
                }
            }
            return result.ToArray();
        }
    }
}
