using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BatDongSan.Helper.Utils;
using BatDongSan.Models.NhanSu;
using BatDongSan.Models.PhieuDeNghi;
using BatDongSan.Models.HeThong;
using BatDongSan.Models.DanhMuc;
using System.Text;
using BatDongSan.Utils.Paging;
using BatDongSan.Helper.Common;
using System.Globalization;
using BatDongSan.Models.QuanLyCaLamViec;

namespace BatDongSan.Controllers.QuanLyCaLamViec
{
    public class CaLamViecController : ApplicationController
    {
        public const string taskIDSystem = "CLV";
        public bool? permission;
        public LinqNhanSuDataContext linqNS = new LinqNhanSuDataContext();
        private LinqHeThongDataContext linqHeThong = new LinqHeThongDataContext();
        private PhanCaModel model;
        private tbl_NS_CaLamViec caLamViec;

        #region Danh sách ca làm việc

        public ActionResult Index(string qSearch)
        {
            try
            {
                #region Role user
                permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenXem);
                if (!permission.HasValue)
                {
                    return View("LogIn");
                }
                if (!permission.Value)
                {
                    return View("AccessDenied");
                }
                #endregion
                if (!string.IsNullOrEmpty(qSearch))
                {
                    var lst = linqNS.tbl_NS_CaLamViecs.OrderByDescending(d => d.maCa).Where(d => d.tenCa.ToLower().Contains(qSearch.ToLower())).ToList();
                    ViewBag.ListPhanCa = lst;
                }
                else
                {
                    ViewBag.ListPhanCa = linqNS.tbl_NS_CaLamViecs.OrderByDescending(d => d.maCa).ToList();
                }

                if (Request.IsAjaxRequest())
                {
                    ViewBag.Ajax = true;
                    return PartialView("ViewIndex");
                }
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View("Failed");
            }
        }
        #endregion

        #region Thêm, xóa, sửa ca làm việc
        public ActionResult Create()
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
                GetThongTinCaLamViec(string.Empty);
                return View(model);

            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View("Failed");
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(FormCollection collection)
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
                BindDataToSave(collection, true);
                linqNS.tbl_NS_CaLamViecs.InsertOnSubmit(caLamViec);
                linqNS.SubmitChanges();
                return RedirectToAction("Edit", "CaLamViec", new { id = caLamViec.maCa });
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.ToString();
                return View("Failed");
            }
        }

        public ActionResult Edit(string id)
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
                GetThongTinCaLamViec(id);
                return View(model);

            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View("Failed");
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(FormCollection collection)
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
                BindDataToSave(collection, false);
                linqNS.SubmitChanges();
                return RedirectToAction("Edit", "CaLamViec", new { id = caLamViec.maCa });
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.ToString();
                return View("Failed");
            }
        }

        [HttpPost]
        public ActionResult Delete(string[] maCa, FormCollection collection)
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
                var delCaLV = linqNS.tbl_NS_CaLamViecs.Where(d => maCa.Contains(d.maCa));
                linqNS.tbl_NS_CaLamViecs.DeleteAllOnSubmit(delCaLV);
                linqNS.SubmitChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.ToString();
                return View("Failed");
            }
        }

        public ActionResult Details(string id)
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenXemChiTiet);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            try
            {
                GetThongTinCaLamViec(id);
                return View(model);

            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View("Failed");
            }
        }


        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Details(FormCollection collection)
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
                string[] maNhanVien = collection.GetValues("maNhanVien");
                var delNhanVienPC = linqNS.tbl_NS_CaLamViec_NhanViens.Where(d => d.maCa == collection.Get("MaCa"));
                linqNS.tbl_NS_CaLamViec_NhanViens.DeleteAllOnSubmit(delNhanVienPC);
                if (maNhanVien != null && maNhanVien.Count() > 0)
                {
                    List<tbl_NS_CaLamViec_NhanVien> caLV = new List<tbl_NS_CaLamViec_NhanVien>();
                    tbl_NS_CaLamViec_NhanVien phanCa;
                    for (int i = 0; i < maNhanVien.Count(); i++)
                    {
                        phanCa = new tbl_NS_CaLamViec_NhanVien();
                        phanCa.maCa = collection.Get("MaCa");
                        phanCa.maNhanVien = collection.GetValues("maNhanVien")[i];
                        phanCa.nguoiLap = GetUser().manv;
                        caLV.Add(phanCa);
                    }
                    linqNS.tbl_NS_CaLamViec_NhanViens.InsertAllOnSubmit(caLV);
                }
                linqNS.SubmitChanges();
                return RedirectToAction("Details", "CaLamViec", new { id = collection.Get("MaCa") });
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.ToString();
                return View("Failed");
            }
        }
        #endregion

        #region Thông tin ca làm việc nhân viên

        public void GetThongTinCaLamViec(string maCa)
        {
            if (string.IsNullOrEmpty(maCa))
            {
                model = new PhanCaModel();
                model.MaCa = IdGeneratorDungChung(GetMaxPhieu(), "CLV");
                model.NgayLap = DateTime.Now;
                model.TenNhanVien = HoVaTen(GetUser().manv);
            }
            else
            {
                model = linqNS.tbl_NS_CaLamViecs.Where(d => d.maCa == maCa).Select(d => new PhanCaModel
                    {
                        MaCa = d.maCa,
                        TenCa = d.tenCa,
                        ThoiGianTu = d.thoiGianTu,
                        ThoiGianDen = d.thoiGianDen,
                        NgayLap = d.ngayLap,
                        MaNhanVien = d.nguoiLap,
                        MaNhanVienUpdate = d.nguoiUpdate,
                        TenNhanVien = HoVaTen(d.nguoiLap),
                        TenNhanVienUpdate = HoVaTen(d.nguoiUpdate),
                        TrangThai = d.trangThai ?? 0,
                        GhiChu = d.ghiChu ?? string.Empty
                    }).FirstOrDefault();
                if (model != null && model.TrangThai == 1)
                {
                    ViewBag.ListNhanVien = (from clv in linqNS.tbl_NS_CaLamViec_NhanViens
                                            join vw in linqNS.vw_NS_DanhSachNhanVienTheoPhongBans on clv.maNhanVien equals vw.maNhanVien into dtDong
                                            from vw2 in dtDong.DefaultIfEmpty()
                                            where clv.maCa == maCa
                                            select new PhanCaNhanVienModel
                                            {
                                                MaNhanVien = clv.maNhanVien,
                                                HoTenNhanVien = vw2.hoTen,
                                                TenPhongBan = vw2.tenPhongBan,
                                                TenChucDanh = vw2.TenChucDanh,
                                            }).ToList();

                }
            }
        }


        /// <summary>
        /// Lấy max phiếu ca làm việc
        /// </summary>
        /// <returns></returns>
        public string GetMaxPhieu()
        {
            try
            {
                string lastID = linqNS.tbl_NS_CaLamViecs.OrderByDescending(d => d.maCa).Select(d => d.maCa).FirstOrDefault();
                return lastID;
            }
            catch
            {
                return string.Empty;
            }
        }

        public void BindDataToSave(FormCollection col, bool isCreate)
        {
            if (isCreate == true)
            {
                caLamViec = new tbl_NS_CaLamViec();
                caLamViec.maCa = IdGeneratorDungChung(GetMaxPhieu(), "CLV");
                caLamViec.ngayLap = DateTime.Now;
                caLamViec.nguoiLap = GetUser().manv;
            }
            else
            {
                caLamViec = linqNS.tbl_NS_CaLamViecs.Where(d => d.maCa == col.Get("MaCa")).FirstOrDefault();
                caLamViec.nguoiUpdate = GetUser().manv;
            }
            caLamViec.tenCa = col.Get("TenCa");
            caLamViec.thoiGianTu = DateTime.ParseExact(col.Get("ThoiGianTu"), "HH:mm", CultureInfo.InvariantCulture);
            caLamViec.thoiGianDen = DateTime.ParseExact(col.Get("ThoiGianDen"), "HH:mm", CultureInfo.InvariantCulture);
            caLamViec.ghiChu = col.Get("GhiChu");
        }
        #endregion

        #region Xác nhận ca làm việc
        public JsonResult XacNhanCaLamViec(string maCaLamViec)
        {
            try
            {
                var updateCLV = linqNS.tbl_NS_CaLamViecs.Where(d => d.maCa == maCaLamViec).FirstOrDefault();
                if (updateCLV != null)
                {
                    updateCLV.trangThai = 1;
                    linqNS.SubmitChanges();
                    return Json(string.Empty);
                }
                else
                {
                    return Json("Error");
                }

            }
            catch
            {
                return Json("Error");
            }
        }
        #endregion

        #region Hiển thị danh sách nhân viên
        public ActionResult GetMoreUsers(int? page, string nhomUser, string searchString)
        {
            int currentPageIndex = page.HasValue ? page.Value : 1;
            int? tongSoDong = 0;
            var users = linqHeThong.sp_Sys_User_Index(null, searchString, null, currentPageIndex, 20).ToList();
            try
            {
                ViewBag.Count = users[0].tongSoDong;
                tongSoDong = users[0].tongSoDong;
            }
            catch
            {
                ViewBag.Count = 0;
            }
            TempData["Params"] = nhomUser + "," + searchString;
            return PartialView("PartialUsers", users.ToPagedList(currentPageIndex, 20, true, tongSoDong));
        }
        #endregion
    }
}
