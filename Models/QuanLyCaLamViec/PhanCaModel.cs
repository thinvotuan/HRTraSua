using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BatDongSan.Models.QuanLyCaLamViec
{
    public class PhanCaModel
    {
        public PhanCaModel()
        {
        }

        public int ID { get; set; }

        public string MaCa { get; set; }

        public string TenCa { get; set; }

        public DateTime? ThoiGianTu { get; set; }

        public DateTime? ThoiGianDen { get; set; }

        public string GhiChu { get; set; }

        public string MaNhanVien { get; set; }

        public string MaNhanVienUpdate { get; set; }

        public string TenNhanVien { get; set; }

        public string TenNhanVienUpdate { get; set; }

        public DateTime? NgayLap { get; set; }

        public int? TrangThai { get; set; }
    }

    public class PhanCaNhanVienModel
    {
        public PhanCaNhanVienModel()
        {
        }

        public string MaNhanVien { get; set; }

        public string HoTenNhanVien { get; set; }

        public string TenPhongBan { get; set; }

        public string TenChucDanh { get; set; }
    }
}