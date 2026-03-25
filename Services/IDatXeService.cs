using bikey.Models;
using bikey.ViewModels;

namespace bikey.Services
{
    public interface IDatXeService
    {
        Task<Xe?> GetXeForBookingAsync(int maXe);
        DatXeViewModel BuildDatXeViewModel(Xe xe, System.Security.Claims.ClaimsPrincipal user);
        void PopulateVehicleInfo(DatXeViewModel model, Xe xe);
        Task<DatCho> CreateDatChoAsync(DatCho datCho);
        Task<DatCho?> GetDatChoByIdAsync(int id);
    }
}
