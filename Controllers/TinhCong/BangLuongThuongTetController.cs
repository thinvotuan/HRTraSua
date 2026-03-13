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
using BatDongSan.Models.DanhMuc;
namespace BatDongSan.Controllers.TinhCong
{
    public class BangLuongThuongTetController : ApplicationController
    {

        private LinqNhanSuDataContext nhanSuContext = new LinqNhanSuDataContext();
        LinqDanhMucDataContext linqDanhMuc = new LinqDanhMucDataContext();
        BatDongSan.Models.NhanSu.LinqThuanViet.LinqThuanVietDataContext lqThuanViet = new BatDongSan.Models.NhanSu.LinqThuanViet.LinqThuanVietDataContext();
        BatDongSan.Models.NhanSu.LinqCT1330.LinqCT1330DataContext lqCT1330 = new BatDongSan.Models.NhanSu.LinqCT1330.LinqCT1330DataContext();
        BatDongSan.Models.NhanSu.LinqCT2220.LinqCT2220DataContext lqCT2220 = new BatDongSan.Models.NhanSu.LinqCT2220.LinqCT2220DataContext();
        private readonly string MCV = "BangLuongThuongTet";
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
            var listLoaiDGs = linqDanhMuc.tbl_DM_XepLoaiDGs.OrderBy(d => d.thuTu).ToList();
            ViewBag.ListLoaiDGs = listLoaiDGs;
            thang(DateTime.Now.Month);
            nam(DateTime.Now.Year);
            return View("");

        }

        public ActionResult LoadIndex(string maPhongBan, string qSearch, int nam, string maXepLoai, int _page = 0)
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
            if (maXepLoai == "AA") { maXepLoai = "A+"; }
            if (maXepLoai == "BB") { maXepLoai = "B+"; }
            int total = nhanSuContext.sp_NS_BangLuongThuongTet(nam, maPhongBan, qSearch, maXepLoai).Count();
            var ckDuyet = "khong";
            var checkDuyet = nhanSuContext.tbl_DuyetBangLuongThuongTets.Where(d => d.nam == nam).FirstOrDefault();
            if (checkDuyet != null) {
                ckDuyet = "duyet";
            }
            ViewBag.ckDuyet = ckDuyet;
            ViewData["lsDanhSach"] = nhanSuContext.sp_NS_BangLuongThuongTet(nam, maPhongBan, qSearch, maXepLoai).ToList();
            ViewData["qSearch"] = qSearch;
            return PartialView("_LoadIndex");
        }

        public ActionResult DuyetLuongThuongTet(int nam)
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
                var checkEx = nhanSuContext.tbl_DuyetBangLuongThuongTets.Where(t => t.nam == nam).FirstOrDefault();
                if (checkEx != null)
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                //End check
                // Insert Row 
                tbl_DuyetBangLuongThuongTet tblDuyetBL = new tbl_DuyetBangLuongThuongTet();
                tblDuyetBL.nam = nam;
                tblDuyetBL.ngayDuyet = DateTime.Now;
                tblDuyetBL.nguoiDuyet = GetUser().manv;
                nhanSuContext.tbl_DuyetBangLuongThuongTets.InsertOnSubmit(tblDuyetBL);
                nhanSuContext.SubmitChanges();
                // End Insert Row
                // Check Exist
                var list = nhanSuContext.tbl_DuyetBangLuongThuongTets.Where(t => t.nam == nam).FirstOrDefault();

                if (list != null)
                {
                    result = new { kq = true };
                }
                SaveActiveHistory("Duyệt bảng lương thưởng tết năm: " + nam + ". User duyệt: " + GetUser().userName);
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
        public void XuatFileBLThuongTet(int nam, string maPhongBan, string maXepLoai, string qSearch)
        {

            var filename = "";
            var virtualPath = HttpRuntime.AppDomainAppVirtualPath;

            filename += "BangLuongThuongTet_" + nam + ".xlsx";

            using (ExcelPackage package = new ExcelPackage())
            {
                //Create a sheet
                package.Workbook.Worksheets.Add("BangLuongThuongTet_" + nam);
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                //Header
                //insert từ dòng nào, bao nhiêu row
                var rowFrom = 1;
                worksheet.InsertRow(rowFrom, 1);
                worksheet.Cells[1, 1].Value = "STT";
                worksheet.Cells[1, 2].Value = "Mã nhân viên";
                worksheet.Cells[1, 3].Value = "Họ tên";
                worksheet.Cells[1, 4].Value = "Họ tên không dấu";
                worksheet.Cells[1, 5].Value = "Số CMND";
                worksheet.Cells[1, 6].Value = "Số TK";
                worksheet.Cells[1, 7].Value = "Chi nhánh";
                worksheet.Cells[1, 8].Value = "Bộ phận tính lương";
                worksheet.Cells[1, 9].Value = "Điểm đánh giá";
                worksheet.Cells[1, 10].Value = "Xếp loại";
                worksheet.Cells[1, 11].Value = "Tổng thu nhập trong năm";

                worksheet.Cells[1, 12].Value = "Lương tháng trung bình";
                worksheet.Cells[1, 13].Value = "Số tháng thưởng";
                worksheet.Cells[1, 14].Value = "Số tiền thưởng";
                worksheet.Cells[1, 15].Value = "Thuế TNCN";
                worksheet.Cells[1, 16].Value = "Số tiền chuyển đợt 1";
                worksheet.Cells[1, 17].Value = "Số tiền chuyển đợt 2";

                worksheet.Column(2).Width = 30;
                worksheet.Column(3).Width = 30;
                worksheet.Column(4).Width = 30;
                worksheet.Column(5).Width = 30;
                worksheet.Column(6).Width = 30;
                worksheet.Column(7).Width = 30;
                worksheet.Column(8).Width = 30;
                worksheet.Column(9).Width = 30;
                worksheet.Column(10).Width = 30;
                worksheet.Column(11).Width = 30;
                worksheet.Column(12).Width = 30;
                worksheet.Column(13).Width = 30;
                worksheet.Column(14).Width = 30;
                worksheet.Column(15).Width = 30;
                worksheet.Column(16).Width = 30;
                worksheet.Column(17).Width = 30;
                //// Formatting style of the header
                using (var range = worksheet.Cells[1, 1, 1, 17])
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
                if (maXepLoai == "AA") { maXepLoai = "A+"; }
                if (maXepLoai == "BB") { maXepLoai = "B+"; }
                var data = nhanSuContext.sp_NS_BangLuongThuongTet(nam, maPhongBan, qSearch, maXepLoai).ToList();

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
                        worksheet.Cells[rowFrom, 2].Value = item.maNhanVien;
                        worksheet.Cells[rowFrom, 3].Value = item.hoTen;
                        worksheet.Cells[rowFrom, 4].Value = item.hoTenKhongDau;
                        worksheet.Cells[rowFrom, 5].Value = item.CMNDSo;

                        worksheet.Cells[rowFrom, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        worksheet.Cells[rowFrom, 6].Value = item.soTaiKhoan;

                        worksheet.Cells[rowFrom, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        worksheet.Cells[rowFrom, 7].Value = item.maChiNhanhNganHang;
                        worksheet.Cells[rowFrom, 8].Value = item.tenKhoiTinhLuong;
                        worksheet.Cells[rowFrom, 9].Value = item.diemDanhGia;
                        worksheet.Cells[rowFrom, 10].Value = item.xepLoai;
                        worksheet.Cells[rowFrom, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        worksheet.Cells[rowFrom, 11].Value = item.tongThuNhapTrongNam;
                        worksheet.Cells[rowFrom, 11].Style.Numberformat.Format = "#,##0.00";
                        worksheet.Cells[rowFrom, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        worksheet.Cells[rowFrom, 12].Value = item.luongThangTrungBinh;
                        worksheet.Cells[rowFrom, 12].Style.Numberformat.Format = "#,##0.00";
                        worksheet.Cells[rowFrom, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        worksheet.Cells[rowFrom, 13].Value = item.soThangThuong;
                        worksheet.Cells[rowFrom, 13].Style.Numberformat.Format = "#,##0.00";
                        worksheet.Cells[rowFrom, 13].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        worksheet.Cells[rowFrom, 14].Value = item.soTienThuong;
                        worksheet.Cells[rowFrom, 14].Style.Numberformat.Format = "#,##0.00";
                        worksheet.Cells[rowFrom, 14].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        worksheet.Cells[rowFrom, 15].Value = item.thue1Thang;
                        worksheet.Cells[rowFrom, 15].Style.Numberformat.Format = "#,##0.00";
                        worksheet.Cells[rowFrom, 15].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        worksheet.Cells[rowFrom, 16].Value = item.chuyenLan1;
                        worksheet.Cells[rowFrom, 16].Style.Numberformat.Format = "#,##0.00";
                        worksheet.Cells[rowFrom, 16].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        worksheet.Cells[rowFrom, 17].Value = item.chuyenLan2;
                        worksheet.Cells[rowFrom, 17].Style.Numberformat.Format = "#,##0.00";
                        worksheet.Cells[rowFrom, 17].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

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
        public ActionResult CreateCL(int? nam, string congTrinh)
        {
            DeNghiChiLuong deNghiCL = new DeNghiChiLuong();
            if (congTrinh == "CTTV")
            {
                deNghiCL.soPhieu = CheckLetterDNCL("DNCL", GetMaxDeNghiChiLuongTV(), 3);
            }
            else if (congTrinh == "CT1330")
            {
                deNghiCL.soPhieu = CheckLetterDNCL("DNCL", GetMaxDeNghiChiLuongCT1330(), 3);
            }
            else if (congTrinh == "CT2220")
            {
                deNghiCL.soPhieu = CheckLetterDNCL("DNCL", GetMaxDeNghiChiLuongCT2220(), 3);
            }
            deNghiCL.thang = 13;
            deNghiCL.nam = nam;
            deNghiCL.maNguoiLap = GetUser().manv;
            deNghiCL.tenNguoiLap = HoVaTen(GetUser().manv);
            deNghiCL.ngayLap = DateTime.Now;
            var chiTietLuong = (from p in nhanSuContext.sp_NS_ThuongTet13(congTrinh,nam)
                                select new DeNghiChiLuongChiTiet
                                {
                                    BHXH = 0,
                                    ChuyenKhoan = Convert.ToDouble(p.thucLanh),
                                    LuongThang = 0,
                                    TenBoPhanTinhLuong = p.phongBan,
                                    ThueTNCN = Convert.ToDouble(p.thue),
                                    TruyLanhTruyThu = 0,
                                    TruyLanhTruyThuBaoHiem = 0,
                                    TruyLanhTruyThuThue = 0,
                                    TruyLanhTruyThuTamUng =0,
                                    PhuCapCT = 0,
                                    PhuCapKhac = 0,

                                }).ToList();
            deNghiCL.DeNghiChiLuongChiTiet = chiTietLuong;
            deNghiCL.maCongTrinh = congTrinh;
            return View(deNghiCL);
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
                else if (maCongTrinh == "CT1330")
                {
                    TaoDeNghiChiLuongCT1330(coll);
                }
                else if (maCongTrinh == "CT2220")
                {
                    TaoDeNghiChiLuongCT2220(coll);
                }

                return RedirectToAction("Index", "BangLuongThuongTet");
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
        public void TaoDeNghiChiLuongCT1330(FormCollection coll)
        {
            BatDongSan.Models.NhanSu.LinqCT1330.tbl_DeNghiChiLuongChiTiet deNghiChiTiet;
            List<BatDongSan.Models.NhanSu.LinqCT1330.tbl_DeNghiChiLuongChiTiet> lsChiTiet = new List<BatDongSan.Models.NhanSu.LinqCT1330.tbl_DeNghiChiLuongChiTiet>();
            BatDongSan.Models.NhanSu.LinqCT1330.tbl_DeNghiChiLuong deNghi = new BatDongSan.Models.NhanSu.LinqCT1330.tbl_DeNghiChiLuong();
            deNghi.maPhieu = CheckLetterDNCL("DNCL", GetMaxDeNghiChiLuongCT1330(), 3);
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
        public void TaoDeNghiChiLuongCT2220(FormCollection coll)
        {
            BatDongSan.Models.NhanSu.LinqCT2220.tbl_DeNghiChiLuongChiTiet deNghiChiTiet;
            List<BatDongSan.Models.NhanSu.LinqCT2220.tbl_DeNghiChiLuongChiTiet> lsChiTiet = new List<BatDongSan.Models.NhanSu.LinqCT2220.tbl_DeNghiChiLuongChiTiet>();
            BatDongSan.Models.NhanSu.LinqCT2220.tbl_DeNghiChiLuong deNghi = new BatDongSan.Models.NhanSu.LinqCT2220.tbl_DeNghiChiLuong();
            deNghi.maPhieu = CheckLetterDNCL("DNCL", GetMaxDeNghiChiLuongCT2220(), 3);
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
