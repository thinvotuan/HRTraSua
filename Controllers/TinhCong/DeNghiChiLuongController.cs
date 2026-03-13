using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BatDongSan.Helper.Common;
using BatDongSan.Utils.Paging;
using BatDongSan.Helper.Utils;
using System.Data;
using System.Globalization;
using System.Text;
using System.IO;
using BatDongSan.Models.NhanSu;
using System.Configuration;
using System.Net.Mail;
using BatDongSan.Models.HeThong;

namespace BatDongSan.Controllers.TinhCong
{
    public class DeNghiChiLuongController : ApplicationController
    {
        //
        // GET: /DeNghiChiLuong/
        LinqNhanSuDataContext _dataContext = new LinqNhanSuDataContext();
        LinqHeThongDataContext ht = new LinqHeThongDataContext();
        BatDongSan.Models.NhanSu.LinqThuanViet.LinqThuanVietDataContext lqThuanViet = new BatDongSan.Models.NhanSu.LinqThuanViet.LinqThuanVietDataContext();
        BatDongSan.Models.NhanSu.LinqCT1330.LinqCT1330DataContext lqCT1330 = new BatDongSan.Models.NhanSu.LinqCT1330.LinqCT1330DataContext();
        BatDongSan.Models.NhanSu.LinqCT2220.LinqCT2220DataContext lqCT2220 = new BatDongSan.Models.NhanSu.LinqCT2220.LinqCT2220DataContext();
        BatDongSan.Models.NhanSu.LinqERPNewCity.LinqERPNewCityDataContext lqCTNewCity = new Models.NhanSu.LinqERPNewCity.LinqERPNewCityDataContext();
        
        private readonly string MCV = "DENGHICHILUONG";
        private bool? permission;
        public DeNghiChiLuong deNghiCL;
        public List<DeNghiChiLuongChiTiet> ListChiTietDeNghi;
        List<DeNghiChiLuong> ListDeNghi = new List<DeNghiChiLuong>();
        public ActionResult Index(int? nam, string qSearch)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            try
            {
                nam = nam ?? DateTime.Now.Year;
                var listDN = lqThuanViet.tbl_DeNghiChiLuongs.Where(d => d.nam == nam);
                var List1330 = lqCT1330.tbl_DeNghiChiLuongs.Where(d => d.nam == nam);
                var List2220 = lqCT2220.tbl_DeNghiChiLuongs.Where(d => d.nam == nam);
                var ListNewCity = lqCTNewCity.tbl_DeNghiChiLuongs.Where(d => d.nam == nam);
                
                if (!string.IsNullOrEmpty(qSearch))
                {
                    listDN = listDN.Where(d => d.maPhieu.Contains(qSearch));
                    List1330 = List1330.Where(d => d.maPhieu.Contains(qSearch));
                    List2220 = List2220.Where(d => d.maPhieu.Contains(qSearch));
                    ListNewCity = ListNewCity.Where(d => d.maPhieu.Contains(qSearch));
                }
                //var listCuoi1 = (from p1 in listDN
                //                select new DeNghiChiLuong {
                //                    maCongTrinh = "CTTV",
                //                    maNguoiLap = p1.nguoiLap,
                //                    nam = p1.nam,
                //                    ngayLap = Convert.ToDateTime(p1.ngayLap),
                //                    noiDung = p1.noiDung,
                //                    soPhieu = p1.maPhieu,
                //                    tenNguoiLap = p1.tenNguoiLap,
                //                    thang = p1.thang,
                //                    trangThai = (int?)lqThuanViet.HT_DMNguoiDuyets.OrderByDescending(d => d.ID).Where(d => d.maPhieu == p1.maPhieu).Select(d => d.trangThai).FirstOrDefault() ?? 0,
                //                    tenBuocDuyet = lqThuanViet.tbl_QuiTrinhDuyet_BuocDuyets.Where(d => d.Id == ((int?)lqThuanViet.HT_DMNguoiDuyets.OrderByDescending(c => c.ID).Where(c => c.maPhieu == p1.maPhieu).Select(c => c.trangThai).FirstOrDefault() ?? 0)).Select(d => d.TenBuocDuyet).FirstOrDefault() ?? "Tạo mới",
                //                    tongTienThucChuyen = lqThuanViet.tbl_DeNghiChiLuongChiTiets.Where(d => d.maPhieu == p1.maPhieu).Sum(d => d.chuyenKhoan) ?? 0
                //                }).OrderByDescending(d=>d.thang).ToList();
                //ListDeNghi.AddRange(listCuoi1);
                //var listCuoi2 = (from p1 in List1330
                //                 select new DeNghiChiLuong
                //                 {
                //                     maCongTrinh = "CT1330",
                //                     maNguoiLap = p1.nguoiLap,
                //                     nam = p1.nam,
                //                     ngayLap = Convert.ToDateTime(p1.ngayLap),
                //                     noiDung = p1.noiDung,
                //                     soPhieu = p1.maPhieu,
                //                     tenNguoiLap = p1.tenNguoiLap,
                //                     thang = p1.thang,
                //                     trangThai = (int?)lqCT1330.HT_DMNguoiDuyets.OrderByDescending(d => d.ID).Where(d => d.maPhieu == p1.maPhieu).Select(d => d.trangThai).FirstOrDefault() ?? 0,
                //                     tenBuocDuyet = lqCT1330.tbl_QuiTrinhDuyet_BuocDuyets.Where(d => d.Id == ((int?)lqCT1330.HT_DMNguoiDuyets.OrderByDescending(c => c.ID).Where(c => c.maPhieu == p1.maPhieu).Select(c => c.trangThai).FirstOrDefault() ?? 0)).Select(d => d.TenBuocDuyet).FirstOrDefault() ?? "Tạo mới",
                //                     tongTienThucChuyen = lqCT1330.tbl_DeNghiChiLuongChiTiets.Where(d => d.maPhieu == p1.maPhieu).Sum(d => d.chuyenKhoan) ?? 0
                //                 }).OrderByDescending(d => d.thang).ToList();
                //ListDeNghi.AddRange(listCuoi2);

                //var listCuoi3 = (from p1 in List2220
                //                 select new DeNghiChiLuong
                //                 {
                //                     maCongTrinh = "CT2220",
                //                     maNguoiLap = p1.nguoiLap,
                //                     nam = p1.nam,
                //                     ngayLap = Convert.ToDateTime(p1.ngayLap),
                //                     noiDung = p1.noiDung,
                //                     soPhieu = p1.maPhieu,
                //                     tenNguoiLap = p1.tenNguoiLap,
                //                     thang = p1.thang,
                //                     trangThai = (int?)lqCT2220.HT_DMNguoiDuyets.OrderByDescending(d => d.ID).Where(d => d.maPhieu == p1.maPhieu).Select(d => d.trangThai).FirstOrDefault() ?? 0,
                //                     tenBuocDuyet = lqCT2220.tbl_QuiTrinhDuyet_BuocDuyets.Where(d => d.Id == ((int?)lqCT2220.HT_DMNguoiDuyets.OrderByDescending(c => c.ID).Where(c => c.maPhieu == p1.maPhieu).Select(c => c.trangThai).FirstOrDefault() ?? 0)).Select(d => d.TenBuocDuyet).FirstOrDefault() ?? "Tạo mới",
                //                     tongTienThucChuyen = lqCT2220.tbl_DeNghiChiLuongChiTiets.Where(d => d.maPhieu == p1.maPhieu).Sum(d => d.chuyenKhoan) ?? 0
                //                 }).OrderByDescending(d => d.thang).ToList();                
                //ListDeNghi.AddRange(listCuoi3);
                var listCuoi4 = (from p1 in ListNewCity
                                 select new DeNghiChiLuong
                                 {
                                     maCongTrinh = "CTNEWCITY",
                                     maNguoiLap = p1.nguoiLap,
                                     nam = p1.nam,
                                     ngayLap = Convert.ToDateTime(p1.ngayLap),
                                     noiDung = p1.noiDung,
                                     soPhieu = p1.maPhieu,
                                     tenNguoiLap = p1.tenNguoiLap,
                                     thang = p1.thang,
                                     trangThai = (int?)lqCTNewCity.HT_DMNguoiDuyets.OrderByDescending(d => d.ID).Where(d => d.maPhieu == p1.maPhieu).Select(d => d.trangThai).FirstOrDefault() ?? 0,
                                     tenBuocDuyet = lqCTNewCity.tbl_QuiTrinhDuyet_BuocDuyets.Where(d => d.Id == ((int?)lqCTNewCity.HT_DMNguoiDuyets.OrderByDescending(c => c.ID).Where(c => c.maPhieu == p1.maPhieu).Select(c => c.trangThai).FirstOrDefault() ?? 0)).Select(d => d.TenBuocDuyet).FirstOrDefault() ?? "Tạo mới",
                                     tongTienThucChuyen = lqCTNewCity.tbl_DeNghiChiLuongChiTiets.Where(d => d.maPhieu == p1.maPhieu).Sum(d => d.chuyenKhoan) ?? 0
                                 }).OrderByDescending(d => d.thang).ToList();
                ListDeNghi.AddRange(listCuoi4);
                ViewBag.ListCT = ListDeNghi.Select(d => d.maCongTrinh).Distinct().ToList();
                if (Request.IsAjaxRequest())
                {
                    return PartialView("PartialIndex", ListDeNghi);
                }
                ViewBag.Search = qSearch;
                BindDataNam(DateTime.Now.Year);
                return View(ListDeNghi);
            }
            catch
            {
                return View("Error");
            }
        }
        public void BindDataNam(int? nam)
        {
            Dictionary<int, int> dict = new Dictionary<int, int>();
            var namCurrent = DateTime.Now.Year;
            for (int i = namCurrent - 5; i <= namCurrent+1; i++)
            {
                dict.Add(i, i);
            }
            ViewBag.Nams = new SelectList(dict, "Key", "Value", nam);
        }
        private void BindDataMonth(int? month)
        {
            Dictionary<int, int> dicYear = new Dictionary<int, int>();
            for (int i = 1; i <= 12; i++)
            {
                dicYear[i] = i;
            }
            ViewBag.Thangs = new SelectList(dicYear, "Key", "Value", month);
        }
        public ActionResult Create()
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenThem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            BindDataMonth(DateTime.Now.Month - 1);
            BindDataNam(DateTime.Now.Year);

            deNghiCL = new DeNghiChiLuong();
            deNghiCL.maNguoiLap = GetUser().manv;
            deNghiCL.tenNguoiLap = HoVaTen(GetUser().manv);
            deNghiCL.ngayLap = DateTime.Now;
            deNghiCL.soPhieu = string.Empty;
            return View(deNghiCL);
        }
        public ActionResult GetMaPhieu(string congTrinh)
        {
            string soPhieu=string.Empty;
            if (congTrinh == "CTTV")
            {
                soPhieu = CheckLetterDNCL("DNCL", GetMaxDeNghiChiLuongTV(), 3);
            }
            else if (congTrinh == "CT1330")
            {
                soPhieu = CheckLetterDNCL("DNCL", GetMaxDeNghiChiLuongCT1330(), 3);
            }
            else if (congTrinh == "CT2220")
            {
                soPhieu = CheckLetterDNCL("DNCL", GetMaxDeNghiChiLuongCT2220(), 3);
            }
            else if (congTrinh == "CTNEWCITY")
            {
                soPhieu = CheckLetterDNCL("DNCL", GetMaxDeNghiChiLuongCTNEWCITY(), 3);
            }
            return Json(soPhieu);
        }
        public string GetMaxDeNghiChiLuongTV()
        {
            return lqThuanViet.tbl_DeNghiChiLuongs.OrderByDescending(d => d.maPhieu).Select(d => d.maPhieu).FirstOrDefault();
        }
        public string GetMaxDeNghiChiLuongCT1330()
        {
            return lqCT1330.tbl_DeNghiChiLuongs.OrderByDescending(d => d.maPhieu).Select(d => d.maPhieu).FirstOrDefault();
        }
        public string GetMaxDeNghiChiLuongCT2220()
        {
            return lqCT2220.tbl_DeNghiChiLuongs.OrderByDescending(d => d.maPhieu).Select(d => d.maPhieu).FirstOrDefault();
        }
        public string GetMaxDeNghiChiLuongCTNEWCITY()
        {
            return lqCTNewCity.tbl_DeNghiChiLuongs.OrderByDescending(d => d.maPhieu).Select(d => d.maPhieu).FirstOrDefault();
        }
        [HttpPost]
        public ActionResult Create(FormCollection coll)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenThem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            try
            {
                var soPhieu = string.Empty;
                var maCongTrinh = coll.Get("congTrinh");
                if (maCongTrinh == "CTTV")
                {
                    TaoDeNghiChiLuongTV(coll, ref soPhieu);
                }
                if (maCongTrinh == "CTNEWCITY")
                {
                    TaoDeNghiChiLuongCTNEWCITY(coll, ref soPhieu);
                }
                else if (maCongTrinh == "CT1330")
                {
                    TaoDeNghiChiLuongCT1330(coll, ref soPhieu);
                }
                else if (maCongTrinh == "CT2220")
                {
                    TaoDeNghiChiLuongCT2220(coll, ref soPhieu);
                }

                return RedirectToAction("Edit", "DeNghiChiLuong", new { id = soPhieu, congTrinh = maCongTrinh });
            }
            catch
            {
                return View("Error");
            }
        }
       
        public void TaoDeNghiChiLuongTV(FormCollection coll, ref string soPhieu)
        {
            BatDongSan.Models.NhanSu.LinqThuanViet.tbl_DeNghiChiLuongChiTiet deNghiChiTiet;
            List<BatDongSan.Models.NhanSu.LinqThuanViet.tbl_DeNghiChiLuongChiTiet> lsChiTiet = new List<BatDongSan.Models.NhanSu.LinqThuanViet.tbl_DeNghiChiLuongChiTiet>();
            BatDongSan.Models.NhanSu.LinqThuanViet.tbl_DeNghiChiLuong deNghi = new BatDongSan.Models.NhanSu.LinqThuanViet.tbl_DeNghiChiLuong();
            deNghi.maPhieu = CheckLetterDNCL("DNCL", GetMaxSoPhieu(), 3);
            soPhieu = deNghi.maPhieu;
            deNghi.thang = Convert.ToInt32(coll.Get("thang"));
            deNghi.nam = Convert.ToInt32(coll.Get("nam"));
            deNghi.ngayLap = DateTime.Now;
            deNghi.nguoiLap = GetUser().manv;
            deNghi.tenNguoiLap = HoVaTen(GetUser().manv);
            deNghi.noiDung = coll.Get("noiDung");

            lqThuanViet.tbl_DeNghiChiLuongs.InsertOnSubmit(deNghi);

            string[] chiTiet = coll.GetValues("TenBoPhanTinhLuong");
            if (chiTiet != null)
            {
                for (int i = 0; i < chiTiet.Count(); i++)
                {
                    deNghiChiTiet = new BatDongSan.Models.NhanSu.LinqThuanViet.tbl_DeNghiChiLuongChiTiet();
                    deNghiChiTiet.maPhieu = deNghi.maPhieu;
                    deNghiChiTiet.tenBoPhanTinhLuong = coll.GetValues("TenBoPhanTinhLuong")[i];
                    deNghiChiTiet.luongThang = Convert.ToDecimal(coll.GetValues("LuongThang")[i]);
                    deNghiChiTiet.phuCapCongTrinh = Convert.ToDecimal(coll.GetValues("PhuCapCT")[i]);
                    deNghiChiTiet.phuCapKhac = Convert.ToDecimal(coll.GetValues("PhuCapKhac")[i]);
                    deNghiChiTiet.BHXH = Convert.ToDecimal(coll.GetValues("BHXH")[i]);
                    deNghiChiTiet.thueTNCN = Convert.ToDecimal(coll.GetValues("ThueTNCN")[i]);
                    deNghiChiTiet.truyLanhTruyThu = Convert.ToDecimal(coll.GetValues("TruyLanhTruyThu")[i]);
                    deNghiChiTiet.chuyenKhoan = Convert.ToDecimal(coll.GetValues("ChuyenKhoan")[i]);

                    deNghiChiTiet.TLTTThue = Convert.ToDecimal(coll.GetValues("TruyLanhTruyThuThue")[i]);
                    deNghiChiTiet.TLTTBaoHiem = Convert.ToDecimal(coll.GetValues("TruyLanhTruyThuBaoHiem")[i]);
                    deNghiChiTiet.TLTTTamUng = Convert.ToDecimal(coll.GetValues("TruyLanhTruyThuTamUng")[i]);

                    var getChung = lqThuanViet.tbl_DeNghiChiLuongChiTiets.Where(d => d.tenBoPhanTinhLuong.Contains(coll.GetValues("TenBoPhanTinhLuong")[i])).OrderByDescending(d=>d.id).FirstOrDefault();
                    deNghiChiTiet.maCongTrinh = getChung != null ? getChung.maCongTrinh : string.Empty;
                    deNghiChiTiet.dtLuong = getChung != null ? getChung.dtLuong : string.Empty;
                    deNghiChiTiet.dtBHXH = getChung != null ? getChung.dtBHXH : string.Empty;
                    deNghiChiTiet.dtThueTN = getChung != null ? getChung.dtThueTN : string.Empty;
                    lsChiTiet.Add(deNghiChiTiet);
                }
                lqThuanViet.tbl_DeNghiChiLuongChiTiets.InsertAllOnSubmit(lsChiTiet);
            }
            lqThuanViet.SubmitChanges();
        }
        public void TaoDeNghiChiLuongCT1330(FormCollection coll, ref string soPhieu)
        {
            BatDongSan.Models.NhanSu.LinqCT1330.tbl_DeNghiChiLuongChiTiet deNghiChiTiet;
            List<BatDongSan.Models.NhanSu.LinqCT1330.tbl_DeNghiChiLuongChiTiet> lsChiTiet = new List<BatDongSan.Models.NhanSu.LinqCT1330.tbl_DeNghiChiLuongChiTiet>();
            BatDongSan.Models.NhanSu.LinqCT1330.tbl_DeNghiChiLuong deNghi = new BatDongSan.Models.NhanSu.LinqCT1330.tbl_DeNghiChiLuong();
            deNghi.maPhieu = CheckLetterDNCL("DNCL", GetMaxDeNghiChiLuongCT1330(), 3);
            soPhieu = deNghi.maPhieu;
            deNghi.thang = Convert.ToInt32(coll.Get("thang"));
            deNghi.nam = Convert.ToInt32(coll.Get("nam"));
            deNghi.ngayLap = DateTime.Now;
            deNghi.nguoiLap = GetUser().manv;
            deNghi.tenNguoiLap = HoVaTen(GetUser().manv);
            deNghi.noiDung = coll.Get("noiDung");

            lqCT1330.tbl_DeNghiChiLuongs.InsertOnSubmit(deNghi);

            string[] chiTiet = coll.GetValues("TenBoPhanTinhLuong");
            if (chiTiet != null)
            {
                for (int i = 0; i < chiTiet.Count(); i++)
                {
                    deNghiChiTiet = new BatDongSan.Models.NhanSu.LinqCT1330.tbl_DeNghiChiLuongChiTiet();
                    deNghiChiTiet.maPhieu = deNghi.maPhieu;
                    deNghiChiTiet.tenBoPhanTinhLuong = coll.GetValues("TenBoPhanTinhLuong")[i];
                    deNghiChiTiet.luongThang = Convert.ToDecimal(coll.GetValues("LuongThang")[i]);
                    deNghiChiTiet.phuCapCongTrinh = Convert.ToDecimal(coll.GetValues("PhuCapCT")[i]);
                    deNghiChiTiet.phuCapKhac = Convert.ToDecimal(coll.GetValues("PhuCapKhac")[i]);
                    deNghiChiTiet.BHXH = Convert.ToDecimal(coll.GetValues("BHXH")[i]);
                    deNghiChiTiet.thueTNCN = Convert.ToDecimal(coll.GetValues("ThueTNCN")[i]);
                    deNghiChiTiet.truyLanhTruyThu = Convert.ToDecimal(coll.GetValues("TruyLanhTruyThu")[i]);
                    deNghiChiTiet.chuyenKhoan = Convert.ToDecimal(coll.GetValues("ChuyenKhoan")[i]);

                    deNghiChiTiet.TLTTThue = Convert.ToDecimal(coll.GetValues("TruyLanhTruyThuThue")[i]);
                    deNghiChiTiet.TLTTBaoHiem = Convert.ToDecimal(coll.GetValues("TruyLanhTruyThuBaoHiem")[i]);
                    deNghiChiTiet.TLTTTamUng = Convert.ToDecimal(coll.GetValues("TruyLanhTruyThuTamUng")[i]);

                    var getChung = lqCT1330.tbl_DeNghiChiLuongChiTiets.Where(d => d.tenBoPhanTinhLuong.Contains(coll.GetValues("TenBoPhanTinhLuong")[i])).OrderByDescending(d => d.id).FirstOrDefault();
                    deNghiChiTiet.maCongTrinh = getChung != null ? getChung.maCongTrinh : string.Empty;
                    deNghiChiTiet.dtLuong = getChung != null ? getChung.dtLuong : string.Empty;
                    deNghiChiTiet.dtBHXH = getChung != null ? getChung.dtBHXH : string.Empty;
                    deNghiChiTiet.dtThueTN = getChung != null ? getChung.dtThueTN : string.Empty;

                    lsChiTiet.Add(deNghiChiTiet);
                }
                lqCT1330.tbl_DeNghiChiLuongChiTiets.InsertAllOnSubmit(lsChiTiet);
            }
            lqCT1330.SubmitChanges();
        }
        public void TaoDeNghiChiLuongCT2220(FormCollection coll, ref string soPhieu)
        {
            BatDongSan.Models.NhanSu.LinqCT2220.tbl_DeNghiChiLuongChiTiet deNghiChiTiet;
            List<BatDongSan.Models.NhanSu.LinqCT2220.tbl_DeNghiChiLuongChiTiet> lsChiTiet = new List<BatDongSan.Models.NhanSu.LinqCT2220.tbl_DeNghiChiLuongChiTiet>();
            BatDongSan.Models.NhanSu.LinqCT2220.tbl_DeNghiChiLuong deNghi = new BatDongSan.Models.NhanSu.LinqCT2220.tbl_DeNghiChiLuong();
            deNghi.maPhieu = CheckLetterDNCL("DNCL", GetMaxDeNghiChiLuongCT2220(), 3);
            soPhieu = deNghi.maPhieu;
            deNghi.thang = Convert.ToInt32(coll.Get("thang"));
            deNghi.nam = Convert.ToInt32(coll.Get("nam"));
            deNghi.ngayLap = DateTime.Now;
            deNghi.nguoiLap = GetUser().manv;
            deNghi.tenNguoiLap = HoVaTen(GetUser().manv);
            deNghi.noiDung = coll.Get("noiDung");

            lqCT2220.tbl_DeNghiChiLuongs.InsertOnSubmit(deNghi);

            string[] chiTiet = coll.GetValues("TenBoPhanTinhLuong");
            if (chiTiet != null)
            {
                for (int i = 0; i < chiTiet.Count(); i++)
                {
                    deNghiChiTiet = new BatDongSan.Models.NhanSu.LinqCT2220.tbl_DeNghiChiLuongChiTiet();
                    deNghiChiTiet.maPhieu = deNghi.maPhieu;
                    deNghiChiTiet.tenBoPhanTinhLuong = coll.GetValues("TenBoPhanTinhLuong")[i];
                    deNghiChiTiet.luongThang = Convert.ToDecimal(coll.GetValues("LuongThang")[i]);
                    deNghiChiTiet.phuCapCongTrinh = Convert.ToDecimal(coll.GetValues("PhuCapCT")[i]);
                    deNghiChiTiet.phuCapKhac = Convert.ToDecimal(coll.GetValues("PhuCapKhac")[i]);
                    deNghiChiTiet.BHXH = Convert.ToDecimal(coll.GetValues("BHXH")[i]);
                    deNghiChiTiet.thueTNCN = Convert.ToDecimal(coll.GetValues("ThueTNCN")[i]);
                    deNghiChiTiet.truyLanhTruyThu = Convert.ToDecimal(coll.GetValues("TruyLanhTruyThu")[i]);
                    deNghiChiTiet.chuyenKhoan = Convert.ToDecimal(coll.GetValues("ChuyenKhoan")[i]);

                    deNghiChiTiet.TLTTThue = Convert.ToDecimal(coll.GetValues("TruyLanhTruyThuThue")[i]);
                    deNghiChiTiet.TLTTBaoHiem = Convert.ToDecimal(coll.GetValues("TruyLanhTruyThuBaoHiem")[i]);
                    deNghiChiTiet.TLTTTamUng = Convert.ToDecimal(coll.GetValues("TruyLanhTruyThuTamUng")[i]);

                    var getChung = lqCT2220.tbl_DeNghiChiLuongChiTiets.Where(d => d.tenBoPhanTinhLuong.Contains(coll.GetValues("TenBoPhanTinhLuong")[i])).OrderByDescending(d => d.id).FirstOrDefault();
                    deNghiChiTiet.maCongTrinh = getChung != null ? getChung.maCongTrinh : string.Empty;
                    deNghiChiTiet.dtLuong = getChung != null ? getChung.dtLuong : string.Empty;
                    deNghiChiTiet.dtBHXH = getChung != null ? getChung.dtBHXH : string.Empty;
                    deNghiChiTiet.dtThueTN = getChung != null ? getChung.dtThueTN : string.Empty;
                    lsChiTiet.Add(deNghiChiTiet);
                }
                lqCT2220.tbl_DeNghiChiLuongChiTiets.InsertAllOnSubmit(lsChiTiet);
            }
            lqCT2220.SubmitChanges();
        }
        public void TaoDeNghiChiLuongCTNEWCITY(FormCollection coll, ref string soPhieu)
        {
            BatDongSan.Models.NhanSu.LinqERPNewCity.tbl_DeNghiChiLuongChiTiet deNghiChiTiet;
            List<BatDongSan.Models.NhanSu.LinqERPNewCity.tbl_DeNghiChiLuongChiTiet> lsChiTiet = new List<BatDongSan.Models.NhanSu.LinqERPNewCity.tbl_DeNghiChiLuongChiTiet>();
            BatDongSan.Models.NhanSu.LinqERPNewCity.tbl_DeNghiChiLuong deNghi = new BatDongSan.Models.NhanSu.LinqERPNewCity.tbl_DeNghiChiLuong();
            deNghi.maPhieu = CheckLetterDNCL("DNCL", GetMaxDeNghiChiLuongCTNEWCITY(), 3);
            soPhieu = deNghi.maPhieu;
            deNghi.thang = Convert.ToInt32(coll.Get("thang"));
            deNghi.nam = Convert.ToInt32(coll.Get("nam"));
            deNghi.ngayLap = DateTime.Now;
            deNghi.nguoiLap = GetUser().manv;
            deNghi.tenNguoiLap = HoVaTen(GetUser().manv);
            deNghi.noiDung = coll.Get("noiDung");

            lqCTNewCity.tbl_DeNghiChiLuongs.InsertOnSubmit(deNghi);

            string[] chiTiet = coll.GetValues("TenBoPhanTinhLuong");
            if (chiTiet != null)
            {
                for (int i = 0; i < chiTiet.Count(); i++)
                {
                    deNghiChiTiet = new BatDongSan.Models.NhanSu.LinqERPNewCity.tbl_DeNghiChiLuongChiTiet();
                    deNghiChiTiet.maPhieu = deNghi.maPhieu;
                    deNghiChiTiet.tenBoPhanTinhLuong = coll.GetValues("TenBoPhanTinhLuong")[i];
                    deNghiChiTiet.luongThang = Convert.ToDecimal(coll.GetValues("LuongThang")[i]);
                    deNghiChiTiet.phuCapCongTrinh = Convert.ToDecimal(coll.GetValues("PhuCapCT")[i]);
                    deNghiChiTiet.phuCapKhac = Convert.ToDecimal(coll.GetValues("PhuCapKhac")[i]);
                    deNghiChiTiet.BHXH = Convert.ToDecimal(coll.GetValues("BHXH")[i]);
                    deNghiChiTiet.thueTNCN = Convert.ToDecimal(coll.GetValues("ThueTNCN")[i]);
                    deNghiChiTiet.truyLanhTruyThu = Convert.ToDecimal(coll.GetValues("TruyLanhTruyThu")[i]);
                    deNghiChiTiet.chuyenKhoan = Convert.ToDecimal(coll.GetValues("ChuyenKhoan")[i]);

                    deNghiChiTiet.TLTTThue = Convert.ToDecimal(coll.GetValues("TruyLanhTruyThuThue")[i]);
                    deNghiChiTiet.TLTTBaoHiem = Convert.ToDecimal(coll.GetValues("TruyLanhTruyThuBaoHiem")[i]);
                    deNghiChiTiet.TLTTTamUng = Convert.ToDecimal(coll.GetValues("TruyLanhTruyThuTamUng")[i]);

                    var getChung = lqCT2220.tbl_DeNghiChiLuongChiTiets.Where(d => d.tenBoPhanTinhLuong.Contains(coll.GetValues("TenBoPhanTinhLuong")[i])).OrderByDescending(d => d.id).FirstOrDefault();
                    deNghiChiTiet.maCongTrinh = getChung != null ? getChung.maCongTrinh : string.Empty;
                    deNghiChiTiet.dtLuong = getChung != null ? getChung.dtLuong : string.Empty;
                    deNghiChiTiet.dtBHXH = getChung != null ? getChung.dtBHXH : string.Empty;
                    deNghiChiTiet.dtThueTN = getChung != null ? getChung.dtThueTN : string.Empty;
                    lsChiTiet.Add(deNghiChiTiet);
                }
                lqCTNewCity.tbl_DeNghiChiLuongChiTiets.InsertAllOnSubmit(lsChiTiet);
            }
            lqCTNewCity.SubmitChanges();
        }
        public string GetMaxSoPhieu()
        {
            return lqThuanViet.tbl_DeNghiChiLuongs.OrderByDescending(d => d.ngayLap).Select(d => d.maPhieu).FirstOrDefault();
        }
        public ActionResult Edit(string id, string congTrinh)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            if (congTrinh == "CTTV")
            {
                BindThongTin(id);
            }
            else if (congTrinh == "CT2220")
            {
                BindThongTinCT2220(id);
            }
            else if (congTrinh == "CT1330")
            {
                BindThongTinCT1330(id);
            }
            else if (congTrinh == "CTNEWCITY")
            {
                BindThongTinCTNEWCITY(id);
            }

            return View(deNghiCL);
        }
        public void BindThongTin(string id)
        {
            deNghiCL = (from p in lqThuanViet.tbl_DeNghiChiLuongs
                        where p.maPhieu == id
                        select new DeNghiChiLuong
                        {
                            maCongTrinh ="CTTV",
                            maNguoiLap = p.nguoiLap,
                            tenNguoiLap = p.tenNguoiLap,
                            nam = p.nam,
                            thang = p.thang,
                            ngayLap = Convert.ToDateTime(p.ngayLap),
                            noiDung = p.noiDung,
                            soPhieu = p.maPhieu,
                        }).FirstOrDefault();

            ListChiTietDeNghi = (from ct in lqThuanViet.tbl_DeNghiChiLuongChiTiets
                                 where ct.maPhieu == id
                                 select new DeNghiChiLuongChiTiet
                                 {
                                     BHXH = Convert.ToDouble(ct.BHXH),
                                     ChuyenKhoan = Convert.ToDouble(ct.chuyenKhoan),
                                     LuongThang = Convert.ToDouble(ct.luongThang),
                                     PhuCapCT = Convert.ToDouble(ct.phuCapCongTrinh),
                                     PhuCapKhac = Convert.ToDouble(ct.phuCapKhac),
                                     TenBoPhanTinhLuong = ct.tenBoPhanTinhLuong,
                                     ThueTNCN = Convert.ToDouble(ct.thueTNCN),
                                     TruyLanhTruyThu = Convert.ToDouble(ct.truyLanhTruyThu),
                                     TruyLanhTruyThuBaoHiem = Convert.ToDouble(ct.TLTTBaoHiem),
                                     TruyLanhTruyThuTamUng = Convert.ToDouble(ct.TLTTTamUng),
                                     TruyLanhTruyThuThue = Convert.ToDouble(ct.TLTTThue),
                                 }).ToList();

            deNghiCL.DeNghiChiLuongChiTiet = ListChiTietDeNghi;

            ViewBag.lisQuiTrinhDuyet = lqThuanViet.sp_QuiTrinhDuyetAS(MCV).Select(d => new QuiTrinhDuyet
            {
                Id = d.Id,
                TenQuiTrinh = d.TenQuiTrinh,
                ChuoiBuocDuyet = d.chuoiBuocDuyet,
            }).ToList();
            ViewBag.URL = Request.Url.AbsoluteUri.ToString();
            ViewBag.trangThai = (int?)lqThuanViet.HT_DMNguoiDuyets.OrderByDescending(d => d.ID).Where(d => d.maPhieu == id).Select(d => d.trangThai).FirstOrDefault() ?? 0;            
        }
        public void BindThongTinCT1330(string id)
        {
            deNghiCL = (from p in lqCT1330.tbl_DeNghiChiLuongs
                        where p.maPhieu == id
                        select new DeNghiChiLuong
                        {
                            maCongTrinh = "CT1330",
                            maNguoiLap = p.nguoiLap,
                            tenNguoiLap = p.tenNguoiLap,
                            nam = p.nam,
                            thang = p.thang,
                            ngayLap = Convert.ToDateTime(p.ngayLap),
                            noiDung = p.noiDung,
                            soPhieu = p.maPhieu,
                        }).FirstOrDefault();

            ListChiTietDeNghi = (from ct in lqCT1330.tbl_DeNghiChiLuongChiTiets
                                 where ct.maPhieu == id
                                 select new DeNghiChiLuongChiTiet
                                 {
                                     BHXH = Convert.ToDouble(ct.BHXH),
                                     ChuyenKhoan = Convert.ToDouble(ct.chuyenKhoan),
                                     LuongThang = Convert.ToDouble(ct.luongThang),
                                     PhuCapCT = Convert.ToDouble(ct.phuCapCongTrinh),
                                     PhuCapKhac = Convert.ToDouble(ct.phuCapKhac),
                                     TenBoPhanTinhLuong = ct.tenBoPhanTinhLuong,
                                     ThueTNCN = Convert.ToDouble(ct.thueTNCN),
                                     TruyLanhTruyThu = Convert.ToDouble(ct.truyLanhTruyThu),
                                     TruyLanhTruyThuBaoHiem = Convert.ToDouble(ct.TLTTBaoHiem),
                                     TruyLanhTruyThuTamUng = Convert.ToDouble(ct.TLTTTamUng),
                                     TruyLanhTruyThuThue = Convert.ToDouble(ct.TLTTThue),
                                 }).ToList();

            deNghiCL.DeNghiChiLuongChiTiet = ListChiTietDeNghi;

            ViewBag.lisQuiTrinhDuyet = lqCT1330.sp_QuiTrinhDuyetAS(MCV).Select(d => new QuiTrinhDuyet
            {
                Id = d.Id,
                TenQuiTrinh = d.TenQuiTrinh,
                ChuoiBuocDuyet = d.chuoiBuocDuyet,
            }).ToList();
            ViewBag.URL = Request.Url.AbsoluteUri.ToString();
            ViewBag.trangThai = (int?)lqCT1330.HT_DMNguoiDuyets.OrderByDescending(d => d.ID).Where(d => d.maPhieu == id).Select(d => d.trangThai).FirstOrDefault() ?? 0;
        }
        public void BindThongTinCT2220(string id)
        {
            deNghiCL = (from p in lqCT2220.tbl_DeNghiChiLuongs
                        where p.maPhieu == id
                        select new DeNghiChiLuong
                        {
                            maCongTrinh = "CT2220",
                            maNguoiLap = p.nguoiLap,
                            tenNguoiLap = p.tenNguoiLap,
                            nam = p.nam,
                            thang = p.thang,
                            ngayLap = Convert.ToDateTime(p.ngayLap),
                            noiDung = p.noiDung,
                            soPhieu = p.maPhieu,
                        }).FirstOrDefault();

            ListChiTietDeNghi = (from ct in lqCT2220.tbl_DeNghiChiLuongChiTiets
                                 where ct.maPhieu == id
                                 select new DeNghiChiLuongChiTiet
                                 {
                                     BHXH = Convert.ToDouble(ct.BHXH),
                                     ChuyenKhoan = Convert.ToDouble(ct.chuyenKhoan),
                                     LuongThang = Convert.ToDouble(ct.luongThang),
                                     PhuCapCT = Convert.ToDouble(ct.phuCapCongTrinh),
                                     PhuCapKhac = Convert.ToDouble(ct.phuCapKhac),
                                     TenBoPhanTinhLuong = ct.tenBoPhanTinhLuong,
                                     ThueTNCN = Convert.ToDouble(ct.thueTNCN),
                                     TruyLanhTruyThu = Convert.ToDouble(ct.truyLanhTruyThu),
                                     TruyLanhTruyThuBaoHiem = Convert.ToDouble(ct.TLTTBaoHiem),
                                     TruyLanhTruyThuTamUng = Convert.ToDouble(ct.TLTTTamUng),
                                     TruyLanhTruyThuThue = Convert.ToDouble(ct.TLTTThue),
                                 }).ToList();

            deNghiCL.DeNghiChiLuongChiTiet = ListChiTietDeNghi;

            ViewBag.lisQuiTrinhDuyet = lqCT2220.sp_QuiTrinhDuyetAS(MCV).Select(d => new QuiTrinhDuyet
            {
                Id = d.Id,
                TenQuiTrinh = d.TenQuiTrinh,
                ChuoiBuocDuyet = d.chuoiBuocDuyet,
            }).ToList();
            ViewBag.URL = Request.Url.AbsoluteUri.ToString();
            ViewBag.trangThai = (int?)lqCT2220.HT_DMNguoiDuyets.OrderByDescending(d => d.ID).Where(d => d.maPhieu == id).Select(d => d.trangThai).FirstOrDefault() ?? 0;
        }
        public void BindThongTinCTNEWCITY(string id)
        {
            deNghiCL = (from p in lqCTNewCity.tbl_DeNghiChiLuongs
                        where p.maPhieu == id
                        select new DeNghiChiLuong
                        {
                            maCongTrinh = "CTNEWCITY",
                            maNguoiLap = p.nguoiLap,
                            tenNguoiLap = p.tenNguoiLap,
                            nam = p.nam,
                            thang = p.thang,
                            ngayLap = Convert.ToDateTime(p.ngayLap),
                            noiDung = p.noiDung,
                            soPhieu = p.maPhieu,
                        }).FirstOrDefault();

            ListChiTietDeNghi = (from ct in lqCTNewCity.tbl_DeNghiChiLuongChiTiets
                                 where ct.maPhieu == id
                                 select new DeNghiChiLuongChiTiet
                                 {
                                     BHXH = Convert.ToDouble(ct.BHXH),
                                     ChuyenKhoan = Convert.ToDouble(ct.chuyenKhoan),
                                     LuongThang = Convert.ToDouble(ct.luongThang),
                                     PhuCapCT = Convert.ToDouble(ct.phuCapCongTrinh),
                                     PhuCapKhac = Convert.ToDouble(ct.phuCapKhac),
                                     TenBoPhanTinhLuong = ct.tenBoPhanTinhLuong,
                                     ThueTNCN = Convert.ToDouble(ct.thueTNCN),
                                     TruyLanhTruyThu = Convert.ToDouble(ct.truyLanhTruyThu),
                                     TruyLanhTruyThuBaoHiem = Convert.ToDouble(ct.TLTTBaoHiem),
                                     TruyLanhTruyThuTamUng = Convert.ToDouble(ct.TLTTTamUng),
                                     TruyLanhTruyThuThue = Convert.ToDouble(ct.TLTTThue),
                                 }).ToList();

            deNghiCL.DeNghiChiLuongChiTiet = ListChiTietDeNghi;

            ViewBag.lisQuiTrinhDuyet = lqCTNewCity.sp_QuiTrinhDuyetAS(MCV).Select(d => new QuiTrinhDuyet
            {
                Id = d.Id,
                TenQuiTrinh = d.TenQuiTrinh,
                ChuoiBuocDuyet = d.chuoiBuocDuyet,
            }).ToList();
            ViewBag.URL = Request.Url.AbsoluteUri.ToString();
            ViewBag.trangThai = (int?)lqCTNewCity.HT_DMNguoiDuyets.OrderByDescending(d => d.ID).Where(d => d.maPhieu == id).Select(d => d.trangThai).FirstOrDefault() ?? 0;
        }
        [HttpPost]
        public ActionResult Edit(FormCollection coll)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            var congTrinh = coll.Get("maCongTrinh");
            if (congTrinh == "CTTV")
            {
                var DeNghi = lqThuanViet.tbl_DeNghiChiLuongs.Where(d => d.maPhieu == coll.Get("soPhieu")).FirstOrDefault();
                DeNghi.noiDung = coll.Get("noiDung");
                lqThuanViet.SubmitChanges();
            }
            else if (congTrinh == "CT2220")
            {
                var DeNghi = lqCT2220.tbl_DeNghiChiLuongs.Where(d => d.maPhieu == coll.Get("soPhieu")).FirstOrDefault();
                DeNghi.noiDung = coll.Get("noiDung");
                lqCT2220.SubmitChanges();
            }
            else if (congTrinh == "CT1330")
            {
                var DeNghi = lqCT1330.tbl_DeNghiChiLuongs.Where(d => d.maPhieu == coll.Get("soPhieu")).FirstOrDefault();
                DeNghi.noiDung = coll.Get("noiDung");
                lqCT1330.SubmitChanges();
            }
            else if (congTrinh == "CTNEWCITY")
            {
                var DeNghi = lqCTNewCity.tbl_DeNghiChiLuongs.Where(d => d.maPhieu == coll.Get("soPhieu")).FirstOrDefault();
                DeNghi.noiDung = coll.Get("noiDung");
                lqCTNewCity.SubmitChanges();
            }
            return RedirectToAction("Edit", new { id = coll.Get("soPhieu"), congTrinh = congTrinh});
        }


        public ActionResult Details(string id, string congTrinh)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXemChiTiet);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            if (congTrinh == "CTTV")
            {
                BindThongTin(id);
            }
            else if (congTrinh == "CT2220")
            {
                BindThongTinCT2220(id);
            }
            else if (congTrinh == "CT1330")
            {
                BindThongTinCT1330(id);
            }
            else if (congTrinh == "CTNEWCITY")
            {
                BindThongTinCTNEWCITY(id);
            }
            return View(deNghiCL);
        }

        [HttpPost]
        public ActionResult Delete(string id, string maCongTrinh)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXoa);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            try
            {
                if (maCongTrinh == "CTTV")
                {
                    var DeNghiChiTiet = lqThuanViet.tbl_DeNghiChiLuongChiTiets.Where(s => s.maPhieu == id).ToList();
                    lqThuanViet.tbl_DeNghiChiLuongChiTiets.DeleteAllOnSubmit(DeNghiChiTiet);
                    var DeNghi = lqThuanViet.tbl_DeNghiChiLuongs.Where(s => s.maPhieu == id).FirstOrDefault();
                    lqThuanViet.tbl_DeNghiChiLuongs.DeleteOnSubmit(DeNghi);
                    lqThuanViet.SubmitChanges();

                    var nguoiDuyet = lqThuanViet.HT_DMNguoiDuyets.Where(d => d.maPhieu == id).ToList();
                    if (nguoiDuyet != null && nguoiDuyet.Count > 0)
                    {
                        lqThuanViet.HT_DMNguoiDuyets.DeleteAllOnSubmit(nguoiDuyet);
                        lqThuanViet.SubmitChanges();
                    }
                }
                else if (maCongTrinh == "CT1330")
                {
                    var DeNghiChiTiet = lqCT1330.tbl_DeNghiChiLuongChiTiets.Where(s => s.maPhieu == id).ToList();
                    lqCT1330.tbl_DeNghiChiLuongChiTiets.DeleteAllOnSubmit(DeNghiChiTiet);
                    var DeNghi = lqCT1330.tbl_DeNghiChiLuongs.Where(s => s.maPhieu == id).FirstOrDefault();
                    lqCT1330.tbl_DeNghiChiLuongs.DeleteOnSubmit(DeNghi);
                    lqCT1330.SubmitChanges();

                    var nguoiDuyet = lqCT1330.HT_DMNguoiDuyets.Where(d => d.maPhieu == id).ToList();
                    if (nguoiDuyet != null && nguoiDuyet.Count > 0)
                    {
                        lqCT1330.HT_DMNguoiDuyets.DeleteAllOnSubmit(nguoiDuyet);
                        lqCT1330.SubmitChanges();
                    }
                }
                else if (maCongTrinh == "CT2220")
                {
                    var DeNghiChiTiet = lqCT2220.tbl_DeNghiChiLuongChiTiets.Where(s => s.maPhieu == id).ToList();
                    lqCT2220.tbl_DeNghiChiLuongChiTiets.DeleteAllOnSubmit(DeNghiChiTiet);
                    var DeNghi = lqCT2220.tbl_DeNghiChiLuongs.Where(s => s.maPhieu == id).FirstOrDefault();
                    lqCT2220.tbl_DeNghiChiLuongs.DeleteOnSubmit(DeNghi);
                    lqCT2220.SubmitChanges();

                    var nguoiDuyet = lqCT2220.HT_DMNguoiDuyets.Where(d => d.maPhieu == id).ToList();
                    if (nguoiDuyet != null && nguoiDuyet.Count > 0)
                    {
                        lqCT2220.HT_DMNguoiDuyets.DeleteAllOnSubmit(nguoiDuyet);
                        lqCT2220.SubmitChanges();
                    }
                }
                else if (maCongTrinh == "CTNEWCITY")
                {
                    var DeNghiChiTiet = lqCTNewCity.tbl_DeNghiChiLuongChiTiets.Where(s => s.maPhieu == id).ToList();
                    lqCTNewCity.tbl_DeNghiChiLuongChiTiets.DeleteAllOnSubmit(DeNghiChiTiet);
                    var DeNghi = lqCTNewCity.tbl_DeNghiChiLuongs.Where(s => s.maPhieu == id).FirstOrDefault();
                    lqCTNewCity.tbl_DeNghiChiLuongs.DeleteOnSubmit(DeNghi);
                    lqCTNewCity.SubmitChanges();

                    var nguoiDuyet = lqCTNewCity.HT_DMNguoiDuyets.Where(d => d.maPhieu == id).ToList();
                    if (nguoiDuyet != null && nguoiDuyet.Count > 0)
                    {
                        lqCTNewCity.HT_DMNguoiDuyets.DeleteAllOnSubmit(nguoiDuyet);
                        lqCTNewCity.SubmitChanges();
                    }
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return View("Error");
            }
        }

     
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ViewsApproval(string id)
        {
            try
            {
                return RedirectToAction("Details", new { id = id });// detail de duyet
            }
            catch
            {
                return View("Error");
            }
        }
        public string CheckLetterDNCL(string preString, string maxValue, int length)
        {
            string yearCurrent = DateTime.Now.Year.ToString().Substring(2, 2);
            string monthCurrent = DateTime.Now.Month.ToString(); // "4"
            //khi thang hien tai nho hon 9 thi cong them "0" vao
            if (Convert.ToInt32(monthCurrent) <= 9)
            {
                monthCurrent = "0" + monthCurrent;
            }
            //Khi tham so select o database la null khoi tao so dau tien
            if (String.IsNullOrEmpty(maxValue))
            {
                string ret = "1";
                while (ret.Length < length)
                {
                    ret = "0" + ret;
                }
                return preString + yearCurrent + monthCurrent + "-" + ret;
            }
            else
            {
                string preStringMax = maxValue.Substring(0, maxValue.IndexOf("-") - 4);
                string maxNumber = maxValue.Substring(maxValue.IndexOf("-") + 1);
                string monthYear = maxValue.Substring(maxValue.IndexOf("-") - 4, 4);
                string monthDb = monthYear.Substring(2, 2); //as "04"

                string stringTemp = maxNumber;
                //Khi thang trong gia tri max bang voi thang create thi cong len cho 1
                if (monthDb == monthCurrent)
                {
                    int strToInt = Convert.ToInt32(maxNumber);
                    maxNumber = Convert.ToString(strToInt + 1);
                    while (maxNumber.Length < stringTemp.Length)
                        maxNumber = "0" + maxNumber;
                }
                else //reset
                {
                    maxNumber = "1";
                    while (maxNumber.Length < stringTemp.Length)
                    {
                        maxNumber = "0" + maxNumber;
                    }
                }

                return preStringMax + yearCurrent + monthCurrent + "-" + maxNumber;
            }
        }
    }
}
