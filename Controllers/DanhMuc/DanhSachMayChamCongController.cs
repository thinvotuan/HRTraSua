using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BatDongSan.Models.DanhMuc;
using BatDongSan.Controllers;
using BatDongSan.Helper.Common;
using BatDongSan.Models.HeThong;
using BatDongSan.Helper.Utils;
using BatDongSan.Models.DBChamCong;

namespace BatDongSan.Controllers.DanhMuc
{

    public class DanhSachMayChamCongController : ApplicationController
    {
        DBChamCongDataContext linqChamCong = new DBChamCongDataContext();

        private Sys_Machine SysMachine;
        private Sys_SiteConection SysSiteConection;
        private bool? permission;
        public const string taskIDSystem = "DanhSachMayChamCong";
        //
        // GET: /KhoiTinhLuong/


        public ActionResult Index()
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion


            var list = linqChamCong.Sp_ViewMachinePB(null,null).ToList();
            ViewBag.List = list;
            var listSite = linqChamCong.Sys_SiteConections.ToList();
            ViewBag.ListSite = listSite;
            return View();
        }
        public ActionResult ViewIndex()
        {
            var list = linqChamCong.Sp_ViewMachinePB(null,null).ToList();
            ViewBag.List = list;
            return PartialView("_ViewIndex");
        }
        public ActionResult ViewIndexSite()
        {
            var list = linqChamCong.Sys_SiteConections.ToList();
            ViewBag.ListSite = list;
            return PartialView("_ViewIndexSite");
        }
        public ActionResult CreateMachine()
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenThem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            ViewBag.TitleHead = "Thêm";
            var listSites = linqChamCong.Sys_SiteConections.ToList();
            listSites.Insert(0, new BatDongSan.Models.DBChamCong.Sys_SiteConection { siteId = "", siteName = "--Chọn--" });
            ViewBag.ListSites = new SelectList(listSites, "siteId", "siteName");
            SysMachine = new Sys_Machine();

            return PartialView("_CreateEditMachine", SysMachine);
        }

        [HttpPost]
        public ActionResult CreateMachine(FormCollection coll)
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenThem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            try
            {
                SysMachine = new Sys_Machine();
                SysMachine.siteId = coll.Get("siteId");
                SysMachine.machineId = coll.Get("machineId");
                SysMachine.MachineName = coll.Get("MachineName");
                SysMachine.remark = coll.Get("remark");
                SysMachine.typeMachine = String.IsNullOrEmpty(coll.Get("typeMachine")) ? 0: Convert.ToInt32(coll.Get("typeMachine"));
                SysMachine.nguoiLap = GetUser().userName;
                SysMachine.ngayLap = DateTime.Now;
                linqChamCong.Sys_Machines.InsertOnSubmit(SysMachine);
                linqChamCong.SubmitChanges();
                var list = linqChamCong.Sp_ViewMachinePB(null,null).ToList();
                ViewBag.List = list;
                return PartialView("_ViewIndex");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult CreateSite()
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenThem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            ViewBag.TitleHead = "Thêm";
            SysSiteConection = new Sys_SiteConection();

            return PartialView("_CreateEditSite", SysSiteConection);
        }

        [HttpPost]
        public ActionResult CreateSite(FormCollection coll)
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenThem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            try
            {
                SysSiteConection = new Sys_SiteConection();
                SysSiteConection.siteId = coll.Get("siteId");
                SysSiteConection.siteName = coll.Get("siteName");
                SysSiteConection.siteAddress = coll.Get("siteAddress");
                SysSiteConection.siteIP = coll.Get("siteIP");
                SysSiteConection.port = coll.Get("port");
                SysSiteConection.databaseName = coll.Get("databaseName");
                SysSiteConection.loginName = coll.Get("loginName");
                SysSiteConection.pdw = coll.Get("pdw");
                SysSiteConection.remark = coll.Get("remark");
                SysSiteConection.nguoiLap = GetUser().userName;
                SysSiteConection.ngayLap = DateTime.Now;
                linqChamCong.Sys_SiteConections.InsertOnSubmit(SysSiteConection);
                linqChamCong.SubmitChanges();
                var list = linqChamCong.Sp_ViewMachinePB(null,null).ToList();
                ViewBag.List = list;
                var listSite = linqChamCong.Sys_SiteConections.ToList();
                ViewBag.ListSite = listSite;
                return PartialView("_ViewIndex");
            }
            catch
            {
                return View();
            }
        }
        //public string GetMax()
        //{
        //    return context.tbl_NS_KhoiTinhLuongs.OrderByDescending(d => d.ngayCapNhat).Select(d => d.maKhoiTinhLuong).FirstOrDefault();
        //}

        public ActionResult EditMachine(string id)
        {
           
            
           
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            ViewBag.TitleHead = "Cập nhật";
            var listSites = linqChamCong.Sys_SiteConections.ToList();
            listSites.Insert(0, new BatDongSan.Models.DBChamCong.Sys_SiteConection { siteId = "", siteName = "--Chọn--" });
            ViewBag.ListSites = new SelectList(listSites, "siteId", "siteName");
            SysMachine = new Sys_Machine();
            SysMachine = linqChamCong.Sys_Machines.Where(d => d.machineId.Equals(id)).FirstOrDefault();
            return PartialView("_CreateEditMachine", SysMachine);
        }
        [HttpPost]
        public ActionResult EditMachine(FormCollection coll, string id)
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            try
            {
                SysMachine = linqChamCong.Sys_Machines.Where(d => d.machineId.Equals(id)).FirstOrDefault();
                SysMachine.siteId = coll.Get("siteId");
                SysMachine.machineId = coll.Get("machineId");
                SysMachine.MachineName = coll.Get("MachineName");
                SysMachine.remark = coll.Get("remark");
                SysMachine.typeMachine = String.IsNullOrEmpty(coll.Get("typeMachine")) ? 0 : Convert.ToInt32(coll.Get("typeMachine"));
                SysMachine.nguoiLap = GetUser().userName;
                SysMachine.ngayLap = DateTime.Now;
                linqChamCong.SubmitChanges();
                var list = linqChamCong.Sp_ViewMachinePB(null,null).ToList();
                ViewBag.List = list;
                return PartialView("_ViewIndex");
            }
            catch
            {
                return View();
            }
        }
        [HttpPost]
        public ActionResult DeleteMachine(string id)
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
                SysMachine = linqChamCong.Sys_Machines.Where(d => d.machineId.Equals(id)).FirstOrDefault();
                linqChamCong.Sys_Machines.DeleteOnSubmit(SysMachine);


                linqChamCong.SubmitChanges();
                var list = linqChamCong.Sp_ViewMachinePB(null,null).ToList();
                ViewBag.List = list;
                return PartialView("_ViewIndex");
            }
            catch
            {
                return View();
            }
        }
        public ActionResult EditSite(string id)
        {



            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            ViewBag.TitleHead = "Cập nhật";
            var listSites = linqChamCong.Sys_SiteConections.ToList();
            listSites.Insert(0, new BatDongSan.Models.DBChamCong.Sys_SiteConection { siteId = "", siteName = "--Chọn--" });
            ViewBag.ListSites = new SelectList(listSites, "siteId", "siteName");
            SysSiteConection = new Sys_SiteConection();
            SysSiteConection = linqChamCong.Sys_SiteConections.Where(d => d.siteId.Equals(id)).FirstOrDefault();
            return PartialView("_CreateEditSite", SysSiteConection);
        }
        [HttpPost]
        public ActionResult EditSite(FormCollection coll, string id)
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            try
            {
                SysSiteConection = linqChamCong.Sys_SiteConections.Where(d => d.siteId.Equals(id)).FirstOrDefault();
                SysSiteConection.siteName = coll.Get("siteName");
                SysSiteConection.siteAddress = coll.Get("siteAddress");
                SysSiteConection.siteIP = coll.Get("siteIP");
                SysSiteConection.port = coll.Get("port");
                SysSiteConection.databaseName = coll.Get("databaseName");
                SysSiteConection.loginName = coll.Get("loginName");
                SysSiteConection.pdw = coll.Get("pdw");
                SysSiteConection.remark = coll.Get("remark");
                SysSiteConection.nguoiLap = GetUser().userName;
                SysSiteConection.ngayLap = DateTime.Now;
                linqChamCong.SubmitChanges();
                
                return PartialView("_ViewIndex");
            }
            catch
            {
                return View();
            }
        }
        [HttpPost]
        public ActionResult DeleteSite(string id)
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
                SysSiteConection = linqChamCong.Sys_SiteConections.Where(d => d.siteId.Equals(id)).FirstOrDefault();
                linqChamCong.Sys_SiteConections.DeleteOnSubmit(SysSiteConection);

               var SysMachine2 = linqChamCong.Sys_Machines.Where(d => d.siteId == id).ToList();
               if (SysMachine2 != null && SysMachine2.Count > 0)
                        {
                            linqChamCong.Sys_Machines.DeleteAllOnSubmit(SysMachine2);
                        }

               linqChamCong.Sys_Machines.DeleteAllOnSubmit(SysMachine2);


               linqChamCong.SubmitChanges();
               
               return PartialView("_ViewIndex");
            }
            catch
            {
                return View();
            }
        }
        // Check trung siteID
        public int CheckSiteID(string siteId)
        {
            var checkList = linqChamCong.Sys_SiteConections.Where(d => d.siteId == siteId).FirstOrDefault();
            if (checkList != null)
            {
                return 1;
            }
            
            return 0;
        }
        //End check
        // Check trung siteID
        public int CheckMachineID(string machineId)
        {
            var checkList = linqChamCong.Sys_Machines.Where(d => d.machineId == machineId).FirstOrDefault();
            if (checkList != null)
            {
                return 1;
            }

            return 0;
        }
        //End check

    }
}
