using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BatDongSan.Helper.Utils;
using BatDongSan.Models.NhanSu;
using System.Text;
using BatDongSan.Utils.Paging;
using BatDongSan.Helper.Common;
using System.Globalization;
using System.Configuration;
using System.Net.Mail;
using BatDongSan.Models.PhieuDeNghi;
using BatDongSan.Models.DanhMuc;

namespace BatDongSan.Controllers.BaoCao
{
    public class BaoCaoNhanSuController : ApplicationController
    {
        private LinqNhanSuDataContext context = new LinqNhanSuDataContext();
        LinqPhieuDeNghiDataContext lqPhieuDN = new LinqPhieuDeNghiDataContext();
        private LinqDanhMucDataContext linqDanhMuc = new LinqDanhMucDataContext();

        private IList<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan> phongBans;
        private IList<BatDongSan.Models.NhanSu.sp_NS_NhanVien_IndexResult> nhanViens;
        private StringBuilder buildTree;
        private readonly string MCV = "BaoCaoNhanSu";
        private bool? permission;
        //
        // GET: /BaoCaoNhanSu/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult BCSinhNhatNV(int? page, int? pageSize, string searchString, int? thang, int? day)
        {
            #region Role user
            permission = GetPermission("BCSinhNhatNV", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            
            BuilThang(DateTime.Now.Month);
            thang = thang.HasValue ? thang : DateTime.Now.Month;
            
            return View("");
        }
        public ActionResult BaoCaoXepLoai(string maPhongBan, int? mucLevel, int? qui, int? nam, string qSearch, int? page, int? pageSize)
        {
            #region Role user
            permission = GetPermission("BCNSSEPLOAI", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            int currentPageIndex = page.HasValue ? page.Value : 1;
            pageSize = pageSize ?? 30;
            int? tongSoDong = 0;


            //
            buildTree = new StringBuilder();
            phongBans = linqDanhMuc.tbl_DM_PhongBans.ToList();
            buildTree = TreePhongBans.BuildTreeDepartment(phongBans);
            ViewBag.PhongBans = buildTree.ToString();
            //
            try
            {
                ViewBag.Count = nhanViens[0].tongSoDong;
                tongSoDong = nhanViens[0].tongSoDong;
            }
            catch
            {
                ViewBag.Count = 0;
            }
            int? NewNam = nam == null ? DateTime.Now.Year : Convert.ToInt32(nam);
            if (qui == null)
            {
                qui = 1;
            }
            ViewData["qui"] = qui;
            ViewData["nam"] = nam;
            ViewData["mucLevel"] = mucLevel;
            ViewData["maPhongBan"] = maPhongBan;
            ViewData["tenPhongBan"] = context.vw_NS_DanhSachNhanVienTheoPhongBans.Where(d => d.maPhongBan == maPhongBan).Select(d => d.tenPhongBan).FirstOrDefault();
            ViewData["qSearch"] = qSearch;



            ViewData["yearWorkings"] = new SelectList(GetYearLimits((int)NewNam, 5), NewNam);

            var listLevel = context.Sys_ChucDanhs.ToList();
            Dictionary<string, string> capBac = new Dictionary<string, string>();
            capBac.Add("", "--Tất cả--");
            foreach (var item in listLevel.OrderByDescending(d => d.SoCapBac))
            {

                capBac.Add(item.MaChucDanh, item.TenChucDanh);
            }
            ViewData["ListCapBac"] = new SelectList(capBac, "Key", "Value", mucLevel);
            return View("");
        }
        public ActionResult LoadBaoCaoXepLoai(string maPhongBan, int? mucLevel, int? qui, int? nam, string qSearch, int _page = 0)
        {
            #region Role user
            permission = GetPermission("BCNSSEPLOAI", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion


            int? NewNam = nam == null ? DateTime.Now.Year : Convert.ToInt32(nam);
            if (qui == null)
            {
                qui = 1;
            }
            int page = _page == 0 ? 1 : _page;
            int pIndex = page;
            int total = context.sp_NS_BaoCaoDanhGiaNhanVien(qui, NewNam, maPhongBan, mucLevel, qSearch).Count();
            PagingLoaderController("/DanhGiaTinNhiem/BaoCaoXepLoai/", total, page, "?qsearch=" + qSearch + "&qui=" + qui + "&NewNam=" + nam + "&maPhongBan=" + maPhongBan + "&mucLevel=" + mucLevel);
            ViewData["lsDanhSach"] = context.sp_NS_BaoCaoDanhGiaNhanVien(qui, NewNam, maPhongBan, mucLevel, qSearch).Skip(start).Take(offset).ToList();

            ViewData["qui"] = qui;
            ViewData["nam"] = nam;
            ViewData["mucLevel"] = mucLevel;
            ViewData["maPhongBan"] = maPhongBan;
            ViewData["tenPhongBan"] = context.vw_NS_DanhSachNhanVienTheoPhongBans.Where(d => d.maPhongBan == maPhongBan).Select(d => d.tenPhongBan).FirstOrDefault();
            ViewData["qSearch"] = qSearch;



            ViewData["yearWorkings"] = new SelectList(GetYearLimits((int)NewNam, 5), NewNam);

            var listLevel = context.Sys_ChucDanhs.ToList();
            Dictionary<string, string> capBac = new Dictionary<string, string>();
            capBac.Add("", "--Tất cả--");
            foreach (var item in listLevel.OrderByDescending(d => d.SoCapBac))
            {

                capBac.Add(item.MaChucDanh, item.TenChucDanh);
            }
            ViewData["ListCapBac"] = new SelectList(capBac, "Key", "Value", mucLevel);
            return PartialView("_LoadBaoCaoXepLoai");
        }
        public ActionResult BCSinhNhatNVViewIndex(int? thang, int? day, int? pageSize, string searchString, int _page = 0)
        {
            try
            {
                #region Role user
                permission = GetPermission("BCSinhNhatNV", BangPhanQuyen.QuyenXem);
                if (!permission.HasValue)
                    return View("LogIn");
                if (!permission.Value)
                    return View("AccessDenied");
                #endregion

                
                context = new LinqNhanSuDataContext();
                ViewBag.isGet = "True";

                string maNhanVien = GetUser().manv;
                int page = _page == 0 ? 1 : _page;
                int pIndex = page;

                int total = context.sp_BC_NS_SinhNhatNhanVien(thang, day, searchString).Count();

                PagingLoaderController("/BaoCaoNhanSu/BCSinhNhatNV/", total, page, "?searchString=" + searchString + "&thang=" + thang + "&day=" + day);
                ViewData["lsDanhSach"] = context.sp_BC_NS_SinhNhatNhanVien(thang, day, searchString).Skip(start).Take(offset).ToList();

                return PartialView("ViewIndexSN");
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }

        }
        private void BuilThang(int thang)
        {
            Dictionary<int, int> dict = new Dictionary<int, int>();
            for (var i = 1; i <= 12; i++)
            {
                dict.Add(i, i);
            }
            ViewBag.Thangs = new SelectList(dict, "Key", "Value", thang);
        }
        public ActionResult BCDiTreVeSom(int? page, int? pageSize, string searchString, string tuNgay, string denNgay)
        {
            #region Role user
            permission = GetPermission("BCDiTreVeSom", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
           
           
            return View("");
        }
        public ActionResult BCDiTreVeSomViewIndex(int? pageSize, string searchString, string tuNgay, string denNgay, int _page = 0)
        {
            try
            {
                #region Role user
                permission = GetPermission("BCDiTreVeSom", BangPhanQuyen.QuyenXem);
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
                context = new LinqNhanSuDataContext();
                ViewBag.isGet = "True";

                string maNhanVien = GetUser().manv;
                int page = _page == 0 ? 1 : _page;
                int pIndex = page;

                int total = context.sp_BC_NS_NhanVienDiTreVeSom(fromDate, toDate, searchString).Count();

                PagingLoaderController("/BaoCaoNhanSu/BCDiTreVeSom/", total, page, "?searchString=" + searchString + "&tuNgay=" + tuNgay + "&denNgay=" + denNgay);
                ViewData["lsDanhSach"] = context.sp_BC_NS_NhanVienDiTreVeSom(fromDate, toDate, searchString).Skip(start).Take(offset).ToList();

                return PartialView("ViewIndexDTVS");
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }

        }
        public ActionResult BCNhanVienNghiPhep(int? page, int? pageSize, string searchString, string tuNgay, string denNgay)
        {
            #region Role user
            permission = GetPermission("BCNhanVienNghiPhep", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            return View("BCNhanVienNghiPhep");
        }
        public ActionResult BCNhanVienNghiPhepViewIndex(int? pageSize, string searchString, string tuNgay, string denNgay, int _page = 0)
        {
            try
            {
                #region Role user
                permission = GetPermission("BCNhanVienNghiPhep", BangPhanQuyen.QuyenXem);
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
                context = new LinqNhanSuDataContext();
                ViewBag.isGet = "True";
                
                string maNhanVien = GetUser().manv;
                int page = _page == 0 ? 1 : _page;
                int pIndex = page;

                int total = context.sp_BC_NS_NhanVienNghiPhep(fromDate, toDate, searchString).Count();

                PagingLoaderController("/BaoCaoNhanSu/BCNhanVienNghiPhep/", total, page, "?searchString=" + searchString + "&tuNgay=" + tuNgay + "&denNgay=" + denNgay);
                ViewData["lsDanhSach"] = context.sp_BC_NS_NhanVienNghiPhep(fromDate, toDate, searchString).Skip(start).Take(offset).ToList();

                return PartialView("ViewIndexNP");
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }

        }
        public ActionResult BCTQNghiPhep()
        {
            #region Role user
            permission = GetPermission("BCTQNghiPhep", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            buildTree = new StringBuilder();
            phongBans = context.GetTable<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan>().ToList();
            buildTree = TreePhongBanStyle.BuildTreeDepartment(phongBans);
            ViewBag.PhongBans = buildTree.ToString();
            nam(DateTime.Now.Year);
            return View("BCTQNghiPhep");
        }
        public ActionResult BCTQNghiPhepViewIndex(int? pageSize, string searchString, string maPhongBan, int nam, int _page = 0)
        {
            try
            {
                #region Role user
                permission = GetPermission("BCTQNghiPhep", BangPhanQuyen.QuyenXem);
                if (!permission.HasValue)
                    return View("LogIn");
                if (!permission.Value)
                    return View("AccessDenied");
                #endregion
                context = new LinqNhanSuDataContext();
                ViewBag.isGet = "True";

                string maNhanVien = GetUser().manv;
                int page = _page == 0 ? 1 : _page;
                int pIndex = page;

                int total = context.sp_BC_NghiPhep_Index(maPhongBan, searchString, nam).Count();

                PagingLoaderController("/BaoCaoNhanSu/BCTQNghiPhep/", total, page, "?searchString=" + searchString + "&maPhongBan=" + maPhongBan + "&nam=" + nam);
                ViewData["lsDanhSach"] = context.sp_BC_NghiPhep_Index(maPhongBan, searchString, nam).Skip(start).Take(offset).ToList();

                return PartialView("ViewIndexNPTQ");
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }

        }

        public ActionResult BCHopDongSapHetHan(int? page, int? pageSize, string searchString, int? soNgayHetHan)
        {
            #region Role user
            permission = GetPermission("BCHopDongSapHetHan", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            
            return View("");
        }
        public ActionResult BCHopDongSapHetHanViewIndex(int? soNgayHetHan, string searchString, int _page = 0)
        {
            try
            {
                #region Role user
                permission = GetPermission("BCHopDongSapHetHan", BangPhanQuyen.QuyenXem);
                if (!permission.HasValue)
                    return View("LogIn");
                if (!permission.Value)
                    return View("AccessDenied");
                #endregion


                context = new LinqNhanSuDataContext();
                ViewBag.isGet = "True";

                string maNhanVien = GetUser().manv;
                int page = _page == 0 ? 1 : _page;
                int pIndex = page;

                int total = context.sp_BC_NS_HopDongSapHetHan(soNgayHetHan, searchString).Count();

                PagingLoaderController("/BaoCaoNhanSu/BCHopDongSapHetHan/", total, page, "?searchString=" + searchString + "&soNgayHetHan=" + soNgayHetHan);
                ViewData["lsDanhSach"] = context.sp_BC_NS_HopDongSapHetHan(soNgayHetHan, searchString).Skip(start).Take(offset).ToList();

                return PartialView("ViewIndexHDHH");
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }

        }
        public ActionResult BCTangCaTrongNam(int? page, int? pageSize, string searchString)
        {
            #region Role user
            permission = GetPermission("BCTangCaTrongNam", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            

           
            return View("");
        }
        public ActionResult BCTangCaTrongNamViewIndex(string searchString, int _page = 0)
        {
            try
            {
                #region Role user
                permission = GetPermission("BCTangCaTrongNam", BangPhanQuyen.QuyenXem);
                if (!permission.HasValue)
                    return View("LogIn");
                if (!permission.Value)
                    return View("AccessDenied");
                #endregion


                context = new LinqNhanSuDataContext();
                ViewBag.isGet = "True";

                string maNhanVien = GetUser().manv;
                int page = _page == 0 ? 1 : _page;
                int pIndex = page;

                int total = lqPhieuDN.sp_NS_TongSoGioTangCa(searchString).Count();

                PagingLoaderController("/BaoCaoNhanSu/BCTangCaTrongNam/", total, page, "?searchString=" + searchString);
                ViewData["lsDanhSach"] = lqPhieuDN.sp_NS_TongSoGioTangCa(searchString).Skip(start).Take(offset).ToList();

                return PartialView("ViewIndexTangCaTrongNam");
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }

        }
        public ActionResult BieuDoLuong()
        {
            thang(DateTime.Now.Month);
            nam(DateTime.Now.Year);
            return View();
        }
        public ActionResult GetListBieuDoLuong(int? thangFrom, int? thangTo, int? namFrom, int? namTo)
        {

            #region Role user
            permission = GetPermission("BieuDoLuong", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            try
            {

                var list = context.sp_BC_NS_BieuDoLuongThang(thangFrom, namFrom, thangTo, namTo).ToList();
                var result = new { kq = list };
                return Json(result, JsonRequestBehavior.AllowGet);





            }
            catch
            {
                var result = new { kq = false };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }
        private void thang(int value)
        {
            Dictionary<int, string> dics = new Dictionary<int, string>();
            for (int i = 0; i < 13; i++)
            {
                dics[i] = i.ToString();
            }
            ViewData["lstThangFrom"] = new SelectList(dics, "Key", "Value", value);
            ViewData["lstThangTo"] = new SelectList(dics, "Key", "Value", value);

        }
        private void nam(int value)
        {
            Dictionary<int, string> dics = new Dictionary<int, string>();
            for (int i = 2015; i < 2031; i++)
            {
                dics[i] = i.ToString();
            }
            ViewData["lstNamFrom"] = new SelectList(dics, "Key", "Value", value);
            ViewData["lstNamTo"] = new SelectList(dics, "Key", "Value", value);
            ViewData["lstNam"] = new SelectList(dics, "Key", "Value", value);

        }

        public ActionResult ListNhomDanhGia(int? nam)
        {
            var listNhomDanhGiaNew = linqDanhMuc.sp_NS_BaoCaoDanhGiaXepLoai_PhongBan(nam).ToList();
            if (listNhomDanhGiaNew.Count == 0)
            {
                listNhomDanhGiaNew = null;
            }

            return Json(new { lists = listNhomDanhGiaNew });
        }
        public ActionResult BaoCaoXepLoaiNam(int? nam)
        {
            #region Role user
            permission = GetPermission("BCNhanSuXepLoaiNam", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion


            int? NewNam = nam == null ? DateTime.Now.Year : Convert.ToInt32(nam);

            ViewData["yearWorkings"] = new SelectList(GetYearLimits((int)NewNam, 5), NewNam);
            var listLoaiDGs = linqDanhMuc.tbl_DM_XepLoaiDGs.OrderBy(d => d.thuTu).ToList();
            ViewBag.ListLoaiDGs = listLoaiDGs;
            // Select ma nhom danh gia
            var listNhomDanhGiaNew = linqDanhMuc.sp_DG_XepLoaiHang(NewNam).ToList();
            ViewBag.ListNhomDanhGiasNews = listNhomDanhGiaNew;
            // End select ma nhom danh gia
            // Get tong so nhan vien
          
            return View("");
        }
        public ActionResult LoadBaoCaoChiTietNhanVienPB(int? nam, string maNhomPhongBan, string maNhomDanhGia, string qSearch, int pageIndex, int pageSize)
        {
            #region Role user
            permission = GetPermission("BCNhanSuXepLoaiNam", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            // Check voi nam nay da tao bao cao xep loai nam chua
            try
            {
                var checkEx = context.tbl_DuyetXepLoaiNVs.Where(t => t.nam == nam).FirstOrDefault();
                if (checkEx != null)
                {


                    var list = linqDanhMuc.sp_NS_BaoCaoDanhGiaXepLoai_Index(nam, maNhomPhongBan, maNhomDanhGia, qSearch, pageIndex, pageSize).ToList();


                    if (list.Count == 0)
                    {
                        list = null;
                    }

                    return Json(new { count = list.Count(), lists = list });
                }
                
                return Json(new { count = 0, lists = "" });
                
                
            }
            catch
            {
                return Json(string.Empty);
            }
        }
        public static List<int> GetYearLimits(int currentYear, int limmit)
        {
            List<int> years = new List<int>();
            for (int i = currentYear - limmit; i <= currentYear + limmit; i++)
            {
                years.Add(i);
            }

            return years;
        }
        // check mail trong thang da send
        public ActionResult CheckUpdateMailSN(int thang, string type)
        {


            try
            {
                #region Role user
                permission = GetPermission("BCSinhNhatNV", BangPhanQuyen.QuyenXem);
                if (!permission.HasValue)
                    return View("LogIn");
                if (!permission.Value)
                    return View("AccessDenied");
                #endregion
                //Check 
                var result = new { kq = false };

              
                if (type == "month")
                {
                     var checkEx = context.tbl_TrangThaiSinhNhats.Where(t => t.nam == DateTime.Now.Year && t.thang == thang && t.day == null).FirstOrDefault();
                     if (checkEx != null)
                     {
                         return Json(result, JsonRequestBehavior.AllowGet);
                     }
                }
                else {
                     var checkEx = context.tbl_TrangThaiSinhNhats.Where(t => t.nam == DateTime.Now.Year && t.thang == thang && t.day == DateTime.Now.Day).FirstOrDefault();
                     if (checkEx != null)
                     {
                         return Json(result, JsonRequestBehavior.AllowGet);
                     }
                }
                    
                //End check
                // Insert Row 
                tbl_TrangThaiSinhNhat tblDuyetBL = new tbl_TrangThaiSinhNhat();
                tblDuyetBL.nam = DateTime.Now.Year;
                tblDuyetBL.thang = thang;
                if (type == "month")
                {
                    tblDuyetBL.day = null;
                }
                else {
                    tblDuyetBL.day = DateTime.Now.Day;
                }
                tblDuyetBL.ngayDuyet = DateTime.Now;
                tblDuyetBL.nguoiDuyet = GetUser().manv;
                context.tbl_TrangThaiSinhNhats.InsertOnSubmit(tblDuyetBL);
                context.SubmitChanges();
                // End Insert Row
                // Check Exist

                
                result = new { kq = true };
                
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch
            {
                var result = new { kq = false };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }
        // end check mail
        public ActionResult SendMailSNThang(int? thang)
        {
            #region Role user
            permission = GetPermission("BCSinhNhatNV", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            string qSearch = "";

            var listSendMails = context.sp_BC_NS_SinhNhatNhanVien(thang,null, qSearch).ToList();
            foreach (var item in listSendMails)
            {
                // Code send mail
                MailHelper mailInit = new MailHelper(); // lay cac tham so trong webconfig
                System.Text.StringBuilder content = new System.Text.StringBuilder();


                //Content 
                content.Append("<div style=\"width: 493px; margin: 16px auto; line-height: 1.7; font-size: 16px; box-shadow: -3px -3px  #4b6580; padding: 10px; color: green; border: 3px dotted #0c7932; border-radius: 7px;\">");
                content.Append("<p>Xin chào: <span style=\"font-size: 25px; color: #fb4d0b;\">" + item.hoTen + "</span>");
                content.Append("</p>");
                content.Append("<p style=\"font-style: italic; font-size: 30px; color: #3eaf3e; text-align: center;\">Chúc mừng <span style=\"font-size: 30px; text-transform: uppercase; color: #fb4d0b;\">Sinh nhật </span><span style=\"font-size: 37px; color: #d21839; display:block; clear:both; float:none;\">" + thang + "/" + DateTime.Now.Year + "</span>");
                content.Append("</p>");
                content.Append("<p style=\"font-style: italic;\">Thanks and Regards!</p>");
                content.Append("<p style=\"font-style: italic;\">Email từ hệ thống nhân sự</p>");
                content.Append("</div>");
                //End content
                //Send only email is @thuanviet.com.vn
                string[] array01 = item.email.ToLower().Split('@');
                string string2 = ConfigurationManager.AppSettings["OnlySend"]; //get domain from config files
                string[] array1 = string2.Split(',');
                // bool EmailofThuanViet;
                //EmailofThuanViet = array1.Contains(array01[1]);
                // if (emailNV == "" || emailNV == null || EmailofThuanViet == false)
                // {
                //    return false;
                // }
                MailAddress toMail = new MailAddress(item.email, item.hoTen); // goi den mail
                mailInit.ToMail = toMail;
                mailInit.Body = content.ToString();
                mailInit.SendMail();
                // End code send mail
            }
            var result = new { kq = true };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SendMailSNNgay(int? thang)
        {
            try
            {
                #region Role user
                permission = GetPermission("BCSinhNhatNV", BangPhanQuyen.QuyenXem);
                if (!permission.HasValue)
                    return View("LogIn");
                if (!permission.Value)
                    return View("AccessDenied");
                #endregion
                string qSearch = "";

                var listSendMails = context.sp_BC_NS_SinhNhatNhanVien(thang, DateTime.Now.Day, qSearch).ToList();
                foreach (var item in listSendMails)
                {
                    // Code send mail
                    MailHelper mailInit = new MailHelper(); // lay cac tham so trong webconfig
                    System.Text.StringBuilder content = new System.Text.StringBuilder();


                    //Content 
                    content.Append("<div style=\"width: 493px; margin: 16px auto; line-height: 1.7; font-size: 16px; box-shadow: -3px -3px  #4b6580; padding: 10px; color: green; border: 3px dotted #0c7932; border-radius: 7px;\">");
                    content.Append("<p>Xin chào: <span style=\"font-size: 25px; color: #fb4d0b;\">" + item.hoTen + "</span>");
                    content.Append("</p>");
                    content.Append("<p style=\"font-style: italic; font-size: 30px; color: #3eaf3e; text-align: center;\">Chúc mừng <span style=\"font-size: 30px; text-transform: uppercase; color: #fb4d0b;\">Sinh nhật </span><span style=\"font-size: 37px; color: #d21839; display:block; clear:both; float:none;\">" + DateTime.Now.Day + "/" + thang + "</span>");
                    content.Append("</p>");
                    content.Append("<p style=\"font-style: italic;\">Thanks and Regards!</p>");
                    content.Append("<p style=\"font-style: italic;\">Email từ hệ thống nhân sự</p>");
                    content.Append("</div>");
                    //End content
                    //Send only email is @thuanviet.com.vn
                    string[] array01 = item.email.ToLower().Split('@');
                    string string2 = ConfigurationManager.AppSettings["OnlySend"]; //get domain from config files
                    string[] array1 = string2.Split(',');
                    // bool EmailofThuanViet;
                    //EmailofThuanViet = array1.Contains(array01[1]);
                    // if (emailNV == "" || emailNV == null || EmailofThuanViet == false)
                    // {
                    //    return false;
                    // }
                    MailAddress toMail = new MailAddress(item.email, item.hoTen); // goi den mail
                    mailInit.ToMail = toMail;
                    mailInit.Body = content.ToString();
                    mailInit.SendMail();
                    // End code send mail
                }

                var result = new { kq = true, day = DateTime.Now.Day };
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch{
                var result = new { kq = false, day = DateTime.Now.Day };
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }

    }
}
