using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace bikey.TagHelpers;

[HtmlTargetElement("stat-card")]
public class StatCardTagHelper : TagHelper
{
    [HtmlAttributeName("label")]
    public string Label { get; set; } = string.Empty;

    [HtmlAttributeName("value")]
    public string Value { get; set; } = string.Empty;

    [HtmlAttributeName("icon")]
    public string Icon { get; set; } = string.Empty;

    [HtmlAttributeName("tone")]
    public string Tone { get; set; } = "blue";

    [HtmlAttributeName("label-id")]
    public string? LabelId { get; set; }

    [HtmlAttributeName("value-id")]
    public string? ValueId { get; set; }

    [HtmlAttributeName("value-class")]
    public string? ValueClass { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var childContent = await output.GetChildContentAsync();

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", $"stat-card {Tone}");

        var contentWrap = new TagBuilder("div");

        var labelEl = new TagBuilder("div");
        labelEl.AddCssClass("stat-label");
        if (!string.IsNullOrWhiteSpace(LabelId))
        {
            labelEl.Attributes["id"] = LabelId;
        }

        labelEl.InnerHtml.Append(Label);

        var valueEl = new TagBuilder("div");
        valueEl.AddCssClass("stat-value");
        if (!string.IsNullOrWhiteSpace(ValueId))
        {
            valueEl.Attributes["id"] = ValueId;
        }

        if (!string.IsNullOrWhiteSpace(ValueClass))
        {
            valueEl.AddCssClass(ValueClass);
        }

        valueEl.InnerHtml.Append(Value);

        contentWrap.InnerHtml.AppendHtml(labelEl);
        contentWrap.InnerHtml.AppendHtml(valueEl);

        if (!childContent.IsEmptyOrWhiteSpace)
        {
            contentWrap.InnerHtml.AppendHtml(childContent);
        }

        var iconWrap = new TagBuilder("div");
        iconWrap.AddCssClass("stat-icon");
        iconWrap.AddCssClass(Tone);
        iconWrap.AddCssClass("text-white");

        var iconEl = new TagBuilder("i");
        iconEl.AddCssClass("bi");
        iconEl.AddCssClass(Icon);
        iconWrap.InnerHtml.AppendHtml(iconEl);

        output.Content.AppendHtml(contentWrap);
        output.Content.AppendHtml(iconWrap);
    }
}
