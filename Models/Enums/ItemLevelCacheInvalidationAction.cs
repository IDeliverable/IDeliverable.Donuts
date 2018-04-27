namespace IDeliverable.Donuts.Models.Enums
{
    public enum ItemLevelCacheInvalidationAction
    {
        DoNothing = 0,
        Evict = 1,
        PreRender = 2
    }
}