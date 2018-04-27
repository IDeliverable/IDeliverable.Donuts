///<reference path="./Typings/jquery.d.ts"/>

$(() =>
{
    $("ul.itemlevelcache-displaytypes li")
        .on("click", (e) =>
        {
            $(e.currentTarget).siblings().removeClass("selected");
            var href = $(e.currentTarget).addClass("selected").find("a").attr("href");

            $(".itemlevelcache-displaytype-settings").hide();
            $(href).show();

            return false;
        })
        .first().click();

    $("input.itemlevelcache-mode-picker")
        .on("change", e =>
        {
            if ($(e.currentTarget).is(":checked"))
            {
                var displayType = $(e.currentTarget).data("for-display-type");
                $(".display-on-cacheitem-" + displayType).toggle($(e.currentTarget).val() === "CacheItem");
            }
        })
        .trigger("change");

    $("input.on-change-mode-picker")
        .on("change", e =>
        {
            if ($(e.currentTarget).is(":checked"))
            {
                var displayType = $(e.currentTarget).data("for-display-type");
                $(".hide-on-prerender-" + displayType).toggle($(e.currentTarget).val() !== "PreRender");
            }
        })
        .trigger("change");

    $("input.inherit-checkbox")
        .on("change", (e) =>
        {
            var elementsToSetChecked = $("[data-default-checked]");
            var elementsToSetValue = $("[data-default-value]");

            if ($(e.currentTarget).is(":checked"))
            {
                elementsToSetChecked.prop("disabled", true);
                elementsToSetValue.prop("disabled", true);

                elementsToSetChecked.each(function ()
                {
                    $(this).prop("checked", ($(this).data("default-checked") === "True")).trigger("change");
                });

                elementsToSetValue.each(function ()
                {
                    $(this).val($(this).data("default-value")).trigger("change");
                });
            }
            else
            {
                elementsToSetChecked.prop("disabled", false);
                elementsToSetValue.prop("disabled", false);
            }
        })
        .trigger("change");
});