using System;
using Orchard;

namespace IDeliverable.Donuts.Providers.Placeholders
{
    public class UserPlaceholderProvider : IPlaceholderProvider
    {
        public UserPlaceholderProvider(IOrchardServices orchardServices)
        {
            mOrchardServices = orchardServices;
        }

        private readonly IOrchardServices mOrchardServices;

        public string ResolvePlaceholder(string placeholderText)
        {
            var currentUser = mOrchardServices.WorkContext.CurrentUser;
            if (currentUser == null)
                return null;

            if (String.Equals(placeholderText, "Username", StringComparison.InvariantCultureIgnoreCase))
                return currentUser.UserName;

            if (String.Equals(placeholderText, "EmailAddress", StringComparison.InvariantCultureIgnoreCase))
                return currentUser.Email;

            if (String.Equals(placeholderText, "UserId", StringComparison.InvariantCultureIgnoreCase))
                return currentUser.Id.ToString();

            return null;
        }
    }
}