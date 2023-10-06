namespace ConsoleGame
{
    public class ItemBehavior : Component
    {
        public enum ItemKind
        {
            Health,
        }

        public ItemKind Kind;
        public float Amount;

        public ItemBehavior(Entity entity) : base(entity)
        {
            Entity.Tags |= Tags.Item;
        }

        public void PickUp(ICanPickUpItem by)
        {
            if (IsDestroyed) return;

            by.OnItemPickedUp(Kind, Amount);
            IsDestroyed = true;
        }
    }

    public interface ICanPickUpItem
    {
        public void OnItemPickedUp(ItemBehavior.ItemKind kind, float amount);
    }
}
