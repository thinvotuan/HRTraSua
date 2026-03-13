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
using System.IO;
using System.Data;

namespace BatDongSan.Controllers.NhanSu
{
    public class PhieuTamUngController : ApplicationController
    {
        LinqPhieuDeNghiDataContext lqPhieuDN = new LinqPhieuDeNghiDataContext();
        LinqNhanSuDataContext linqNS = new LinqNhanSuDataContext();
        LinqDanhMucDataContext linqDM = new LinqDanhMucDataContext();
        BatDongSan.Models.NhanSu.LinqThuanViet.LinqThuanVietDataContext lqThuanViet = new BatDongSan.Models.NhanSu.LinqThuanViet.LinqThuanVietDataContext();

        tbl_NS_PhieuTamUng tblPhieuDeNghi;
        IList<tbl_NS_PhieuTamUng> tblPhieuDeNghis;
        PhieuTamUng PhieuDeNghiModel;
        public const string taskIDSystem = "PhieuTamUng";//REGWORKVOTE
        public bool? permission;
        //
        // GET: /PhieuCongTac/

        public ActionResult Index()
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            BindDataTrangThai(taskIDSystem);
            var userName = GetUser().manv;


            ViewBag.isGet = "True";
            thang(DateTime.Now.Month);
            nam(DateTime.Now.Year);
            return View();

        }
        public ActionResult ViewIndex(int? page, string qSearch, int nam, int thang, string trangThai)
        {
            var userName = GetUser().manv;

            ViewBag.isGet = "True";
            var tblPhieuDeNghis = linqNS.sp_NS_PhieuTamUng_Index(nam, thang, qSearch, trangThai).ToList();
            int currentPageIndex = page.HasValue ? page.Value : 1;
            ViewBag.Count = tblPhieuDeNghis.Count();
            ViewBag.Search = qSearch;
            ViewBag.nam = nam;
            ViewBag.thang = thang;
            ViewBag.trangThai = trangThai;
            ViewBag.maNhanVien = GetUser().manv;
            return PartialView("ViewIndex", tblPhieuDeNghis.ToPagedList(currentPageIndex, 50));

        }

        public ActionResult Create()
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenThem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            PhieuDeNghiModel = new PhieuTamUng();
            PhieuDeNghiModel.maPhieu = GenerateUtil.CheckLetter("DNTU", GetMax());
            PhieuDeNghiModel.ngayLap = DateTime.Now;
            PhieuDeNghiModel.NhanVienLapPhieu = new BatDongSan.Models.NhanSu.NhanVienModel
            {
                maNhanVien = GetUser().manv,
                hoVaTen = HoVaTen(GetUser().manv)
            };
            PhieuDeNghiModel.NhanVien = new BatDongSan.Models.NhanSu.NhanVienModel();
            PhieuDeNghiModel.soTien = 0;

            builtThang(DateTime.Now.Month);
            builtNam(DateTime.Now.Year);

            return View(PhieuDeNghiModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(FormCollection coll)
        {
            try
            {
                tblPhieuDeNghi = new tbl_NS_PhieuTamUng();
                tblPhieuDeNghi.soPhieu = GenerateUtil.CheckLetter("DNTU", GetMax());
                tblPhieuDeNghi.nguoiLap = GetUser().manv;
                tblPhieuDeNghi.ngayTamUng = String.IsNullOrEmpty(coll.Get("ngayTamUng")) ? (DateTime?)null : DateTime.ParseExact(coll.Get("ngayTamUng"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                tblPhieuDeNghi.ngayLap = DateTime.Now;
                tblPhieuDeNghi.maNhanVien = coll.Get("NhanVien.maNhanVien");

                tblPhieuDeNghi.tamUngThang = Convert.ToInt32(coll.Get("thang"));
                tblPhieuDeNghi.tamUngNam = Convert.ToInt32(coll.Get("nam"));
                tblPhieuDeNghi.soTien = Convert.ToDouble(coll.Get("soTien"));
                tblPhieuDeNghi.ghiChu = coll.Get("ghiChu");

                linqNS.tbl_NS_PhieuTamUngs.InsertOnSubmit(tblPhieuDeNghi);
                linqNS.SubmitChanges();

                return RedirectToAction("Edit", new { id = tblPhieuDeNghi.soPhieu });
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }
        public string GetMax()
        {
            return linqNS.tbl_NS_PhieuTamUngs.OrderByDescending(d => d.ngayLap).Select(d => d.soPhieu).FirstOrDefault() ?? string.Empty;
        }
        [HttpPost]
        public ActionResult Delete(string id)
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
                var phieu = linqNS.tbl_NS_PhieuTamUngs.Where(d => d.soPhieu == id).FirstOrDefault();
                linqNS.tbl_NS_PhieuTamUngs.DeleteOnSubmit(phieu);
                linqNS.SubmitChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View("error");
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

            var dataPhieuCongTac = linqNS.tbl_NS_PhieuTamUngs.Where(d => d.soPhieu == id).FirstOrDefault();
            PhieuDeNghiModel = new PhieuTamUng();
            PhieuDeNghiModel.maPhieu = dataPhieuCongTac.soPhieu;
            PhieuDeNghiModel.ngayLap = dataPhieuCongTac.ngayLap;
            PhieuDeNghiModel.nguoiLap = dataPhieuCongTac.nguoiLap;
            PhieuDeNghiModel.ngayTamUng = dataPhieuCongTac.ngayTamUng;
            PhieuDeNghiModel.maQuiTrinhDuyet = dataPhieuCongTac.maQuiTrinhDuyet ?? 0;
            PhieuDeNghiModel.NhanVienLapPhieu = new BatDongSan.Models.NhanSu.NhanVienModel
            {
                maNhanVien = dataPhieuCongTac.nguoiLap,
                hoVaTen = HoVaTen(dataPhieuCongTac.nguoiLap)
            };

            PhieuDeNghiModel.NhanVien = new BatDongSan.Models.NhanSu.NhanVienModel
            {
                maNhanVien = dataPhieuCongTac.maNhanVien,
                hoVaTen = HoVaTen(dataPhieuCongTac.maNhanVien)
            };
            PhieuDeNghiModel.soTien = (decimal?)dataPhieuCongTac.soTien;
            PhieuDeNghiModel.tamUngThang = dataPhieuCongTac.tamUngThang;
            PhieuDeNghiModel.tamUngNam = dataPhieuCongTac.tamUngNam;
            PhieuDeNghiModel.ghiChu = dataPhieuCongTac.ghiChu;

            builtNam(dataPhieuCongTac.tamUngNam ?? 0);
            builtThang(dataPhieuCongTac.tamUngThang ?? 0);
            DMNguoiDuyetController nd = new DMNguoiDuyetController();
            PhieuDeNghiModel.Duyet = nd.GetDetailByMaPhieuTheoQuiTrinhDong(PhieuDeNghiModel.maPhieu, PhieuDeNghiModel.maQuiTrinhDuyet);
            LinqHeThongDataContext ht = new LinqHeThongDataContext();
            string hoTen = (ht.GetTable<tbl_NS_NhanVien>().Where(d => d.maNhanVien == GetUser().manv).Select(d => d.ho).FirstOrDefault() ?? string.Empty) + " " + (ht.GetTable<tbl_NS_NhanVien>().Where(d => d.maNhanVien == GetUser().manv).Select(d => d.ten).FirstOrDefault() ?? string.Empty);
            ViewBag.HoTen = hoTen;
            int trangThaiHT = (int?)ht.tbl_HT_DMNguoiDuyets.OrderByDescending(d => d.ID).Where(d => d.maPhieu == id).Select(d => d.trangThai).FirstOrDefault() ?? 0;
            ViewBag.TenTrangThaiDuyet = ht.tbl_HT_QuiTrinhDuyet_BuocDuyets.Where(d => d.id == trangThaiHT).Select(d => d.maBuocDuyet).FirstOrDefault() ?? string.Empty;
            ViewBag.URL = Request.Url.AbsoluteUri.ToString();
            var maBuocDuyet = ht.tbl_HT_DMNguoiDuyets.OrderByDescending(d => d.ID).Where(d => d.maPhieu == id).FirstOrDefault();
            ViewBag.TenBuocDuyet = ht.tbl_HT_QuiTrinhDuyet_BuocDuyets.Where(d => d.id == (maBuocDuyet == null ? 0 : maBuocDuyet.trangThai)).Select(d => d.maBuocDuyet).FirstOrDefault() ?? string.Empty;
            return View(PhieuDeNghiModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(string id, FormCollection coll)
        {
            try
            {
                tblPhieuDeNghi = linqNS.tbl_NS_PhieuTamUngs.Where(d => d.soPhieu == id).FirstOrDefault();
                tblPhieuDeNghi.ngayTamUng = String.IsNullOrEmpty(coll.Get("ngayTamUng")) ? (DateTime?)null : DateTime.ParseExact(coll.Get("ngayTamUng"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                tblPhieuDeNghi.tamUngThang = Convert.ToInt32(coll.Get("thang"));
                tblPhieuDeNghi.tamUngNam = Convert.ToInt32(coll.Get("nam"));
                tblPhieuDeNghi.soTien = Convert.ToDouble(coll.Get("soTien"));
                tblPhieuDeNghi.ghiChu = coll.Get("ghiChu");
                linqNS.SubmitChanges();

                return RedirectToAction("Edit", new { id = tblPhieuDeNghi.soPhieu });
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }


        public ActionResult Details(string id)
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            var dataPhieuCongTac = linqNS.tbl_NS_PhieuTamUngs.Where(d => d.soPhieu == id).FirstOrDefault();
            PhieuDeNghiModel = new PhieuTamUng();
            PhieuDeNghiModel.ngayLap = dataPhieuCongTac.ngayLap;
            PhieuDeNghiModel.nguoiLap = dataPhieuCongTac.nguoiLap;
            PhieuDeNghiModel.maPhieu = dataPhieuCongTac.soPhieu;
            PhieuDeNghiModel.ngayTamUng = dataPhieuCongTac.ngayTamUng;
            PhieuDeNghiModel.maQuiTrinhDuyet = dataPhieuCongTac.maQuiTrinhDuyet ?? 0;
            PhieuDeNghiModel.NhanVienLapPhieu = new BatDongSan.Models.NhanSu.NhanVienModel
            {
                maNhanVien = dataPhieuCongTac.nguoiLap,
                hoVaTen = HoVaTen(dataPhieuCongTac.nguoiLap)
            };

            PhieuDeNghiModel.NhanVien = new BatDongSan.Models.NhanSu.NhanVienModel
            {
                maNhanVien = dataPhieuCongTac.maNhanVien,
                hoVaTen = HoVaTen(dataPhieuCongTac.maNhanVien)
            };
            PhieuDeNghiModel.tamUngThang = dataPhieuCongTac.tamUngThang;
            PhieuDeNghiModel.tamUngNam = dataPhieuCongTac.tamUngNam;
            PhieuDeNghiModel.ngayLap = dataPhieuCongTac.ngayLap;
            PhieuDeNghiModel.ghiChu = dataPhieuCongTac.ghiChu;
            PhieuDeNghiModel.soTien = Convert.ToDecimal(dataPhieuCongTac.soTien);
            DMNguoiDuyetController nd = new DMNguoiDuyetController();
            PhieuDeNghiModel.Duyet = nd.GetDetailByMaPhieuTheoQuiTrinhDong(PhieuDeNghiModel.maPhieu, PhieuDeNghiModel.maQuiTrinhDuyet);
            LinqHeThongDataContext ht = new LinqHeThongDataContext();
            string hoTen = (ht.GetTable<tbl_NS_NhanVien>().Where(d => d.maNhanVien == GetUser().manv).Select(d => d.ho).FirstOrDefault() ?? string.Empty) + " " + (ht.GetTable<tbl_NS_NhanVien>().Where(d => d.maNhanVien == GetUser().manv).Select(d => d.ten).FirstOrDefault() ?? string.Empty);
            ViewBag.HoTen = hoTen;
            int trangThaiHT = (int?)ht.tbl_HT_DMNguoiDuyets.OrderByDescending(d => d.ID).Where(d => d.maPhieu == id).Select(d => d.trangThai).FirstOrDefault() ?? 0;
            ViewBag.TenTrangThaiDuyet = ht.tbl_HT_QuiTrinhDuyet_BuocDuyets.Where(d => d.id == trangThaiHT).Select(d => d.maBuocDuyet).FirstOrDefault() ?? string.Empty;
            ViewBag.URL = Request.Url.AbsoluteUri.ToString();
            var maBuocDuyet = ht.tbl_HT_DMNguoiDuyets.OrderByDescending(d => d.ID).Where(d => d.maPhieu == id).FirstOrDefault();
            ViewBag.TenBuocDuyet = ht.tbl_HT_QuiTrinhDuyet_BuocDuyets.Where(d => d.id == (maBuocDuyet == null ? 0 : maBuocDuyet.trangThai)).Select(d => d.maBuocDuyet).FirstOrDefault() ?? string.Empty;
            return View(PhieuDeNghiModel);
        }
        public FileResult DownloadImportFile()
        {
            string savedFileName = Path.Combine("/UploadFiles/Template/", "PhieuNhapThongTinTamUng.xlsx");
            return File(savedFileName, "multipart/form-data", "PhieuNhapThongTinTamUng.xlsx");
        }
        public ActionResult ImportExcelData(string excelPath)
        {
            try
            {
                string[] supportedFiles = { ".xlsx", ".xls" };
                HttpPostedFileBase File;
                File = Request.Files[0];
                if (File.ContentLength > 0)
                {
                    string extension = Path.GetExtension(File.FileName);
                    bool exist = Array.Exists(supportedFiles, element => element == extension);
                    if (exist == false)
                    {
                        return Json(new { success = false });
                    }
                    else
                    {
                        var date = DateTime.Now.ToString("yyyyMMdd-HHMMss");
                        string savedLocation = "/UploadFiles/PhieuTamUng/";
                        Directory.CreateDirectory(savedLocation);
                        var filePath = Server.MapPath(savedLocation);
                        string fileName = date.ToString() + File.FileName;
                        string savedFileName = Path.Combine(filePath, fileName);
                        File.SaveAs(savedFileName);

                        ExcelDataProcessing excelDataProcessor = new ExcelDataProcessing(savedFileName);
                        DataTable dt = excelDataProcessor.GetDataTableWorkSheet("PhieuTamUng");
                        // IList<tbl_NS_PhieuTamUng> tamUngImports= new List<tbl_NS_PhieuTamUng>();

                        foreach (DataRow row in dt.Rows)
                        {
                            if (String.IsNullOrEmpty(row["Mã nhân viên"].ToString()))
                                break;
                            //Check if ma nhan vien
                            var checkMaNV = linqNS.tbl_NS_PhieuTamUngs.Where(d => d.maNhanVien == row["Mã nhân viên"].ToString()).FirstOrDefault();
                            if (checkMaNV == null)
                            {
                                tbl_NS_PhieuTamUng tamUngImport = new tbl_NS_PhieuTamUng();
                                tamUngImport.maNhanVien = row["Mã nhân viên"].ToString();
                                tamUngImport.soPhieu = GenerateUtil.CheckLetter("DNTU", GetMax());
                                tamUngImport.nguoiLap = GetUser().manv;
                                tamUngImport.ngayTamUng = DateTime.Now;
                                tamUngImport.ngayLap = DateTime.Now;
                                tamUngImport.tamUngThang = String.IsNullOrEmpty(row["Tháng"].ToString()) ? 0 : Convert.ToInt32(row["Tháng"]);
                                tamUngImport.tamUngNam = String.IsNullOrEmpty(row["Năm"].ToString()) ? 0 : Convert.ToInt32(row["Năm"]);
                                tamUngImport.soTien = String.IsNullOrEmpty(row["Số tiền"].ToString()) ? 0 : Convert.ToDouble(row["Số tiền"]);
                                tamUngImport.ghiChu = row["Ghi chú"].ToString();
                                linqNS.tbl_NS_PhieuTamUngs.InsertOnSubmit(tamUngImport);
                                linqNS.SubmitChanges();
                            }

                        }

                        System.IO.File.Delete(Server.MapPath("/UploadFiles/PhieuTamUng/" + fileName));
                    }
                }
                SaveActiveHistory("Import danh sách phiếu tạm ứng");
                return Json(new { success = true });
            }
            catch
            {
                return View();
            }
        }

        public ActionResult ChonNhanVien()
        {
            StringBuilder buildTree = new StringBuilder();
            var phongBans = linqDM.tbl_DM_PhongBans.ToList();
            buildTree = TreePhongBans.BuildTreeDepartment(phongBans);
            ViewBag.NVPB = buildTree.ToString();
            return PartialView("_ChonNhanVien");
        }
        public ActionResult LoadNhanVien(int? page, string searchString, string maPhongBan)
        {
            IList<sp_PB_DanhSachNhanVienResult> phongBan1s;
            phongBan1s = linqDM.sp_PB_DanhSachNhanVien(searchString, maPhongBan).ToList();
            ViewBag.isGet = "True";
            int currentPageIndex = page.HasValue ? page.Value : 1;
            ViewBag.Count = phongBan1s.Count();
            ViewBag.Search = searchString;
            ViewBag.MaPhongBan = maPhongBan;
            return PartialView("_LoadNhanVien", phongBan1s.ToPagedList(currentPageIndex, 10));
        }

        public void buitlLoaiThuNhapKhac(string select)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            var loaiThuNhapKhac = linqDM.tbl_DM_ThuNhapKhacs.ToList();

            dict.Add("", "[Chọn loại thu nhập]");
            foreach (var item in loaiThuNhapKhac)
            {
                dict.Add(item.maLoaiThuNhapKhac.ToString(), item.tenLoaiThuNhapKhac);
            }
            ViewBag.loaiThuNhapKhac = new SelectList(dict, "Key", "Value", select);

        }
        public void builtThang(int thang)
        {
            Dictionary<int, int> dict = new Dictionary<int, int>();
            for (int i = 1; i <= 12; i++)
            {
                dict.Add(i, i);
            }
            ViewBag.Thangs = new SelectList(dict, "Key", "Value", thang);
        }
        public void builtNam(int nam)
        {
            Dictionary<int, int> dict = new Dictionary<int, int>();
            for (int i = DateTime.Now.Year - 5; i <= DateTime.Now.Year + 5; i++)
            {
                dict.Add(i, i);
            }
            ViewBag.Nams = new SelectList(dict, "Key", "Value", nam);
        }
        public string HoVaTen(string MaNV)
        {

            return linqNS.tbl_NS_NhanViens.Where(d => d.maNhanVien == MaNV).Select(d => d.ho + " " + d.ten).FirstOrDefault();
        }
        public ActionResult ChonPhongBan()
        {
            StringBuilder buildTree = new StringBuilder();
            IList<tbl_DM_PhongBan> phongBans = linqDM.tbl_DM_PhongBans.ToList();
            buildTree = TreePhongBans.BuildTreeDepartment(phongBans);
            ViewBag.PhongBan = buildTree.ToString();
            return PartialView("_ChonPhongBan");
        }
        private void thang(int value)
        {
            Dictionary<int, string> dics = new Dictionary<int, string>();
            for (int i = 1; i < 13; i++)
            {
                dics[i] = i.ToString();
            }
            ViewData["thang"] = new SelectList(dics, "Key", "Value", value);
            ViewData["thangtc"] = new SelectList(dics, "Key", "Value", value);
        }
        private void nam(int value)
        {
            Dictionary<int, string> dics = new Dictionary<int, string>();
            for (int i = (DateTime.Now.Year - 5); i < (DateTime.Now.Year + 10); i++)
            {
                dics[i] = i.ToString();
            }
            ViewData["nam"] = new SelectList(dics, "Key", "Value", value);
            ViewData["namtc"] = new SelectList(dics, "Key", "Value", value);
        }
        public ActionResult XacNhan(string id)
        {

            return Json(string.Empty);
        }

        #region Duyệt qui trình động
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ViewsApproval(string id)
        {
            try
            {
                return RedirectToAction("Details", new { id = id });// detail de duyet
            }
            catch
            {
                return View();
            }
        }


        /// <summary>
        /// Duyệt theo qui trình động phiếu nghỉ phép
        /// </summary>
        /// <param name="maPhieu"></param>
        /// <param name="maQuiTrinh"></param>
        /// <param name="sendMail"></param>
        /// <param name="sendSMS"></param>
        /// <returns></returns>
        public ActionResult DuyetTheoQuiTrinhDong(string maPhieu, string maCongViec, bool sendMail, bool sendSMS, string maNhanVien, int soNgayNghiPhep, string lyDo, int idQuiTrinh)
        {
            try
            {
                LinqHeThongDataContext ht = new LinqHeThongDataContext();
                string hoTen = (ht.GetTable<tbl_NS_NhanVien>().Where(d => d.maNhanVien == GetUser().manv).Select(d => d.ho).FirstOrDefault() ?? string.Empty) + " " + (ht.GetTable<tbl_NS_NhanVien>().Where(d => d.maNhanVien == GetUser().manv).Select(d => d.ten).FirstOrDefault() ?? string.Empty);
                DMNguoiDuyetController _nguoiDuyet = new DMNguoiDuyetController();
                var kq = _nguoiDuyet.DuyeTheoQuiTrinhDong(maPhieu, maCongViec, sendMail, sendSMS, GetUser().manv, hoTen, soNgayNghiPhep, lyDo, idQuiTrinh, string.Empty);
                return kq;
            }
            catch (Exception e)
            {
                return Json("Lỗi: " + e.Message);
            }
        }
        #endregion


        //Chi Luong tạm ứng
        public ActionResult CreateCL(int? thang, int? nam, string congTrinh)
        {
            DeNghiChiLuong deNghiCL = new DeNghiChiLuong();
            deNghiCL.soPhieu = CheckLetterDNCL("DNCL", GetMaxDeNghiChiLuongTV(), 3);

            deNghiCL.thang = thang;
            deNghiCL.nam = nam;
            deNghiCL.maNguoiLap = GetUser().manv;
            deNghiCL.tenNguoiLap = HoVaTen(GetUser().manv);
            deNghiCL.ngayLap = DateTime.Now;
            var chiTietLuong = (from p in linqNS.sp_NS_TamUngLuong(congTrinh, thang, nam)
                                select new DeNghiChiLuongChiTiet
                                {
                                    BHXH = 0,
                                    ChuyenKhoan = p.thucLanh,
                                    LuongThang = 0,
                                    TenBoPhanTinhLuong = p.phongBan,
                                    ThueTNCN = 0,
                                    TruyLanhTruyThu = 0,
                                    TruyLanhTruyThuBaoHiem = 0,
                                    TruyLanhTruyThuThue = 0,
                                    TruyLanhTruyThuTamUng = 0,
                                    PhuCapCT = 0,
                                    PhuCapKhac = 0,

                                }).ToList();
            deNghiCL.DeNghiChiLuongChiTiet = chiTietLuong;
            deNghiCL.maCongTrinh = congTrinh;
            return View(deNghiCL);
        }
        [HttpPost]
        public ActionResult CreateCL(FormCollection coll)
        {
            try
            {
                var maCongTrinh = coll.Get("maCongTrinh");
                TaoDeNghiChiLuongTV(coll);

                return RedirectToAction("Index", "PhieuTamUng");
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
        public string GetMaxDeNghiChiLuongTV()
        {
            return lqThuanViet.tbl_DeNghiChiLuongs.OrderByDescending(d => d.maPhieu).Select(d => d.maPhieu).FirstOrDefault();
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

        #region Xác nhận thu nhập khác
        public JsonResult XacNhanPhieuTamUng(string maPhieu)
        {
            try
            {
                var updatePN = linqNS.tbl_NS_PhieuTamUngs.Where(d => d.soPhieu == maPhieu).FirstOrDefault();
                if (updatePN != null)
                {
                    updatePN.maQuiTrinhDuyet = 1;
                    linqNS.SubmitChanges();
                }
                return Json(string.Empty);
            }
            catch
            {
                return Json("Error");
            }
        }
        #endregion
    }
}
