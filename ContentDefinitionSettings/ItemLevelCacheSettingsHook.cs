using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using IDeliverable.Donuts.Extensions;
using IDeliverable.Donuts.Models;
using IDeliverable.Donuts.Providers;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace IDeliverable.Donuts.ContentDefinitionSettings
{
    public class ItemLevelCacheSettingsHook : ContentDefinitionEditorEventsBase
    {
        public ItemLevelCacheSettingsHook(IEnumerable<ICompositeCacheKeyProvider> compositeCacheKeyProviders, INotifier notifier)
        {
            mCompositeCacheKeyProviders = compositeCacheKeyProviders;
            mNotifier = notifier;

            T = NullLocalizer.Instance;
        }

        private const string mPartName = "ItemLevelCachePart";

        public Localizer T { get; set; }

        private readonly IEnumerable<ICompositeCacheKeyProvider> mCompositeCacheKeyProviders;
        private readonly INotifier mNotifier;

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition)
        {
            if (definition.PartDefinition.Name != mPartName)
                yield break;

            var vm = new ItemLevelCacheContentDefinitionSettingsVm
            {
                Settings = definition.Settings.GetModel<Dictionary<string, ItemLevelCacheSettings>>(Constants.ContentTypeDefinitionSettingsKey),
                CompositeCacheKeyProviders = mCompositeCacheKeyProviders.OrderBy(p => p.Name)
            };

            if (vm.Settings == null || vm.Settings.Count == 0)
            {
                vm.Settings = new Dictionary<string, ItemLevelCacheSettings>()
                {
                    {"Detail", new ItemLevelCacheSettings()}
                };
            }

            var allCompositeCacheKeyProviders = mCompositeCacheKeyProviders.ToDictionary(p => p.TechnicalName, p => false);
            foreach (var key in vm.Settings.Keys)
            {
                vm.Settings[key].CompositeCacheKeyProviders = vm.Settings[key].CompositeCacheKeyProviders.Merge(allCompositeCacheKeyProviders);
            }

            vm.ContentTypeDisplayName = definition.ContentTypeDefinition.DisplayName;

            yield return GetTemplateViewModel(vm);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel)
        {
            if (builder.Name != mPartName)
                yield break;

            var vm = new ItemLevelCacheContentDefinitionSettingsVm();

            if (updateModel.TryUpdateModel(vm, Constants.ContentTypeDefinitionSettingsKey, null, null))
            {
                if (!String.IsNullOrWhiteSpace(vm.NewDisplayType))
                {
                    if (vm.Settings.ContainsKey(vm.NewDisplayType))
                    {
                        mNotifier.Warning(T("The display type {0} has already been configured for {1}.", vm.NewDisplayType, builder.TypeName));

                        yield return GetTemplateViewModel(vm);
                    }

                    vm.Settings[vm.NewDisplayType] = new ItemLevelCacheSettings();
                }

                BuildSettings(vm, builder);
            }

            yield return GetTemplateViewModel(vm);
        }

        private void BuildSettings(ItemLevelCacheContentDefinitionSettingsVm settings, ContentTypePartDefinitionBuilder builder)
        {
            //// TODO: Add the ability to delete settings.
            //var typeDefinition = _contentDefinitionManager.GetTypeDefinition(builder.TypeName);
            //var part = typeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == builder.Name);

            //if (part != null && part.Settings != null) // part will probably never be null, but it's always good to check first just in case
            //{
            //    foreach (var setting in part.Settings.Where(s=>s.Key.StartsWith($"{mModelPrefix}.Settings[")))
            //    {
            //        part.Settings.Remove(setting.Key);
            //    }
            //}

            foreach (var setting in settings.Settings)
            {
                builder.WithSetting($"{Constants.ContentTypeDefinitionSettingsKey}[{setting.Key}].Mode", setting.Value.Mode.ToString());
                builder.WithSetting($"{Constants.ContentTypeDefinitionSettingsKey}[{setting.Key}].InvalidationAction", setting.Value.InvalidationAction.ToString());
                builder.WithSetting($"{Constants.ContentTypeDefinitionSettingsKey}[{setting.Key}].CacheDurationSeconds", setting.Value.CacheDurationSeconds.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting($"{Constants.ContentTypeDefinitionSettingsKey}[{setting.Key}].CacheGraceTimeSeconds", setting.Value.CacheGraceTimeSeconds.ToString(CultureInfo.InvariantCulture));

                foreach (var compositeCacheKeyProvider in setting.Value.CompositeCacheKeyProviders)
                {
                    builder.WithSetting($"{Constants.ContentTypeDefinitionSettingsKey}[{setting.Key}].CompositeCacheKeyProviders[{compositeCacheKeyProvider.Key}]", compositeCacheKeyProvider.Value.ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        private TemplateViewModel GetTemplateViewModel(ItemLevelCacheContentDefinitionSettingsVm vm)
        {
            return DefinitionTemplate(vm, "ItemLevelCacheSettings", Constants.ContentTypeDefinitionSettingsKey);
        }
    }
}