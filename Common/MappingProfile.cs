using AutoMapper;
using bikey.Models;
using bikey.ViewModels;

namespace bikey.Common
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<PhanQuyen, PermissionSetInput>().ReverseMap();

            CreateMap<Xe, ChiTietXeViewModel>()
                .ForMember(dest => dest.LoaiXe, opt => opt.MapFrom(src => src.LoaiXe != null ? src.LoaiXe.TenLoaiXe : "Xe máy"))
                .ForMember(dest => dest.Slug, opt => opt.Ignore())
                .ForMember(dest => dest.MoTa, opt => opt.Ignore())
                .ForMember(dest => dest.HinhAnh, opt => opt.Ignore());

            CreateMap<Xe, DatXeViewModel>()
                .ForMember(dest => dest.MaXe, opt => opt.MapFrom(src => src.MaXe))
                .ForMember(dest => dest.TenXe, opt => opt.MapFrom(src => src.TenXe))
                .ForMember(dest => dest.BienSoXe, opt => opt.MapFrom(src => src.BienSoXe))
                .ForMember(dest => dest.HangXe, opt => opt.MapFrom(src => src.HangXe))
                .ForMember(dest => dest.DongXe, opt => opt.MapFrom(src => src.DongXe))
                .ForMember(dest => dest.TrangThaiXe, opt => opt.MapFrom(src => src.TrangThai))
                .ForMember(dest => dest.GiaThueNgay, opt => opt.MapFrom(src => src.GiaThue))
                .ForMember(dest => dest.GiaTriXe, opt => opt.MapFrom(src => src.GiaTriXe))
                .ForMember(dest => dest.TenLoaiXe, opt => opt.MapFrom(src => src.LoaiXe != null ? src.LoaiXe.TenLoaiXe : "—"))
                .ForMember(dest => dest.HinhAnhXe, opt => opt.MapFrom(src => StringHelpers.NormalizeImageUrl(src.HinhAnhHienThi)))
                .ForMember(dest => dest.HoTenKhachHang, opt => opt.Ignore())
                .ForMember(dest => dest.SoDienThoai, opt => opt.Ignore())
                .ForMember(dest => dest.DiaChi, opt => opt.Ignore())
                .ForMember(dest => dest.CanCuoc, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.NgayNhanDuKien, opt => opt.Ignore())
                .ForMember(dest => dest.NgayTraDuKien, opt => opt.Ignore())
                .ForMember(dest => dest.GhiChu, opt => opt.Ignore())
                .ForMember(dest => dest.TongTienDuKien, opt => opt.Ignore());
        }
    }
}
