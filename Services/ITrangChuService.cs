using bikey.ViewModels;

namespace bikey.Services
{
    public interface ITrangChuService
    {
        Task<TrangChuViewModel> BuildTrangChuViewModelAsync();
        Task<ChiTietXeViewModel?> GetChiTietXeAsync(string slug);
    }
}
