using System;
using KanbanApi.Models;

namespace KanbanApi.Controllers;

internal static class AttachmentUrlHelper
{
    public static string BuildAttachmentUrl(Attachment attachment)
    {
        var relative = (attachment.RelativePath ?? string.Empty).Replace("\\", "/");
        if (string.IsNullOrWhiteSpace(relative))
        {
            return string.Empty;
        }

        if (!relative.StartsWith("/"))
        {
            relative = $"/{relative}";
        }

        return relative.StartsWith("/uploads", StringComparison.OrdinalIgnoreCase)
            ? relative
            : $"/uploads{relative}";
    }
}
