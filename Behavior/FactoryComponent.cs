namespace ConsoleGame.Behavior;

public abstract class FactoryComponent<T> : NetworkComponent
{
    public readonly List<T> ProducibleThings = new();
    public readonly Queue<Producible<T>> Queue = new();

    public class Producible<TProduct>
    {
        public readonly TProduct Product;
        public float StartedTime;
        public readonly float RequiredTime;

        public bool IsDone => (StartedTime != 0f) && (Time.Now - StartedTime >= RequiredTime);

        public Producible(TProduct product, float requiredTime)
        {
            Product = product;
            StartedTime = 0f;
            RequiredTime = requiredTime;
        }
    }

    protected FactoryComponent(Entity entity) : base(entity) { }

    public override void Update()
    {
        if (Game.NetworkMode == NetworkMode.Client) return;
        if (!Queue.TryPeek(out Producible<T>? producible)) return;

        if (producible.IsDone)
        {
            OnProductDone(producible.Product);
            Queue.Dequeue();

            if (Queue.TryPeek(out Producible<T>? next))
            { next.StartedTime = Time.Now; }
        }
    }

    public void Enqueue(T product, float requiredTime)
    {
        Producible<T> producible = new(product, requiredTime);
        if (Queue.Count == 0)
        { producible.StartedTime = Time.Now; }
        Queue.Enqueue(producible);
    }

    protected abstract void OnProductDone(T product);
}
