using System.Collections.Generic;
using bikey.Models;

namespace bikey.ViewModels
{
    public enum XeTableVariant
    {
        QuanLy,
        DaXoa
    }

    public class XeTablePartialModel
    {
        public IReadOnlyList<Xe> Items { get; set; } = new List<Xe>();

        public XeTableVariant Variant { get; set; } = XeTableVariant.QuanLy;

        /// <summary>
        /// Nội dung khi không có dòng nào; nếu null sẽ dùng mặc định theo <see cref="Variant"/>.
        /// </summary>
        public string? EmptyMessage { get; set; }
    }
}
