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
            buildTree = new StringBuilder();
            phongBans = nhanSuContext.GetTable<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan>().ToList();
            buildTree = TreePhongBanAjax.BuildTreeDepartment(phongBans);
            ViewBag.PhongBans = buildTree.ToString();
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
        public ActionResult ViewChiTietLuong(string thang, string nam, string maNhanVien)
        {
            #region Role user
            permission = GetPermission("XemBangLuong", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            var dsMauIn = nhanSuContext.GetTable<BatDongSan.Models.HeThong.Sys_PrintTemplate>().Where(d => d.maMauIn == "MIBLNV").FirstOrDefault();
            string noiDung = string.Empty;
            var ds = nhanSuContext.tbl_NS_BangLuongNhanViens
                               .Where(t => t.maNhanVien == maNhanVien && t.thang.ToString() == thang && nam == t.nam.ToString()).FirstOrDefault();
            ViewData["chiTiet"] = dsMauIn;

            if (dsMauIn != null)
            {
                double tongBaoHiem = (ds.baoHiem ?? 0);
                double tongPhuCapTruyLanh = (ds.congTacPhi ?? 0) + (ds.TTTLBaoHiem ?? 0) + (ds.TTTLThue ?? 0) + (ds.TTTLTamUng ?? 0) + (ds.phuCapKhac ?? 0) + (ds.TTTLLuong ?? 0);
                noiDung = dsMauIn.html.Replace("{$thang}", thang)
                    .Replace("{$nam}", nam)
                    .Replace("{$hoVaTen}", ds.hoTen)
                    .Replace("{$tongLuong}", String.Format("{0:###,##0}", ds.tongLuong ?? 0))
                    .Replace("{$luongDongBaoHiem}", String.Format("{0:###,##0}", ds.luongDongBaoHiem ?? 0))
                    .Replace("{$khoanBoSungLuong}", String.Format("{0:###,##0}", ds.khoanBoSungLuong ?? 0))
                    .Replace("{$phuCapCongTrinh}", String.Format("{0:###,##0}", ds.phuCapCongTrinh ?? 0))
                    .Replace("{$ngayCongChuan}", Convert.ToString(ds.ngayCongChuan ?? 0))
                    .Replace("{$ngayCongTinhLuong}", Convert.ToString(ds.tongNgayCong ?? 0))
                    .Replace("{$ngayCong}", Convert.ToString((ds.soNgayQuet ?? 0) + (ds.soNgayCongTac ?? 0)))
                    .Replace("{$nghiBu}", Convert.ToString(ds.soNgayNghiBu ?? 0))
                    .Replace("{$nghiPhep}", Convert.ToString(ds.soNgayNghiPhep ?? 0))
                    .Replace("{$nghiLeTet}", Convert.ToString(ds.soNgayNghiLe ?? 0))
                    .Replace("{$luyKeThangTruoc}", Convert.ToString(ds.soNgayPhepLuyKeThangTruoc ?? 0))
                    .Replace("{$nghiKhongLuong}", Convert.ToString(ds.soNgayNghiKhongLuong ?? 0))
                    //Lương Theo Ngày Công
                    .Replace("{$luongTheoNgayCong}", String.Format("{0:###,##0}", ds.luongThang ?? 0))
                    //Phụ Cấp Khác Và Truy Lãnh

                    .Replace("{$tongPhuCapTruyLanh}", String.Format("{0:###,##0}", tongPhuCapTruyLanh))
                    .Replace("{$tienPhuCapCongTacTD}", String.Format("{0:###,##0}", ds.congTacPhi ?? 0))
                    .Replace("{$tienTTTLBaoHiem}", String.Format("{0:###,##0}", ds.TTTLBaoHiem ?? 0))
                    .Replace("{$tienTTTLThue}", String.Format("{0:###,##0}", ds.TTTLThue ?? 0))
                    .Replace("{$tienTTTLTamUng}", String.Format("{0:###,##0}", ds.TTTLTamUng ?? 0))
                    .Replace("{$tienTTTLLuong}", String.Format("{0:###,##0}", ds.TTTLLuong ?? 0))
                    .Replace("{$TienPhuCapKhac}", String.Format("{0:###,##0}", ds.phuCapKhac ?? 0))
                    //Các Khoản Khấu Trừ
                    .Replace("{$tongKhauTru}", String.Format("{0:###,##0}", tongBaoHiem + (ds.thue ?? 0)))
                    .Replace("{$tongBaoHiem}", String.Format("{0:###,##0}", tongBaoHiem))
                    .Replace("{$tienThueTNCC}", String.Format("{0:###,##0}", ds.thue ?? 0))
                    .Replace("{$tienGiamTruGC}", String.Format("{0:###,##0}", (ds.giamTruBanThan ?? 0) + (ds.giamTruNguoiPhuThuoc ?? 0)))
                    .Replace("{$tongThucNhanLuong}", String.Format("{0:###,##0}", ds.thucLanh ?? 0));

            }
            ViewBag.NoiDung = noiDung;
            // return PartialView("_ViewChiTietLuong");
            return PartialView("_ViewChiTietLuongTemplate");
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
        public ActionResult SendBangLuongNV(int thang, int nam)
        {

            //string maPhongBan = "";
            //string qSearch = "";
            //var list = nhanSuContext.tbl_DuyetBangLuongNVs.Where(t => t.nam == nam && t.thang == thang).FirstOrDefault();
            //var result = new { kq = false };
            //if (list == null)
            //{
            //    return Json(result, JsonRequestBehavior.AllowGet);
            //}

            //var listSendMails = nhanSuContext.sp_NS_BangLuongNhanVien(maPhongBan, thang, nam, qSearch).ToList();
            //foreach (var ds in listSendMails)
            //{
            //    var dsMauIn = nhanSuContext.GetTable<BatDongSan.Models.HeThong.Sys_PrintTemplate>().Where(d => d.maMauIn == "MIBLNV").FirstOrDefault();
            //    string noiDung = string.Empty;
            //    //Replace bang luong send mail
            //    double tongBaoHiem = (ds.baoHiem ?? 0);
            //    double tongPhuCapTruyLanh = (ds.congTacPhi ?? 0) + (ds.TTTLBaoHiem ?? 0) + (ds.TTTLThue ?? 0) + (ds.TTTLTamUng ?? 0) + (ds.phuCapKhac ?? 0) + (ds.TTTLLuong ?? 0);
            //    noiDung = dsMauIn.html.Replace("{$thang}", Convert.ToString(thang))
            //        .Replace("{$nam}", Convert.ToString(nam))
            //        .Replace("{$hoVaTen}", ds.hoTen)
            //        .Replace("{$tongLuong}", String.Format("{0:###,##0}", ds.tongLuong ?? 0))
            //        .Replace("{$luongDongBaoHiem}", String.Format("{0:###,##0}", ds.luongDongBaoHiem ?? 0))
            //        .Replace("{$khoanBoSungLuong}", String.Format("{0:###,##0}", ds.khoanBoSungLuong ?? 0))
            //        .Replace("{$phuCapCongTrinh}", String.Format("{0:###,##0}", ds.phuCapCongTrinh ?? 0))
            //        .Replace("{$ngayCongChuan}", Convert.ToString(ds.ngayCongChuan ?? 0))
            //        .Replace("{$ngayCongTinhLuong}", Convert.ToString(ds.tongNgayCong ?? 0))
            //        .Replace("{$ngayCong}", Convert.ToString((ds.soNgayQuet ?? 0) + (ds.soNgayCongTac ?? 0)))
            //        .Replace("{$nghiBu}", Convert.ToString(ds.soNgayNghiBu ?? 0))
            //        .Replace("{$nghiPhep}", Convert.ToString(ds.soNgayNghiPhep ?? 0))
            //        .Replace("{$nghiLeTet}", Convert.ToString(ds.soNgayNghiLe ?? 0))
            //        .Replace("{$luyKeThangTruoc}", Convert.ToString(ds.soNgayPhepLuyKeThangTruoc ?? 0))
            //        .Replace("{$nghiKhongLuong}", Convert.ToString(ds.soNgayNghiKhongLuong ?? 0))
            //        //Lương Theo Ngày Công
            //        .Replace("{$luongTheoNgayCong}", String.Format("{0:###,##0}", ds.luongThang ?? 0))
            //        //Phụ Cấp Khác Và Truy Lãnh

            //        .Replace("{$tongPhuCapTruyLanh}", String.Format("{0:###,##0}", tongPhuCapTruyLanh))
            //        .Replace("{$tienPhuCapCongTacTD}", String.Format("{0:###,##0}", ds.congTacPhi ?? 0))
            //        .Replace("{$tienTTTLBaoHiem}", String.Format("{0:###,##0}", ds.TTTLBaoHiem ?? 0))
            //        .Replace("{$tienTTTLThue}", String.Format("{0:###,##0}", ds.TTTLThue ?? 0))
            //        .Replace("{$tienTTTLTamUng}", String.Format("{0:###,##0}", ds.TTTLTamUng ?? 0))
            //        .Replace("{$tienTTTLLuong}", String.Format("{0:###,##0}", ds.TTTLLuong ?? 0))
            //        .Replace("{$TienPhuCapKhac}", String.Format("{0:###,##0}", ds.phuCapKhac ?? 0))
            //        //Các Khoản Khấu Trừ
            //        .Replace("{$tongKhauTru}", String.Format("{0:###,##0}", tongBaoHiem + (ds.thue ?? 0)))
            //        .Replace("{$tongBaoHiem}", String.Format("{0:###,##0}", tongBaoHiem))
            //        .Replace("{$tienThueTNCC}", String.Format("{0:###,##0}", ds.thue ?? 0))
            //        .Replace("{$tienGiamTruGC}", String.Format("{0:###,##0}", (ds.giamTruBanThan ?? 0) + (ds.giamTruNguoiPhuThuoc ?? 0)))
            //        .Replace("{$tongThucNhanLuong}", String.Format("{0:###,##0}", ds.thucLanh ?? 0));
            //    //End
            //    // Code send mail
            //    MailHelper mailInit = new MailHelper(); // lay cac tham so trong webconfig
            //    System.Text.StringBuilder content = new System.Text.StringBuilder();

            //    content.Append("<h3>Email từ hệ thống nhân sự</h3>");
            //    content.Append("<p>Xin chào: " + ds.hoTen + " !</p>");
            //    //Content
            //    content.Append(noiDung);

            //    //End content
            //    content.Append("<p style='font-style:italic'>Thanks and Regards!</p>");
            //    //Send only email is @thuanviet.com.vn
            //    string[] array01 = ds.email.ToLower().Split('@');
            //    //string string2 = ConfigurationManager.AppSettings["OnlySend"]; //get domain from config files
            //    //string[] array1 = string2.Split(',');
            //    // bool EmailofThuanViet;
            //    //EmailofThuanViet = array1.Contains(array01[1]);
            //    // if (emailNV == "" || emailNV == null || EmailofThuanViet == false)
            //    // {
            //    //    return false;
            //    // }
            //    MailAddress toMail = new MailAddress(ds.email, ds.hoTen); // goi den mail
            //    mailInit.ToMail = toMail;
            //    mailInit.Body = content.ToString();
            //    mailInit.SendMail();
            //    // End code send mail
            //}
            //result = new { kq = true };
            //SaveActiveHistory("Send mail bảng lương nhân viên: " + thang + " năm: " + nam);
            return Json(result, JsonRequestBehavior.AllowGet);
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
        public void XuatFileBLNVRemove(int thang, int nam)
        {

            try
            {

                string maPhongBan = "";
                string qSearch = "";
                var filename = "";
                var virtualPath = HttpRuntime.AppDomainAppVirtualPath;

                var fileStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(virtualPath + @"\Content\Report\ReportTemplate.xls"), FileMode.Open, FileAccess.Read);

                var workbook = new HSSFWorkbook(fileStream, true);
                filename += "BangLuongNV_" + nam + "_" + thang + ".xls";

                #region format style excel cell
                /*style title start*/
                //tạo font cho các title
                //font tiêu đề 
                HSSFFont hFontTieuDe = (HSSFFont)workbook.CreateFont();
                hFontTieuDe.FontHeightInPoints = 11;
                hFontTieuDe.Boldweight = 100 * 10;
                hFontTieuDe.FontName = "Times New Roman";
                //hFontTieuDe.Color = HSSFColor.BLUE.index;

                //font tiêu đề 
                HSSFFont hFontTieuDeUnderline = (HSSFFont)workbook.CreateFont();
                hFontTieuDeUnderline.FontHeightInPoints = 11;
                hFontTieuDeUnderline.Boldweight = 100 * 10;
                hFontTieuDeUnderline.FontName = "Times New Roman";
                hFontTieuDeUnderline.Underline = 1;
                //hFontTieuDe.Color = HSSFColor.BLUE.index;


                HSSFFont hFontTieuDeItalic = (HSSFFont)workbook.CreateFont();
                hFontTieuDeItalic.FontHeightInPoints = 11;
                //hFontTieuDeItalic.Boldweight = 100 * 10;
                hFontTieuDeItalic.FontName = "Times New Roman";
                hFontTieuDeItalic.IsItalic = true;
                //hFontTieuDe.Color = HSSFColor.BLUE.index;


                HSSFFont hFontTieuDeLarge = (HSSFFont)workbook.CreateFont();
                hFontTieuDeLarge.FontHeightInPoints = 16;
                hFontTieuDeLarge.Boldweight = 100 * 10;
                hFontTieuDeLarge.FontName = "Times New Roman";
                //hFontTieuDeLarge.Color = HSSFColor.BLUE.index;

                //font tiêu đề 
                HSSFFont hFontTongGiaTriHT = (HSSFFont)workbook.CreateFont();
                hFontTongGiaTriHT.FontHeightInPoints = 11;
                hFontTongGiaTriHT.Boldweight = (short)FontBoldWeight.BOLD;
                hFontTongGiaTriHT.FontName = "Times New Roman";
                hFontTongGiaTriHT.Color = HSSFColor.BLACK.index;

                //font thông tin bảng tính
                HSSFFont hFontTT = (HSSFFont)workbook.CreateFont();
                hFontTT.IsItalic = true;
                hFontTT.Boldweight = (short)FontBoldWeight.BOLD;
                hFontTT.Color = HSSFColor.BLACK.index;
                hFontTT.FontName = "Times New Roman";
                hFontTieuDe.FontHeightInPoints = 11;

                //font chứ hoa đậm
                HSSFFont hFontNommalUpper = (HSSFFont)workbook.CreateFont();
                hFontNommalUpper.Boldweight = (short)FontBoldWeight.BOLD;
                hFontNommalUpper.Color = HSSFColor.BLACK.index;
                hFontNommalUpper.FontName = "Times New Roman";

                //font chữ bình thường
                HSSFFont hFontNommal = (HSSFFont)workbook.CreateFont();
                hFontNommal.Color = HSSFColor.BLACK.index;
                hFontNommal.FontName = "Times New Roman";

                //font chữ bình thường đậm
                HSSFFont hFontNommalBold = (HSSFFont)workbook.CreateFont();
                hFontNommalBold.Color = HSSFColor.BLACK.index;
                hFontNommalBold.Boldweight = (short)FontBoldWeight.BOLD;
                hFontNommalBold.FontName = "Times New Roman";

                //tạo font cho các title end

                //Set style
                var styleTitle = workbook.CreateCellStyle();
                styleTitle.SetFont(hFontTieuDe);
                styleTitle.Alignment = HorizontalAlignment.LEFT;

                //Set styleUnderline
                var styleTitleUnderline = workbook.CreateCellStyle();
                styleTitleUnderline.SetFont(hFontTieuDeUnderline);
                styleTitleUnderline.Alignment = HorizontalAlignment.LEFT;

                //Set style In nghiêng
                var styleTitleItalic = workbook.CreateCellStyle();
                styleTitleItalic.SetFont(hFontTieuDeItalic);
                styleTitleItalic.Alignment = HorizontalAlignment.LEFT;

                //Set style Large font
                var styleTitleLarge = workbook.CreateCellStyle();
                styleTitleLarge.SetFont(hFontTieuDeLarge);
                styleTitleLarge.Alignment = HorizontalAlignment.LEFT;

                //style infomation
                var styleInfomation = workbook.CreateCellStyle();
                styleInfomation.SetFont(hFontTT);
                styleInfomation.Alignment = HorizontalAlignment.LEFT;

                //style header
                var styleheadedColumnTable = workbook.CreateCellStyle();
                styleheadedColumnTable.SetFont(hFontNommalUpper);
                styleheadedColumnTable.WrapText = true;
                styleheadedColumnTable.BorderBottom = CellBorderType.THIN;
                styleheadedColumnTable.BorderLeft = CellBorderType.THIN;
                styleheadedColumnTable.BorderRight = CellBorderType.THIN;
                styleheadedColumnTable.BorderTop = CellBorderType.THIN;
                styleheadedColumnTable.VerticalAlignment = VerticalAlignment.CENTER;
                styleheadedColumnTable.Alignment = HorizontalAlignment.CENTER;

                //style sum cell
                var styleCellSumary = workbook.CreateCellStyle();
                styleCellSumary.SetFont(hFontNommalUpper);
                styleCellSumary.WrapText = true;
                styleCellSumary.BorderBottom = CellBorderType.THIN;
                styleCellSumary.BorderLeft = CellBorderType.THIN;
                styleCellSumary.BorderRight = CellBorderType.THIN;
                styleCellSumary.BorderTop = CellBorderType.THIN;
                styleCellSumary.VerticalAlignment = VerticalAlignment.CENTER;
                styleCellSumary.Alignment = HorizontalAlignment.RIGHT;

                var styleHeading1 = workbook.CreateCellStyle();
                styleHeading1.SetFont(hFontNommalBold);
                styleHeading1.WrapText = true;
                styleHeading1.BorderBottom = CellBorderType.THIN;
                styleHeading1.BorderLeft = CellBorderType.THIN;
                styleHeading1.BorderRight = CellBorderType.THIN;
                styleHeading1.BorderTop = CellBorderType.THIN;
                styleHeading1.VerticalAlignment = VerticalAlignment.CENTER;
                styleHeading1.Alignment = HorizontalAlignment.LEFT;

                var hStyleConLeft = (HSSFCellStyle)workbook.CreateCellStyle();
                hStyleConLeft.SetFont(hFontNommal);
                hStyleConLeft.VerticalAlignment = VerticalAlignment.TOP;
                hStyleConLeft.Alignment = HorizontalAlignment.LEFT;
                hStyleConLeft.WrapText = true;
                hStyleConLeft.BorderBottom = CellBorderType.THIN;
                hStyleConLeft.BorderLeft = CellBorderType.THIN;
                hStyleConLeft.BorderRight = CellBorderType.THIN;
                hStyleConLeft.BorderTop = CellBorderType.THIN;

                var hStyleConRight = (HSSFCellStyle)workbook.CreateCellStyle();
                hStyleConRight.SetFont(hFontNommal);
                hStyleConRight.VerticalAlignment = VerticalAlignment.TOP;
                hStyleConRight.Alignment = HorizontalAlignment.RIGHT;
                hStyleConRight.BorderBottom = CellBorderType.THIN;
                hStyleConRight.BorderLeft = CellBorderType.THIN;
                hStyleConRight.BorderRight = CellBorderType.THIN;
                hStyleConRight.BorderTop = CellBorderType.THIN;


                var hStyleConCenter = (HSSFCellStyle)workbook.CreateCellStyle();
                hStyleConCenter.SetFont(hFontNommal);
                hStyleConCenter.VerticalAlignment = VerticalAlignment.TOP;
                hStyleConCenter.Alignment = HorizontalAlignment.CENTER;
                hStyleConCenter.BorderBottom = CellBorderType.THIN;
                hStyleConCenter.BorderLeft = CellBorderType.THIN;
                hStyleConCenter.BorderRight = CellBorderType.THIN;
                hStyleConCenter.BorderTop = CellBorderType.THIN;
                //set style end
                #endregion

                //Khai báo row
                Row rowC = null;


                //Group lại theo phòng ban, mỗi phòng ban là một sheet
                var danhSachBLGroupBys = nhanSuContext.sp_NS_BangLuongNhanVien(maPhongBan, thang, nam, qSearch).GroupBy(s => new { s.phongBan });
                foreach (var item in danhSachBLGroupBys)
                {
                    int count = 1;
                    var sheet = workbook.CreateSheet(item.Key.phongBan);

                    //Khai báo row đầu tiên
                    int firstRowNumber = 3;

                    string cellTenCty = "TỔNG CÔNG TY XDCTGT 6 - CÔNG TY CỔ PHẦN";
                    var titleCellCty = HSSFCellUtil.CreateCell(sheet.CreateRow(0), 0, cellTenCty.ToUpper());
                    titleCellCty.CellStyle = styleTitle;

                    string cellTitleMain = "BẢNG TỔNG HỢP LƯƠNG THÁNG " + thang + "/" + nam;
                    var titleCellTitleMain = HSSFCellUtil.CreateCell(sheet.CreateRow(1), 5, cellTitleMain.ToUpper());
                    titleCellTitleMain.CellStyle = styleTitleLarge;

                    firstRowNumber++;

                    var list1 = new List<string>();
                    list1.Add("STT");
                    list1.Add("Họ và Tên");
                    list1.Add("Tổng mức lương");// trường này sẽ colspan = 2
                    list1.Add("");
                    list1.Add("Khoản bổ sung");
                    list1.Add("Công");
                    list1.Add("Tổng cộng");
                    list1.Add("Lễ, phép");
                    list1.Add("Lương lễ, phép");
                    list1.Add("Truy lĩnh");
                    list1.Add("Thực lĩnh");
                    list1.Add("BHXH + Y tế + TN");
                    list1.Add("Thuế TNCN");
                    list1.Add("Truy thu");
                    list1.Add("Đoàn phí");
                    list1.Add("Đảng phí");
                    list1.Add("Tiền ăn giữa ca");
                    list1.Add("Còn lãnh");


                    var list2 = new List<string>();
                    list2.Add("STT");
                    list2.Add("Đơn vị");
                    list2.Add("Lương");// trường này sẽ colspan = 2
                    list2.Add("Phụ cấp lương");
                    list2.Add("Khoản bổ sung");
                    list2.Add("Công");
                    list2.Add("Tổng cộng");
                    list2.Add("Lễ, phép");
                    list2.Add("Lương lễ, phép");
                    list2.Add("Truy lĩnh");
                    list2.Add("Thực lĩnh");
                    list2.Add("BHXH + Y tế + TN");
                    list2.Add("Thuế TNCN");
                    list2.Add("Truy thu");
                    list2.Add("Đoàn phí");
                    list2.Add("Đảng phí");
                    list2.Add("Tiền ăn giữa ca");
                    list2.Add("Còn lãnh");

                    var idRowStart = firstRowNumber; // bat dau o dong thu 4
                    var headerRow = sheet.CreateRow(idRowStart);
                    int rowend = idRowStart;
                    ReportHelperExcel.CreateHeaderRow(headerRow, 0, styleheadedColumnTable, list1);
                    idRowStart++;
                    var headerRow1 = sheet.CreateRow(idRowStart);
                    ReportHelperExcel.CreateHeaderRow(headerRow1, 0, styleheadedColumnTable, list2);

                    sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 17, 17));
                    sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 16, 16));
                    sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 15, 15));
                    sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 14, 14));
                    sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 13, 13));
                    sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 12, 12));
                    sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 11, 11));
                    sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 10, 10));
                    sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 9, 9));
                    sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 8, 8));
                    sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 7, 7));
                    sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 6, 6));
                    sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 5, 5));
                    sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 4, 4));
                    sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow.RowNum, 2, 3));
                    sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 1, 1));
                    sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 0, 0));



                    sheet.SetColumnWidth(0, 8 * 210);
                    sheet.SetColumnWidth(1, 30 * 210);
                    sheet.SetColumnWidth(2, 15 * 210);
                    sheet.SetColumnWidth(3, 15 * 210);
                    sheet.SetColumnWidth(4, 15 * 210);
                    sheet.SetColumnWidth(5, 15 * 210);
                    sheet.SetColumnWidth(6, 15 * 210);
                    sheet.SetColumnWidth(7, 15 * 210);
                    sheet.SetColumnWidth(8, 15 * 210);
                    sheet.SetColumnWidth(9, 15 * 210);
                    sheet.SetColumnWidth(10, 15 * 210);
                    sheet.SetColumnWidth(11, 15 * 210);

                    sheet.SetColumnWidth(12, 15 * 210);
                    sheet.SetColumnWidth(13, 15 * 210);
                    sheet.SetColumnWidth(14, 15 * 210);
                    sheet.SetColumnWidth(15, 10 * 210);
                    sheet.SetColumnWidth(16, 15 * 210);
                    sheet.SetColumnWidth(17, 25 * 210);

                    var stt = 0;
                    int dem = 0;
                    double? sumLuongLe = 0;
                    string tenPhongBan = string.Empty;
                    if (item.Count() > 0)
                    {
                        tenPhongBan = item.Key.phongBan;
                    }

                    string cellTitlePhongBan = count.ToString() + ". " + tenPhongBan;
                    var titleCellTitlePhongBan = HSSFCellUtil.CreateCell(sheet.CreateRow(2), 0, cellTitlePhongBan);
                    titleCellTitlePhongBan.CellStyle = styleTitleUnderline;
                    count++;
                    //Giai đoạn
                    foreach (var item1 in item)
                    {
                        dem = 0;
                        double? luongNghiLePhep = ((item1.tongLuong ?? 0) * ((item1.soNgayNghiLe ?? 0) + (item1.soNgayNghiPhep ?? 0))) / item1.ngayCongChuan;
                        sumLuongLe += luongNghiLePhep;
                        stt++;
                        idRowStart++;

                        rowC = sheet.CreateRow(idRowStart);
                        ReportHelperExcel.SetAlignment(rowC, dem++, stt.ToString(), hStyleConCenter);
                        ReportHelperExcel.SetAlignment(rowC, dem++, item1.hoTen, hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item1.luongThang), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item1.phuCapLuong), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item1.khoanBoSungLuong), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", (item1.soNgayCongTac ?? 0) + (item1.soNgayQuet ?? 0)), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item1.tongLuong), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", (item1.soNgayNghiLe ?? 0) + (item1.soNgayNghiPhep ?? 0)), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", luongNghiLePhep), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item1.truyLanh), hStyleConRight);

                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item1.thucLanh), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", (item1.baoHiemTN + item1.baoHiemXH + item1.baoHiemYTe)), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item1.thue), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item1.truyThu), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item1.doanPhi), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item1.dangPhi), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item1.phuCapTienAn), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item1.thucLanh), hStyleConRight);
                    }

                    int demT = 0;
                    idRowStart++;
                    Row rowT = sheet.CreateRow(idRowStart);
                    ReportHelperExcel.SetAlignment(rowT, demT++, (stt).ToString(), styleheadedColumnTable);
                    ReportHelperExcel.SetAlignment(rowT, demT++, "", styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", item.Sum(s => s.luongThang)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", item.Sum(s => s.phuCapLuong)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", item.Sum(s => s.khoanBoSungLuong)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", item.Sum(s => ((s.soNgayCongTac ?? 0) + (s.soNgayQuet ?? 0)))), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", item.Sum(s => s.tongLuong)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", item.Sum(s => ((s.soNgayNghiLe ?? 0) + (s.soNgayNghiPhep ?? 0)))), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", sumLuongLe), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", item.Sum(s => s.truyLanh)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", item.Sum(s => s.thucLanh)), styleCellSumary);

                    ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", item.Sum(s => (s.baoHiemTN + s.baoHiemXH + s.baoHiemYTe))), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", item.Sum(s => s.thue)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", item.Sum(s => s.truyThu)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", item.Sum(s => s.doanPhi)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", item.Sum(s => s.dangPhi)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", item.Sum(s => s.phuCapTienAn)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", item.Sum(s => s.thucLanh)), styleCellSumary);

                    idRowStart = idRowStart + 2;
                    string cellFooterSoTien = "(" + CharacterHelper.DocTienBangChu((decimal)item.Sum(s => s.thucLanh), string.Empty) + ")";
                    var titleCellFooterSoTien = HSSFCellUtil.CreateCell(sheet.CreateRow(idRowStart), 12, cellFooterSoTien);
                    titleCellFooterSoTien.CellStyle = styleTitleItalic;

                    idRowStart = idRowStart + 2;
                    string cellFooterNgayLap = "Tp.Hồ Chí Minh, ngày   tháng  năm " + nam;
                    var titleCellFooterNgayLap = HSSFCellUtil.CreateCell(sheet.CreateRow(idRowStart), 13, cellFooterNgayLap);
                    titleCellFooterNgayLap.CellStyle = styleTitleItalic;

                    idRowStart = idRowStart + 2;
                    string cellFooterPTC = "PHÒNG TỔ CHỨC CB-LĐ";
                    var titleCellFooterPTC = HSSFCellUtil.CreateCell(sheet.CreateRow(idRowStart), 1, cellFooterPTC);
                    titleCellFooterPTC.CellStyle = styleTitle;

                    string cellFooterKT = "PHÒNG TÀI CHÍNH KẾ TOÁN";
                    var titleCellFooterKT = HSSFCellUtil.CreateCell(sheet.GetRow(idRowStart), 7, cellFooterKT);
                    titleCellFooterKT.CellStyle = styleTitle;

                    string cellFooterTGD = "TỔNG GIÁM ĐỐC";
                    var titleCellFooterTGD = HSSFCellUtil.CreateCell(sheet.GetRow(idRowStart), 14, cellFooterTGD);
                    titleCellFooterTGD.CellStyle = styleTitle;
                }

                var stream = new MemoryStream();
                workbook.Write(stream);

                Response.ContentType = "application/vnd.ms-excel";
                Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", filename));
                Response.Clear();

                Response.BinaryWrite(stream.GetBuffer());
                Response.End();
            }
            catch
            {

            }
        }
        public void XuatFileBLNV(int thang, int nam, string maPhongBan, string qSearch)
        {
            var filename = "";
            var virtualPath = HttpRuntime.AppDomainAppVirtualPath;
            var fileStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(virtualPath + @"\Content\Report\ReportTemplate.xls"), FileMode.Open, FileAccess.Read);

            var workbook = new HSSFWorkbook(fileStream, true);
            filename += "BangLuongNV_" + thang + "_" + nam + ".xls";


            var sheet = workbook.GetSheet("danhsachnhanvien");

            /*style title start*/
            //tạo font cho các title
            //font tiêu đề 
            HSSFFont hFontTieuDe = (HSSFFont)workbook.CreateFont();
            hFontTieuDe.FontHeightInPoints = 18;
            hFontTieuDe.Boldweight = 100 * 10;
            hFontTieuDe.FontName = "Times New Roman";
            hFontTieuDe.Color = HSSFColor.BLUE.index;
            HSSFFont hFontTieuDe2 = (HSSFFont)workbook.CreateFont();
            hFontTieuDe2.FontHeightInPoints = 15;
            hFontTieuDe2.Boldweight = 100 * 10;
            hFontTieuDe2.FontName = "Times New Roman";
            hFontTieuDe2.Color = HSSFColor.BLACK.index;

            //font tiêu đề 
            HSSFFont hFontTongGiaTriHT = (HSSFFont)workbook.CreateFont();
            hFontTongGiaTriHT.FontHeightInPoints = 11;
            hFontTongGiaTriHT.Boldweight = (short)FontBoldWeight.BOLD;
            hFontTongGiaTriHT.FontName = "Times New Roman";
            hFontTongGiaTriHT.Color = HSSFColor.BLACK.index;

            //font thông tin bảng tính
            HSSFFont hFontTT = (HSSFFont)workbook.CreateFont();
            hFontTT.IsItalic = true;
            hFontTT.Boldweight = (short)FontBoldWeight.BOLD;
            hFontTT.Color = HSSFColor.BLACK.index;
            hFontTT.FontName = "Times New Roman";
            hFontTieuDe.FontHeightInPoints = 11;

            //font chứ hoa đậm
            HSSFFont hFontNommalUpper = (HSSFFont)workbook.CreateFont();
            hFontNommalUpper.Boldweight = (short)FontBoldWeight.BOLD;
            hFontNommalUpper.Color = HSSFColor.BLACK.index;
            hFontNommalUpper.FontName = "Times New Roman";

            //font chữ bình thường
            HSSFFont hFontNommal = (HSSFFont)workbook.CreateFont();
            hFontNommal.Color = HSSFColor.BLACK.index;
            hFontNommal.FontName = "Times New Roman";

            //font chữ bình thường đậm
            HSSFFont hFontNommalBold = (HSSFFont)workbook.CreateFont();
            hFontNommalBold.Color = HSSFColor.BLACK.index;
            hFontNommalBold.Boldweight = (short)FontBoldWeight.BOLD;
            hFontNommalBold.FontName = "Times New Roman";

            //tạo font cho các title end

            //Set style
            var styleTitle = workbook.CreateCellStyle();
            styleTitle.SetFont(hFontTieuDe);
            styleTitle.Alignment = HorizontalAlignment.LEFT;
            var styleTitle1 = workbook.CreateCellStyle();
            styleTitle1.SetFont(hFontTieuDe2);
            styleTitle1.Alignment = HorizontalAlignment.CENTER;

            //style infomation
            var styleInfomation = workbook.CreateCellStyle();
            styleInfomation.SetFont(hFontTT);
            styleInfomation.Alignment = HorizontalAlignment.LEFT;

            //style header
            var styleheadedColumnTable = workbook.CreateCellStyle();
            styleheadedColumnTable.SetFont(hFontNommalUpper);
            styleheadedColumnTable.WrapText = true;
            styleheadedColumnTable.BorderBottom = CellBorderType.THIN;
            styleheadedColumnTable.BorderLeft = CellBorderType.THIN;
            styleheadedColumnTable.BorderRight = CellBorderType.THIN;
            styleheadedColumnTable.BorderTop = CellBorderType.THIN;
            styleheadedColumnTable.VerticalAlignment = VerticalAlignment.CENTER;
            styleheadedColumnTable.Alignment = HorizontalAlignment.CENTER;

            var styleHeading1 = workbook.CreateCellStyle();
            styleHeading1.SetFont(hFontNommalBold);
            styleHeading1.WrapText = true;
            styleHeading1.BorderBottom = CellBorderType.THIN;
            styleHeading1.BorderLeft = CellBorderType.THIN;
            styleHeading1.BorderRight = CellBorderType.THIN;
            styleHeading1.BorderTop = CellBorderType.THIN;
            styleHeading1.VerticalAlignment = VerticalAlignment.CENTER;
            styleHeading1.Alignment = HorizontalAlignment.LEFT;

            var hStyleConLeft = (HSSFCellStyle)workbook.CreateCellStyle();
            hStyleConLeft.SetFont(hFontNommal);
            hStyleConLeft.VerticalAlignment = VerticalAlignment.TOP;
            hStyleConLeft.Alignment = HorizontalAlignment.LEFT;
            hStyleConLeft.WrapText = true;
            hStyleConLeft.BorderBottom = CellBorderType.THIN;
            hStyleConLeft.BorderLeft = CellBorderType.THIN;
            hStyleConLeft.BorderRight = CellBorderType.THIN;
            hStyleConLeft.BorderTop = CellBorderType.THIN;

            var hStyleConRight = (HSSFCellStyle)workbook.CreateCellStyle();
            hStyleConRight.SetFont(hFontNommal);
            hStyleConRight.VerticalAlignment = VerticalAlignment.TOP;
            hStyleConRight.Alignment = HorizontalAlignment.RIGHT;
            hStyleConRight.BorderBottom = CellBorderType.THIN;
            hStyleConRight.BorderLeft = CellBorderType.THIN;
            hStyleConRight.BorderRight = CellBorderType.THIN;
            hStyleConRight.BorderTop = CellBorderType.THIN;


            var hStyleConCenter = (HSSFCellStyle)workbook.CreateCellStyle();
            hStyleConCenter.SetFont(hFontNommal);
            hStyleConCenter.VerticalAlignment = VerticalAlignment.TOP;
            hStyleConCenter.Alignment = HorizontalAlignment.CENTER;
            hStyleConCenter.BorderBottom = CellBorderType.THIN;
            hStyleConCenter.BorderLeft = CellBorderType.THIN;
            hStyleConCenter.BorderRight = CellBorderType.THIN;
            hStyleConCenter.BorderTop = CellBorderType.THIN;
            //set style end


            Row rowC = null;
            //Khai báo row đầu tiên
            int firstRowNumber = 1;

            string rowtitle = "BẢNG LƯƠNG NHÂN VIÊN CHÍNH THỨC";
            var titleCell = HSSFCellUtil.CreateCell(sheet.CreateRow(firstRowNumber), 6, rowtitle.ToUpper());
            titleCell.CellStyle = styleTitle;
            firstRowNumber = firstRowNumber + 2;
            string rowtitle1 = "Tháng: " + thang + " năm: " + nam;
            var titleCell1 = HSSFCellUtil.CreateCell(sheet.CreateRow(firstRowNumber), 6, rowtitle1);
            titleCell1.CellStyle = styleTitle1;

            firstRowNumber = firstRowNumber + 2;

            var list1 = new List<string>();
            list1.Add("STT");
            list1.Add("Mã nhân viên");
            list1.Add("Họ và tên");
            list1.Add("Họ và tên");
            list1.Add("CMND");
            list1.Add("STK");
            list1.Add("Bộ Phận Tính Lương");
            list1.Add("Bộ phận");
            list1.Add("Chức danh");
            list1.Add("Tổng lương");
            list1.Add("Lương bảo hiểm");
            list1.Add("Khoản bổ sung lương");
            list1.Add("Phụ cấp công trình");
            list1.Add("Ngày công chuẩn");
            list1.Add("Ngày quét thực tế & công tác");
            list1.Add("Ngày nghỉ phép năm");
            list1.Add("Ngày nghỉ bù");
            list1.Add("Ngày nghỉ lễ, tết");
            list1.Add("Ngày lũy kế tháng trước");
            list1.Add("Tổng số ngày tính lương");
            list1.Add("Lương tháng");
            list1.Add("Bảo hiểm");
            list1.Add("Giảm trừ bản thân");
            list1.Add("Giảm trừ người phụ thuộc");
            list1.Add("Tổng thu nhập chịu thuế");
            list1.Add("Thuế");
            list1.Add("Phụ cấp công tác");
            list1.Add("TTTL Tạm ứng");
            list1.Add("TTTL Lương");
            list1.Add("TTTL Thuế");
            list1.Add("TTTL Bảo hiểm");
            list1.Add("Thực chuyển đợt này");

            //Start row 13
            var headerRow = sheet.CreateRow(6);
            ReportHelperExcel.CreateHeaderRow(headerRow, 0, styleheadedColumnTable, list1);

            //Create header end

            var idRowStart = 6;
            var datas = nhanSuContext.sp_NS_BangLuongNhanVien(maPhongBan, thang, nam, qSearch).ToList();
            //#region
            if (datas != null && datas.Count > 0)
            {
                var stt = 0;
                int dem = 0;
                //Giai đoạn
                foreach (var item1 in datas)
                {
                    dem = 0;
                    stt++;
                    idRowStart++;
                    rowC = sheet.CreateRow(idRowStart);
                    ReportHelperExcel.SetAlignment(rowC, dem++, stt.ToString(), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.maNhanVien, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.hoTen, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.hoTenCoDau, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.soCMND, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.soTaiKhoan, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.boPhanTinhLuong, hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.phongBan, hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.chucVu, hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item1.tongLuong), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item1.luongDongBaoHiem), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item1.khoanBoSungLuong), hStyleConRight);

                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item1.phuCapCongTrinh), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item1.ngayCongChuan), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", (item1.soNgayCongTac ?? 0) + (item1.soNgayQuet ?? 0)), hStyleConRight);
                    //Ngày nghỉ phép năm
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item1.soNgayNghiPhep), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item1.soNgayNghiBu), hStyleConRight);
                    //Ngày nghỉ lễ, tết
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item1.soNgayNghiLe), hStyleConRight);
                    //Ngày lũy kế tháng trước
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item1.soNgayPhepLuyKeThangTruoc), hStyleConRight);
                    //Tổng số ngày tính lương
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item1.tongNgayCong), hStyleConRight);
                    //Lương tháng
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item1.luongThang), hStyleConRight);
                    //Bảo hiểm
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item1.baoHiem ?? 0), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item1.giamTruBanThan), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item1.giamTruNguoiPhuThuoc), hStyleConRight);
                    //Tổng thu nhập chịu thuế

                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item1.tongThuNhapChiuThue), hStyleConRight);

                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item1.thue), hStyleConRight);
                    //Phụ cấp công tác
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item1.congTacPhi), hStyleConRight);
                    //TTTLTamUng
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item1.TTTLTamUng), hStyleConRight);
                    //TTTLLuong
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item1.TTTLLuong), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item1.TTTLThue), hStyleConRight);
                    //TTTLThue
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item1.TTTLBaoHiem), hStyleConRight);
                    //Thực chuyển đợt này
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item1.thucLanh), hStyleConRight);
                }

                sheet.SetColumnWidth(0, 8 * 210);
                sheet.SetColumnWidth(1, 30 * 210);
                sheet.SetColumnWidth(2, 30 * 210);
                sheet.SetColumnWidth(3, 30 * 210);
                sheet.SetColumnWidth(4, 30 * 210);
                sheet.SetColumnWidth(5, 30 * 210);
                sheet.SetColumnWidth(6, 30 * 210);
                sheet.SetColumnWidth(7, 30 * 210);
                sheet.SetColumnWidth(8, 30 * 210);
                sheet.SetColumnWidth(9, 30 * 210);
                sheet.SetColumnWidth(10, 30 * 210);
                sheet.SetColumnWidth(11, 30 * 210);

                sheet.SetColumnWidth(12, 30 * 210);
                sheet.SetColumnWidth(13, 30 * 210);
                sheet.SetColumnWidth(14, 30 * 210);
                sheet.SetColumnWidth(15, 30 * 210);
                sheet.SetColumnWidth(16, 30 * 210);
                sheet.SetColumnWidth(17, 30 * 210);
                sheet.SetColumnWidth(18, 30 * 210);
                sheet.SetColumnWidth(19, 30 * 210);
                sheet.SetColumnWidth(20, 30 * 210);
                sheet.SetColumnWidth(21, 30 * 210);
                sheet.SetColumnWidth(22, 30 * 210);
                sheet.SetColumnWidth(23, 30 * 210);
                sheet.SetColumnWidth(24, 30 * 210);
                sheet.SetColumnWidth(25, 30 * 210);
                sheet.SetColumnWidth(26, 30 * 210);
                sheet.SetColumnWidth(27, 30 * 210);
                sheet.SetColumnWidth(28, 30 * 210);
                sheet.SetColumnWidth(29, 30 * 210);
                sheet.SetColumnWidth(30, 30 * 210);
                sheet.SetColumnWidth(31, 30 * 210);
                sheet.SetColumnWidth(32, 30 * 210);
                sheet.SetColumnWidth(33, 30 * 210);
                sheet.SetColumnWidth(34, 30 * 210);
                sheet.SetColumnWidth(35, 30 * 210);
            }
            else
            {

                sheet.SetColumnWidth(0, 8 * 210);
                sheet.SetColumnWidth(1, 30 * 210);
                sheet.SetColumnWidth(2, 30 * 210);
                sheet.SetColumnWidth(3, 30 * 210);
                sheet.SetColumnWidth(4, 30 * 210);
                sheet.SetColumnWidth(5, 30 * 210);
                sheet.SetColumnWidth(6, 30 * 210);
                sheet.SetColumnWidth(7, 30 * 210);
                sheet.SetColumnWidth(8, 30 * 210);
                sheet.SetColumnWidth(9, 30 * 210);
                sheet.SetColumnWidth(10, 30 * 210);
                sheet.SetColumnWidth(11, 30 * 210);

                sheet.SetColumnWidth(12, 30 * 210);
                sheet.SetColumnWidth(13, 30 * 210);
                sheet.SetColumnWidth(14, 30 * 210);
                sheet.SetColumnWidth(15, 30 * 210);
                sheet.SetColumnWidth(16, 30 * 210);
                sheet.SetColumnWidth(17, 30 * 210);
                sheet.SetColumnWidth(18, 30 * 210);
                sheet.SetColumnWidth(19, 30 * 210);
                sheet.SetColumnWidth(20, 30 * 210);
                sheet.SetColumnWidth(21, 30 * 210);
                sheet.SetColumnWidth(22, 30 * 210);
                sheet.SetColumnWidth(23, 30 * 210);
                sheet.SetColumnWidth(24, 30 * 210);
                sheet.SetColumnWidth(25, 30 * 210);
                sheet.SetColumnWidth(26, 30 * 210);
                sheet.SetColumnWidth(27, 30 * 210);
                sheet.SetColumnWidth(28, 30 * 210);
                sheet.SetColumnWidth(29, 30 * 210);
                sheet.SetColumnWidth(30, 30 * 210);
                sheet.SetColumnWidth(31, 30 * 210);
                sheet.SetColumnWidth(32, 30 * 210);
                sheet.SetColumnWidth(33, 30 * 210);
                sheet.SetColumnWidth(34, 30 * 210);
                sheet.SetColumnWidth(35, 30 * 210);

            }

            var stream = new MemoryStream();
            workbook.Write(stream);

            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", filename));
            Response.Clear();

            Response.BinaryWrite(stream.GetBuffer());
            Response.End();

        }

        #endregion

        #region  Xuat File Bang Luong Nhan Vien theo bộ phận
        public void XuatFileBLNVBoPhan(int thang, int nam)
        {
            try
            {
                var filename = "";
                var virtualPath = HttpRuntime.AppDomainAppVirtualPath;

                var fileStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(virtualPath + @"\Content\Report\ReportTemplate.xls"), FileMode.Open, FileAccess.Read);

                var workbook = new HSSFWorkbook(fileStream, true);
                filename += "BangTongHopLuongCacBoPhan_" + nam + "_" + thang + ".xls";

                #region format style excel cell
                /*style title start*/
                //tạo font cho các title
                //font tiêu đề 
                HSSFFont hFontTieuDe = (HSSFFont)workbook.CreateFont();
                hFontTieuDe.FontHeightInPoints = 11;
                hFontTieuDe.Boldweight = 100 * 10;
                hFontTieuDe.FontName = "Times New Roman";
                //hFontTieuDe.Color = HSSFColor.BLUE.index;

                //font tiêu đề 
                HSSFFont hFontTieuDeUnderline = (HSSFFont)workbook.CreateFont();
                hFontTieuDeUnderline.FontHeightInPoints = 11;
                hFontTieuDeUnderline.Boldweight = 100 * 10;
                hFontTieuDeUnderline.FontName = "Times New Roman";
                hFontTieuDeUnderline.Underline = 1;
                //hFontTieuDe.Color = HSSFColor.BLUE.index;


                HSSFFont hFontTieuDeItalic = (HSSFFont)workbook.CreateFont();
                hFontTieuDeItalic.FontHeightInPoints = 11;
                //hFontTieuDeItalic.Boldweight = 100 * 10;
                hFontTieuDeItalic.FontName = "Times New Roman";
                hFontTieuDeItalic.IsItalic = true;
                //hFontTieuDe.Color = HSSFColor.BLUE.index;


                HSSFFont hFontTieuDeLarge = (HSSFFont)workbook.CreateFont();
                hFontTieuDeLarge.FontHeightInPoints = 16;
                hFontTieuDeLarge.Boldweight = 100 * 10;
                hFontTieuDeLarge.FontName = "Times New Roman";
                //hFontTieuDeLarge.Color = HSSFColor.BLUE.index;

                //font tiêu đề 
                HSSFFont hFontTongGiaTriHT = (HSSFFont)workbook.CreateFont();
                hFontTongGiaTriHT.FontHeightInPoints = 11;
                hFontTongGiaTriHT.Boldweight = (short)FontBoldWeight.BOLD;
                hFontTongGiaTriHT.FontName = "Times New Roman";
                hFontTongGiaTriHT.Color = HSSFColor.BLACK.index;

                //font thông tin bảng tính
                HSSFFont hFontTT = (HSSFFont)workbook.CreateFont();
                hFontTT.IsItalic = true;
                hFontTT.Boldweight = (short)FontBoldWeight.BOLD;
                hFontTT.Color = HSSFColor.BLACK.index;
                hFontTT.FontName = "Times New Roman";
                hFontTieuDe.FontHeightInPoints = 11;

                //font chứ hoa đậm
                HSSFFont hFontNommalUpper = (HSSFFont)workbook.CreateFont();
                hFontNommalUpper.Boldweight = (short)FontBoldWeight.BOLD;
                hFontNommalUpper.Color = HSSFColor.BLACK.index;
                hFontNommalUpper.FontName = "Times New Roman";

                //font chữ bình thường
                HSSFFont hFontNommal = (HSSFFont)workbook.CreateFont();
                hFontNommal.Color = HSSFColor.BLACK.index;
                hFontNommal.FontName = "Times New Roman";

                //font chữ bình thường đậm
                HSSFFont hFontNommalBold = (HSSFFont)workbook.CreateFont();
                hFontNommalBold.Color = HSSFColor.BLACK.index;
                hFontNommalBold.Boldweight = (short)FontBoldWeight.BOLD;
                hFontNommalBold.FontName = "Times New Roman";

                //tạo font cho các title end

                //Set style
                var styleTitle = workbook.CreateCellStyle();
                styleTitle.SetFont(hFontTieuDe);
                styleTitle.Alignment = HorizontalAlignment.LEFT;

                //Set styleUnderline
                var styleTitleUnderline = workbook.CreateCellStyle();
                styleTitleUnderline.SetFont(hFontTieuDeUnderline);
                styleTitleUnderline.Alignment = HorizontalAlignment.LEFT;

                //Set style In nghiêng
                var styleTitleItalic = workbook.CreateCellStyle();
                styleTitleItalic.SetFont(hFontTieuDeItalic);
                styleTitleItalic.Alignment = HorizontalAlignment.LEFT;

                //Set style Large font
                var styleTitleLarge = workbook.CreateCellStyle();
                styleTitleLarge.SetFont(hFontTieuDeLarge);
                styleTitleLarge.Alignment = HorizontalAlignment.LEFT;

                //style infomation
                var styleInfomation = workbook.CreateCellStyle();
                styleInfomation.SetFont(hFontTT);
                styleInfomation.Alignment = HorizontalAlignment.LEFT;

                //style header
                var styleheadedColumnTable = workbook.CreateCellStyle();
                styleheadedColumnTable.SetFont(hFontNommalUpper);
                styleheadedColumnTable.WrapText = true;
                styleheadedColumnTable.BorderBottom = CellBorderType.THIN;
                styleheadedColumnTable.BorderLeft = CellBorderType.THIN;
                styleheadedColumnTable.BorderRight = CellBorderType.THIN;
                styleheadedColumnTable.BorderTop = CellBorderType.THIN;
                styleheadedColumnTable.VerticalAlignment = VerticalAlignment.CENTER;
                styleheadedColumnTable.Alignment = HorizontalAlignment.CENTER;

                //style sum cell
                var styleCellSumary = workbook.CreateCellStyle();
                styleCellSumary.SetFont(hFontNommalUpper);
                styleCellSumary.WrapText = true;
                styleCellSumary.BorderBottom = CellBorderType.THIN;
                styleCellSumary.BorderLeft = CellBorderType.THIN;
                styleCellSumary.BorderRight = CellBorderType.THIN;
                styleCellSumary.BorderTop = CellBorderType.THIN;
                styleCellSumary.VerticalAlignment = VerticalAlignment.CENTER;
                styleCellSumary.Alignment = HorizontalAlignment.RIGHT;

                var styleHeading1 = workbook.CreateCellStyle();
                styleHeading1.SetFont(hFontNommalBold);
                styleHeading1.WrapText = true;
                styleHeading1.BorderBottom = CellBorderType.THIN;
                styleHeading1.BorderLeft = CellBorderType.THIN;
                styleHeading1.BorderRight = CellBorderType.THIN;
                styleHeading1.BorderTop = CellBorderType.THIN;
                styleHeading1.VerticalAlignment = VerticalAlignment.CENTER;
                styleHeading1.Alignment = HorizontalAlignment.LEFT;

                var hStyleConLeft = (HSSFCellStyle)workbook.CreateCellStyle();
                hStyleConLeft.SetFont(hFontNommal);
                hStyleConLeft.VerticalAlignment = VerticalAlignment.TOP;
                hStyleConLeft.Alignment = HorizontalAlignment.LEFT;
                hStyleConLeft.WrapText = true;
                hStyleConLeft.BorderBottom = CellBorderType.THIN;
                hStyleConLeft.BorderLeft = CellBorderType.THIN;
                hStyleConLeft.BorderRight = CellBorderType.THIN;
                hStyleConLeft.BorderTop = CellBorderType.THIN;

                var hStyleConRight = (HSSFCellStyle)workbook.CreateCellStyle();
                hStyleConRight.SetFont(hFontNommal);
                hStyleConRight.VerticalAlignment = VerticalAlignment.TOP;
                hStyleConRight.Alignment = HorizontalAlignment.RIGHT;
                hStyleConRight.BorderBottom = CellBorderType.THIN;
                hStyleConRight.BorderLeft = CellBorderType.THIN;
                hStyleConRight.BorderRight = CellBorderType.THIN;
                hStyleConRight.BorderTop = CellBorderType.THIN;


                var hStyleConCenter = (HSSFCellStyle)workbook.CreateCellStyle();
                hStyleConCenter.SetFont(hFontNommal);
                hStyleConCenter.VerticalAlignment = VerticalAlignment.TOP;
                hStyleConCenter.Alignment = HorizontalAlignment.CENTER;
                hStyleConCenter.BorderBottom = CellBorderType.THIN;
                hStyleConCenter.BorderLeft = CellBorderType.THIN;
                hStyleConCenter.BorderRight = CellBorderType.THIN;
                hStyleConCenter.BorderTop = CellBorderType.THIN;
                //set style end
                #endregion

                //Khai báo row
                Row rowC = null;



                var sheet = workbook.CreateSheet("BangLuongTheoBoPhan");

                //Khai báo row đầu tiên
                int firstRowNumber = 3;

                string cellTenCty = "Sàn giao dịch NewCity";
                var titleCellCty = HSSFCellUtil.CreateCell(sheet.CreateRow(0), 0, cellTenCty.ToUpper());
                titleCellCty.CellStyle = styleTitle;

                string cellTenCacBanDH = "CÁC BAN ĐIỀU HÀNH DỰ ÁN";
                var titleCellTenCacBanDH = HSSFCellUtil.CreateCell(sheet.CreateRow(1), 0, cellTenCacBanDH.ToUpper());
                titleCellTenCacBanDH.CellStyle = styleTitle;

                string cellTitleMain = "BẢNG TỔNG HỢP LƯƠNG THÁNG " + thang + "/" + nam;
                var titleCellTitleMain = HSSFCellUtil.CreateCell(sheet.CreateRow(2), 3, cellTitleMain.ToUpper());
                titleCellTitleMain.CellStyle = styleTitleLarge;
                titleCellTitleMain.Row.Height = 300;
                firstRowNumber++;

                var list1 = new List<string>();
                list1.Add("STT");
                list1.Add("Phòng ban");
                list1.Add("Lương tháng");// trường này sẽ colspan = 2
                list1.Add("Bảo hiểm");
                list1.Add("Thuế");
                list1.Add("Truy thu,\ntruy lãnh Lương");
                list1.Add("Truy thu,\ntruy lãnh Thuế");
                list1.Add("Truy thu,\ntruy lãnh Bảo hiểm");
                list1.Add("Truy thu,\ntruy lãnh Tạm ứng");
                list1.Add("Thực lĩnh");


                //var list2 = new List<string>();
                //list2.Add("TS");
                //list2.Add("Đơn vị");
                //list2.Add("Lương");// trường này sẽ colspan = 2
                //list2.Add("Phụ cấp lương");
                //list2.Add("Khoản bổ sung");
                //list2.Add("Công");
                //list2.Add("Tổng cộng");
                //list2.Add("Lễ, phép");
                //list2.Add("Lương lễ, phép");
                //list2.Add("Truy lĩnh");
                //list2.Add("Thực lĩnh");
                //list2.Add("BHXH + Y tế + TN");
                //list2.Add("Thuế TNCN");
                //list2.Add("Truy thu");
                //list2.Add("Đoàn phí");
                //list2.Add("Đảng phí");
                //list2.Add("Tiền ăn giữa ca");
                //list2.Add("Còn lãnh");

                var idRowStart = firstRowNumber; // bat dau o dong thu 4
                var headerRow = sheet.CreateRow(idRowStart);
                int rowend = idRowStart;
                ReportHelperExcel.CreateHeaderRow(headerRow, 0, styleheadedColumnTable, list1);
                idRowStart++;
                //var headerRow1 = sheet.CreateRow(idRowStart);
                //ReportHelperExcel.CreateHeaderRow(headerRow1, 0, styleheadedColumnTable, list2);

                //sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 17, 17));
                //sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 16, 16));
                //sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 15, 15));
                //sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 14, 14));
                //sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 13, 13));
                //sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 12, 12));
                //sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 11, 11));
                //sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 10, 10));
                //sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 9, 9));
                //sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 8, 8));
                //sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 7, 7));
                //sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 6, 6));
                //sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 5, 5));
                //sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 4, 4));
                //sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow.RowNum, 2, 3));
                //sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 1, 1));
                //sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 0, 0));



                sheet.SetColumnWidth(0, 5 * 210);
                sheet.SetColumnWidth(1, 40 * 210);
                sheet.SetColumnWidth(2, 20 * 210);
                sheet.SetColumnWidth(3, 20 * 210);
                sheet.SetColumnWidth(4, 20 * 210);
                sheet.SetColumnWidth(5, 20 * 210);
                sheet.SetColumnWidth(6, 20 * 210);
                sheet.SetColumnWidth(7, 20 * 210);
                sheet.SetColumnWidth(8, 20 * 210);
                sheet.SetColumnWidth(9, 20 * 210);


                var data = nhanSuContext.sp_NS_BangLuongTheoBoPhan(thang, nam, "CTTV").ToList();
                var data1330 = nhanSuContext.sp_NS_BangLuongTheoBoPhan(thang, nam, "CT1330").ToList();
                var data2220 = nhanSuContext.sp_NS_BangLuongTheoBoPhan(thang, nam, "CT2220").ToList();

                double? TongLuong = 0;
                double? TongBaoHiem = 0;
                double? TongTienThue = 0;
                double? TongTLTTLuong = 0;
                double? TongTLTTThue = 0;
                double? TongTLTTBaoHiem = 0;
                double? TongTLTTTamUng = 0;
                double? TongThuclanh = 0;

                idRowStart++;
                int demTV = 0;
                Row rowTV = sheet.CreateRow(idRowStart);
                CellRangeAddress cellRangeAddress = new CellRangeAddress(idRowStart, idRowStart, demTV, demTV + 9);
                sheet.AddMergedRegion(cellRangeAddress);
                ReportHelperExcel.SetAlignment(rowTV, demTV++, "Sàn giao dịch NewCity", hStyleConLeft);
                ReportHelperExcel.SetAlignment(rowTV, demTV++, "", hStyleConLeft);
                ReportHelperExcel.SetAlignment(rowTV, demTV++, "", hStyleConRight);
                ReportHelperExcel.SetAlignment(rowTV, demTV++, "", hStyleConRight);
                ReportHelperExcel.SetAlignment(rowTV, demTV++, "", hStyleConRight);
                ReportHelperExcel.SetAlignment(rowTV, demTV++, "", hStyleConRight);
                ReportHelperExcel.SetAlignment(rowTV, demTV++, "", hStyleConRight);
                ReportHelperExcel.SetAlignment(rowTV, demTV++, "", hStyleConRight);
                ReportHelperExcel.SetAlignment(rowTV, demTV++, "", hStyleConRight);
                ReportHelperExcel.SetAlignment(rowTV, demTV++, "", hStyleConRight);


                var stt = 0;
                var stt1330 = 0;
                var stt2220 = 0;
                int dem = 0;

                foreach (var item in data)
                {
                    TongLuong += item.luongThang;
                    TongBaoHiem += item.baoHiem;
                    TongTienThue += item.thue;
                    TongTLTTLuong += item.TTTLLuong;
                    TongTLTTThue += item.TTTLThue;
                    TongTLTTBaoHiem += item.TTTLBaoHiem;
                    TongTLTTTamUng += item.TTTLTamUng;
                    TongThuclanh += item.thucLanh;

                    dem = 0;
                    stt++;
                    idRowStart++;

                    rowC = sheet.CreateRow(idRowStart);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item.tongSo.ToString(), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item.phongBan, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongThang), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.baoHiem), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.thue), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.TTTLLuong), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.TTTLThue), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.TTTLBaoHiem), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.TTTLTamUng), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.thucLanh), hStyleConRight);
                }

                idRowStart++;
                int dem1330 = 0;
                Row row1330 = sheet.CreateRow(idRowStart);
                CellRangeAddress cellRangeAddress1330 = new CellRangeAddress(idRowStart, idRowStart, dem1330, dem1330 + 9);
                sheet.AddMergedRegion(cellRangeAddress1330);
                ReportHelperExcel.SetAlignment(row1330, dem1330++, "Công trình 1.330 căn hộ", hStyleConLeft);
                ReportHelperExcel.SetAlignment(row1330, dem1330++, "", hStyleConLeft);
                ReportHelperExcel.SetAlignment(row1330, dem1330++, "", hStyleConRight);
                ReportHelperExcel.SetAlignment(row1330, dem1330++, "", hStyleConRight);
                ReportHelperExcel.SetAlignment(row1330, dem1330++, "", hStyleConRight);
                ReportHelperExcel.SetAlignment(row1330, dem1330++, "", hStyleConRight);
                ReportHelperExcel.SetAlignment(row1330, dem1330++, "", hStyleConRight);
                ReportHelperExcel.SetAlignment(row1330, dem1330++, "", hStyleConRight);
                ReportHelperExcel.SetAlignment(row1330, dem1330++, "", hStyleConRight);
                ReportHelperExcel.SetAlignment(row1330, dem1330++, "", hStyleConRight);


                int demC1330 = 0;
                Row rowCon1330 = null;
                foreach (var item in data1330)
                {
                    TongLuong += item.luongThang;
                    TongBaoHiem += item.baoHiem;
                    TongTienThue += item.thue;
                    TongTLTTLuong += item.TTTLLuong;
                    TongTLTTThue += item.TTTLThue;
                    TongTLTTBaoHiem += item.TTTLBaoHiem;
                    TongTLTTTamUng += item.TTTLTamUng;
                    TongThuclanh += item.thucLanh;

                    demC1330 = 0;
                    stt1330++;
                    idRowStart++;

                    rowCon1330 = sheet.CreateRow(idRowStart);
                    ReportHelperExcel.SetAlignment(rowCon1330, demC1330++, item.tongSo.ToString(), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowCon1330, demC1330++, item.phongBan, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowCon1330, demC1330++, String.Format("{0:#,##0}", item.luongThang), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowCon1330, demC1330++, String.Format("{0:#,##0}", item.baoHiem), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowCon1330, demC1330++, String.Format("{0:#,##0}", item.thue), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowCon1330, demC1330++, String.Format("{0:#,##0}", item.TTTLLuong), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowCon1330, demC1330++, String.Format("{0:#,##0}", item.TTTLThue), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowCon1330, demC1330++, String.Format("{0:#,##0}", item.TTTLBaoHiem), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowCon1330, demC1330++, String.Format("{0:#,##0}", item.TTTLTamUng), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowCon1330, demC1330++, String.Format("{0:#,##0}", item.thucLanh), hStyleConRight);
                }

                idRowStart++;
                int dem2220 = 0;
                Row row2220 = sheet.CreateRow(idRowStart);
                CellRangeAddress cellRangeAddress2220 = new CellRangeAddress(idRowStart, idRowStart, dem2220, dem2220 + 9);
                sheet.AddMergedRegion(cellRangeAddress2220);
                ReportHelperExcel.SetAlignment(row2220, dem2220++, "Công trình 2.220 căn hộ", hStyleConLeft);
                ReportHelperExcel.SetAlignment(row2220, dem2220++, "", hStyleConLeft);
                ReportHelperExcel.SetAlignment(row2220, dem2220++, "", hStyleConRight);
                ReportHelperExcel.SetAlignment(row2220, dem2220++, "", hStyleConRight);
                ReportHelperExcel.SetAlignment(row2220, dem2220++, "", hStyleConRight);
                ReportHelperExcel.SetAlignment(row2220, dem2220++, "", hStyleConRight);
                ReportHelperExcel.SetAlignment(row2220, dem2220++, "", hStyleConRight);
                ReportHelperExcel.SetAlignment(row2220, dem2220++, "", hStyleConRight);
                ReportHelperExcel.SetAlignment(row2220, dem2220++, "", hStyleConRight);
                ReportHelperExcel.SetAlignment(row2220, dem2220++, "", hStyleConRight);


                int demC2220 = 0;
                Row rowCon2220 = null;
                foreach (var item in data2220)
                {
                    TongLuong += item.luongThang;
                    TongBaoHiem += item.baoHiem;
                    TongTienThue += item.thue;
                    TongTLTTLuong += item.TTTLLuong;
                    TongTLTTThue += item.TTTLThue;
                    TongTLTTBaoHiem += item.TTTLBaoHiem;
                    TongTLTTTamUng += item.TTTLTamUng;
                    TongThuclanh += item.thucLanh;

                    demC2220 = 0;
                    stt2220++;
                    idRowStart++;

                    rowCon2220 = sheet.CreateRow(idRowStart);
                    ReportHelperExcel.SetAlignment(rowCon2220, demC2220++, item.tongSo.ToString(), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowCon2220, demC2220++, item.phongBan, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowCon2220, demC2220++, String.Format("{0:#,##0}", item.luongThang), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowCon2220, demC2220++, String.Format("{0:#,##0}", item.baoHiem), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowCon2220, demC2220++, String.Format("{0:#,##0}", item.thue), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowCon2220, demC2220++, String.Format("{0:#,##0}", item.TTTLLuong), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowCon2220, demC2220++, String.Format("{0:#,##0}", item.TTTLThue), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowCon2220, demC2220++, String.Format("{0:#,##0}", item.TTTLBaoHiem), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowCon2220, demC2220++, String.Format("{0:#,##0}", item.TTTLTamUng), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowCon2220, demC2220++, String.Format("{0:#,##0}", item.thucLanh), hStyleConRight);
                }


                int demT = 0;
                idRowStart++;
                Row rowT = sheet.CreateRow(idRowStart);

                CellRangeAddress cellRangeAddressT = new CellRangeAddress(idRowStart, idRowStart, demT, demT + 1);
                sheet.AddMergedRegion(cellRangeAddressT);
                ReportHelperExcel.SetAlignment(rowT, demT++, "TỔNG CỘNG", styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, "", styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", TongLuong), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", TongBaoHiem), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", TongTienThue), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", TongTLTTLuong), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", TongTLTTThue), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", TongTLTTBaoHiem), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", TongTLTTTamUng), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", TongThuclanh), styleCellSumary);

                idRowStart = idRowStart + 2;
                string cellFooterSoTien = "(" + CharacterHelper.DocTienBangChu((decimal)TongThuclanh, string.Empty) + ")";
                var titleCellFooterSoTien = HSSFCellUtil.CreateCell(sheet.CreateRow(idRowStart), 5, cellFooterSoTien);
                titleCellFooterSoTien.CellStyle = styleTitleItalic;

                idRowStart = idRowStart + 2;
                string cellFooterNgayLap = "Tp.Hồ Chí Minh, ngày   tháng  năm " + nam;
                var titleCellFooterNgayLap = HSSFCellUtil.CreateCell(sheet.CreateRow(idRowStart), 6, cellFooterNgayLap);
                titleCellFooterNgayLap.CellStyle = styleTitleItalic;

                idRowStart = idRowStart + 2;
                string cellFooterPTC = "PHÒNG TỔ CHỨC CB-LĐ";
                var titleCellFooterPTC = HSSFCellUtil.CreateCell(sheet.CreateRow(idRowStart), 1, cellFooterPTC);
                titleCellFooterPTC.CellStyle = styleTitle;

                string cellFooterKT = "PHÒNG TÀI CHÍNH KẾ TOÁN";
                var titleCellFooterKT = HSSFCellUtil.CreateCell(sheet.GetRow(idRowStart), 3, cellFooterKT);
                titleCellFooterKT.CellStyle = styleTitle;

                string cellFooterTGD = "TỔNG GIÁM ĐỐC";
                var titleCellFooterTGD = HSSFCellUtil.CreateCell(sheet.GetRow(idRowStart), 7, cellFooterTGD);
                titleCellFooterTGD.CellStyle = styleTitle;


                var stream = new MemoryStream();
                workbook.Write(stream);

                Response.ContentType = "application/vnd.ms-excel";
                Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", filename));
                Response.Clear();

                Response.BinaryWrite(stream.GetBuffer());
                Response.End();
            }
            catch
            {

            }



            //var filename = "";
            //var virtualPath = HttpRuntime.AppDomainAppVirtualPath;

            //filename += "BangLuongNVBoPhan_" + nam + "_" + thang + ".xlsx";

            //using (ExcelPackage package = new ExcelPackage())
            //{
            //    //Create a sheet
            //    package.Workbook.Worksheets.Add("BangLuongNVBoPhan_" + nam + "_" + thang);
            //    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
            //    //Header
            //    //insert từ dòng nào, bao nhiêu row
            //    var rowFrom = 1;
            //    worksheet.InsertRow(rowFrom, 1);
            //    worksheet.Cells[1, 1].Value = "STT";
            //    worksheet.Cells[1, 2].Value = "Phòng ban";
            //    worksheet.Cells[1, 3].Value = "Tháng";
            //    worksheet.Cells[1, 4].Value = "Năm";
            //    worksheet.Cells[1, 5].Value = "Tổng lương";
            //    worksheet.Cells[1, 6].Value = "Bảo hiểm";
            //    worksheet.Cells[1, 7].Value = "Thuế";
            //    worksheet.Cells[1, 8].Value = "Truy thu";
            //    worksheet.Cells[1, 9].Value = "Truy lãnh";
            //    worksheet.Cells[1, 10].Value = "Thực lãnh";
            //    worksheet.Column(2).Width = 35;
            //    worksheet.Column(3).Width = 35;
            //    worksheet.Column(4).Width = 20;
            //    worksheet.Column(5).Width = 20;
            //    worksheet.Column(6).Width = 20;
            //    worksheet.Column(7).Width = 20;
            //    worksheet.Column(8).Width = 20;
            //    worksheet.Column(9).Width = 20;
            //    worksheet.Column(10).Width = 20;
            //    //// Formatting style of the header
            //    using (var range = worksheet.Cells[1, 1, 2, 10])
            //    {
            //        // Setting bold font
            //        range.Style.Font.Bold = true;
            //        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            //        // Setting fill type solid
            //        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            //        // Setting background color dark blue
            //        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            //        // Setting font color
            //        range.Style.Font.Color.SetColor(System.Drawing.Color.White);
            //    }



            //    #region
            //    //Body
            //    var data = nhanSuContext.sp_NS_BangLuongTheoBoPhan(thang, nam).ToList();

            //    if (data != null && data.Count > 0)
            //    {
            //        var countSTT = 1;
            //        foreach (var item in data)
            //        {

            //            rowFrom = rowFrom + 1;
            //            worksheet.InsertRow(rowFrom, 1);
            //            worksheet.Cells[rowFrom, 1].Value = countSTT++;
            //            worksheet.Cells[rowFrom, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
            //            worksheet.Cells[rowFrom, 1].Style.Font.Bold = true;
            //            worksheet.Cells[rowFrom, 2].Value = item.boPhanTinhLuong;
            //            worksheet.Cells[rowFrom, 3].Value = item.thang;
            //            worksheet.Cells[rowFrom, 4].Value = item.nam;


            //            worksheet.Cells[rowFrom, 5].Value = item.tongLuong;
            //            worksheet.Cells[rowFrom, 5].Style.Numberformat.Format = "#,##0.000";

            //            worksheet.Cells[rowFrom, 6].Value = item.baoHiem;
            //            worksheet.Cells[rowFrom, 6].Style.Numberformat.Format = "#,##0.000";

            //            worksheet.Cells[rowFrom, 7].Value = item.thue;
            //            worksheet.Cells[rowFrom, 7].Style.Numberformat.Format = "#,##0";

            //            worksheet.Cells[rowFrom, 8].Value = item.truyThu;
            //            worksheet.Cells[rowFrom, 8].Style.Numberformat.Format = "#,##0.000";

            //            worksheet.Cells[rowFrom, 9].Value = item.truyLanh;
            //            worksheet.Cells[rowFrom, 9].Style.Numberformat.Format = "#,##0.000";

            //            worksheet.Cells[rowFrom, 10].Value = item.thucLanh;
            //            worksheet.Cells[rowFrom, 10].Style.Numberformat.Format = "#,##0.000";
            //        }
            //    }

            //    #endregion

            //    //Generate A File
            //    Byte[] bin = package.GetAsByteArray();

            //    Response.BinaryWrite(bin);
            //    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            //    Response.AddHeader("content-disposition", string.Format("attachment;filename={0}", filename));
            //}
        }


        #endregion End Xuat File
        // Xuat File Bang Luong Nhan Vien theo bộ phận
        public void XuatFileBLNN(int thang, int nam)
        {

            string maPhongBan = "";
            string qSearch = "";
            var filename = "";
            var virtualPath = HttpRuntime.AppDomainAppVirtualPath;

            filename += "BangLuongChuyenNN_" + nam + "_" + thang + ".xlsx";

            using (ExcelPackage package = new ExcelPackage())
            {
                //Create a sheet
                package.Workbook.Worksheets.Add("BangLuongChuyenNN_" + nam + "_" + thang);
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                //Header
                //insert từ dòng nào, bao nhiêu row
                var rowFrom = 1;
                worksheet.InsertRow(rowFrom, 1);
                worksheet.Cells[1, 1].Value = "STT";
                worksheet.Cells[1, 2].Value = "Họ tên";
                worksheet.Cells[1, 3].Value = "Số tài khoản";
                worksheet.Cells[1, 4].Value = "Tên ngân hàng";
                worksheet.Cells[1, 5].Value = "Số CMND";
                worksheet.Cells[1, 6].Value = "Thực lãnh";

                worksheet.Column(2).Width = 35;
                worksheet.Column(3).Width = 35;
                worksheet.Column(4).Width = 20;
                worksheet.Column(5).Width = 20;
                worksheet.Column(6).Width = 20;
                //// Formatting style of the header
                using (var range = worksheet.Cells[1, 1, 2, 6])
                {
                    // Setting bold font
                    range.Style.Font.Bold = true;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    // Setting fill type solid
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    // Setting background color dark blue
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                    // Setting font color
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                }



                #region
                //Body
                var data = nhanSuContext.sp_NS_BangLuongChuyenNganHang(thang, nam, maPhongBan, qSearch).ToList();

                if (data != null && data.Count > 0)
                {
                    var countSTT = 1;
                    foreach (var item in data)
                    {

                        rowFrom = rowFrom + 1;
                        worksheet.InsertRow(rowFrom, 1);
                        worksheet.Cells[rowFrom, 1].Value = countSTT++;
                        worksheet.Cells[rowFrom, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                        worksheet.Cells[rowFrom, 1].Style.Font.Bold = true;
                        worksheet.Cells[rowFrom, 2].Value = item.hoTen;
                        worksheet.Cells[rowFrom, 3].Value = item.soTaiKhoan;
                        worksheet.Cells[rowFrom, 4].Value = item.tenNganHang;
                        worksheet.Cells[rowFrom, 5].Value = item.soCMND;
                        worksheet.Cells[rowFrom, 6].Value = item.thucLanh;
                        worksheet.Cells[rowFrom, 6].Style.Numberformat.Format = "#,##0.000";
                        worksheet.Cells[rowFrom, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    }
                }

                #endregion

                //Generate A File
                Byte[] bin = package.GetAsByteArray();

                Response.BinaryWrite(bin);
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", string.Format("attachment;filename={0}", filename));
            }
        }

        // End Xuat File

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
    }
}
