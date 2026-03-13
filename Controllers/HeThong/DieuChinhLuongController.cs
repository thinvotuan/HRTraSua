using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BatDongSan.Models.HeThong;
using BatDongSan.Helper.Common;
using BatDongSan.Utils.Paging;
using System.Text;
using BatDongSan.Helper.Utils;
using BatDongSan.Models.NhanSu;
using BatDongSan.Models.DanhMuc;


namespace BatDongSan.Controllers.HeThong
{
    public class DieuChinhLuongController : ApplicationController
    {
        //
        // GET: /DieuChinhLuong/
        private LinqNhanSuDataContext context = new LinqNhanSuDataContext();
        LinqDanhMucDataContext linqDM = new LinqDanhMucDataContext();
        private StringBuilder buildTree;
        private IList<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan> phongBans;
        private readonly string MCV = "DieuChinhLuong";
        private NhanVienModel model;
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
            try
            {
                
                buildTree = new StringBuilder();
                phongBans = context.GetTable<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan>().ToList();
                buildTree = TreePhongBanStyle.BuildTreeDepartment(phongBans);
                model = new NhanVienModel();
                ViewBag.PhongBans = buildTree.ToString();
                var trinhDoChuyenMons = context.tbl_NS_CauHinhTrinhDos.ToList();
                trinhDoChuyenMons.Insert(0, new BatDongSan.Models.NhanSu.tbl_NS_CauHinhTrinhDo { maTrinhDo = "", tenTrinhDo = "--Chọn bằng cấp--" });
                ViewBag.HocHams = new SelectList(trinhDoChuyenMons, "maTrinhDo", "tenTrinhDo");
                var chucDanhs = context.Sys_ChucDanhs.OrderBy(o => o.TenChucDanh).ToList();
                chucDanhs.Insert(0, new BatDongSan.Models.NhanSu.Sys_ChucDanh { MaChucDanh = "", TenChucDanh = "--Chọn chức danh--" });
                ViewBag.ChucDanhs = new SelectList(chucDanhs, "MaChucDanh", "TenChucDanh", model.maChucDanh);
                return View();
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }
        public ActionResult ViewIndex(string searchString, string maPhongBan, string bangCap, string chucDanh, string thamNien, string mucLuong)
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
                var maNhanVien = GetUser().manv;
                var listNV = context.sp_NS_DieuChinhLuongInDex(maPhongBan, bangCap, chucDanh, searchString, maNhanVien).ToList();
                ViewBag.listNV = listNV;
                decimal sumTongDX = listNV.Sum(d => d.luongDCTBP)??0;
                decimal sumTongDL = listNV.Sum(d => d.luongDuyetBTGD) ?? 0;
                decimal sumTLTruocDC = listNV.Sum(d => d.tongLuong) ?? 0;
                decimal sumluongHTMoi = listNV.Sum(d => d.luongHTMoi) ?? 0;
                ViewBag.sumTongDX = sumTongDX;
                ViewBag.sumTongDL = sumTongDL;
                ViewBag.sumTLTruocDC = sumTLTruocDC;
                ViewBag.sumluongHTMoi = sumluongHTMoi;
                return PartialView("PartialIndex");
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }
        public ActionResult IndexBoPhan()
        {
            #region Role user
            permission = GetPermission("DieuChinhLuongBP", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            try
            {

                buildTree = new StringBuilder();
                phongBans = context.GetTable<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan>().ToList();
                buildTree = TreePhongBanStyle.BuildTreeDepartment(phongBans);
                model = new NhanVienModel();
                ViewBag.PhongBans = buildTree.ToString();
                var trinhDoChuyenMons = context.tbl_NS_CauHinhTrinhDos.ToList();
                trinhDoChuyenMons.Insert(0, new BatDongSan.Models.NhanSu.tbl_NS_CauHinhTrinhDo { maTrinhDo = "", tenTrinhDo = "--Chọn bằng cấp--" });
                ViewBag.HocHams = new SelectList(trinhDoChuyenMons, "maTrinhDo", "tenTrinhDo");
                var chucDanhs = context.Sys_ChucDanhs.OrderBy(o => o.TenChucDanh).ToList();
                chucDanhs.Insert(0, new BatDongSan.Models.NhanSu.Sys_ChucDanh { MaChucDanh = "", TenChucDanh = "--Chọn chức danh--" });
                ViewBag.ChucDanhs = new SelectList(chucDanhs, "MaChucDanh", "TenChucDanh", model.maChucDanh);
                return View();
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }
        public ActionResult ViewBoPhan(string searchString, string maPhongBan, string bangCap, string chucDanh, string thamNien, string mucLuong)
        {
            #region Role user
            permission = GetPermission("DieuChinhLuongBP", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            try
            {
                var maNhanVien = GetUser().manv;
                var listNV = context.sp_NS_DieuChinhLuongInDex(maPhongBan, bangCap, chucDanh, searchString, maNhanVien).ToList();
                ViewBag.listNV = listNV;
                decimal sumTongDX = listNV.Sum(d => d.luongDCTBP) ?? 0;
                decimal sumTongDL = listNV.Sum(d => d.luongDuyetBTGD) ?? 0;
                decimal sumTLTruocDC = listNV.Sum(d => d.tongLuong) ?? 0;
                decimal sumluongHTMoi = listNV.Sum(d => d.luongHTMoi) ?? 0;
                ViewBag.sumTongDX = sumTongDX;
                ViewBag.sumTongDL = sumTongDL;
                ViewBag.sumTLTruocDC = sumTLTruocDC;
                ViewBag.sumluongHTMoi = sumluongHTMoi;
                return PartialView("PartialBoPhan");
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }
        public ActionResult CauHinh() { 
        #region Role user
            permission = GetPermission("DieuChinhLuongCH", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            try
            {
                ViewBag.listTrinhDos = context.tbl_NS_CauHinhTrinhDos.ToList();
                ViewBag.listNhomCVs = context.tbl_NS_CauHinhNhomCVs.OrderBy(d=>d.id).ToList();
                return View("CauHinh");
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public int SaveQuanLyTrinhDo(FormCollection form)
        {

            #region Role user
            permission = GetPermission("DieuChinhLuongCH", BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return 0;
            if (!permission.Value)
                return 0;
            #endregion
            try
            {

               
                var deleteTD = context.tbl_NS_CauHinhTrinhDos.ToList();
                if (deleteTD != null)
                {

                    context.tbl_NS_CauHinhTrinhDos.DeleteAllOnSubmit(deleteTD);
                    
                }
                // Create New;
                tbl_NS_CauHinhTrinhDo tblCauHinhTrinhDo = null;
                List<tbl_NS_CauHinhTrinhDo> lst_tblCauHinhTrinhDo = new List<tbl_NS_CauHinhTrinhDo>();

                string[] thuHang = form.GetValues("tiLe");


                if (thuHang != null)
                {
                    for (int i = 0; i < thuHang.Length; i++)
                    {

                        tblCauHinhTrinhDo = new tbl_NS_CauHinhTrinhDo();
                        tblCauHinhTrinhDo.maTrinhDo = Convert.ToString(form.GetValues("maTrinhDo")[i])??String.Empty;
                        tblCauHinhTrinhDo.tenTrinhDo = Convert.ToString(form.GetValues("tenTrinhDo")[i])??String.Empty;
                        tblCauHinhTrinhDo.tiLe = String.IsNullOrEmpty(form.GetValues("tiLe")[i])? 0 : Convert.ToDouble(form.GetValues("tiLe")[i]);
                        tblCauHinhTrinhDo.khoiDiem = String.IsNullOrEmpty(form.GetValues("khoiDiem")[i]) ? 0 : Convert.ToDecimal(form.GetValues("khoiDiem")[i]);
                        tblCauHinhTrinhDo.nguoiLap = GetUser().manv;
                        tblCauHinhTrinhDo.ngayLap = DateTime.Now;
                        lst_tblCauHinhTrinhDo.Add(tblCauHinhTrinhDo);

                    }
                    context.tbl_NS_CauHinhTrinhDos.InsertAllOnSubmit(lst_tblCauHinhTrinhDo);
                }

                context.SubmitChanges();
                return 1;


            }
            catch
            {
                return 0;
            }
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public int SaveNhomCV(FormCollection form)
        {

            #region Role user
            permission = GetPermission("DieuChinhLuongCH", BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return 0;
            if (!permission.Value)
                return 0;
            #endregion
            try
            {


                var deleteTD = context.tbl_NS_CauHinhNhomCVs.ToList();
                if (deleteTD != null)
                {

                    context.tbl_NS_CauHinhNhomCVs.DeleteAllOnSubmit(deleteTD);

                }
                // Create New;
                tbl_NS_CauHinhNhomCV tblCauHinhNhomCV = null;
                List<tbl_NS_CauHinhNhomCV> lst_tblCauHinhNhomCV = new List<tbl_NS_CauHinhNhomCV>();

                string[] thuHang = form.GetValues("tenNCV");


                if (thuHang != null)
                {
                    for (int i = 0; i < thuHang.Length; i++)
                    {

                        tblCauHinhNhomCV = new tbl_NS_CauHinhNhomCV();
                        tblCauHinhNhomCV.maNCV = Convert.ToString(form.GetValues("maNCV")[i])?? String.Empty;
                        tblCauHinhNhomCV.tenNCV = Convert.ToString(form.GetValues("tenNCV")[i]) ?? String.Empty;

                        tblCauHinhNhomCV.phuCap = String.IsNullOrEmpty(form.GetValues("phuCap")[i]) ? 0 : Convert.ToDecimal(form.GetValues("phuCap")[i]);
                        tblCauHinhNhomCV.nguoiLap = GetUser().manv;
                        tblCauHinhNhomCV.ngayLap = DateTime.Now;
                        lst_tblCauHinhNhomCV.Add(tblCauHinhNhomCV);

                    }
                    context.tbl_NS_CauHinhNhomCVs.InsertAllOnSubmit(lst_tblCauHinhNhomCV);
                }

                context.SubmitChanges();
                return 1;


            }
            catch
            {
                return 0;
            }
        }
        public int UpdateDieuChinh(string maNhanVien, double luongKN, double LuongTH, double LuongDuyetBTGD)
        {
            #region Role user
            permission = GetPermission("DieuChinhLuong", BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return 1;
            if (!permission.Value)
                return 1;
            #endregion

            try
            {
                tbl_NS_DieuChinhLuong KetQuaDCL = new tbl_NS_DieuChinhLuong();
                KetQuaDCL = context.tbl_NS_DieuChinhLuongs.Where(d => d.maNhanVien == maNhanVien).FirstOrDefault();
                if (KetQuaDCL != null)
                    {
                        KetQuaDCL.luongKiemNhiem = Convert.ToDecimal(luongKN);
                        KetQuaDCL.luongThuHut = Convert.ToDecimal(LuongTH);
                        KetQuaDCL.luongDuyetBTGD = Convert.ToDecimal(LuongDuyetBTGD);
                        KetQuaDCL.nguoiCapNhat = GetUser().manv;
                        KetQuaDCL.ngayCapNhat = DateTime.Now;
                        context.SubmitChanges();
                    }


                    SaveActiveHistory("Ban TGD: điều chỉnh lương, Mã NV: " + maNhanVien);

                    return 1;
               


            }
            catch (Exception e)
            {
                return 0;
            }
        }
        public int UpdateDieuChinhBoPhan(string maNhanVien, double luongKN, double LuongTH, double deXuatLuong)
        {
            #region Role user
            permission = GetPermission("DieuChinhLuongBP", BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return 1;
            if (!permission.Value)
                return 1;
            #endregion

            try
            {
                tbl_NS_DieuChinhLuong KetQuaDCL = new tbl_NS_DieuChinhLuong();
                KetQuaDCL = context.tbl_NS_DieuChinhLuongs.Where(d => d.maNhanVien == maNhanVien).FirstOrDefault();
                if (KetQuaDCL != null)
                {
                    KetQuaDCL.luongKiemNhiem = Convert.ToDecimal(luongKN);
                    KetQuaDCL.luongThuHut = Convert.ToDecimal(LuongTH);
                    KetQuaDCL.luongDCTBP = Convert.ToDecimal(deXuatLuong);
                    KetQuaDCL.luongDuyetBTGD = Convert.ToDecimal(deXuatLuong);
                    KetQuaDCL.nguoiCapNhat = GetUser().manv;
                    KetQuaDCL.ngayCapNhat = DateTime.Now;
                    context.SubmitChanges();
                }


                SaveActiveHistory("Bộ phận: điều chỉnh lương, Mã NV: " + maNhanVien);

                return 1;



            }
            catch (Exception e)
            {
                return 0;
            }
        }
        public int UpdateYKien(string maNhanVien, string yKien)
        {
            #region Role user
            permission = GetPermission("DieuChinhLuong", BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return 1;
            if (!permission.Value)
                return 1;
            #endregion

            try
            {
                tbl_NS_DieuChinhLuong KetQuaDCL = new tbl_NS_DieuChinhLuong();
                KetQuaDCL = context.tbl_NS_DieuChinhLuongs.Where(d => d.maNhanVien == maNhanVien).FirstOrDefault();
                if (KetQuaDCL != null)
                {
                    KetQuaDCL.nhanXet = yKien;
                    context.SubmitChanges();
                }


                SaveActiveHistory("Cập nhật đề xuất ý kiến đến NV: " + maNhanVien);

                return 1;



            }
            catch (Exception e)
            {
                return 0;
            }
        }
    }
}
