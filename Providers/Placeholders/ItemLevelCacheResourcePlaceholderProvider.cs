using System;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;

namespace IDeliverable.Donuts.Providers.Placeholders
{
    [OrchardFeature("IDeliverable.Donuts.Resources")]
    public class ItemLevelCacheResourcePlaceholderProvider: IPlaceholderProvider
    {
        private readonly IShapeFactory mShapeFactory;
        private readonly IShapeDisplay mShapeDisplay;

        public ItemLevelCacheResourcePlaceholderProvider(IShapeFactory shapeFactory, IShapeDisplay shapeDisplay)
        {
            mShapeFactory = shapeFactory;
            mShapeDisplay = shapeDisplay;
        }

        public string ResolvePlaceholder(string placeholderText)
        {
            var placeholderParts = placeholderText.Split(new [] { "::" }, StringSplitOptions.RemoveEmptyEntries);

            if (placeholderParts[0] != "ItemLevelCacheResource" || placeholderParts.Length != 2)
                return null;

            switch (placeholderParts[1])
            {
                case "Metas":
                    return GetMarkup(mShapeFactory.Create("Metas"));
                case "HeadLinks":
                    return GetMarkup(mShapeFactory.Create("HeadLinks"));
                case "StylesheetLinks":
                    return GetMarkup(mShapeFactory.Create("StylesheetLinks"));
                case "HeadScripts":
                    return GetMarkup(mShapeFactory.Create("HeadScripts"));
                case "FootScripts":
                    return GetMarkup(mShapeFactory.Create("FootScripts"));
            }

            return null;
        }

        private string GetMarkup(dynamic shape)
        {
            return (string) mShapeDisplay.Display(shape);
        }
    }
}