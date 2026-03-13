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
using Worldsoft.Mvc.Web.Util;
using NPOI.HSSF.UserModel.Contrib;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
namespace BatDongSan.Controllers.TinhCong
{
    public class BangChamCongTongHopController : ApplicationController
    {

        private LinqNhanSuDataContext nhanSuContext = new LinqNhanSuDataContext();
        private IList<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan> phongBans;
        private StringBuilder buildTree;
        private readonly string MCV = "ChamCongAM";
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

            int page = _page == 0 ? 1 : _page;
            int pIndex = page;
            int total = nhanSuContext.sp_NS_BangChamCongChiTiet(thang, nam, qSearch, "").Count();
            PagingLoaderFullController("/BangChamCongTongHop/LoadBangChamCongChiTiet/", total, page, "?qsearch=" + qSearch);
            ViewData["lsDanhSach"] = nhanSuContext.sp_NS_BangChamCongChiTiet(thang, nam, qSearch, "").Skip(start).Take(offset).ToList();

            ViewData["qSearch"] = qSearch;
            return PartialView("_LoadBangChamCongChiTiet");
        }


        #region Xuat File XuatFileBangTongHopCongThang
        public void XuatFileBangTongHopCongThang(string qSearch, string maPhongBan, int thang, int nam, int _page = 0)
        {
            try
            {
                var filename = "";
                var virtualPath = HttpRuntime.AppDomainAppVirtualPath;

                var fileStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(virtualPath + @"\Content\Report\ReportTemplateTHCT.xls"), FileMode.Open, FileAccess.Read);

                var workbook = new HSSFWorkbook(fileStream, true);

                filename += "BangTongHopCongThang_" + thang + "_" + nam + ".xls";

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



                var sheet = workbook.CreateSheet("BangTongHopCongThang");

                //Khai báo row đầu tiên
                int firstRowNumber = 3;

                string cellTenCty = "TỔNG CÔNG TY XDCTGT 6 - CÔNG TY CỔ PHẦN";
                var titleCellCty = HSSFCellUtil.CreateCell(sheet.CreateRow(0), 0, cellTenCty.ToUpper());
                titleCellCty.CellStyle = styleTitle;

                string cellTenCacBanDH = "CÁC BAN ĐIỀU HÀNH DỰ ÁN";
                var titleCellTenCacBanDH = HSSFCellUtil.CreateCell(sheet.CreateRow(1), 1, cellTenCacBanDH.ToUpper());
                titleCellTenCacBanDH.CellStyle = styleTitle;

                string cellTitleMain = "BẢNG TỔNG HỢP CÔNG THÁNG " + thang + "/" + nam;
                var titleCellTitleMain = HSSFCellUtil.CreateCell(sheet.CreateRow(2), 5, cellTitleMain.ToUpper());
                titleCellTitleMain.CellStyle = styleTitle;

                firstRowNumber++;

                var list1 = new List<string>();
                list1.Add("STT");
                list1.Add("Họ tên");
                list1.Add("Mã nhân viên");
                list1.Add("Mã chấm công");
                list1.Add("Phòng ban");
                list1.Add("Chức vụ");
                list1.Add("Công chuẩn");
                list1.Add("Số ngày quét");
                list1.Add("Công tác");
                list1.Add("Nghỉ phép có lương");
                list1.Add("Nghỉ phép không lương");
                list1.Add("Nghỉ lễ");
                list1.Add("Lũy kế tháng trước");
                list1.Add("Tổng cộng");

                var idRowStart = firstRowNumber; // bat dau o dong thu 4
                var headerRow = sheet.CreateRow(idRowStart);
                int rowend = idRowStart;
                ReportHelperExcel.CreateHeaderRow(headerRow, 0, styleheadedColumnTable, list1);
                idRowStart++;
                sheet.SetColumnWidth(0, 5 * 210);
                sheet.SetColumnWidth(1, 30 * 210);
                sheet.SetColumnWidth(2, 15 * 210);
                sheet.SetColumnWidth(3, 15 * 210);
                sheet.SetColumnWidth(4, 30 * 210);
                sheet.SetColumnWidth(5, 15 * 210);
                sheet.SetColumnWidth(6, 15 * 210);
                sheet.SetColumnWidth(7, 15 * 210);
                sheet.SetColumnWidth(8, 15 * 210);
                sheet.SetColumnWidth(9, 15 * 210);
                sheet.SetColumnWidth(10, 15 * 210);
                sheet.SetColumnWidth(11, 15 * 210);


                var data = nhanSuContext.sp_NS_BangTongHopCongThang(thang, nam, qSearch, "", 0, maPhongBan).ToList();
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
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.phongBan, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.chucVu, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.congChuan, hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.ngayQuet, hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.congTac, hStyleConRight);

                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.nghiPhep, hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.nghiPhepKhongLuong, hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.nghiLe, hStyleConRight);

                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.luyKeThangTruoc, hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.tongCong, hStyleConRight);
                }


                idRowStart = idRowStart + 2;
                var date = DateTime.Now.Day;
                string cellFooterNgayLap = "Tp.Hồ Chí Minh, ngày " + date + " tháng " + thang + " năm " + nam;
                var titleCellFooterNgayLap = HSSFCellUtil.CreateCell(sheet.CreateRow(idRowStart), 8, cellFooterNgayLap);
                titleCellFooterNgayLap.CellStyle = styleTitleItalic;

                //idRowStart = idRowStart + 2;
                //string cellFooterPTC = "PHÒNG TỔ CHỨC CB-LĐ";
                //var titleCellFooterPTC = HSSFCellUtil.CreateCell(sheet.CreateRow(idRowStart), 1, cellFooterPTC);
                //titleCellFooterPTC.CellStyle = styleTitle;

                //string cellFooterKT = "PHÒNG TÀI CHÍNH KẾ TOÁN";
                //var titleCellFooterKT = HSSFCellUtil.CreateCell(sheet.GetRow(idRowStart), 7, cellFooterKT);
                //titleCellFooterKT.CellStyle = styleTitle;

                //string cellFooterTGD = "TỔNG GIÁM ĐỐC";
                //var titleCellFooterTGD = HSSFCellUtil.CreateCell(sheet.GetRow(idRowStart), 14, cellFooterTGD);
                //titleCellFooterTGD.CellStyle = styleTitle;


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

        public ActionResult XemBangChamCongTongHop()
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
        public ActionResult LoadBangChamCongTongHop(string qSearch, string maPhongBan, int thang, int nam, int _page = 0)
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
            int total = nhanSuContext.sp_NS_BangTongHopCongThang(thang, nam, qSearch, "", 0, maPhongBan).Count();
            PagingLoaderFullController("/BangChamCongTongHop/LoadBangChamCongTongHop/", total, page, "?qsearch=" + qSearch);
            ViewData["lsDanhSach"] = nhanSuContext.sp_NS_BangTongHopCongThang(thang, nam, qSearch, "", 0, maPhongBan).Skip(start).Take(1000).ToList();

            ViewData["qSearch"] = qSearch;
            return PartialView("_LoadBangChamCongTongHop");
        }
        public ActionResult UpdateBangChamCongTH(int thang, int nam)
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

                var list = nhanSuContext.sp_NS_BangTongHopCongThang_TinhToan(thang, nam);


                var result = new { kq = true };
                SaveActiveHistory("Tính công tháng: " + thang + " năm: " + nam);
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch
            {
                return View();
            }

        }
        public ActionResult UpdateBangChamCongChiTiet(int thang, int nam)
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
                SaveActiveHistory("Tính công chi tiết: " + thang + " năm: " + nam);
                var list = nhanSuContext.sp_Ns_CapNhatBangCongChiTiet(thang, nam);


                var result = new { kq = true };

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
            ViewData["thangtc"] = new SelectList(dics, "Key", "Value", value);
        }
        private void nam(int value)
        {
            Dictionary<int, string> dics = new Dictionary<int, string>();
            for (int i = 2015; i < 2031; i++)
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

                string cellTenCacBanDH = "CÁC BAN ĐIỀU HÀNH DỰ ÁN";
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
                list1.Add("Giờ vào");
                list1.Add("Giờ ra");
                list1.Add("Loại");
                list1.Add("Số giờ công");
                list1.Add("Thành tiền");
                list1.Add("Phụ cấp tiền ăn");
                var idRowStart = firstRowNumber; // bat dau o dong thu 4
                var headerRow = sheet.CreateRow(idRowStart);
                int rowend = idRowStart;
                ReportHelperExcel.CreateHeaderRow(headerRow, 0, styleheadedColumnTable, list1);
                idRowStart++;
                sheet.SetColumnWidth(0, 5 * 210);
                sheet.SetColumnWidth(1, 30 * 210);
                sheet.SetColumnWidth(2, 15 * 210);
                sheet.SetColumnWidth(3, 15 * 210);
                sheet.SetColumnWidth(4, 30 * 210);
                sheet.SetColumnWidth(5, 15 * 210);
                sheet.SetColumnWidth(6, 15 * 210);
                sheet.SetColumnWidth(7, 15 * 210);
                sheet.SetColumnWidth(8, 15 * 210);
                sheet.SetColumnWidth(9, 15 * 210);
                sheet.SetColumnWidth(10, 15 * 210);
                sheet.SetColumnWidth(11, 15 * 210);
                sheet.SetColumnWidth(12, 15 * 210);

                var data = nhanSuContext.sp_NS_BangChamCongChiTiet(thang, nam, qSearch, "").ToList();

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
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.maVanTay, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0: dd/MM/yyyy}", item1.ngayLam), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.ngayLam.Value.DayOfWeek.ToString(), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.quetVao.Value.ToString("HH:mm:ss"), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.quetRa.Value.ToString("HH:mm:ss"), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.loai, hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.soGioCong, hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item1.thanhTienLuong), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item1.thanhTienPhuCap), hStyleConRight);
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
