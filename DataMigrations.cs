using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace IDeliverable.Donuts
{
    public class DataMigrations : DataMigrationImpl
    {
        public int Create()
        {
            ContentDefinitionManager.AlterPartDefinition("ItemLevelCachePart", cfg => cfg
                .Attachable()
                .WithDescription("Provides functionality and configuration for item-level output caching (e.g. donut caching and donut hole caching)."));

            return 1;
        }
    }
}