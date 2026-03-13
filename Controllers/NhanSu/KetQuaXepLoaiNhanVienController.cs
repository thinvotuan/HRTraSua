using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using BatDongSan.Helper.Common;
using BatDongSan.Models.DanhMuc;
using BatDongSan.Models.NhanSu;
namespace BatDongSan.Controllers.NhanSu
{
    public class KetQuaXepLoaiNhanVienController : ApplicationController
    {
        //
        // GET: /DanhGiaXepLoaiTheoBPCon/
        LinqDanhMucDataContext linqDM =  new LinqDanhMucDataContext();
        private LinqNhanSuDataContext hr = new LinqNhanSuDataContext();
        private readonly string MCV = "KQXLNhanVien";
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
              
                int? NewNam =(DateTime.Now.Year-1);

                ViewData["yearWorkings"] = new SelectList(GetYearLimits((int)NewNam, 5), NewNam);
                return View();
            }
            catch{
            return View("Error");
            }
        }
        public ActionResult LoadIndex(int? nam)
        {
            int flash = 0;
            var checkEx = hr.tbl_DuyetXepLoaiNVs.Where(t => t.nam == nam).FirstOrDefault();
            if (checkEx != null)
            {
                flash = 1;
            }
            var listXLDG = linqDM.tbl_DM_XepLoaiDGs.ToList();
            ViewBag.listXLDG = listXLDG;
            var list = linqDM.tbl_NS_KetQuaDanhGiaXepLoais.Where(d=>d.nam == nam && d.maNhanVien == GetUser().manv).ToList();
            ViewBag.List = list;
            ViewBag.flash = flash;
            return PartialView("_LoadIndex");
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
    }
}
