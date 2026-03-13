using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using BatDongSan.Helper.Common;
using BatDongSan.Models.DanhMuc;

namespace BatDongSan.Controllers.NhanSu
{
    public class DanhGiaXepLoaiTheoBPConController : ApplicationController
    {
        //
        // GET: /DanhGiaXepLoaiTheoBPCon/
        LinqDanhMucDataContext linqDM =  new LinqDanhMucDataContext();
        private readonly string MCV = "DGXLTheoBPCon";
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
              
                int? NewNam =DateTime.Now.Year;

                ViewData["yearWorkings"] = new SelectList(GetYearLimits((int)NewNam, 5), NewNam);
                return View();
            }
            catch{
            return View("Error");
            }
        }
        public ActionResult LoadIndex(int? nam, string qSearch)
        {
            var list = linqDM.sp_DG_DanhGiaXepLoaiTheoBPCon_Index(nam, qSearch).ToList();
            ViewBag.List = list;
            return PartialView("_LoadIndex");
        }
        public ActionResult DanhSachPPNhomDanhGia()
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

                var list = linqDM.tbl_DM_PhuongPhapNhomDanhGia_Cons.ToList();
                ViewBag.ListPPNHomDG = list;
                return View();
            }
            catch (Exception e)
            {

                return View("Error");
            }
        }
        
        public ActionResult LoadTiLeKhenThuong(string maPPNhomDG, string maPhieu, int nam)
        {
            #region Role user
            permission = GetPermission("BCXLNam", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            // Check maPPNhomDG have tbl_XepLoaiTongCon
            var checkmaPPNhomDG = linqDM.tbl_XepLoaiTongCons.Where(d => d.maPhieu == maPhieu && d.nam == nam).FirstOrDefault();
            if (checkmaPPNhomDG == null)
            {
                List<tbl_XepLoaiTongCon> lst_tblXepLoaiTong = new List<tbl_XepLoaiTongCon>();

                var listLoai2 = linqDM.tbl_DM_XepLoaiDGs.ToList();
                foreach (var item in listLoai2)
                {
                    
                        tbl_XepLoaiTongCon tblXepLoaiTong = new tbl_XepLoaiTongCon();
                        tblXepLoaiTong.nam = nam;
                        tblXepLoaiTong.maPhieu = maPhieu;
                        tblXepLoaiTong.maLoai = item.maXepLoai;
                        tblXepLoaiTong.soLuong = 0;
                        tblXepLoaiTong.tiLe = 0;
                        lst_tblXepLoaiTong.Add(tblXepLoaiTong);
                    
                }
                linqDM.tbl_XepLoaiTongCons.InsertAllOnSubmit(lst_tblXepLoaiTong);
                linqDM.SubmitChanges();
            }
            var listXL = linqDM.sp_DG_XepLoaiTongCon(maPhieu, nam).ToList();
            var toTal = listXL.Count();
            ViewBag.listXLTong = listXL;
            ViewBag.toTal = toTal;
            ViewBag.TongSoNV = linqDM.sp_DG_GetTongSoNVPB_Con(maPPNhomDG).Sum(d => d.sl) ?? 0;
            

            return PartialView("_LoadTiLeKhenThuong");
        }
        public ActionResult LoadTiLeKhenThuongTheoXH(string maPPNhomDG, string maPhieu, int nam)
        {

    // Ti le khen thuong theo xep hang
            var lstXepLoaiHang = linqDM.sp_DG_XepLoaiHangCon(maPPNhomDG).Select(t => t.thuHang).Distinct().ToList();
            List<tbl_XepLoaiTheoHangCon> lst_tblXepLoaiTheoHang = new List<tbl_XepLoaiTheoHangCon>();
            foreach (var itemHang in lstXepLoaiHang)
            {
                var lstDanhGia = linqDM.tbl_DM_XepLoaiDGs.ToList();
                foreach (var itemLDG in lstDanhGia)
                {
                    var checkLDG = linqDM.tbl_XepLoaiTheoHangCons.Where(d => d.nam == nam && d.thuTuHang == Convert.ToDouble(itemHang ?? 0) && d.maPhieu == maPhieu).FirstOrDefault();
                    if (checkLDG == null)
                    {
                        tbl_XepLoaiTheoHangCon tblXepLoaiTheohang = new tbl_XepLoaiTheoHangCon();
                        tblXepLoaiTheohang.maLoai = itemLDG.maXepLoai;
                        tblXepLoaiTheohang.nam = Convert.ToInt32(nam);
                        tblXepLoaiTheohang.maPhieu = maPhieu;
                        tblXepLoaiTheohang.thuTuHang = Convert.ToDouble(itemHang);
                        tblXepLoaiTheohang.soLuongNV = Convert.ToDouble(linqDM.sp_DG_XepLoaiHangCon(maPPNhomDG).Where(t => t.thuHang == itemHang).Sum(t => t.sL));
                        tblXepLoaiTheohang.tiLeChoi = 0;
                        tblXepLoaiTheohang.phanTram = 0;
                        tblXepLoaiTheohang.soLuong = 0;
                        lst_tblXepLoaiTheoHang.Add(tblXepLoaiTheohang);
                    }

                }

            }
            linqDM.tbl_XepLoaiTheoHangCons.InsertAllOnSubmit(lst_tblXepLoaiTheoHang);
            linqDM.SubmitChanges();
            // End ti le khen thuong theo xep hang
            var lstTheoHangIndex = linqDM.sp_DG_XepLoaiTheoHang_Index_Con(maPhieu,nam).ToList();
            ViewBag.totalHang = lstTheoHangIndex.Count();
            ViewBag.lstTheoHangIndex = lstTheoHangIndex;
            var listXLTheoBoPhan = linqDM.sp_DG_XepLoaiTheoBoPhanCon(nam, maPPNhomDG).ToList();
            ViewBag.listXLTheoBoPhan = listXLTheoBoPhan;
            return PartialView("_LoadTiLeKhenThuongTheoXH");
    }
        //
        public string UpdateTiLe(int? nam, string tiLes, string soLuongs, string maLoais, string maPhieu, string MaPPNhomDG)
        {
          

            try
            {
                
                    // Update
                    string[] TiLe = tiLes.Split(',');
                    string[] SoLuong = soLuongs.Split(',');
                    string[] MaLoai = maLoais.Split(',');

                    if (TiLe != null)
                    {
                        for (int i = 0; i < TiLe.Length; i++)
                        {
                            tbl_XepLoaiTongCon mucLuongThuong = new tbl_XepLoaiTongCon();
                            mucLuongThuong = linqDM.tbl_XepLoaiTongCons.Where(d => d.maLoai == MaLoai[i] && d.nam == nam && d.maPhieu == maPhieu).FirstOrDefault();
                            mucLuongThuong.maLoai = MaLoai[i];

                            mucLuongThuong.nam = nam;
                            mucLuongThuong.soLuong = Convert.ToDouble(SoLuong[i]);
                            mucLuongThuong.tiLe = Convert.ToDouble(TiLe[i]);
                            linqDM.SubmitChanges();

                        }

                    }
                    var checkPhieuHienTai = linqDM.tbl_NS_DanhGiaXepLoaiTheoBPCons.Where(d => d.maPhieu == maPhieu && d.maPhuongPhapDanhGia == MaPPNhomDG && d.nam == nam).FirstOrDefault();
                    if (checkPhieuHienTai == null)
                    {
                        SavePhieuHienTai(maPhieu, MaPPNhomDG, nam ?? DateTime.Now.Year);
                    }


                    return "true";
                
                


            }
            catch (Exception e)
            {
                return "Lỗi: " + e.Message;
            }
        }
        public int checkDaTaoNhomDanhGia(int nam, string maPPNhomDG) {
             var checkPhieuHienTai = linqDM.tbl_NS_DanhGiaXepLoaiTheoBPCons.Where(d =>  d.maPhuongPhapDanhGia == maPPNhomDG && d.nam == nam).FirstOrDefault();
             if (checkPhieuHienTai != null)
             {
                 return 1;
             }
            return 0;
        }
        public int SavePhieuHienTai(string maPhieu, string MaPPNhomDG, int nam) {
            // Check maPhieu va nam co chua.

            tbl_NS_DanhGiaXepLoaiTheoBPCon tblXLBPCon = new tbl_NS_DanhGiaXepLoaiTheoBPCon();
            tblXLBPCon.maPhieu = maPhieu;
            tblXLBPCon.nam = nam;
            tblXLBPCon.maPhuongPhapDanhGia = MaPPNhomDG;
            tblXLBPCon.ngayLap = DateTime.Now;
            tblXLBPCon.nguoiLap = GetUser().manv;
            linqDM.tbl_NS_DanhGiaXepLoaiTheoBPCons.InsertOnSubmit(tblXLBPCon);
            linqDM.SubmitChanges();
            // end check
            return 1;
        }
        // GET: /DanhGiaXepLoaiTheoBPCon/Details/5
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult SaveKhenThuongTH(FormCollection form)
        {

            #region Role user
            permission = GetPermission("BCXLNam", BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            try
            {

                // Ti le khen thuong theo xep hang
                var deleteThuHang = linqDM.tbl_XepLoaiTheoHangCons.Where(d => d.nam == Convert.ToInt32(form.Get("namThuHang")) && d.maPhieu == form.Get("maPhieuTH")).ToList();
                if (deleteThuHang != null)
                {
                    var tongSoThuHang = deleteThuHang.Count();
                    linqDM.tbl_XepLoaiTheoHangCons.DeleteAllOnSubmit(deleteThuHang);
                    linqDM.SubmitChanges();
                }
                // Create New;
                tbl_XepLoaiTheoHangCon tblXepLoaiTheoHang = null;
                List<tbl_XepLoaiTheoHangCon> lst_tblXepLoaiTheoHang = new List<tbl_XepLoaiTheoHangCon>();

                string[] thuHang = form.GetValues("thuTuHang");


                if (thuHang != null)
                {
                    for (int i = 0; i < thuHang.Length; i++)
                    {

                        tblXepLoaiTheoHang = new tbl_XepLoaiTheoHangCon();
                        tblXepLoaiTheoHang.nam = Convert.ToInt32(form.Get("namThuHang"));
                        tblXepLoaiTheoHang.maPhieu = Convert.ToString(form.Get("maPhieuTH"));
                        tblXepLoaiTheoHang.thuTuHang = Convert.ToDouble(form.GetValues("thuTuHang")[i]);
                        tblXepLoaiTheoHang.soLuongNV = Convert.ToInt32(form.GetValues("soLuongNV")[i]);
                        tblXepLoaiTheoHang.tiLeChoi = Convert.ToDouble(form.GetValues("tiLeChoi")[i]);
                        tblXepLoaiTheoHang.soLuong = Convert.ToDouble(form.GetValues("soLuongHang")[i]);
                        tblXepLoaiTheoHang.phanTram = Convert.ToDouble(form.GetValues("tilephantram")[i]);
                        tblXepLoaiTheoHang.maLoai = form.GetValues("maLoaiHang")[i];



                        lst_tblXepLoaiTheoHang.Add(tblXepLoaiTheoHang);

                    }
                    linqDM.tbl_XepLoaiTheoHangCons.InsertAllOnSubmit(lst_tblXepLoaiTheoHang);
                }

                linqDM.SubmitChanges();
                var checkPhieuHienTai = linqDM.tbl_NS_DanhGiaXepLoaiTheoBPCons.Where(d => d.maPhieu == form.Get("maPhieuTH") && d.maPhuongPhapDanhGia == form.Get("maPPDGTH") && d.nam == Convert.ToInt32(form.Get("namThuHang"))).FirstOrDefault();
                if (checkPhieuHienTai == null)
                {
                    SavePhieuHienTai(form.Get("maPhieuTH"), form.Get("maPPDGTH"), Convert.ToInt32(form.Get("namThuHang")));
                }
                var lstTheoHangIndex = linqDM.sp_DG_XepLoaiTheoHang_Index_Con(form.Get("maPhieuTH"), Convert.ToInt32(form.Get("namThuHang"))).ToList();
                ViewBag.totalHang = lstTheoHangIndex.Count();
               
                ViewBag.lstTheoHangIndex = lstTheoHangIndex;
                var listXLTheoBoPhan = linqDM.sp_DG_XepLoaiTheoBoPhanCon(Convert.ToInt32(form.Get("namThuHang")), form.Get("maPPDGTH")).ToList();
                ViewBag.listXLTheoBoPhan = listXLTheoBoPhan;
                // End ti le khen thuong theo xep hang
                return PartialView("_LoadTiLeKhenThuongTheoXH");


            }
            catch
            {
                return View();
            }
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public string SaveXepLoaiTheoBP(FormCollection form)
        {

          

            try
            {

                // Ti le khen thuong theo xep hang
                var deleteThuHang = linqDM.tbl_XepLoaiTheoBoPhanCons.Where(d => d.nam == Convert.ToInt32(form.Get("namThuHangXLBP")) && d.maPhieu == form.Get("maPhieuXLBP")).ToList();
                if (deleteThuHang != null)
                {
                    var tongSoThuHang = deleteThuHang.Count();
                    linqDM.tbl_XepLoaiTheoBoPhanCons.DeleteAllOnSubmit(deleteThuHang);
                    linqDM.SubmitChanges();
                }
                // Create New;
                tbl_XepLoaiTheoBoPhanCon tblXepLoaiTheoHang = null;
                List<tbl_XepLoaiTheoBoPhanCon> lst_tblXepLoaiTheoHang = new List<tbl_XepLoaiTheoBoPhanCon>();

                string[] thuHang = form.GetValues("thuTuHangXLBP");


                if (thuHang != null)
                {
                    for (int i = 0; i < thuHang.Length; i++)
                    {

                        tblXepLoaiTheoHang = new tbl_XepLoaiTheoBoPhanCon();
                        tblXepLoaiTheoHang.nam = Convert.ToInt32(form.Get("namThuHangXLBP"));

                        tblXepLoaiTheoHang.maPhieu = form.Get("maPhieuXLBP");
                        tblXepLoaiTheoHang.thuTuHang = Convert.ToDouble(form.GetValues("thuTuHangXLBP")[i]);
                        tblXepLoaiTheoHang.maPhongBan = form.GetValues("maPhongBanXLBP")[i];
                        tblXepLoaiTheoHang.maLoai = form.GetValues("maLoaiHangXLBP")[i];
                        tblXepLoaiTheoHang.soLuong = Convert.ToDouble(form.GetValues("soLuongXLBP")[i]);



                        lst_tblXepLoaiTheoHang.Add(tblXepLoaiTheoHang);

                    }
                    linqDM.tbl_XepLoaiTheoBoPhanCons.InsertAllOnSubmit(lst_tblXepLoaiTheoHang);
                }
                var Update = linqDM.sp_NS_CapNhatKetQuaXepLoaiPhongBanCon(form.Get("maPhieuXLBP"), Convert.ToInt32(form.Get("namThuHangXLBP")));

                linqDM.SubmitChanges();


                return "true";

            }
            catch
            {
                return "false";
            }
        }

        public string ResetTiLeKhenThuong(string maPPNhomDG, string maPhieu, int nam)
        {
           
            try
            {
                // Delete truoc khi insert new
                var delXLTh = linqDM.tbl_XepLoaiTheoHangCons.Where(d => d.nam == nam && d.maPhieu == maPhieu).ToList();
                if (delXLTh != null)
                {
                    linqDM.tbl_XepLoaiTheoHangCons.DeleteAllOnSubmit(delXLTh);
                    linqDM.SubmitChanges();
                }
                // End delete truoc khi insert
                var lstXepLoaiHang = linqDM.sp_DG_XepLoaiHangCon(maPPNhomDG).Select(t => t.thuHang).Distinct().ToList();
                List<tbl_XepLoaiTheoHangCon> lst_tblXepLoaiTheoHang = new List<tbl_XepLoaiTheoHangCon>();
                foreach (var itemHang in lstXepLoaiHang)
                {
                    var lstDanhGia = linqDM.tbl_DM_XepLoaiDGs.ToList();
                    foreach (var itemLDG in lstDanhGia)
                    {
                        var checkLDG = linqDM.tbl_XepLoaiTheoHangCons.Where(d => d.nam == nam && d.thuTuHang == Convert.ToDouble(itemHang ?? 0) && d.maPhieu == maPhieu).FirstOrDefault();
                        if (checkLDG == null)
                        {
                            tbl_XepLoaiTheoHangCon tblXepLoaiTheohang = new tbl_XepLoaiTheoHangCon();
                            tblXepLoaiTheohang.maLoai = itemLDG.maXepLoai;
                            tblXepLoaiTheohang.nam = Convert.ToInt32(nam);
                            tblXepLoaiTheohang.maPhieu = maPhieu;
                            tblXepLoaiTheohang.thuTuHang = Convert.ToDouble(itemHang);
                            tblXepLoaiTheohang.soLuongNV = Convert.ToDouble(linqDM.sp_DG_XepLoaiHangCon(maPPNhomDG).Where(t => t.thuHang == itemHang).Sum(t => t.sL));
                            tblXepLoaiTheohang.tiLeChoi = 0;
                            tblXepLoaiTheohang.phanTram = 0;
                            tblXepLoaiTheohang.soLuong = 0;
                            lst_tblXepLoaiTheoHang.Add(tblXepLoaiTheohang);
                        }

                    }

                }
                linqDM.tbl_XepLoaiTheoHangCons.InsertAllOnSubmit(lst_tblXepLoaiTheoHang);
                linqDM.SubmitChanges();
                var deleteThuHang = linqDM.tbl_XepLoaiTheoBoPhanCons.Where(d => d.nam == Convert.ToInt32(nam) && d.maPhieu == maPhieu).ToList();
                if (deleteThuHang != null)
                {
                    linqDM.tbl_XepLoaiTheoBoPhanCons.DeleteAllOnSubmit(deleteThuHang);
                    linqDM.SubmitChanges();
                }



                SaveActiveHistory("Khởi tạo lại giá trị bộ phận con năm: " + nam + " PP:" + maPPNhomDG + " maPhieu " + maPhieu);
                return "true";
            }
            catch (Exception e)
            {
                return "Lỗi: " + e.Message;
            }
        }
        //
        // GET: /DanhGiaXepLoaiTheoBPCon/Create

        public ActionResult Create()
        {
            int? NewNam = DateTime.Now.Year;
            ViewData["yearWorkings"] = new SelectList(GetYearLimits((int)NewNam, 5), NewNam);
            ViewBag.maPhieu = IdGenerator();
            return View();
        }

        //
        // POST: /DanhGiaXepLoaiTheoBPCon/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /DanhGiaXepLoaiTheoBPCon/Edit/5

        public ActionResult Edit(string id)
        {
            var getThongTin = linqDM.tbl_NS_DanhGiaXepLoaiTheoBPCons.Where(d => d.maPhieu == id).FirstOrDefault();
            
            
            ViewBag.nam = getThongTin.nam;
            ViewBag.maPhieu = id;
            ViewBag.maPPNhomDG = getThongTin.maPhuongPhapDanhGia;
            return View();
        }

        //
        // POST: /DanhGiaXepLoaiTheoBPCon/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        public int DeletePhieu(string maPhieu, int?nam) {
            // Delete tbl_NS_DanhGiaXepLoaiTheoBPCons
            var DeleteBP = linqDM.tbl_NS_DanhGiaXepLoaiTheoBPCons.Where(d => d.maPhieu == maPhieu && d.nam == nam).ToList();
            linqDM.tbl_NS_DanhGiaXepLoaiTheoBPCons.DeleteAllOnSubmit(DeleteBP);
            // tbl_XepLoaiTheoHangCon
            var DeleteHangCon = linqDM.tbl_XepLoaiTheoHangCons.Where(d => d.maPhieu == maPhieu && d.nam == nam).ToList();
            linqDM.tbl_XepLoaiTheoHangCons.DeleteAllOnSubmit(DeleteHangCon);
            // tbl_XepLoaiTongCon
            var DeleteTongCon = linqDM.tbl_XepLoaiTongCons.Where(d => d.maPhieu == maPhieu && d.nam == nam).ToList();
            linqDM.tbl_XepLoaiTongCons.DeleteAllOnSubmit(DeleteTongCon);
            // tbl_XepLoaiTheoBoPhanCon
            var DeleteBoPhanCon = linqDM.tbl_XepLoaiTheoBoPhanCons.Where(d => d.maPhieu == maPhieu && d.nam == nam).ToList();
            linqDM.tbl_XepLoaiTheoBoPhanCons.DeleteAllOnSubmit(DeleteBoPhanCon);
            linqDM.SubmitChanges();
            return 1;
        }
        public ActionResult Details(string id) {
            var getThongTin = linqDM.tbl_NS_DanhGiaXepLoaiTheoBPCons.Where(d => d.maPhieu == id).FirstOrDefault();


            ViewBag.nam = getThongTin.nam;
            ViewBag.maPhieu = id;
            ViewBag.maPPNhomDG = getThongTin.maPhuongPhapDanhGia;
            return View();
        }
        
        //
        // GET: /DanhGiaXepLoaiTheoBPCon/Delete/5

        

        private void namHT(int value)
        {
            Dictionary<int, string> dics = new Dictionary<int, string>();
            for (int i = (DateTime.Now.Year - 5); i < (DateTime.Now.Year + 5); i++)
            {
                dics[i] = i.ToString();
            }
            ViewData["nam"] = new SelectList(dics, "Key", "Value", value);
            
        }
        public string IdGenerator()
        {
            StringBuilder sb = new StringBuilder();
            var date = DateTime.Now;
            string lastID = linqDM.tbl_NS_DanhGiaXepLoaiTheoBPCons.OrderByDescending(d => d.ngayLap).Select(d => d.maPhieu).FirstOrDefault();
            string nam = date.Year.ToString();
            nam = nam.Remove(0, 2);
            string thang = string.Empty;
            if (date.Month < 10)
            {
                thang = "0" + date.Month;
            }
            else
            {
                thang = date.Month.ToString();
            }
            if (String.IsNullOrEmpty(lastID))
            {
                return "DGXLTBPCon-" + nam + thang + "001";
            }
            else
            {
                int? idSuffix = Convert.ToInt32(lastID.Substring(lastID.Length - 3)) + 1;
                if (idSuffix <= 0 || idSuffix == null)
                    return "DGXLTBPCon-" + nam + thang + "001";
                else
                {
                    sb.Append(idSuffix.ToString());
                    while (sb.Length < 3)
                    {
                        sb.Insert(0, "0");
                    }
                    return "DGXLTBPCon-" + nam + thang + sb.ToString();
                }
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
    }
}
