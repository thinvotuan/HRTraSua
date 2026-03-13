using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BatDongSan.Models.DanhMuc;
using BatDongSan.Controllers;
using BatDongSan.Utils.Paging;
using BatDongSan.Helper.Common;
using BatDongSan.Models.HeThong;
using BatDongSan.Models.NhanSu;
using System.Text;
using BatDongSan.Helper.Utils;
using BatDongSan.Models.DBChamCong;

namespace BatDongSan.Controllers.DanhMuc
{
    public class PhongBanController : ApplicationController
    {
        private LinqDanhMucDataContext linqDanhMuc = new LinqDanhMucDataContext();

        private LinqHeThongDataContext context = new LinqHeThongDataContext();
        private LinqNhanSuDataContext linqNS = new LinqNhanSuDataContext();
        private IList<tbl_DM_PhongBan> phongBans;
        private tbl_DM_PhongBan phongBan;
        private bool? permission;
        public const string taskIDSystem = "PhongBan";
        private StringBuilder buildTree = null;

        public ActionResult Index()
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return RedirectToAction("AccessDenied");
            #endregion


            try
            {
                buildTree = new StringBuilder();
                phongBans = linqDanhMuc.tbl_DM_PhongBans.ToList();
                buildTree = TreePhongBans.BuildTreeDepartment(phongBans);
                ViewBag.departments = buildTree.ToString();

                ViewBag.shiftType = linqDanhMuc.tbl_NS_PhanCas.Where(t => t.tenPhanCa != "").ToList();
                ViewBag.listNhomCV = linqNS.tbl_NS_CauHinhNhomCVs.ToList();
                ViewBag.page = 0;
                ViewBag.total = 0;
                string flag = AdminNhanSu(GetUser().manv);
                ViewBag.flagAdmin = flag;
                return View(phongBans);
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("Error");
            }
        }
        public int ImportHRToERP() {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return 0;
            if (!permission.Value)
                return 0;
            #endregion
            LinqHeThongDataContext contextHT = new LinqHeThongDataContext();
            //contextHT.sp_ImportDateNhanSuToERP();
            //SaveActiveHistory("Import dữ liệu nhân sự đến ERP");
            return 1;
        
        }
        public ActionResult LoadNhanVien(int? id, string qSearch, int _page, string parrentId)
        {
            try
            {
                ViewData["parrentId"] = parrentId;
                string parentID = linqDanhMuc.tbl_DM_PhongBans.Where(d => d.maPhongBan == parrentId).Select(d => d.maCha).FirstOrDefault() ?? string.Empty;
                if (String.IsNullOrEmpty(parentID))
                {
                    parrentId = string.Empty;
                }
                //int page = _page == 0 ? 1 : _page;
                //int pIndex = page;
                //int total = IsNullOrEmpty.sp_PB_DanhSachNhanVien(qSearch, parrentId).Count();
                //PagingLoaderController("/Department.mvc/Index/", total, page, "?qsearch=" + qSearch + "&parrentId=" + parrentId);
                //ViewData["nhanVien"] = hr.sp_PB_DanhSachNhanVien(qSearch, parrentId).Skip(start).Take(offset).ToList();

                ViewData["qSearch"] = qSearch ?? string.Empty;
                return PartialView("LoadNhanVien");

            }
            catch (Exception e)
            {
                ViewData["Message"] = e.Message;
                return View("Error");
            }
        }

        public JsonResult GetDepartmentByID(string departmentID)
        {
            try
            {
                phongBan = new tbl_DM_PhongBan();
                var record = linqDanhMuc.tbl_DM_PhongBans.Where(d => d.maPhongBan == departmentID).FirstOrDefault();

                phongBan.maPhongBan = record.maPhongBan;
                phongBan.tenPhongBan = record.tenPhongBan;
                phongBan.maCha = record.maCha != null ? record.maCha : String.Empty;
                phongBan.ghiChu = record.ghiChu;
                phongBan.maPhanCa = record.maPhanCa;
                phongBan.maNhanVienDuyet = record.maNhanVienDuyet;

                PhongBanModel Data = new PhongBanModel();
                Data.MaPhongBan = phongBan.maPhongBan;
                Data.Ten = phongBan.tenPhongBan;
                Data.maCha = phongBan.maCha != null ? record.maCha : String.Empty;
                Data.GhiChu = phongBan.ghiChu;
                Data.maPhanCa = phongBan.maPhanCa;
                Data.maNhomCV = record.maNhomCV;
                Data.maNhanVienDuyet = record.maNhanVienDuyet;
                Data.tenNhanVienDuyet = HoVaTen(record.maNhanVienDuyet);
                return Json(Data);
            }
            catch (Exception ex)
            {
                return Json(null);
            }
        }
        public string HoVaTen(string MaNV)
        {

            return linqNS.tbl_NS_NhanViens.Where(d => d.maNhanVien == MaNV).Select(d => d.ho + " " + d.ten).FirstOrDefault();
        }

        public JsonResult GetDepartmentByIDAnHien(string departmentID)
        {
            string hasValue = string.Empty;
            try
            {

                var check = linqDanhMuc.tbl_DM_PhongBans.Where(d => d.maCha != null && d.maCha != string.Empty && d.maPhongBan == departmentID).FirstOrDefault();
                if (check != null)
                {
                    hasValue = "1";
                }
                else
                {
                    hasValue = "0";
                }
                return Json(hasValue);
            }
            catch (Exception ex)
            {
                return Json(hasValue);
            }
        }
        public JsonResult LuuPhongBan(string departmentID, string departmentName, string parentId, string note, string shiftId, bool flag, string NhomCVId, string maNguoiDuyet)
        {

            try
            {
                phongBan = new tbl_DM_PhongBan();
                phongBan.maPhongBan = departmentID;
                phongBan.maCha = parentId == "" ? null : parentId;
                phongBan.tenPhongBan = departmentName;
                phongBan.nguoiLap = GetUser().userName;
                phongBan.ngayLap = DateTime.Now;
                phongBan.ghiChu = note;
                phongBan.maNhanVienDuyet = maNguoiDuyet;
                
                try
                {
                    phongBan.maPhanCa = (int)Convert.ToInt64(shiftId);
                    phongBan.maNhomCV = NhomCVId;
                }
                catch { }
                int data = 0;
                if (flag == true)
                {
                    if (CheckExistDepartment(departmentID))
                    {
                        data = 0;
                    }
                    else if (CheckExistDepartmentName(departmentName))
                    {
                        data = 1;
                    }
                    else
                    {
                        Insert(phongBan);
                        data = 2;
                    }
                }
                else
                {
                    phongBan.ngayCapNhat = DateTime.Now;
                    Update(phongBan);
                    data = 2;
                }

                return Json(data);
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return Json(null);
            }
        }

        public void Insert(tbl_DM_PhongBan phongBan)
        {
            linqDanhMuc.tbl_DM_PhongBans.InsertOnSubmit(phongBan);
            linqDanhMuc.SubmitChanges();
        }

        public void Update(tbl_DM_PhongBan department)
        {
            var departmentLinq = (from t in linqDanhMuc.tbl_DM_PhongBans
                                 where t.maPhongBan.Equals(department.maPhongBan)
                                 select t).FirstOrDefault();
           // tbl_DM_PhongBan phongban = departmentLinq.SingleOrDefault();
            departmentLinq.tenPhongBan = department.tenPhongBan;
            departmentLinq.nguoiLap = department.nguoiLap;
            departmentLinq.ngayCapNhat = DateTime.Now;
            departmentLinq.ghiChu = department.ghiChu;
            departmentLinq.maPhanCa = department.maPhanCa;
            departmentLinq.maNhomCV = department.maNhomCV;
            departmentLinq.maCha = department.maCha;
            departmentLinq.maNhanVienDuyet = department.maNhanVienDuyet;
            linqDanhMuc.SubmitChanges();
        }

        public bool CheckExistDepartment(string maPhongBan)
        {
            return linqDanhMuc.tbl_DM_PhongBans.Where(d => d.maPhongBan == maPhongBan).Count() > 0 ? true : false;
        }

        public bool CheckExistDepartmentName(string tenPhong)
        {
            return linqDanhMuc.tbl_DM_PhongBans.Where(d => d.tenPhongBan == tenPhong).Count() > 0 ? true : false;
        }

        public ActionResult CapNhatPhongBanCha(int? page, string searchString)
        {
            ViewBag.isGet = "True";
            IList<sp_DM_DanhSachBoPhanResult> phongBans;
            phongBans = linqDanhMuc.sp_DM_DanhSachBoPhan(searchString).ToList();
            int currentPageIndex = page.HasValue ? page.Value : 1;
            ViewBag.Count = phongBans.Count();
            return PartialView("_DanhSachPhongBan", phongBans.ToPagedList(currentPageIndex, 20));
        }

        public ActionResult XoaPhongBan(string departmentID)
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenXoa);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            try
            {
                RemovePhongBan(departmentID);
            }
            catch (Exception e)
            {
                
                ViewData["Message"] = e.Message;
                return View("Error");
            }
            return Json(String.Empty);
        }

        public void RemovePhongBan(string departmentID)
        {            
            DeletePhongBanCon(departmentID);
            var departmentLinq = from t in linqDanhMuc.tbl_DM_PhongBans
                                 where t.maPhongBan.Equals(departmentID)
                                 select t;
            linqDanhMuc.tbl_DM_PhongBans.DeleteOnSubmit(departmentLinq.SingleOrDefault());
            linqDanhMuc.SubmitChanges();
        }

        private void DeletePhongBanCon(string departmentID)
        {
            var childNode = from t in linqDanhMuc.tbl_DM_PhongBans
                            where t.maCha.Equals(departmentID)
                            select t.maPhongBan;
            if (childNode.Count() > 0)
            {
                foreach (var item in childNode)
                {
                    var sqlDeleteChild = linqDanhMuc.tbl_DM_PhongBans
                        .Where(d => d.maPhongBan.Equals(item));
                    if (sqlDeleteChild.Count() > 0)
                    {
                        linqDanhMuc.tbl_DM_PhongBans.DeleteAllOnSubmit(sqlDeleteChild);
                        //Recursive
                        DeletePhongBanCon(item);
                    }
                }
            }
        }
        public ActionResult ViewMachinePB(string deparmentID)
        {
             try
            {
            DBChamCongDataContext linqChamCong = new DBChamCongDataContext();

            var list = linqChamCong.Sp_ViewMachinePB(null,null).ToList();
            ViewBag.List = list;
            ViewBag.DeparmentID = deparmentID;
            return View();
            }
             catch (Exception e)
             {
                 ViewData["Message"] = e.Message;
                 return View("Error");
             }
        }
       
        public ActionResult LoadPhongBanMayChamCong(int _page, string parrentId)
        {
            try
            {
                int page = _page == 0 ? 1 : _page;
                int pIndex = page;
                int total = linqDanhMuc.sp_PB_MayChamCong(parrentId).Count();
                PagingLoaderController("/PhongBan/Index/", total, page, "?parrentId=" + parrentId);
                ViewData["chamCong"] = linqDanhMuc.sp_PB_MayChamCong(parrentId).Skip(start).Take(offset).ToList();


                return PartialView("LoadPhongBanMayChamCong");
            }
            catch (Exception e)
            {
                ViewData["Message"] = e.Message;
                return View("Error");
            }


        }
        public string UpdateMaMayPB(string MaMay, string MaPB)
        {

            try
            {
                //Check xem co trong csdl chua, chua co moi insert
                string checkMaMay = linqDanhMuc.tbl_PhongBan_MayChamCongs.Where(d => d.maMayChamCong == MaMay && d.maPhongBan == MaPB).Select(d => d.maPhongBan).FirstOrDefault() ?? string.Empty;
                if (String.IsNullOrEmpty(checkMaMay))
                {
                    //Get thong tin may cham cong
                    DBChamCongDataContext linqChamCongServer = new DBChamCongDataContext();
                    var list = (from sy in linqChamCongServer.Sys_Machines
                                join si in linqChamCongServer.Sys_SiteConections on sy.siteId equals si.siteId
                                where sy.machineId == MaMay
                                select new
                                {
                                    machineId = sy.machineId,
                                    MachineName = sy.MachineName,
                                    remark = sy.remark,
                                    tenCongTruongVp = si.siteName
                                }).FirstOrDefault();
                    //End
                    tbl_PhongBan_MayChamCong tblPhongBanMayCC = new tbl_PhongBan_MayChamCong();
                    tblPhongBanMayCC.maPhongBan = MaPB;
                    tblPhongBanMayCC.maMayChamCong = MaMay;
                    tblPhongBanMayCC.ghiChu = list.remark;
                    tblPhongBanMayCC.tenMay = list.MachineName;
                    tblPhongBanMayCC.nguoiThem = GetUser().userName;
                    tblPhongBanMayCC.tenCongTruongVp = list.tenCongTruongVp;
                    tblPhongBanMayCC.ngayThem = DateTime.Now;

                    linqDanhMuc.tbl_PhongBan_MayChamCongs.InsertOnSubmit(tblPhongBanMayCC);
                    linqDanhMuc.SubmitChanges();
                    return "true";
                }
                else
                {
                    return "Đã được thêm";
                }


            }
            catch (Exception e)
            {
                return "Lỗi: " + e.Message;
            }
        }
        public string RemoveMaMayPhongBan(string MaMay, string MaPB)
        {

            try
            {
                var list = linqDanhMuc.tbl_PhongBan_MayChamCongs.Where(d => d.maPhongBan == MaPB && d.maMayChamCong == MaMay);
                linqDanhMuc.tbl_PhongBan_MayChamCongs.DeleteAllOnSubmit(list);


                linqDanhMuc.SubmitChanges();

                return "true";


            }
            catch (Exception e)
            {
                return "Lỗi: " + e.Message;
            }
        }
        public ActionResult NhanVienDuyet()
        {
            buildTree = new StringBuilder();
            phongBans = linqDanhMuc.tbl_DM_PhongBans.ToList();
            buildTree = TreePhongBans.BuildTreeDepartment(phongBans);
            ViewBag.NVPB = buildTree.ToString();
            return PartialView("_NhanVienPhongBan");
        }
        public ActionResult DanhSachPhongBan()
        {
            buildTree = new StringBuilder();
            phongBans = linqDanhMuc.tbl_DM_PhongBans.ToList();
            buildTree = TreePhongBans.BuildTreeDepartment(phongBans);
            ViewBag.NVPB = buildTree.ToString();
            return PartialView("_PartDanhSachPhongBan");
        }

        public ActionResult LoadNhanVienDuyet(int? page, string searchString, string maPhongBan)
        {
            IList<sp_PB_DanhSachNhanVienResult> phongBan1s;
            phongBan1s = linqDanhMuc.sp_PB_DanhSachNhanVien(searchString, maPhongBan).ToList();
            ViewBag.isGet = "True";
            int currentPageIndex = page.HasValue ? page.Value : 1;
            ViewBag.Count = phongBan1s.Count();
            ViewBag.Search = searchString;
            ViewBag.MaPhongBan = maPhongBan;
            return PartialView("LoadNhanVienDuyet", phongBan1s.ToPagedList(currentPageIndex, 10));
        }

        public ActionResult LoadNhanVienPhongBan(int? page, string searchString, string maPhongBan)
        {
            try
            {
                string parentID = linqDanhMuc.tbl_DM_PhongBans.Where(d => d.maPhongBan == maPhongBan).Select(d => d.maCha).FirstOrDefault() ?? string.Empty;
                if (String.IsNullOrEmpty(parentID))
                {
                    maPhongBan = string.Empty;
                }
                IList<sp_PB_DanhSachNhanVienResult> phongBan1s;
                phongBan1s = linqDanhMuc.sp_PB_DanhSachNhanVien(searchString, maPhongBan).ToList();
                ViewBag.isGet = "True";
                int currentPageIndex = page.HasValue ? page.Value : 1;
                ViewBag.Count = phongBan1s.Count();
                ViewBag.parrentId = maPhongBan;
                ViewBag.qSearchNV = searchString ?? string.Empty;
                return PartialView("LoadNhanVien",phongBan1s.ToPagedList(currentPageIndex, 10));
            }
            catch (Exception e)
            {
                ViewData["Message"] = e.Message;
                return View("Error");
            }
        }
        public ActionResult SoDoToChuc()
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return RedirectToAction("AccessDenied");
            #endregion


            try
            {
                buildTree = new StringBuilder();
                phongBans = linqDanhMuc.tbl_DM_PhongBans.ToList();
                buildTree = TreePhongBans.BuildTreeDepartment(phongBans);
                ViewBag.departments = buildTree.ToString();

                ViewBag.shiftType = linqDanhMuc.tbl_NS_PhanCas.Where(t => t.tenPhanCa != "").ToList();
                ViewBag.page = 0;
                ViewBag.total = 0;
                return View(phongBans);
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("Error");
            }
        }
        public ActionResult LoadTreeNhanVien(string parrentId, string qSearch, int? soCapBac, int pageIndex, int pageSize)
        {


            try
            {

                var list = context.sp_PB_DanhSachNhanVien_Tree(qSearch, parrentId, soCapBac, pageIndex, pageSize).ToList();
                int totalList = context.sp_PB_DanhSachNhanVien_Tree(qSearch, parrentId, soCapBac, 0, 10000).Count();


                if (list.Count == 0)
                {
                    list = null;
                }
                
                return Json(new { count = totalList, lists = list });
               
            }
            catch
            {
                return Json(string.Empty);
            }

        }

        //public ActionResult LoadNhanVienLienKetView(int? page, string parrentId)
        //{
        //    try
        //    {
                
        //        string parentID = linqDanhMuc.tbl_DM_PhongBans.Where(d => d.maPhongBan == parrentId).Select(d => d.maCha).FirstOrDefault() ?? string.Empty;
        //        if (String.IsNullOrEmpty(parentID))
        //        {
        //            parrentId = string.Empty;
        //        }
        //        IList<sp_PB_DanhSachNhanVienLienKetResult> phongBan1s;
        //        phongBan1s = linqDanhMuc.sp_PB_DanhSachNhanVienLienKet(parrentId).ToList();
        //        ViewBag.isGet = "True";
        //        int currentPageIndex = page.HasValue ? page.Value : 1;
        //        ViewBag.Count = phongBan1s.Count();
        //        ViewBag.parrentId = parrentId;
        //        return PartialView("LoadNhanVienLienKetView",phongBan1s.ToPagedList(currentPageIndex, 10));
        //    }
        //    catch (Exception e)
        //    {
        //        ViewData["Message"] = e.Message;
        //        return View("Error");
        //    }


        //}
        public string removeNVLienKetPhongBan(string MaNV, string MaPB)
        {
            try
            {
                var list = linqNS.tbl_NS_NhanVienLienKetPhongBans.Where(d => d.employeeId == MaNV && d.departmentId == MaPB);
                linqNS.tbl_NS_NhanVienLienKetPhongBans.DeleteAllOnSubmit(list);
                linqNS.SubmitChanges();
                return "true";
            }
            catch (Exception e)
            {
                return "Lỗi: " + e.Message;
            }
        }
        public ActionResult ChonNhanVienLienKet()
        {
            buildTree = new StringBuilder();
            phongBans = linqDanhMuc.tbl_DM_PhongBans.ToList();
            buildTree = TreePhongBans.BuildTreeDepartment(phongBans);
            ViewBag.NVPB = buildTree.ToString();
            return PartialView("_NhanVienLienKet");
        }
        public ActionResult LoadNhanVienLienKet(int? page, string searchString, string maPhongBan)
        {
            IList<sp_PB_DanhSachNhanVienResult> phongBan1s;
            phongBan1s = linqDanhMuc.sp_PB_DanhSachNhanVien(searchString, maPhongBan).ToList();
            ViewBag.isGet = "True";
            int currentPageIndex = page.HasValue ? page.Value : 1;
            ViewBag.Count = phongBan1s.Count();
            ViewBag.Search = searchString;
            ViewBag.MaPhongBan = maPhongBan;
            return PartialView("LoadNhanVienLienKet", phongBan1s.ToPagedList(currentPageIndex, 10));
        }
        public string updateListNVLienKetPhongBan(string MaNV, string MaPB)
        {

            try
            {
                // Update
                string[] MaNVs = MaNV.Split(',');
                List<tbl_NS_NhanVienLienKetPhongBan> lst_HR_EmployeeDepartment = new List<tbl_NS_NhanVienLienKetPhongBan>();
                tbl_NS_NhanVienLienKetPhongBan tblEmployeeDepart = null;
                if (MaNV != null)
                {
                    for (int i = 0; i < MaNVs.Length; i++)
                    {
                        if (linqNS.tbl_NS_NhanVienLienKetPhongBans.Where(d => d.employeeId.Contains(MaNVs[i])).FirstOrDefault() == null)
                        {
                            tblEmployeeDepart = new tbl_NS_NhanVienLienKetPhongBan();
                            tblEmployeeDepart.departmentId = MaPB;
                            tblEmployeeDepart.employeeId = MaNVs[i];
                            tblEmployeeDepart.userName = GetUser().userName;
                            tblEmployeeDepart.updateDate = DateTime.Now;
                            lst_HR_EmployeeDepartment.Add(tblEmployeeDepart);
                        }

                    }
                    linqNS.tbl_NS_NhanVienLienKetPhongBans.InsertAllOnSubmit(lst_HR_EmployeeDepartment);
                }

                linqNS.SubmitChanges();
                return "true";


            }
            catch (Exception e)
            {
                return "Lỗi: " + e.Message;
            }
        }

        public ActionResult chuyenNV(string arrMaNV)
        {
            buildTree = new StringBuilder();
            phongBans = linqDanhMuc.tbl_DM_PhongBans.ToList();
            buildTree = TreePhongBans.BuildTreeDepartment(phongBans);
            ViewBag.PhongBanChuyen = buildTree.ToString();
            ViewBag.NhanVienChuyen = arrMaNV;
            return PartialView("_PhongBanChuyen");           
        }
        public string updateListNVPhongBan(string MaNV, string MaPB)
        {

            try
            {
                // Update
                string[] MaNVs = MaNV.Split(',');
                List<tbl_NS_NhanVienPhongBan> lst_HR_EmployeeDepartment = new List<tbl_NS_NhanVienPhongBan>();
                tbl_NS_NhanVienPhongBan tblEmployeeDepart = null;
                if (MaNV != null)
                {
                    for (int i = 0; i < MaNVs.Length; i++)
                    {

                        tblEmployeeDepart = new tbl_NS_NhanVienPhongBan();
                        tblEmployeeDepart.maPhongBan = MaPB;
                        tblEmployeeDepart.maNhanVien = MaNVs[i];
                        tblEmployeeDepart.nguoiLap = GetUser().userName;
                        tblEmployeeDepart.ngayLap = DateTime.Now;
                        lst_HR_EmployeeDepartment.Add(tblEmployeeDepart);

                    }
                    linqNS.tbl_NS_NhanVienPhongBans.InsertAllOnSubmit(lst_HR_EmployeeDepartment);
                }

                linqNS.SubmitChanges();
                return "true";


            }
            catch (Exception e)
            {
                return "Lỗi: " + e.Message;
            }
        }

        public ActionResult ListDSNhanVien(int? page, string qSearch, string parrentId)
        {
            try
            {
                
                var ParentGoc = linqDanhMuc.tbl_DM_PhongBans.Where(d => d.maCha == null).FirstOrDefault();
                var record = linqDanhMuc.sp_PB_DanhSachNhanVien(qSearch, string.Empty).Where(d=>d.departmentId!=parrentId).ToList();
                ViewBag.isGet = "True";
                int currentPageIndex = page.HasValue ? page.Value : 1;
                ViewBag.Count = record.Count();
                ViewBag.qSearchNVAdd = qSearch;
                ViewBag.parrentIdAdd = parrentId;
                return PartialView("_ListDSNhanVien", record.ToPagedList(currentPageIndex, 20));     
            }
            catch (Exception e)
            {
                ViewData["Message"] = e.Message;
                return View("Error");
            }
        }

        public string updateNVPhongBan(string MaNV, string MaPB)
        {
            try
            {
                tbl_NS_NhanVienPhongBan tblEmployeeDepart = new tbl_NS_NhanVienPhongBan();
                tblEmployeeDepart.maPhongBan = MaPB;
                tblEmployeeDepart.maNhanVien = MaNV;
                tblEmployeeDepart.nguoiLap = GetUser().userName;
                tblEmployeeDepart.ngayLap = DateTime.Now;

                linqNS.tbl_NS_NhanVienPhongBans.InsertOnSubmit(tblEmployeeDepart);
                linqNS.SubmitChanges();
                return "true";
            }
            catch (Exception e)
            {
                return "Lỗi: " + e.Message;
            }
        }
        // Load nhan vien lien ket.
        public ActionResult LoadNhanVienLienKetView(string parrentId)
        {
            try
            {
                string parentID = linqDanhMuc.tbl_DM_PhongBans.Where(d => d.maPhongBan == parrentId).Select(d => d.maCha).FirstOrDefault() ?? string.Empty;
                if (String.IsNullOrEmpty(parentID))
                {
                    parrentId = string.Empty;
                }
                int total = linqDanhMuc.sp_PB_DanhSachNhanVienLienKet(parrentId).Count();
               
                ViewData["nhanVien"] = linqDanhMuc.sp_PB_DanhSachNhanVienLienKet(parrentId).ToList();
                ViewData["total"] = total;

                return PartialView("LoadNhanVienLienKetView");
            }
            catch (Exception e)
            {
                ViewData["Message"] = e.Message;
                return View("Error");
            }


        }
        // End
    }
}
