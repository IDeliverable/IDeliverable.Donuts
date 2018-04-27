# IDeliverable.Donuts
A module for the Orchard CMS that can dramatically improve the performance of websites by using **different caching strategies for different sections of the rendered page**. Exclude specific sections from output cache (i.e. **donut caching**), output cache only specific sections (i.e. **donut hole caching**), or cache both but with different durations or other cache control parameters. Cache control can be configured on the **content type and/or content item levels**, and **per display type** (e.g. summary vs. detail) using easy to use configuration tools.

The built-in output caching in Orchard is a powerful but blunt instrument. It allows you to configure caching policy only globally, with a small subset of options also configurable per route. Since the vast majority of requests on any website execute the same route (namely `/Contents/Item/Display`) in practice the built-in output caching forces you to create one caching policy for everything.

However, as site owners and developers quickly realize when building more advanced Orchard websites, not every piece of markup your website serves is subject to the same caching needs. Output caching every page in its entirety only really works well when your website has no personalized or frequently changing content.

**IDeliverable.Donuts** extends Orchard's output caching to allow you to configure output caching on a much more granular level, by creating entirely different caching policies for different sections and areas on your page. **IDeliverable.Donuts** lets you easily implement caching strategies like *donut caching* and *donut hole caching* (hence the name) by letting you pick and choose which areas of your website are output cached, and which ones are not.

## Features

### Donut (hole) caching

_Donut caching_ is a caching strategy in which most of the page is output cached but certain sections which are inappropriate to cache (such as highly personalized or highly transient widgets) are excluded from the page-level output cache. _Donut hole caching_ is the opposite strategy, in which most of the page is *not* output cached but certain sections which are necessary to cache (such as highly expensive or highly static widgets) are output cached. **IDeliverable.Donuts** delivers these concepts to Orchard.

### Granular cache control

**IDeliverable.Donuts** supports the same cache control options as the built-in output cache, but allows you to configure them on a much more granular level. Caching policy is specified per display type (such as `Detail` or `Summary`) and can be configured on each content type separately, and even optionally overridden on individual content items.

### Pre-rendering

**IDeliverable.Donuts** also supports the concept of proactively regenerating individual content items in the background whenever they are changed, while continuing to serve the old version of the content item from output cache until the new version has finished pre-rendering. No more cache misses due to content edits, and no more making your users wait around while their request regenerates the evicted content item.

### Super-fast projections

Because caching policy is specified per display type, **IDeliverable.Donuts** lets you take advantage of output caching in projections by configuring one caching policy for the `Summary` display type (of the content items rendered in the projection) and one (possibly different) policy for the projection or projection widget itself, allowing you to create extremely well-performing projections even in cases where items are frequently added.

### Placeholders

Placeholders are a set of well-known variables that can be placed into content when editing a content item. When a page is requested by a user and the content item is rendered by Orchard, the placeholders will be replaced with the actual value given the context of the request. Placeholders are very similar to _tokens_ in Orchard, except placeholders **play nice with output caching** because they are processed and substituted **after** the output cache filter in the request lifecycle. For example, you can put the `username` placeholder into the header of all your pages, and the correct username will be shown to the correct user **even if the page is output cached**.

### Based on Orchard.OutputCache

**IDeliverable.Donuts** uses the existing output cache framework to provide an experience that you'll already be familiar with, and it relies on the built-in output cache's configured storage pipeline, whether it's in-memory storage, SQL Server, Redis, file system, or anything else including custom storage providers you may have written, so there's no need to configure any additional cache storage.

### Easy to use

**IDeliverable.Donuts** fits naturally into your existing website, and you don't need to factor in the functionality when designing new websites. Caching policy can be configured on any content type or content item (including widgets) by simply attaching the **Item-Level Cache** part to the content type. The part configuration UI has been carefully designed to be consistent with the built-in output cache feature, and will feel instantly familiar to Orchard site owners and developers.

### Extensible

Orchard is all about extensibility, and so are our Orchard modules. Developers can extend the functionality of the **IDeliverable.Donuts** module by developing their own custom placeholders, and by creating custom composite cache key providers to vary the output caching by parameters and dimensions other than those supported out of the box.

## Use Cases

There are many use cases for **IDeliverable.Donuts**. Here are just a few:

### Cache personalized content

On websites with wide-spread personalized content, it can be difficult to use output caching at all. For example, if your website includes some piece of dynamic personalized content (such as the user's name) shown on a majority of pages, output caching is effectively impossible.

**IDeliverable.Donuts** can solve this problem in two ways:

- If your personalized content is a widget, you can apply a caching policy to that individual widget to either vary it by username or exclude it from output caching completely.
- Or you can use _placeholders_ to embed user-specific values into any piece of content. For example, you can safely embed a placeholder representing the current user's name into any output cached content, and **IDeliverable.Donuts** will make sure that the correct name is shown to the correct user. What's more, placeholders are extensible. You can create your own custom placeholders and use them in all the same places where you can use the default placeholders.

### Prevent entire-site cache eviction

Evicting pages from the output cache can be a costly exercise, especially when **all** pages on your website suddenly need to be evicted under heavy user load. Yet this scenario is more common than you might think, such as when changing a menu, header or footer that appears on every page of your website.

**IDeliverable.Donuts** can stop this from happening by caching those global widgets separately from each page's cached markup. Whenever you change a donut-hole-cached widget, only that widget's cache item is evicted, while all the pages on which that widget appears are left in the cache.

### Speed up search results and lists

One of the biggest and most common causes for slow page rendering in Orchard websites is the cost of rendering each content item in `Summary` view when displaying a list of content items.

With **IDeliverable.Donuts** you can configure each display type on a content item individually. This means you can output cache each content item's `Summary` markup, so that no matter which list, projection, or search query they end up being display on, the markup always comes from the cache.

### Speed up critical user flows

Sometimes certain sections of your website are just not appropriate for output caching. For example, your website may have a checkout process that simply cannot be output cached because it contains complex, transaction specific data.

You can use **IDeliverable.Donuts** in cases like this to configure donut caching and exclude the transaction specific content, allowing you to output cache the rest of the page.

### Cache pages for longer

When using the built-in output caching in Orchard, pages can typically only be cached for as long as **all** the content on the page is still valid. For example, if your website has an offer on the home page that is only valid for the next hour, you would have to evict the entire home page once that offer expires.

With **IDeliverable.Donuts**, each content item on the page can have its own cache duration, so you can cache that home page for a week or more, safe in the knowledge that your offer widget will expire and be evicted from output cache in an hour.

## Getting Started

### Installation

To install the module from Orchard Gallery:

1. In the Orchard admin UI, navigate to **Modules -> Gallery**.
1. Search for "IDeliverable.Donuts".
1. Click **Install** to install the module.

To install the module using the ZIP file:

1. Download the [module ZIP file](https://github.com/IDeliverable/IDeliverable.Donuts/archive/master.zip).
1. Unzip the contents into the `Modules` folder of your Orchard installation (this creates an `IDeliverable.Donuts` subfolder).

To integrate the module into your development workflow, unzip the contents into the `Orchard.Web\Modules` folder of your local repository and add it to source control (if any).

### Basic configuration

1. Enable the feature **Item-Level Output Cache**.
1. Attach the **Item-Level Cache** content part to one or more content types. You can now configure different caching policies when editing the content types themselves (using the **Edit Content Type Definition** page) or when editing individual content items of those types.

### System requirements and compatibility
		
**IDeliverable.Donuts** is compatibility-tested and supported on **Orchard version 1.10.x**. The module might also work on older or newer versions of Orchard but this is not guaranteed.

We make a commitment that the current release of our modules should always work with the current minor release of Orchard (e.g. 1.10) and across all subsequent revision releases (e.g. 1.10.1, 1.10.2 and so on). We strive to always conduct compatibility testing (and release an updated module if necessary) within two weeks of every new Orchard release.

The module provides the following features with their respective dependencies:

- *Item-Level Output Cache* (`IDeliverable.Donuts`) depends on `Orchard.OutputCache`.

## Documentation
If you want to learn more about how to extend IDeliverable.Donuts with more functionality (such as new placeholders, or custom caching logic) as a developer, please check out our [Developer Guide](http://www.ideliverable.com/documentation/ideliverable-donuts/developer-guide).