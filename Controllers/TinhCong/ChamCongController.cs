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
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.HSSF.Util;
using NPOI.HSSF.UserModel.Contrib;
using Worldsoft.Mvc.Web.Util;
namespace BatDongSan.Controllers.TinhCong
{
    public class ChamCongController : ApplicationController
    {

        private LinqNhanSuDataContext nhanSuContext = new LinqNhanSuDataContext();
        private IList<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan> phongBans;
        private StringBuilder buildTree;
        private readonly string MCV = "ChamCongAdmin";
        private bool? permission;
        public ActionResult XemTinhHinhRaVao()
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
        public ActionResult LoadXemTinhHinhRaVao(string qSearch, int thang, int nam, int _page = 0)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            string maNhanVien = GetUser().manv;
            int page = _page == 0 ? 1 : _page;
            int pIndex = page;


            int total = nhanSuContext.sp_NS_XemTinhHinhRaVao(maNhanVien, thang, nam, qSearch).Count();
            PagingLoaderFullController("/ChamCong/XemTinhHinhRaVao/", total, page, "?qsearch=" + qSearch + "&maNhanVien=" + maNhanVien);
            ViewData["lsDanhSach"] = nhanSuContext.sp_NS_XemTinhHinhRaVao(maNhanVien, thang, nam, qSearch).Skip(start).Take(offset).ToList();

            ViewData["qSearch"] = qSearch;
            return PartialView("_LoadXemTinhHinhRaVao");

        }
        //public ActionResult XemTinhHinhRaVaoCongNhan()
        //{
        //    #region Role user
        //    permission = GetPermission("XemRaVaoToanCTy", BangPhanQuyen.QuyenXem);
        //    if (!permission.HasValue)
        //        return View("LogIn");
        //    if (!permission.Value)
        //        return View("AccessDenied");
        //    #endregion
        //    thang(DateTime.Now.Month);
        //    nam(DateTime.Now.Year);
        //    return View("");
        //}
        //public ActionResult LoadXemTinhHinhRaVaoCongNhan(string qSearch, int thang, int nam, int _page = 0)
        //{
        //    #region Role user
        //    permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
        //    if (!permission.HasValue)
        //        return View("LogIn");
        //    if (!permission.Value)
        //        return View("AccessDenied");
        //    #endregion
        //    //BatDongSan.Models.ChamCong.LinqChamCongServerDataContext contextCC = new BatDongSan.Models.ChamCong.LinqChamCongServerDataContext();
        //    string maNhanVien = GetUser().manv;
        //    int page = _page == 0 ? 1 : _page;
        //    int pIndex = page;
        //    ////Get ma cham cong
        //    //var getMaCC = nhanSuContext.tbl_NS_NhanViens.Where(d => d.maNhanVien == maNhanVien).FirstOrDefault();
        //    //if (getMaCC != null)
        //    //{
        //    //    var maChamCong = getMaCC.maChamCong;

        //    int total = nhanSuContext.sp_NS_XemTinhHinhRaVao(maNhanVien, thang, nam, qSearch).Count();
        //    PagingLoaderFullController("/ChamCong/XemTinhHinhRaVao/", total, page, "?qsearch=" + qSearch + "&maNhanVien=" + maNhanVien);
        //    ViewData["lsDanhSach"] = nhanSuContext.sp_NS_XemTinhHinhRaVao(maNhanVien, thang, nam, qSearch).Skip(start).Take(offset).ToList();

        //    ViewData["qSearch"] = qSearch;
        //    return PartialView("_LoadXemTinhHinhRaVao");
        //    //}
        //    //else {
        //    //    return View("error");
        //    //}
        //}
        public ActionResult XemBangLuong()
        {
            #region Role user
            permission = GetPermission("XemBangLuong", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            nam(DateTime.Now.Year);
            return View("");
        }

        public ActionResult ViewChiTietLuong(string thang, string nam)
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
                               .Where(t => t.maNhanVien == GetUser().manv && t.thang.ToString() == thang && nam == t.nam.ToString()).FirstOrDefault();
            ViewData["chiTiet"] = dsMauIn;
            if (dsMauIn != null)
            {
                double tongBaoHiem = (ds.baoHiem ?? 0);
                double tongPhuCapTruyLanh = (ds.congTacPhi ?? 0) + (ds.TTTLBaoHiem ?? 0) + (ds.TTTLThue ?? 0) + (ds.TTTLTamUng ?? 0) + (ds.phuCapKhac ?? 0) + (ds.TTTLLuong ?? 0);
                noiDung = dsMauIn.html.Replace("{$thang}", Convert.ToString(thang))
                    .Replace("{$nam}", Convert.ToString(nam))
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
        public ActionResult LoadXemBangLuong(int nam, int _page = 0)
        {
            #region Role user
            permission = GetPermission("XemBangLuong", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            string maNhanVien = GetUser().manv;
            int page = _page == 0 ? 1 : _page;
            int pIndex = page;
            int total = nhanSuContext.sp_NS_BangLuongDanhChoNhanVien(maNhanVien, nam).Count();
            PagingLoaderFullController("/ChamCong/XemBangLuong/", total, page, "?maNhanVien=" + maNhanVien);
            ViewData["lsDanhSach"] = nhanSuContext.sp_NS_BangLuongDanhChoNhanVien(maNhanVien, nam).Skip(start).Take(offset).ToList();

            return PartialView("_LoadXemBangLuong");
        }
        public ActionResult LoadBangLuongThangMuoi()
        {
            #region Role user
            permission = GetPermission("XemBangLuong", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            ViewBag.lstThangMuoi = nhanSuContext.tbl_NS_NhanVien_Thuongs.Where(d => d.maNhanVien == GetUser().manv).ToList();
            return PartialView("_LoadBangLuongThangMuoi");
        }
        public ActionResult XemBangLuongThuongTet()
        {
            #region Role user
            permission = GetPermission("XemBangLuongTT", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            nam(DateTime.Now.Year);
            return View("");
        }
        public ActionResult LoadXemBangLuongThuongTet(int nam)
        {
            #region Role user
            permission = GetPermission("XemBangLuongTT", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            string maNhanVien = GetUser().manv;
            string ckDuyet = "khong";
            var checkDuyet = nhanSuContext.tbl_DuyetBangLuongThuongTets.Where(d => d.nam == nam).FirstOrDefault();
            if (checkDuyet != null)
            {
                ckDuyet = "duyet";
            }
            ViewBag.ckDuyet = ckDuyet;
            ViewData["lsDanhSach"] = nhanSuContext.sp_NS_BangLuongThuongTet(nam, "", "", "").Where(d => d.maNhanVien == maNhanVien).ToList();

            return PartialView("_LoadXemBangLuongThuongTet");
        }
        public ActionResult XemBangLuongNam2017()
        {
            #region Role user
            permission = GetPermission("XemBangLuongNam2017", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion


            return View("");
        }
        public ActionResult LoadXemBangLuong2017()
        {
            #region Role user
            permission = GetPermission("XemBangLuongNam2017", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            string maNhanVien = GetUser().manv;
            ViewBag.lsDanhSach = nhanSuContext.tbl_NS_DieuChinhLuongs.Where(d => d.maNhanVien == maNhanVien && d.nam == 2017).ToList();

            return PartialView("LoadXemBangLuong2017");
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
            ViewBag.lstThangMuoi = nhanSuContext.tbl_NS_NhanVien_Thuong11s.OrderBy(d => d.maNhanVien).Where(d => d.maNhanVien == GetUser().manv).ToList();
            return PartialView("_LoadBangLuongThangMuoiMot");
        }
        public ActionResult XemBangChamCongChiTiet()
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
        public ActionResult LoadBangChamCongChiTiet(string qSearch, int thang, int nam, int _page = 0)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            string maNhanVien = GetUser().manv;
            int page = _page == 0 ? 1 : _page;
            int pIndex = page;
            int total = nhanSuContext.sp_NS_BangChamCongChiTiet(thang, nam, qSearch, maNhanVien).Count();
            PagingLoaderFullController("/ChamCong/LoadBangChamCongChiTiet/", total, page, "?qsearch=" + qSearch);
            ViewData["lsDanhSach"] = nhanSuContext.sp_NS_BangChamCongChiTiet(thang, nam, qSearch, maNhanVien).Skip(start).Take(offset).ToList();

            ViewData["qSearch"] = qSearch;
            return PartialView("_LoadBangChamCongChiTiet");
        }


        public ActionResult XemTinhHinhRaVaoCongNhan()
        {
            #region Role user
            permission = GetPermission("ChamCongAdminCN", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            return View("");
        }
        public ActionResult LoadXemTinhHinhRaVaoCongNhan(string qSearch, string tuNgay, string denNgay, int _page = 0)
        {
            #region Role user
            permission = GetPermission("ChamCongAdminCN", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            DateTime? fromDate = null;
            DateTime? toDate = null;
            if (!String.IsNullOrEmpty(tuNgay))
                fromDate = DateTime.ParseExact(tuNgay, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            if (!String.IsNullOrEmpty(denNgay))
                toDate = DateTime.ParseExact(denNgay, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            string maNhanVien = GetUser().manv;
            int page = _page == 0 ? 1 : _page;
            int pIndex = page;

            int total = nhanSuContext.sp_NS_XemTinhHinhRaVaoCongNhan(fromDate, toDate, qSearch).Count();
            PagingLoaderFullController("/ChamCong/XemTinhHinhRaVaoCongNhan/", total, page, "?qsearch=" + qSearch);
            ViewData["lsDanhSach"] = nhanSuContext.sp_NS_XemTinhHinhRaVaoCongNhan(fromDate, toDate, qSearch).Skip(start).Take(offset).ToList();

            ViewData["qSearch"] = qSearch;
            ViewBag.tuNgay = tuNgay;
            ViewBag.denNgay = tuNgay;
            return PartialView("_LoadXemTinhHinhRaVaoCongNhan");

        }


        public ActionResult LoadBangChamCongTongHop(string qSearch, int nam, int _page = 0)
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
            string maNhanVien = GetUser().manv;
            int total = nhanSuContext.sp_NS_BangTongHopCongThang(null, nam, qSearch, maNhanVien, 0, null).Count();
            PagingLoaderFullController("/BangChamCongTongHop/LoadBangChamCongTongHop/", total, page, "?qsearch=" + qSearch);
            ViewData["lsDanhSach"] = nhanSuContext.sp_NS_BangTongHopCongThang(null, nam, qSearch, maNhanVien, 0, null).Skip(start).Take(offset).ToList();

            ViewData["qSearch"] = qSearch;
            return PartialView("_LoadBangChamCongTongHop");
        }
        private void thang(int value)
        {
            Dictionary<int, string> dics = new Dictionary<int, string>();
            for (int i = 1; i < 13; i++)
            {
                dics[i] = i.ToString();
            }
            ViewData["thang"] = new SelectList(dics, "Key", "Value", value);
            ViewData["thangtc"] = new SelectList(dics, "Key", "Value", value);
        }
        private void nam(int value)
        {
            Dictionary<int, string> dics = new Dictionary<int, string>();
            for (int i = (DateTime.Now.Year - 5); i < (DateTime.Now.Year + 5); i++)
            {
                dics[i] = i.ToString();
            }
            ViewData["nam"] = new SelectList(dics, "Key", "Value", value);
            ViewData["namtc"] = new SelectList(dics, "Key", "Value", value);
        }

        #region Xuất file chi tiết chấm công theo giờ quét
        public void XuatFileChiTietGioQuet(int thang, int nam, string qSearch)
        {
            try
            {
                var filename = "";
                var virtualPath = HttpRuntime.AppDomainAppVirtualPath;

                var fileStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(virtualPath + @"\Content\Report\ReportTemplateTHCT.xls"), FileMode.Open, FileAccess.Read);

                var workbook = new HSSFWorkbook(fileStream, true);

                filename += "BangChamCongChiTietThang_" + thang + "_" + nam + ".xls";

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



                var sheet = workbook.CreateSheet("BangChiTietGioQuet");

                //Khai báo row đầu tiên
                int firstRowNumber = 3;

                string cellTenCty = "THE ALLEY";
                var titleCellCty = HSSFCellUtil.CreateCell(sheet.CreateRow(0), 0, cellTenCty.ToUpper());
                titleCellCty.CellStyle = styleTitle;

                string cellTenCacBanDH = "";
                var titleCellTenCacBanDH = HSSFCellUtil.CreateCell(sheet.CreateRow(1), 1, cellTenCacBanDH.ToUpper());
                titleCellTenCacBanDH.CellStyle = styleTitle;

                string cellTitleMain = "BẢNG CHẤM CÔNG CHI TIẾT THÁNG " + thang + "/" + nam;
                var titleCellTitleMain = HSSFCellUtil.CreateCell(sheet.CreateRow(2), 3, cellTitleMain.ToUpper());
                titleCellTitleMain.CellStyle = styleTitle;

                firstRowNumber++;

                var list1 = new List<string>();
                list1.Add("STT");
                list1.Add("Họ tên");
                list1.Add("Mã nhân viên");
                list1.Add("Mã vân tay");
                list1.Add("Ngày quét");
                list1.Add("Tên ngày");
                list1.Add("Giờ quyét");
                var idRowStart = firstRowNumber; // bat dau o dong thu 4
                var headerRow = sheet.CreateRow(idRowStart);
                int rowend = idRowStart;
                ReportHelperExcel.CreateHeaderRow(headerRow, 0, styleheadedColumnTable, list1);
                idRowStart++;
                sheet.SetColumnWidth(0, 5 * 210);
                sheet.SetColumnWidth(1, 30 * 210);
                sheet.SetColumnWidth(2, 15 * 210);
                sheet.SetColumnWidth(3, 30 * 210);
                sheet.SetColumnWidth(4, 30 * 210);
                sheet.SetColumnWidth(5, 15 * 210);
                sheet.SetColumnWidth(6, 15 * 210);

                var data = nhanSuContext.sp_NS_XemTinhHinhRaVao(GetUser().manv, thang, nam, qSearch).ToList();

                var stt = 0;
                int dem = 0;

                foreach (var item1 in data)
                {
                    dem = 0;

                    stt++;
                    idRowStart++;

                    rowC = sheet.CreateRow(idRowStart);
                    ReportHelperExcel.SetAlignment(rowC, dem++, stt.ToString(), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.hoTen, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.maNhanVien, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.maChamCong, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0: dd/MM/yyyy}", item1.checktime), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.checktime.Value.DayOfWeek.ToString(), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.checktime.Value.ToString("HH:mm:ss"), hStyleConCenter);
                }


                idRowStart = idRowStart + 2;
                var date = DateTime.Now.Day;
                string cellFooterNgayLap = "Tp.Hồ Chí Minh, ngày " + date + " tháng " + thang + " năm " + nam;
                var titleCellFooterNgayLap = HSSFCellUtil.CreateCell(sheet.CreateRow(idRowStart), 8, cellFooterNgayLap);
                titleCellFooterNgayLap.CellStyle = styleTitleItalic;


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
        #endregion
    }
}
