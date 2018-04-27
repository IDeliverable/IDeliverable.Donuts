using System.Collections.Generic;
using Autofac.Features.Metadata;
using IDeliverable.Donuts.Events;
using Orchard.Environment.Extensions;
using Orchard.UI.Resources;

namespace IDeliverable.Donuts.Services
{
    [OrchardFeature("IDeliverable.Donuts.Resources")]
    [OrchardSuppressDependency("Orchard.UI.Resources.ResourceManager")]
    public class EventRaisingResourceManager : ResourceManager
    {
        private bool SuppressRequireEvents { get; set; }

        public EventRaisingResourceManager(IEnumerable<Meta<IResourceManifestProvider>> resourceProviders, IResourceManagerEvents resourceManagerEvents)
            : base(resourceProviders)
        {
            mResourceManagerEvents = resourceManagerEvents;
        }

        private readonly IResourceManagerEvents mResourceManagerEvents;

        public override void RegisterFootScript(string script)
        {
            base.RegisterFootScript(script);
            mResourceManagerEvents.FootScriptRegistered(script);
        }

        public override void RegisterHeadScript(string script)
        {
            base.RegisterHeadScript(script);
            mResourceManagerEvents.HeadScriptRegistered(script);
        }

        public override RequireSettings Require(string resourceType, string resourceName)
        {
            // Include calls require under the hood, so we will end up with dupicate events in that case.
            if (!SuppressRequireEvents)
            {
                mResourceManagerEvents.ResourceRequired(resourceType, resourceName);
            }

            return base.Require(resourceType, resourceName);
        }

        public override RequireSettings Include(string resourceType, string resourcePath, string resourceDebugPath, string relativeFromPath)
        {
            SuppressRequireEvents = true;
            RequireSettings result;

            try
            {
                result = base.Include(resourceType, resourcePath, resourceDebugPath, relativeFromPath);
            }
            finally
            {
                SuppressRequireEvents = false;
            }

            mResourceManagerEvents.ResourceIncluded(resourceType, resourcePath, resourceDebugPath, relativeFromPath);

            return result;
        }
    }
}