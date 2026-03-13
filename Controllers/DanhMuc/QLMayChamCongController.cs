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
    public class QLMayChamCongController : ApplicationController
    {
        private bool? permission;
        public const string taskIDSystem = "QLMayChamCong";

        public ActionResult Index()
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return RedirectToAction("AccessDenied");
            #endregion
            DBChamCongDataContext linqChamCong = new DBChamCongDataContext();
            var listSite = linqChamCong.Sys_SiteConections.ToList();
            listSite.Insert(0, new Sys_SiteConection { siteId = "", siteName = "[Chọn công trình / VP]" });
            ViewBag.listSite = new SelectList(listSite, "siteId", "siteName");

            var groupCongTrinh = linqChamCong.Sys_SiteConections.ToList();
            ViewBag.LstCongTrinh = groupCongTrinh;
            return View();
        }
        public ActionResult ViewIndex(int? pageSize, string searchString, string maCongTrinh, int _page = 0)
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
                DBChamCongDataContext linqChamCong = new DBChamCongDataContext();
                var groupCongTrinh = linqChamCong.Sys_SiteConections.ToList();
                ViewBag.LstCongTrinh = groupCongTrinh;
                int page = _page == 0 ? 1 : _page;
                int pIndex = page;
                int total = linqChamCong.Sp_ViewMachinePB(maCongTrinh, searchString).Count();
                PagingLoaderController("/TaiLieuUpload/Index/", total, page, "?searchString=" + searchString + "&maCongTrinh=" + maCongTrinh);
                ViewData["lsDanhSach"] = linqChamCong.Sp_ViewMachinePB(maCongTrinh, searchString).Skip(start).Take(offset).ToList();
                ViewBag.MaNhanVien = GetUser().manv;
                return PartialView("ViewIndex");
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }

        }


        public int CapNhatMayCC(string idMachine, string maChineName, string remark, string siteCongTrinh)
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return 0;
            if (!permission.Value)
                return 0;
            #endregion
            try
            {
                DBChamCongDataContext linqChamCong = new DBChamCongDataContext();

                var maChine = linqChamCong.Sys_Machines.Where(d => d.machineId == idMachine).FirstOrDefault();
                maChine.MachineName = maChineName;
                maChine.siteId = siteCongTrinh;
                maChine.remark = remark;
                linqChamCong.SubmitChanges();
                return 1;
            }
            catch (Exception ex)
            {
                return 0;
            }

        }
        public int ThemMoiMayCC(string idMachine, string maChineName, string remark, string siteCongTrinh, int typeMaChine)
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return 0;
            if (!permission.Value)
                return 0;
            #endregion
            try
            {
                DBChamCongDataContext linqChamCong = new DBChamCongDataContext();
                // Check machinID exist
                var checkMaChine = linqChamCong.Sys_Machines.Where(d => d.machineId == idMachine).FirstOrDefault();
                if (checkMaChine == null)
                {
                    Sys_Machine maChine = new Sys_Machine();
                    maChine.machineId = idMachine;
                    maChine.MachineName = maChineName;
                    maChine.siteId = siteCongTrinh;
                    maChine.remark = remark;
                    maChine.typeMachine = typeMaChine;
                    maChine.nguoiLap = GetUser().manv;
                    maChine.ngayLap = DateTime.Now;
                    linqChamCong.Sys_Machines.InsertOnSubmit(maChine);
                    linqChamCong.SubmitChanges();
                    return 1;
                }
                else {
                    return 2;
                }
            }
            catch (Exception ex)
            {
                return 0;
            }

        }
        
        public int InportMayCCToHR()
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return 0;
            if (!permission.Value)
                return 0;
            #endregion
            try
            {
                LinqNhanSuDataContext linqNhanSu = new LinqNhanSuDataContext();
                DBChamCongDataContext linqChamCong = new DBChamCongDataContext();
                var list = linqChamCong.Sp_ViewMachinePB(null, null).ToList();
                var DeleteMayCCHR = linqNhanSu.Sys_MachineHRs.ToList();
                linqNhanSu.Sys_MachineHRs.DeleteAllOnSubmit(DeleteMayCCHR);
                linqNhanSu.SubmitChanges();
                // Add list
                Sys_MachineHR tblSysMaC = null;
                List<Sys_MachineHR> lst_tblNhanVienBangCap = new List<Sys_MachineHR>();




                if (list != null && list.Count > 0)
                {
                    foreach (var item in list)
                    {

                        tblSysMaC = new Sys_MachineHR();
                        tblSysMaC.machineId = item.machineId;
                        tblSysMaC.MachineName = item.MachineName;
                        tblSysMaC.ngayLap = DateTime.Now;
                        tblSysMaC.nguoiLap = GetUser().manv;
                        tblSysMaC.siteName = item.siteName;
                        tblSysMaC.siteId = item.siteId;
                        lst_tblNhanVienBangCap.Add(tblSysMaC);


                    }
                    linqNhanSu.Sys_MachineHRs.InsertAllOnSubmit(lst_tblNhanVienBangCap);
                    linqNhanSu.SubmitChanges();
                }
                // End add list

                return 1;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
       

        [AcceptVerbs(HttpVerbs.Post)]
        public int SaveCongTrinh(FormCollection form)
        {

            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return 0;
            if (!permission.Value)
                return 0;
            #endregion
            try
            {
                DBChamCongDataContext linqChamCong = new DBChamCongDataContext();

                var deleteTD = linqChamCong.Sys_SiteConections.ToList();
                if (deleteTD != null)
                {

                    linqChamCong.Sys_SiteConections.DeleteAllOnSubmit(deleteTD);

                }
                // Create New;
                Sys_SiteConection tblCauHinhNhomCV = null;
                List<Sys_SiteConection> lst_tblCauHinhNhomCV = new List<Sys_SiteConection>();

                string[] thuHang = form.GetValues("siteId");


                if (thuHang != null)
                {
                    for (int i = 0; i < thuHang.Length; i++)
                    {

                        tblCauHinhNhomCV = new Sys_SiteConection();
                        tblCauHinhNhomCV.siteId = Convert.ToString(form.GetValues("siteId")[i]) ?? String.Empty;
                        tblCauHinhNhomCV.siteIP = Convert.ToString(form.GetValues("siteIP")[i]) ?? String.Empty;
                        tblCauHinhNhomCV.siteAddress = Convert.ToString(form.GetValues("siteAddress")[i]) ?? String.Empty;
                        tblCauHinhNhomCV.siteName = Convert.ToString(form.GetValues("siteName")[i]) ?? String.Empty;
                        tblCauHinhNhomCV.port = Convert.ToString(form.GetValues("port")[i]) ?? String.Empty;
                        tblCauHinhNhomCV.loginName = Convert.ToString(form.GetValues("loginName")[i]) ?? String.Empty;
                        tblCauHinhNhomCV.pdw = Convert.ToString(form.GetValues("pdw")[i]) ?? String.Empty;
                        tblCauHinhNhomCV.databaseName = Convert.ToString(form.GetValues("databaseName")[i]) ?? String.Empty;
                        tblCauHinhNhomCV.remark = Convert.ToString(form.GetValues("remark")[i]) ?? String.Empty;
                        tblCauHinhNhomCV.nguoiLap = GetUser().manv;
                        tblCauHinhNhomCV.ngayLap = DateTime.Now;
                        lst_tblCauHinhNhomCV.Add(tblCauHinhNhomCV);

                    }
                    linqChamCong.Sys_SiteConections.InsertAllOnSubmit(lst_tblCauHinhNhomCV);
                }

                linqChamCong.SubmitChanges();
                return 1;


            }
            catch
            {
                return 0;
            }
        }

    }
}
