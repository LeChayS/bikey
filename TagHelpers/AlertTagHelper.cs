using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace bikey.TagHelpers;

[HtmlTargetElement("alert-box")]
public class AlertTagHelper : TagHelper
{
    [HtmlAttributeName("type")]
    public string Type { get; set; } = "info";

    [HtmlAttributeName("title")]
    public string? Title { get; set; }

    [HtmlAttributeName("closable")]
    public bool Closable { get; set; } = true;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var content = (await output.GetChildContentAsync()).GetContent()?.Trim();

        var alertType = Type?.ToLowerInvariant() switch
        {
            "success" => "success",
            "warning" => "warning",
            "danger" => "danger",
            _ => "info"
        };

        var icon = alertType switch
        {
            "success" => "bi-check-circle-fill",
            "warning" => "bi-exclamation-triangle-fill",
            "danger" => "bi-x-circle-fill",
            _ => "bi-info-circle-fill"
        };

        // ROOT
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", $"alert alert-{alertType} custom-alert-horizontal");
        output.Attributes.SetAttribute("role", "alert");

        // ICON
        var iconTag = new TagBuilder("i");
        iconTag.AddCssClass("bi");
        iconTag.AddCssClass(icon);
        iconTag.AddCssClass("me-2");

        // CONTENT WRAPPER
        var contentWrapper = new TagBuilder("div");
        contentWrapper.AddCssClass("flex-grow-1");

        // TITLE
        if (!string.IsNullOrWhiteSpace(Title))
        {
            var titleTag = new TagBuilder("div");
            titleTag.AddCssClass("alert-title");
            titleTag.InnerHtml.Append(Title);
            contentWrapper.InnerHtml.AppendHtml(titleTag);
        }

        // BODY
        if (!string.IsNullOrWhiteSpace(content))
        {
            var bodyTag = new TagBuilder("div");
            bodyTag.AddCssClass("alert-content");
            bodyTag.InnerHtml.AppendHtml(content);
            contentWrapper.InnerHtml.AppendHtml(bodyTag);
        }

        output.Content.AppendHtml(iconTag);
        output.Content.AppendHtml(contentWrapper);

        // CLOSE BUTTON
        if (Closable)
        {
            var button = new TagBuilder("button");
            button.AddCssClass("btn-close ms-2");
            button.Attributes.Add("type", "button");
            button.Attributes.Add("data-bs-dismiss", "alert");
            button.Attributes.Add("aria-label", "Close");

            output.Content.AppendHtml(button);
        }
    }
}