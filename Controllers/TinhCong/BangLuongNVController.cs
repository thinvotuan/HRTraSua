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
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Style;
using System.Configuration;
using System.Net.Mail;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel.Contrib;
using Worldsoft.Mvc.Web.Util;
using NPOI.SS.Util;
namespace BatDongSan.Controllers.TinhCong
{
    public class BangLuongNVController : ApplicationController
    {

        private LinqNhanSuDataContext nhanSuContext = new LinqNhanSuDataContext();
        BatDongSan.Models.NhanSu.LinqThuanViet.LinqThuanVietDataContext lqThuanViet = new BatDongSan.Models.NhanSu.LinqThuanViet.LinqThuanVietDataContext();
        //BatDongSan.Models.NhanSu.LinqCT1330.LinqCT1330DataContext lqCT1330 = new BatDongSan.Models.NhanSu.LinqCT1330.LinqCT1330DataContext();
        //BatDongSan.Models.NhanSu.LinqCT2220.LinqCT2220DataContext lqCT2220 = new BatDongSan.Models.NhanSu.LinqCT2220.LinqCT2220DataContext();
        private readonly string MCV = "BangLuongNV";
        private IList<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan> phongBans;
        private StringBuilder buildTree;
        private bool? permission;
        public ActionResult Index()
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion            
            thang(DateTime.Now.Month);
            nam(DateTime.Now.Year);
            return View("");

        }

        public ActionResult LoadBangLuongNV(string maPhongBan, string qSearch, int thang, int nam, int _page = 0)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            int page = _page == 0 ? 1 : _page;
            int pIndex = page;
            int total = nhanSuContext.sp_NS_BangLuongNhanVien(maPhongBan, thang, nam, qSearch).Count();
            PagingLoaderFullController("/BangLuongNV/LoadBangLuongNV/", total, page, "?qsearch=" + qSearch);
            ViewData["lsDanhSach"] = nhanSuContext.sp_NS_BangLuongNhanVien(maPhongBan, thang, nam, qSearch).Skip(start).Take(1000).ToList();

            ViewData["qSearch"] = qSearch;
            var kqCheck = 0;
            var checkEx = nhanSuContext.tbl_DuyetBangLuongNVs.Where(t => t.nam == nam && t.thang == thang).FirstOrDefault();
            if (checkEx != null)
            {
                kqCheck = 1;
            }
            ViewData["kqCheck"] = kqCheck;
            return PartialView("_LoadBangLuongNV");
        }




        public ActionResult LoadBangLuongTheoBP(int thang, int nam, int _page = 0)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            int page = _page == 0 ? 1 : _page;
            int pIndex = page;
            //int total = nhanSuContext.sp_NS_BangLuongTheoBoPhan(thang, nam,null).Count();
            //PagingLoaderFullController("/BangLuongNV/LoadBangLuongTheoBP/", total, page, "?qsearch=" + null);
            //ViewData["lsDanhSachBP"] = nhanSuContext.sp_NS_BangLuongTheoBoPhan(thang, nam,null).Skip(start).Take(offset).ToList();

            ViewData["lsCTTV"] = nhanSuContext.sp_NS_BangLuongTheoBoPhan(thang, nam, "CTTV").ToList();
            ViewData["lsCT1330"] = nhanSuContext.sp_NS_BangLuongTheoBoPhan(thang, nam, "CT1330").ToList();
            ViewData["lsCT2220"] = nhanSuContext.sp_NS_BangLuongTheoBoPhan(thang, nam, "CT2220").ToList();

            return PartialView("_LoadBangLuongTheoBP");
        }
        public ActionResult LoadBangLuongThangMuoi()
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            //var lstKPI = nhanSuContext.tbl_NS_NhanVien_KPIs.ToList();
            //foreach (var item in lstKPI) {
            //    var nhanVienThuong = nhanSuContext.tbl_NS_NhanVien_Thuongs.Where(d => d.maNhanVien == item.maNhanVien).FirstOrDefault();
            //    if (nhanVienThuong != null)
            //    {
            //        nhanVienThuong.diemKPI = item.diemKPI;
            //        nhanSuContext.SubmitChanges();
            //    }
            //}
            ViewBag.lstThangMuoi = nhanSuContext.tbl_NS_NhanVien_Thuongs.OrderBy(d => d.maNhanVien).ToList();
            return PartialView("_LoadBangLuongThangMuoi");
        }
        public ActionResult LoadBangLuongThangMuoiMot()
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            //var lstKPI = nhanSuContext.tbl_NS_NhanVien_KPIs.ToList();
            //foreach (var item in lstKPI) {
            //    var nhanVienThuong = nhanSuContext.tbl_NS_NhanVien_Thuongs.Where(d => d.maNhanVien == item.maNhanVien).FirstOrDefault();
            //    if (nhanVienThuong != null)
            //    {
            //        nhanVienThuong.diemKPI = item.diemKPI;
            //        nhanSuContext.SubmitChanges();
            //    }
            //}
            ViewBag.lstThangMuoi = nhanSuContext.tbl_NS_NhanVien_Thuong11s.OrderBy(d => d.maNhanVien).ToList();
            return PartialView("_LoadBangLuongThangMuoiMot");
        }
        public ActionResult BangLuongNV()
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            buildTree = new StringBuilder();
            phongBans = nhanSuContext.GetTable<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan>().ToList();
            buildTree = TreePhongBanAjax.BuildTreeDepartment(phongBans);
            ViewBag.PhongBans = buildTree.ToString();
            thang(DateTime.Now.Month);
            nam(DateTime.Now.Year);
            return View("");

        }
        public ActionResult BangLuongChuyenNN()
        {
            #region Role user
            permission = GetPermission("BangLuongNVNN", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            buildTree = new StringBuilder();
            phongBans = nhanSuContext.GetTable<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan>().ToList();
            buildTree = TreePhongBanAjax.BuildTreeDepartment(phongBans);
            ViewBag.PhongBans = buildTree.ToString();
            thang(DateTime.Now.Month);
            nam(DateTime.Now.Year);
            return View("");

        }

        public ActionResult LoadBangLuongChuyenNN(string maPhongBan, string qSearch, int thang, int nam, int _page = 0)
        {
            #region Role user
            permission = GetPermission("BangLuongNVNN", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            int page = _page == 0 ? 1 : _page;
            int pIndex = page;
            int total = nhanSuContext.sp_NS_BangLuongChuyenNganHang(thang, nam, maPhongBan, qSearch).Count();
            PagingLoaderFullController("/BangLuongNV/BangLuongChuyenNN/", total, page, "?qsearch=" + qSearch);
            ViewData["lsDanhSach"] = nhanSuContext.sp_NS_BangLuongChuyenNganHang(thang, nam, maPhongBan, qSearch).Skip(start).Take(offset).ToList();
            ViewData["qSearch"] = qSearch;
            return PartialView("_LoadBangLuongChuyenNN");
        }

        public ActionResult UpdateLuongNV(int thang, int nam)
        {


            try
            {
                #region Role user
                permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
                if (!permission.HasValue)
                    return View("LogIn");
                if (!permission.Value)
                    return View("AccessDenied");
                #endregion
                //Check 
                var result = new { kq = false };
                var checkEx = nhanSuContext.tbl_DuyetBangLuongNVs.Where(t => t.nam == nam && t.thang == thang).FirstOrDefault();
                if (checkEx != null)
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                //End check
                var list = nhanSuContext.sp_NS_TinhLuongNhanVien(thang, nam);
                result = new { kq = true };
                SaveActiveHistory("Tính lương nhân viên tháng: " + thang + " năm: " + nam);
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch
            {
                return View();
            }

        }
        public ActionResult DuyetLuongNV(int thang, int nam)
        {


            try
            {
                #region Role user
                permission = GetPermission(MCV, BangPhanQuyen.QuyenDuyet);
                if (!permission.HasValue)
                    return View("LogIn");
                if (!permission.Value)
                    return View("AccessDenied");
                #endregion
                //Check 
                var result = new { kq = false };
                var checkEx = nhanSuContext.tbl_DuyetBangLuongNVs.Where(t => t.nam == nam && t.thang == thang).FirstOrDefault();
                if (checkEx != null)
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                //End check
                // Insert Row 
                tbl_DuyetBangLuongNV tblDuyetBL = new tbl_DuyetBangLuongNV();
                tblDuyetBL.nam = nam;
                tblDuyetBL.thang = thang;
                tblDuyetBL.ngayDuyet = DateTime.Now;
                tblDuyetBL.nguoiDuyet = GetUser().manv;
                nhanSuContext.tbl_DuyetBangLuongNVs.InsertOnSubmit(tblDuyetBL);
                nhanSuContext.SubmitChanges();
                // End Insert Row
                // Check Exist
                var list = nhanSuContext.tbl_DuyetBangLuongNVs.Where(t => t.nam == nam && t.thang == thang).FirstOrDefault();

                if (list != null)
                {
                    result = new { kq = true };
                }
                SaveActiveHistory("Duyệt lương nhân viên tháng: " + thang + " năm: " + nam);
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch
            {
                return View();
            }

        }

        private void thang(int value)
        {
            Dictionary<int, string> dics = new Dictionary<int, string>();
            for (int i = 0; i < 13; i++)
            {
                dics[i] = i.ToString();
            }
            ViewData["thang"] = new SelectList(dics, "Key", "Value", value);
            ViewData["thangBP"] = new SelectList(dics, "Key", "Value", value);

        }
        private void nam(int value)
        {
            Dictionary<int, string> dics = new Dictionary<int, string>();
            for (int i = 2015; i < 2031; i++)
            {
                dics[i] = i.ToString();
            }
            ViewData["nam"] = new SelectList(dics, "Key", "Value", value);
            ViewData["namBP"] = new SelectList(dics, "Key", "Value", value);

        }

        #region Xuat File Bang Luong Nhan Vien

        #endregion


        public ActionResult CheckTaoDNCL(int? thang, int? nam, string congTrinh)
        {
            string hasValue = string.Empty;
            if (congTrinh == "CTTV")
            {
                var isTonTai = lqThuanViet.tbl_DeNghiChiLuongs.Where(d => d.thang == thang && d.nam == nam).FirstOrDefault();
                if (isTonTai != null)
                {
                    hasValue = "true";
                }
            }
            //else if (congTrinh == "CT1330")
            //{
            //    var isTonTai = lqCT1330.tbl_DeNghiChiLuongs.Where(d => d.thang == thang && d.nam == nam).FirstOrDefault();
            //    if (isTonTai != null)
            //    {
            //        hasValue = "true";
            //    }
            //}
            //else if (congTrinh == "CT2220")
            //{
            //    var isTonTai = lqCT2220.tbl_DeNghiChiLuongs.Where(d => d.thang == thang && d.nam == nam).FirstOrDefault();
            //    if (isTonTai != null)
            //    {
            //        hasValue = "true";
            //    }
            //}
            return Json(hasValue);
        }

        public ActionResult CreateCL(int? thang, int? nam, string congTrinh)
        {
            DeNghiChiLuong deNghiCL = new DeNghiChiLuong();
            if (congTrinh == "CTTV")
            {
                deNghiCL.soPhieu = CheckLetterDNCL("DNCL", GetMaxDeNghiChiLuongTV(), 3);
            }
            //else if (congTrinh == "CT1330")
            //{
            //    deNghiCL.soPhieu = CheckLetterDNCL("DNCL", GetMaxDeNghiChiLuongCT1330(), 3);
            //}
            //else if (congTrinh == "CT2220")
            //{
            //    deNghiCL.soPhieu = CheckLetterDNCL("DNCL", GetMaxDeNghiChiLuongCT2220(), 3);
            //}
            deNghiCL.thang = thang;
            deNghiCL.nam = nam;
            deNghiCL.maNguoiLap = GetUser().manv;
            deNghiCL.tenNguoiLap = HoVaTen(GetUser().manv);
            deNghiCL.ngayLap = DateTime.Now;
            var chiTietLuong = (from p in nhanSuContext.sp_NS_BangLuongTheoBoPhan(thang, nam, congTrinh)
                                select new DeNghiChiLuongChiTiet
                                {
                                    BHXH = p.baoHiem,
                                    ChuyenKhoan = p.thucLanh,
                                    LuongThang = p.luongThang,
                                    TenBoPhanTinhLuong = p.boPhanTinhLuong,
                                    ThueTNCN = p.thue,
                                    TruyLanhTruyThu = p.TTTLLuong,
                                    TruyLanhTruyThuBaoHiem = p.TTTLBaoHiem,
                                    TruyLanhTruyThuThue = p.TTTLThue,
                                    TruyLanhTruyThuTamUng = p.TTTLTamUng,
                                    PhuCapCT = p.phuCapCongTrinh,
                                    PhuCapKhac = p.phuCapKhac,

                                }).ToList();
            deNghiCL.DeNghiChiLuongChiTiet = chiTietLuong;
            deNghiCL.maCongTrinh = congTrinh;
            return View(deNghiCL);
        }
        public string GetMaxDeNghiChiLuongTV()
        {
            return lqThuanViet.tbl_DeNghiChiLuongs.OrderByDescending(d => d.maPhieu).Select(d => d.maPhieu).FirstOrDefault();
        }
        //public string GetMaxDeNghiChiLuongCT1330()
        //{
        //    return lqCT1330.tbl_DeNghiChiLuongs.OrderByDescending(d => d.maPhieu).Select(d => d.maPhieu).FirstOrDefault();
        //}
        //public string GetMaxDeNghiChiLuongCT2220()
        //{
        //    return lqCT2220.tbl_DeNghiChiLuongs.OrderByDescending(d => d.maPhieu).Select(d => d.maPhieu).FirstOrDefault();
        //}
        [HttpPost]
        public ActionResult CreateCL(FormCollection coll)
        {
            try
            {
                var maCongTrinh = coll.Get("maCongTrinh");
                if (maCongTrinh == "CTTV")
                {
                    TaoDeNghiChiLuongTV(coll);
                }
                //else if (maCongTrinh == "CT1330")
                //{
                //    TaoDeNghiChiLuongCT1330(coll);
                //}
                //else if (maCongTrinh == "CT2220")
                //{
                //    TaoDeNghiChiLuongCT2220(coll);
                //}

                return RedirectToAction("Index", "BangLuongNV");
            }
            catch
            {
                return View("Error");
            }
        }
        public void TaoDeNghiChiLuongTV(FormCollection coll)
        {
            BatDongSan.Models.NhanSu.LinqThuanViet.tbl_DeNghiChiLuongChiTiet deNghiChiTiet;
            List<BatDongSan.Models.NhanSu.LinqThuanViet.tbl_DeNghiChiLuongChiTiet> lsChiTiet = new List<BatDongSan.Models.NhanSu.LinqThuanViet.tbl_DeNghiChiLuongChiTiet>();
            BatDongSan.Models.NhanSu.LinqThuanViet.tbl_DeNghiChiLuong deNghi = new BatDongSan.Models.NhanSu.LinqThuanViet.tbl_DeNghiChiLuong();
            deNghi.maPhieu = CheckLetterDNCL("DNCL", GetMaxDeNghiChiLuongTV(), 3);
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

                    deNghiChiTiet.TLTTThue = Convert.ToDecimal(coll.GetValues("TruyLanhTruyThuThue")[i]);
                    deNghiChiTiet.TLTTBaoHiem = Convert.ToDecimal(coll.GetValues("TruyLanhTruyThuBaoHiem")[i]);
                    deNghiChiTiet.TLTTTamUng = Convert.ToDecimal(coll.GetValues("TruyLanhTruyThuTamUng")[i]);


                    deNghiChiTiet.chuyenKhoan = Convert.ToDecimal(coll.GetValues("ChuyenKhoan")[i]);

                    var getChung = lqThuanViet.tbl_DeNghiChiLuongChiTiets.Where(d => d.tenBoPhanTinhLuong.Contains(coll.GetValues("TenBoPhanTinhLuong")[i])).OrderByDescending(d => d.id).FirstOrDefault();
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
        //public void TaoDeNghiChiLuongCT1330(FormCollection coll)
        //{
        //    BatDongSan.Models.NhanSu.LinqCT1330.tbl_DeNghiChiLuongChiTiet deNghiChiTiet;
        //    List<BatDongSan.Models.NhanSu.LinqCT1330.tbl_DeNghiChiLuongChiTiet> lsChiTiet = new List<BatDongSan.Models.NhanSu.LinqCT1330.tbl_DeNghiChiLuongChiTiet>();
        //    BatDongSan.Models.NhanSu.LinqCT1330.tbl_DeNghiChiLuong deNghi = new BatDongSan.Models.NhanSu.LinqCT1330.tbl_DeNghiChiLuong();
        //    deNghi.maPhieu = CheckLetterDNCL("DNCL", GetMaxDeNghiChiLuongCT1330(), 3);
        //    deNghi.thang = Convert.ToInt32(coll.Get("thang"));
        //    deNghi.nam = Convert.ToInt32(coll.Get("nam"));
        //    deNghi.ngayLap = DateTime.Now;
        //    deNghi.nguoiLap = GetUser().manv;
        //    deNghi.tenNguoiLap = HoVaTen(GetUser().manv);
        //    deNghi.noiDung = coll.Get("noiDung");

        //    lqCT1330.tbl_DeNghiChiLuongs.InsertOnSubmit(deNghi);

        //    string[] chiTiet = coll.GetValues("TenBoPhanTinhLuong");
        //    if (chiTiet != null)
        //    {
        //        for (int i = 0; i < chiTiet.Count(); i++)
        //        {
        //            deNghiChiTiet = new BatDongSan.Models.NhanSu.LinqCT1330.tbl_DeNghiChiLuongChiTiet();
        //            deNghiChiTiet.maPhieu = deNghi.maPhieu;
        //            deNghiChiTiet.tenBoPhanTinhLuong = coll.GetValues("TenBoPhanTinhLuong")[i];
        //            deNghiChiTiet.luongThang = Convert.ToDecimal(coll.GetValues("LuongThang")[i]);
        //            deNghiChiTiet.phuCapCongTrinh = Convert.ToDecimal(coll.GetValues("PhuCapCT")[i]);
        //            deNghiChiTiet.phuCapKhac = Convert.ToDecimal(coll.GetValues("PhuCapKhac")[i]);
        //            deNghiChiTiet.BHXH = Convert.ToDecimal(coll.GetValues("BHXH")[i]);
        //            deNghiChiTiet.thueTNCN = Convert.ToDecimal(coll.GetValues("ThueTNCN")[i]);
        //            deNghiChiTiet.truyLanhTruyThu = Convert.ToDecimal(coll.GetValues("TruyLanhTruyThu")[i]);
        //            deNghiChiTiet.chuyenKhoan = Convert.ToDecimal(coll.GetValues("ChuyenKhoan")[i]);

        //            deNghiChiTiet.TLTTThue = Convert.ToDecimal(coll.GetValues("TruyLanhTruyThuThue")[i]);
        //            deNghiChiTiet.TLTTBaoHiem = Convert.ToDecimal(coll.GetValues("TruyLanhTruyThuBaoHiem")[i]);
        //            deNghiChiTiet.TLTTTamUng = Convert.ToDecimal(coll.GetValues("TruyLanhTruyThuTamUng")[i]);

        //            var getChung = lqCT1330.tbl_DeNghiChiLuongChiTiets.Where(d => d.tenBoPhanTinhLuong.Contains(coll.GetValues("TenBoPhanTinhLuong")[i])).OrderByDescending(d => d.id).FirstOrDefault();
        //            deNghiChiTiet.maCongTrinh = getChung != null ? getChung.maCongTrinh : string.Empty;
        //            deNghiChiTiet.dtLuong = getChung != null ? getChung.dtLuong : string.Empty;
        //            deNghiChiTiet.dtBHXH = getChung != null ? getChung.dtBHXH : string.Empty;
        //            deNghiChiTiet.dtThueTN = getChung != null ? getChung.dtThueTN : string.Empty;
        //            lsChiTiet.Add(deNghiChiTiet);
        //        }
        //        lqCT1330.tbl_DeNghiChiLuongChiTiets.InsertAllOnSubmit(lsChiTiet);
        //    }
        //    lqCT1330.SubmitChanges();
        //}
        //public void TaoDeNghiChiLuongCT2220(FormCollection coll)
        //{
        //    BatDongSan.Models.NhanSu.LinqCT2220.tbl_DeNghiChiLuongChiTiet deNghiChiTiet;
        //    List<BatDongSan.Models.NhanSu.LinqCT2220.tbl_DeNghiChiLuongChiTiet> lsChiTiet = new List<BatDongSan.Models.NhanSu.LinqCT2220.tbl_DeNghiChiLuongChiTiet>();
        //    BatDongSan.Models.NhanSu.LinqCT2220.tbl_DeNghiChiLuong deNghi = new BatDongSan.Models.NhanSu.LinqCT2220.tbl_DeNghiChiLuong();
        //    deNghi.maPhieu = CheckLetterDNCL("DNCL", GetMaxDeNghiChiLuongCT2220(), 3);
        //    deNghi.thang = Convert.ToInt32(coll.Get("thang"));
        //    deNghi.nam = Convert.ToInt32(coll.Get("nam"));
        //    deNghi.ngayLap = DateTime.Now;
        //    deNghi.nguoiLap = GetUser().manv;
        //    deNghi.tenNguoiLap = HoVaTen(GetUser().manv);
        //    deNghi.noiDung = coll.Get("noiDung");

        //    lqCT2220.tbl_DeNghiChiLuongs.InsertOnSubmit(deNghi);

        //    string[] chiTiet = coll.GetValues("TenBoPhanTinhLuong");
        //    if (chiTiet != null)
        //    {
        //        for (int i = 0; i < chiTiet.Count(); i++)
        //        {
        //            deNghiChiTiet = new BatDongSan.Models.NhanSu.LinqCT2220.tbl_DeNghiChiLuongChiTiet();
        //            deNghiChiTiet.maPhieu = deNghi.maPhieu;
        //            deNghiChiTiet.tenBoPhanTinhLuong = coll.GetValues("TenBoPhanTinhLuong")[i];
        //            deNghiChiTiet.luongThang = Convert.ToDecimal(coll.GetValues("LuongThang")[i]);
        //            deNghiChiTiet.phuCapCongTrinh = Convert.ToDecimal(coll.GetValues("PhuCapCT")[i]);
        //            deNghiChiTiet.phuCapKhac = Convert.ToDecimal(coll.GetValues("PhuCapKhac")[i]);
        //            deNghiChiTiet.BHXH = Convert.ToDecimal(coll.GetValues("BHXH")[i]);
        //            deNghiChiTiet.thueTNCN = Convert.ToDecimal(coll.GetValues("ThueTNCN")[i]);
        //            deNghiChiTiet.truyLanhTruyThu = Convert.ToDecimal(coll.GetValues("TruyLanhTruyThu")[i]);
        //            deNghiChiTiet.chuyenKhoan = Convert.ToDecimal(coll.GetValues("ChuyenKhoan")[i]);

        //            deNghiChiTiet.TLTTThue = Convert.ToDecimal(coll.GetValues("TruyLanhTruyThuThue")[i]);
        //            deNghiChiTiet.TLTTBaoHiem = Convert.ToDecimal(coll.GetValues("TruyLanhTruyThuBaoHiem")[i]);
        //            deNghiChiTiet.TLTTTamUng = Convert.ToDecimal(coll.GetValues("TruyLanhTruyThuTamUng")[i]);

        //            var getChung = lqCT2220.tbl_DeNghiChiLuongChiTiets.Where(d => d.tenBoPhanTinhLuong.Contains(coll.GetValues("TenBoPhanTinhLuong")[i])).OrderByDescending(d => d.id).FirstOrDefault();
        //            deNghiChiTiet.maCongTrinh = getChung != null ? getChung.maCongTrinh : string.Empty;
        //            deNghiChiTiet.dtLuong = getChung != null ? getChung.dtLuong : string.Empty;
        //            deNghiChiTiet.dtBHXH = getChung != null ? getChung.dtBHXH : string.Empty;
        //            deNghiChiTiet.dtThueTN = getChung != null ? getChung.dtThueTN : string.Empty;

        //            lsChiTiet.Add(deNghiChiTiet);
        //        }
        //        lqCT2220.tbl_DeNghiChiLuongChiTiets.InsertAllOnSubmit(lsChiTiet);
        //    }
        //    lqCT2220.SubmitChanges();
        //}
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

        #region Tính lương nhân viên Thin.Vo
        public JsonResult TinhLuongNhanVienTheoDot(int nam, int thang, int soDot, string tuNgay, string denNgay)
        {
            try
            {
                DateTime? fromDate = null;
                DateTime? toDate = null;
                if (!String.IsNullOrEmpty(tuNgay))
                {
                    fromDate = DateTime.ParseExact(tuNgay, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }
                if (!String.IsNullOrEmpty(denNgay))
                {
                    toDate = DateTime.ParseExact(denNgay, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }
                return Json(string.Empty);
            }
            catch
            {
                return Json(string.Empty);
            }
        }
        #endregion

        #region View chi tiết bảng lương nhân viên
        public ActionResult ViewChiTietLuong(int? thang, int? nam, string maNhanVien)
        {
            #region Role user
            permission = GetPermission("XemBangLuong", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            ViewBag.BangLuongNV = nhanSuContext.sp_NS_BangTongHopCongThang(thang, nam, null, maNhanVien, null, null).FirstOrDefault();
            ViewBag.ListPhuCap = nhanSuContext.sp_NS_BangLuongNhanVien_PhuCap(thang, nam, maNhanVien).ToList();
            return PartialView("_ViewChiTietLuongTemplate");
        }
        #endregion
    }
}
