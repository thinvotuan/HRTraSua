using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using BatDongSan.Helper.Common;
using BatDongSan.Helper.Utils;
using BatDongSan.Models.DanhMuc;
using BatDongSan.Models.HeThong;
using BatDongSan.Models.NhanSu;
using BatDongSan.Utils.Paging;
using System.IO;
using System.Text.RegularExpressions;
using BatDongSan.Helper;

namespace BatDongSan.Controllers.NhanSu
{
    public class QuanLyThongBaoController : ApplicationController
    {
        private LinqHeThongDataContext contentHT = new LinqHeThongDataContext();
        private LinqDanhMucDataContext contextDM = new LinqDanhMucDataContext();
        private LinqNhanSuDataContext contextNS = new LinqNhanSuDataContext();
        private IList<sp_ThongBao_IndexResult> thongBaos;
        private IList<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan> phongBans;
        List<tbl_FileDinhKem> lstFile = new List<tbl_FileDinhKem>();
        private StringBuilder buildTree;
        private int defaultPageSize = 20;
        private readonly string MCV = "QuanLyThongBao";
        private bool? permission;
        //
        // GET: /ThongBao/

        public ActionResult Index(int? page, int? pageSize, string searchString)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            try
            {
                buildTree = new StringBuilder();
                phongBans = contextDM.GetTable<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan>().ToList();
                buildTree = TreePhongBanStyle.BuildTreeDepartment(phongBans);
                ViewBag.PhongBans = buildTree.ToString();

                var getPhongBan = contextNS.tbl_NS_NhanVienPhongBans.Where(d => d.maNhanVien == GetUser().manv).OrderByDescending(d => d.ngayLap).Select(d => d.maPhongBan).FirstOrDefault();

                LinqHeThongDataContext contentHT = new LinqHeThongDataContext();
                BatDongSan.Models.NhanSuService.NSServiceDataContext nsservice = new BatDongSan.Models.NhanSuService.NSServiceDataContext();
                var userThongBaos = nsservice.Sys_EmailGuiThongBaos.Where(d => d.maNhanVien == GetUser().manv).Select(d => d.idThongBao).Distinct().ToList();
                var TbDaXem = contentHT.Sys_User_ThongBaos.Where(d => d.manv == GetUser().manv).Select(d => d.idThongBao).Distinct().ToList();
                var listTB = (from p in contentHT.Sys_ThongBaos
                              where (userThongBaos.Contains(p.id) && !TbDaXem.Contains(p.id))
                              select p).Count();
                Session["listThongBao"] = listTB;

                int currentPageIndex = page.HasValue ? page.Value : 1;
                defaultPageSize = pageSize ?? defaultPageSize;
                int? tongSoDong = 0;
                TempData["Params"] = pageSize + "," + searchString;
                thongBaos = contentHT.sp_ThongBao_Index(searchString, currentPageIndex, defaultPageSize).ToList();

                try
                {
                    ViewBag.Count = thongBaos[0].tongSoDong;
                    tongSoDong = thongBaos[0].tongSoDong;
                }
                catch
                {
                    ViewBag.Count = 0;
                }

                if (Request.IsAjaxRequest())
                {
                    return PartialView("PartialIndex", thongBaos.ToPagedList(currentPageIndex, defaultPageSize, true, tongSoDong));
                }
                return View(thongBaos.ToPagedList(currentPageIndex, defaultPageSize, true, tongSoDong));
            }
            catch (Exception ex)
            {

                ViewBag.Message = ex.Message;
                return View("Error");
            }
        }
        public ActionResult IndexView(int? id, bool? msg)
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
                //var getPhongBan = contextNS.tbl_NS_NhanVienPhongBans.Where(d => d.maNhanVien == GetUser().manv).OrderByDescending(d => d.ngayLap).Select(d => d.maPhongBan).FirstOrDefault();

                //var listThongBao = contentHT.sp_Sys_User_ThongBao(GetUser().manv, getPhongBan).ToList();
                //ViewBag.lsThongBao = listThongBao;
                BatDongSan.Models.NhanSuService.NSServiceDataContext nsservice = new BatDongSan.Models.NhanSuService.NSServiceDataContext();
                var userThongBaos = nsservice.Sys_EmailGuiThongBaos.Where(d => d.maNhanVien == GetUser().manv).Select(d => d.idThongBao).Distinct().ToList();
                List<UserThongBaoModel> listTB = new List<UserThongBaoModel>();
                listTB = (from p in contentHT.Sys_ThongBaos
                          where userThongBaos.Contains(p.id)
                          join q in contentHT.Sys_User_ThongBaos on p.id equals q.idThongBao into giatri
                          from gt in giatri.DefaultIfEmpty()
                          select new UserThongBaoModel
                          {
                              id = p.id,
                              tenThongBao = p.tenThongBao,
                              trangThaiXem = gt.idThongBao
                          }
                                 ).Distinct().ToList();

                return View("IndexView", listTB);
            }
            catch (Exception ex)
            {

                ViewBag.Message = ex.Message;
                return View("Error");
            }
        }

        //
        // GET: /ThongBao/Details/5


        public ActionResult Details(int id, bool? msg)
        {

            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            try
            {
                buildTree = new StringBuilder();
                phongBans = contextDM.GetTable<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan>().ToList();
                buildTree = TreePhongBanStyle.BuildTreeDepartment(phongBans);
                ViewBag.PhongBans = buildTree.ToString();



                if (msg == true)
                {
                    ViewBag.Message = "Cập nhật dữ liệu thành công";
                }
                else
                {
                    ViewBag.Message = string.Empty;
                }
                Sys_ThongBao thongBao = new Sys_ThongBao();

                thongBao = contentHT.Sys_ThongBaos.Where(d => d.id.Equals(id)).FirstOrDefault();
                if (thongBao != null)
                {
                    ViewBag.data = thongBao;
                    lstFile = contextNS.tbl_FileDinhKems.Where(d => d.identification == Convert.ToString(id)).ToList();
                    List<NhanVienModel> NhanViens = new List<NhanVienModel>();
                    var nhanViens = contextNS.tbl_NS_NhanViens.OrderBy(d => d.ten).Where(d => d.trangThai == 0).ToList();
                    ViewBag.nhanViens = nhanViens;
                    Sys_ThongBao data = new Sys_ThongBao();

                    var capBacChucDanh = contextDM.tbl_DM_CapBacChucDanhs.OrderBy(d => d.soCapBac).ToList();

                    ViewBag.capBacChucDanh = capBacChucDanh;
                    List<PhongBanModel> PBs = new List<PhongBanModel>();
                    if (thongBao.listPhongBan != null)
                    {

                        var listPB = thongBao.listPhongBan.ToString().Split(',');

                        PBs = (from p in contextDM.tbl_DM_PhongBans
                               where listPB.Contains(p.maPhongBan)
                               select new PhongBanModel
                               {
                                   MaPhongBan = p.maPhongBan,
                                   Ten = p.tenPhongBan
                               }).ToList();

                    }
                    ViewBag.PBs = PBs;
                    return View(lstFile);
                }
                else
                {
                    ViewData["Message"] = "Error";
                    return View("Error");
                }
            }
            catch (Exception ex)
            {

                ViewBag.Message = ex.Message;
                return View("Error");
            }
        }
        public ActionResult DetailsView(int id, bool? msg)
        {

            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXemChiTiet);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            try
            {
                if (msg == true)
                {
                    ViewBag.Message = "Cập nhật dữ liệu thành công";
                }
                else
                {
                    ViewBag.Message = string.Empty;
                }
                Sys_ThongBao thongBao = new Sys_ThongBao();

                thongBao = contentHT.Sys_ThongBaos.Where(d => d.id.Equals(id)).FirstOrDefault();
                if (thongBao != null)
                {
                    ViewBag.data = thongBao;

                    return View();
                }
                else
                {
                    ViewData["Message"] = "Error";
                    return View("Error");
                }
            }
            catch (Exception ex)
            {

                ViewBag.Message = ex.Message;
                return View("Error");
            }
        }
        //
        // GET: /ThongBao/Create

        public ActionResult Create()
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenThem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            try
            {
                buildTree = new StringBuilder();
                phongBans = contextDM.GetTable<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan>().ToList();
                buildTree = TreePhongBanStyle.BuildTreeDepartment(phongBans);
                ViewBag.PhongBans = buildTree.ToString();
                List<NhanVienModel> NhanViens = new List<NhanVienModel>();
                var nhanViens = from nv in contextNS.tbl_NS_NhanViens
                                orderby nv.ten
                                where (nv.trangThai == 0)
                                select new NhanVienModel
                                {
                                    maNhanVien = nv.maNhanVien,

                                    hoVaTen = nv.ho + " " + nv.ten
                                };
                NhanViens = nhanViens.ToList();
                ViewData["Sender"] = new SelectList(NhanViens, "maNhanVien", "hoVaTen", String.Empty);
                Sys_ThongBao data = new Sys_ThongBao();

                var capBacChucDanh = from cp in contextDM.tbl_DM_CapBacChucDanhs
                                     orderby cp.soCapBac
                                     select new
                                     {
                                         maCapBac = Convert.ToString(cp.soCapBac),
                                         tenCapBac = cp.tenCapBac
                                     };
                ViewData["capBacCDS"] = new SelectList(capBacChucDanh, "maCapBac", "tenCapBac", String.Empty);

                return View("");
            }
            catch (Exception ex)
            {

                ViewBag.Message = ex.Message;
                return View("Error");
            }
        }

        //
        // POST: /ThongBao/Create
       
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Create(FormCollection collection, IEnumerable<HttpPostedFileBase> fileDinhKems)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenThem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            try
            {
                // TODO: Add insert logic here

                Sys_ThongBao thongBao = new Sys_ThongBao();
                thongBao.tenThongBao = collection.Get("tenThongBao");
                thongBao.titleMail = collection.Get("titleMail");
                thongBao.noiDungThongBao = collection.Get("noiDungThongBao");
                thongBao.noiDungSMS = collection.Get("noiDungSMS");
                thongBao.maPhongBan = collection["maPhongBan"];
                thongBao.listCapBac = collection["ListCapBac"];
                thongBao.listPhongBan = collection["lstPhongBanSeleted"];
                thongBao.listNhanVien = collection["ListNhanVien"];
                thongBao.ngayLap = DateTime.Now;
                thongBao.nguoiLap = GetUser().manv;
                contentHT.Sys_ThongBaos.InsertOnSubmit(thongBao);
                contentHT.SubmitChanges();
                int id = contentHT.Sys_ThongBaos.OrderByDescending(d => d.id).FirstOrDefault().id;
                FileUploading(collection, fileDinhKems, id);
                return RedirectToAction("Edit/", new { id = id });

            }



            catch (Exception ex)
            {

                ViewBag.Message = ex.Message;
                return View("Error");
            }
        }



        //
        // GET: /ThongBao/Edit/5

        public ActionResult Edit(int id, bool? msg)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            try
            {
                buildTree = new StringBuilder();
                phongBans = contextDM.GetTable<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan>().ToList();
                buildTree = TreePhongBanStyle.BuildTreeDepartment(phongBans);
                ViewBag.PhongBans = buildTree.ToString();

                ViewBag.Message = "Đã cập nhật giữ liệu thành công.";


                Sys_ThongBao thongBao = new Sys_ThongBao();

                thongBao = contentHT.Sys_ThongBaos.Where(d => d.id.Equals(id)).FirstOrDefault();
                if (thongBao != null)
                {
                    ViewBag.data = thongBao;
                    lstFile = contextNS.tbl_FileDinhKems.Where(d => d.identification == Convert.ToString(id)).ToList();
                    List<NhanVienModel> NhanViens = new List<NhanVienModel>();
                    var nhanViens = contextNS.tbl_NS_NhanViens.OrderBy(d => d.ten).Where(d => d.trangThai == 0).ToList();
                    ViewBag.nhanViens = nhanViens;
                    Sys_ThongBao data = new Sys_ThongBao();

                    var capBacChucDanh = contextDM.tbl_DM_CapBacChucDanhs.OrderBy(d => d.soCapBac).ToList();

                    ViewBag.capBacChucDanh = capBacChucDanh;
                    List<PhongBanModel> PBs = new List<PhongBanModel>();
                    if (thongBao.listPhongBan != null)
                    {

                        var listPB = thongBao.listPhongBan.ToString().Split(',');

                        PBs = (from p in contextDM.tbl_DM_PhongBans
                               where listPB.Contains(p.maPhongBan)
                               select new PhongBanModel
                               {
                                   MaPhongBan = p.maPhongBan,
                                   Ten = p.tenPhongBan
                               }).ToList();

                    }
                    ViewBag.PBs = PBs;
                    return View(lstFile);
                }
                else
                {
                    ViewData["Message"] = "Error";
                    return View("Error");
                }
            }
            catch (Exception ex)
            {

                ViewBag.Message = ex.Message;
                return View("Error");
            }
        }

        //
        // POST: /ThongBao/Edit/5

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Edit(int id, FormCollection collection, IEnumerable<HttpPostedFileBase> fileDinhKems)
        {
            try
            {
                // TODO: Add update logic here

                Sys_ThongBao thongBao = new Sys_ThongBao();
                thongBao = contentHT.Sys_ThongBaos.Where(d => d.id.Equals(id)).FirstOrDefault();
                thongBao.tenThongBao = collection.Get("tenThongBao");
                thongBao.titleMail = collection.Get("titleMail");
                thongBao.noiDungThongBao = collection.Get("noiDungThongBao");
                thongBao.noiDungSMS = collection.Get("noiDungSMS");
                thongBao.maPhongBan = collection["maPhongBan"];
                thongBao.listCapBac = collection["ListCapBac"];
                thongBao.listPhongBan = collection["lstPhongBanSeleted"];
                thongBao.listNhanVien = collection["ListNhanVien"];
                contentHT.SubmitChanges();
                FileUploading(collection, fileDinhKems, id);
                return RedirectToAction("Edit/" + id, new { msg = true });
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("Error");
            }
        }
        [HttpPost]
        public ActionResult DeleteFile(int id)
        {
            try
            {
                var fileDinhkem = contextNS.tbl_FileDinhKems.Where(s => s.id == id).FirstOrDefault();
                contextNS.tbl_FileDinhKems.DeleteOnSubmit(fileDinhkem);
                contextNS.SubmitChanges();
                System.IO.File.Delete(fileDinhkem.taiLieuURL);
                return Json(String.Empty);
            }
            catch
            {
                return View();
            }
        }

        private bool FileUploading(FormCollection collection, IEnumerable<HttpPostedFileBase> files, int id)
        {
            try
            {
                files = files.Where(s => s != null).OrderBy(o => o.FileName);
                string[] nameacceptable = collection.GetValues("nameaccept");
                string[] thumbnails = collection.GetValues("thumbnail");
                List<tbl_FileDinhKem> taiLieus = new List<tbl_FileDinhKem>();
                foreach (var file in files)
                {
                    if (file != null)
                    {
                        tbl_FileDinhKem taiLieu = new tbl_FileDinhKem();
                        var date = DateTime.Now.ToString("yyyyMMdd-HHMMss");
                        string filePath = "/UploadFiles/MThongBao/";

                        //Tạo tên mới cho file upload
                        string Generatedname = date.ToString() + file.FileName;
                        Directory.CreateDirectory(filePath);
                        var filePathOriginal = Server.MapPath(filePath);
                        if (nameacceptable.Contains(file.FileName))
                        {
                            string savedFileName = Path.Combine(filePathOriginal, Generatedname);
                            taiLieu.controller = GenerateUtil.GetRouteData().Controller;// Get Tên controller
                            taiLieu.Action = GenerateUtil.GetRouteData().Action; //Get Tên Action
                            taiLieu.savedFileName = date.ToString() + file.FileName;
                            taiLieu.maNguoiUpLoad = GetUser().manv;
                            taiLieu.ngayLap = DateTime.Now;
                            taiLieu.tenNguoiUpLoad = (string)Session["TenNhanVien"];
                            taiLieu.originalFileName = file.FileName;
                            taiLieu.savedFileName = Generatedname;
                            taiLieu.taiLieuURL = filePathOriginal + Generatedname;
                            taiLieu.identification = Convert.ToString(id);
                            if (Array.Exists(thumbnails, s => s.Contains(file.FileName)) && file.ContentType.Contains("image") == false)
                            {
                                string thumbnail = thumbnails.Where(s => s.Contains(file.FileName)).First();
                                //Tách giá trị value thumbnail thành 2 phần 1-Tên file, 2-Tên đường dẫn thumbnail
                                taiLieu.thumbnailURL = Regex.Split(thumbnail, "-SplitPoint-").Last();
                            }
                            else
                            {
                                if (file.ContentType.Contains("image"))
                                {
                                    taiLieu.thumbnailURL = "/UploadFiles/MThongBao/" + Generatedname;
                                }
                            }
                            file.SaveAs(savedFileName);                            
                            taiLieus.Add(taiLieu);
                            int Index = Array.IndexOf(nameacceptable, file.FileName);
                            Array.Clear(nameacceptable, Index, 1);
                        }
                    }
                }
                contextNS.tbl_FileDinhKems.InsertAllOnSubmit(taiLieus);
                contextNS.SubmitChanges();
                return true;
            }
            catch
            {
                return false;
            }

        }
        public ActionResult Download(int id)
        {
            try
            {
                var taiLieu = contextNS.tbl_FileDinhKems.Where(s => s.id == id).FirstOrDefault();
                string savedFileName = Path.Combine("/UploadFiles/MThongBao/", taiLieu.savedFileName);
                return new DownloadResult { VirtualPath = savedFileName, FileDownloadName = taiLieu.originalFileName };
            }
            catch
            {
                return Json("");
            }
        }

        public IQueryable<Sys_ThongBao> FindAll()
        {

            var sql = from p in contentHT.Sys_ThongBaos
                      select p;
            return sql;
        }


        //
        // GET: /ThongBao/Delete/5

        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //
        // POST: /ThongBao/Delete/5

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Delete(int id)
        {
            try
            {
                // TODO: Add delete logic here

                Sys_ThongBao thongBao = new Sys_ThongBao();
                thongBao = contentHT.Sys_ThongBaos.Where(d => d.id.Equals(id)).FirstOrDefault();
                contentHT.Sys_ThongBaos.DeleteOnSubmit(thongBao);
                contentHT.SubmitChanges();

                var User_ThongBao = contentHT.Sys_User_ThongBaos.Where(d => d.idThongBao == id);
                contentHT.Sys_User_ThongBaos.DeleteAllOnSubmit(User_ThongBao);
                var deleteFile = contextNS.tbl_FileDinhKems.Where(d => d.identification == Convert.ToString(id)).ToList();
                contextNS.tbl_FileDinhKems.DeleteAllOnSubmit(deleteFile);
                var tblEmailTB = contentHT.Sys_EmailGuiThongBaos.Where(d => d.idThongBao == id).ToList();
                contentHT.Sys_EmailGuiThongBaos.DeleteAllOnSubmit(tblEmailTB);
                contextNS.SubmitChanges();
                contentHT.SubmitChanges();
            }
            catch (Exception ex)
            {

                return Json(ex.Message);
            }
            return Json(String.Empty);
        }
        public ActionResult GetThongTin(int id)
        {
            //Check with manv and idthongbao exist
            var exist = contentHT.Sys_User_ThongBaos.Where(d => d.idThongBao.Equals(id) && d.manv == GetUser().manv).FirstOrDefault();
            if (exist == null)
            {
                //
                Sys_User_ThongBao thongBao = new Sys_User_ThongBao();
                thongBao.idThongBao = id;
                thongBao.manv = GetUser().manv;
                contentHT.Sys_User_ThongBaos.InsertOnSubmit(thongBao);
                contentHT.SubmitChanges();
            }

            ViewBag.thongTin = contentHT.Sys_ThongBaos.Where(s => s.id == id).Select(d => d.noiDungThongBao).FirstOrDefault();
            var fileDinhKem = contextNS.tbl_FileDinhKems.Where(d => d.identification == Convert.ToString(id)).ToList();

            // Count thong bao
            BatDongSan.Models.NhanSuService.NSServiceDataContext nsservice = new BatDongSan.Models.NhanSuService.NSServiceDataContext();
            var userThongBaos = nsservice.Sys_EmailGuiThongBaos.Where(d => d.maNhanVien == GetUser().manv).Select(d => d.idThongBao).Distinct().ToList();
            var TbDaXem = contentHT.Sys_User_ThongBaos.Where(d => d.manv == GetUser().manv).Select(d => d.idThongBao).Distinct().ToList();
            var listTB = (from p in contentHT.Sys_ThongBaos
                          where (userThongBaos.Contains(p.id) && !TbDaXem.Contains(p.id))
                          select p).Count();
            Session["listThongBao"] = listTB;

            return PartialView("_PartialView", fileDinhKem);
        }

        #region Gửi email thông báo
        [ValidateInput(false)]
        public ActionResult SendEmailThongBao(int id)
        {
            try
            {
                StringBuilder content = new StringBuilder();
                Sys_ThongBao thongBao = contentHT.Sys_ThongBaos.Where(d => d.id == id).FirstOrDefault();

                List<Sys_EmailGuiThongBao> emailThongBaos = new List<Sys_EmailGuiThongBao>();
                List<string> nhanVienList = new List<string>();

                // Cap bac chuc danh
                if (thongBao.listPhongBan != null)
                {
                    var arrPB = thongBao.listPhongBan.Split(',').ToArray();
                    for (int j = 0; j < arrPB.Length; j++)
                    {
                        if (arrPB[j] != "")
                        {
                            List<string> nhanVienTam = new List<string>();
                            if (thongBao.listCapBac != null)
                            {
                                var arrCD = thongBao.listCapBac.Split(',').ToArray();
                                nhanVienTam = (from q in contentHT.sp_PB_DanhSachNhanVien_Tree("", Convert.ToString(arrPB[j]), 0, 0, 30000)
                                               where (arrCD.Contains(Convert.ToString(q.soCapBac)))
                                               select q.maNhanVien).ToList();
                            }
                            else
                            {
                                nhanVienTam = (from q in contentHT.sp_PB_DanhSachNhanVien_Tree("", Convert.ToString(arrPB[j]), 0, 0, 30000)
                                               select q.maNhanVien).ToList();
                            }
                            foreach (var itemTemp in nhanVienTam)
                            {
                                nhanVienList.Add(itemTemp);
                            }
                        }
                    }

                }
                if (thongBao.listNhanVien != null)
                {
                    var arrNV = thongBao.listNhanVien.Split(',').ToArray();
                    for (int k = 0; k < arrNV.Length; k++)
                    {
                        if (Convert.ToString(arrNV[k]).Length > 2)
                        {
                            nhanVienList.Add(Convert.ToString(arrNV[k]));
                        }
                    }
                }
                var lstNhanVienSendMail = (from p in contextNS.tbl_NS_NhanViens
                                           where nhanVienList.Contains(p.maNhanVien)
                                           where (p.trangThai == 0 && p.email != "" && p.email != null)
                                           select new NhanVienModel
                                           {
                                               maNhanVien = p.maNhanVien,
                                               hoVaTen = p.ho + " " + p.ten,
                                               email = p.email
                                           }
                                               ).Distinct().ToList();
                var listFile = contextNS.tbl_FileDinhKems.Where(d => d.identification == Convert.ToString(id)).Select(d => d.taiLieuURL).ToArray();
                var lstFileService = contextNS.tbl_FileDinhKems.Where(d => d.identification == Convert.ToString(id)).ToList();
                MailHelper mailInit = new MailHelper();// lay cac tham so trong webconfig
                BatDongSan.Models.NhanSuService.NSServiceDataContext hrservice = new BatDongSan.Models.NhanSuService.NSServiceDataContext();
                // delete TB
                var deleTB = hrservice.Sys_EmailGuiThongBaos.Where(d => d.idThongBao == id).ToList();
                hrservice.Sys_EmailGuiThongBaos.DeleteAllOnSubmit(deleTB);
                hrservice.SubmitChanges();
                // End
                foreach (var item in lstNhanVienSendMail)
                {
                    content.Clear();
                    //content.Append("<h3>Email từ hệ thống nhân sự</h3>");
                    content.Append(thongBao.noiDungThongBao);
                    //content.Append("<p style='font-style: italic;'>Thanks and Regards!</p>");
                    BatDongSan.Models.NhanSuService.Sys_EmailGuiThongBao emailThongBao = new BatDongSan.Models.NhanSuService.Sys_EmailGuiThongBao();
                    emailThongBao.idThongBao = id;
                    emailThongBao.ngayGui = DateTime.Now;
                    emailThongBao.nguoiGui = GetUser().manv;
                    emailThongBao.tieuDe = thongBao.tenThongBao ?? null;
                    emailThongBao.noiDung = content.ToString();
                    emailThongBao.emailGuiDen = item.email;
                    emailThongBao.maNhanVien = item.maNhanVien;
                    emailThongBao.titleMail = thongBao.titleMail;

                   // if (SendMailGeneral(thongBao.tenThongBao, item.email, emailThongBao.noiDung, listFile))
                   // {
                   //     emailThongBao.trangThaiGui = true;
                   // }
                   // else
                   // {
                   //     emailThongBao.trangThaiGui = null;
                   //}
                    emailThongBao.trangThaiGui = null;
                    hrservice.Sys_EmailGuiThongBaos.InsertOnSubmit(emailThongBao);
                    hrservice.SubmitChanges();
                    // Them vao file dinh kem
                    if (lstFileService != null && lstFileService.Count > 0)
                    {
                        // delete files
                        var listFileDK = hrservice.tbl_FileDinhKems.Where(d => d.identification == Convert.ToString(id)).ToList();
                        hrservice.tbl_FileDinhKems.DeleteAllOnSubmit(listFileDK);
                        hrservice.SubmitChanges();
                        // End
                        List<BatDongSan.Models.NhanSuService.tbl_FileDinhKem> lstFileDK = new List<Models.NhanSuService.tbl_FileDinhKem>();
                       BatDongSan.Models.NhanSuService.tbl_FileDinhKem tblDinhKem;

                        foreach (var itemFile in lstFileService)
                        {
                            tblDinhKem = new Models.NhanSuService.tbl_FileDinhKem();
                            tblDinhKem.controller = itemFile.controller;
                            tblDinhKem.Action = itemFile.Action;
                            tblDinhKem.identification = itemFile.identification;
                            tblDinhKem.originalFileName = itemFile.originalFileName;
                            tblDinhKem.savedFileName = itemFile.savedFileName;
                            tblDinhKem.maNguoiUpLoad = itemFile.maNguoiUpLoad;
                            tblDinhKem.tenNguoiUpLoad = itemFile.tenNguoiUpLoad;
                            tblDinhKem.ngayLap = itemFile.ngayLap ?? DateTime.Now;
                            tblDinhKem.taiLieuURL = itemFile.taiLieuURL;
                            tblDinhKem.thumbnailURL = itemFile.thumbnailURL;
                            lstFileDK.Add(tblDinhKem);
                        }
                        hrservice.tbl_FileDinhKems.InsertAllOnSubmit(lstFileDK);
                        hrservice.SubmitChanges();
                    }
                    // End them vao file dinh kem
                }
                thongBao.daGuiEmail = true;
                contentHT.SubmitChanges();

                return Json(string.Empty);
            }
            catch
            {
                return View();
            }
        }
        #endregion
        #region Gửi email thông báo
        [ValidateInput(false)]
        public ActionResult SendEmailThongBaoBi(int id)
        {
            try
            {
                StringBuilder content = new StringBuilder();
                Sys_ThongBao thongBao = contentHT.Sys_ThongBaos.Where(d => d.id == id).FirstOrDefault();

                List<Sys_EmailGuiThongBao> emailThongBaos = new List<Sys_EmailGuiThongBao>();
                List<string> nhanVienList = new List<string>();

               
              
                            nhanVienList.Add(Convert.ToString("NV-1604744"));
                       
                var lstNhanVienSendMail = (from p in contextNS.tbl_NS_NhanViens
                                           where nhanVienList.Contains(p.maNhanVien)
                                           where (p.trangThai == 0 && p.email != "" && p.email != null)
                                           select new NhanVienModel
                                           {
                                               maNhanVien = p.maNhanVien,
                                               hoVaTen = p.ho + " " + p.ten,
                                               email = p.email
                                           }
                                               ).Distinct().Where(d=>d.maNhanVien == "NV-1604744").ToList();
                var listFile = contextNS.tbl_FileDinhKems.Where(d => d.identification == Convert.ToString(id)).Select(d => d.taiLieuURL).ToArray();
                var lstFileService = contextNS.tbl_FileDinhKems.Where(d => d.identification == Convert.ToString(id)).ToList();
                MailHelper mailInit = new MailHelper();// lay cac tham so trong webconfig
                BatDongSan.Models.NhanSuService.NSServiceDataContext hrservice = new BatDongSan.Models.NhanSuService.NSServiceDataContext();
                // delete TB
                var deleTB = hrservice.Sys_EmailGuiThongBaos.Where(d => d.idThongBao == id).ToList();
                hrservice.Sys_EmailGuiThongBaos.DeleteAllOnSubmit(deleTB);
                hrservice.SubmitChanges();
                // End
                foreach (var item in lstNhanVienSendMail)
                {
                    content.Clear();
                    //content.Append("<h3>Email từ hệ thống nhân sự</h3>");
                    content.Append(thongBao.noiDungThongBao);
                    //content.Append("<p style='font-style: italic;'>Thanks and Regards!</p>");
                    BatDongSan.Models.NhanSuService.Sys_EmailGuiThongBao emailThongBao = new BatDongSan.Models.NhanSuService.Sys_EmailGuiThongBao();
                    emailThongBao.idThongBao = id;
                    emailThongBao.ngayGui = DateTime.Now;
                    emailThongBao.nguoiGui = GetUser().manv;
                    emailThongBao.tieuDe = thongBao.tenThongBao ?? null;
                    emailThongBao.noiDung = content.ToString();
                    emailThongBao.emailGuiDen = item.email;
                    emailThongBao.maNhanVien = item.maNhanVien;
                    emailThongBao.titleMail = thongBao.titleMail;

                   // if (SendMailGeneral(thongBao.tenThongBao, item.email, emailThongBao.noiDung, listFile))
                   // {
                   //     emailThongBao.trangThaiGui = true;
                   // }
                   // else
                   // {
                   //     emailThongBao.trangThaiGui = null;
                   //}
                    emailThongBao.trangThaiGui = null;
                    hrservice.Sys_EmailGuiThongBaos.InsertOnSubmit(emailThongBao);
                    hrservice.SubmitChanges();
                    // Them vao file dinh kem
                    if (lstFileService != null && lstFileService.Count > 0)
                    {
                        // delete files
                        var listFileDK = hrservice.tbl_FileDinhKems.Where(d => d.identification == Convert.ToString(id)).ToList();
                        hrservice.tbl_FileDinhKems.DeleteAllOnSubmit(listFileDK);
                        hrservice.SubmitChanges();
                        // End
                        List<BatDongSan.Models.NhanSuService.tbl_FileDinhKem> lstFileDK = new List<Models.NhanSuService.tbl_FileDinhKem>();
                       BatDongSan.Models.NhanSuService.tbl_FileDinhKem tblDinhKem;

                        foreach (var itemFile in lstFileService)
                        {
                            tblDinhKem = new Models.NhanSuService.tbl_FileDinhKem();
                            tblDinhKem.controller = itemFile.controller;
                            tblDinhKem.Action = itemFile.Action;
                            tblDinhKem.identification = itemFile.identification;
                            tblDinhKem.originalFileName = itemFile.originalFileName;
                            tblDinhKem.savedFileName = itemFile.savedFileName;
                            tblDinhKem.maNguoiUpLoad = itemFile.maNguoiUpLoad;
                            tblDinhKem.tenNguoiUpLoad = itemFile.tenNguoiUpLoad;
                            tblDinhKem.ngayLap = itemFile.ngayLap ?? DateTime.Now;
                            tblDinhKem.taiLieuURL = itemFile.taiLieuURL;
                            tblDinhKem.thumbnailURL = itemFile.thumbnailURL;
                            lstFileDK.Add(tblDinhKem);
                        }
                        hrservice.tbl_FileDinhKems.InsertAllOnSubmit(lstFileDK);
                        hrservice.SubmitChanges();
                    }
                    // End them vao file dinh kem
                }
                thongBao.daGuiEmail = null;
                contentHT.SubmitChanges();

                return Json(string.Empty);
            }
            catch
            {
                return View();
            }
        }
        #endregion
        
        #region Gửi email thông báo
        [ValidateInput(false)]
        public ActionResult SendSMSThongBao(int id)
        {
            try
            {
                StringBuilder content = new StringBuilder();
                Sys_ThongBao thongBao = contentHT.Sys_ThongBaos.Where(d => d.id == id).FirstOrDefault();

                List<Sys_SMSGuiThongBao> emailThongBaos = new List<Sys_SMSGuiThongBao>();
                List<string> nhanVienList = new List<string>();

                // Cap bac chuc danh
                if (thongBao.listPhongBan != null)
                {
                    var arrPB = thongBao.listPhongBan.Split(',').ToArray();
                    for (int j = 0; j < arrPB.Length; j++)
                    {
                        if (arrPB[j] != "")
                        {
                            List<string> nhanVienTam = new List<string>();
                            if (thongBao.listCapBac != null)
                            {
                                var arrCD = thongBao.listCapBac.Split(',').ToArray();
                                nhanVienTam = (from q in contentHT.sp_PB_DanhSachNhanVien_Tree("", Convert.ToString(arrPB[j]), 0, 0, 30000)
                                               where (arrCD.Contains(Convert.ToString(q.soCapBac)))
                                               select q.maNhanVien).ToList();
                            }
                            else
                            {
                                nhanVienTam = (from q in contentHT.sp_PB_DanhSachNhanVien_Tree("", Convert.ToString(arrPB[j]), 0, 0, 30000)
                                               select q.maNhanVien).ToList();
                            }
                            foreach (var itemTemp in nhanVienTam)
                            {
                                nhanVienList.Add(itemTemp);
                            }
                        }
                    }

                }
                if (thongBao.listNhanVien != null)
                {
                    var arrNV = thongBao.listNhanVien.Split(',').ToArray();
                    for (int k = 0; k < arrNV.Length; k++)
                    {
                        if (Convert.ToString(arrNV[k]).Length > 2)
                        {
                            nhanVienList.Add(Convert.ToString(arrNV[k]));
                        }
                    }
                }
                var lstNhanVienSendMail = (from p in contextNS.tbl_NS_NhanViens
                                           where nhanVienList.Contains(p.maNhanVien)
                                           where (p.trangThai == 0)
                                           select new NhanVienModel
                                           {
                                               maNhanVien = p.maNhanVien,
                                               hoVaTen = p.ho + " " + p.ten,
                                               email = p.email,
                                               phoneNumber1 = p.phoneNumber1
                                           }
                                               ).Distinct().ToList();

                MailHelper mailInit = new MailHelper();// lay cac tham so trong webconfig      

                foreach (var item in lstNhanVienSendMail)
                {
                    if (item.phoneNumber1 != "")
                    {
                        Sys_SMSGuiThongBao emailThongBao = new Sys_SMSGuiThongBao();
                        emailThongBao.idThongBao = id;
                        emailThongBao.ngayGui = DateTime.Now;
                        emailThongBao.nguoiGui = GetUser().manv;

                        emailThongBao.noiDung = thongBao.noiDungSMS;
                        emailThongBao.SDTGuiDen = item.phoneNumber1;
                        emailThongBao.maNhanVien = item.maNhanVien;
                        if (SendSMSTB(item.phoneNumber1, thongBao.noiDungSMS))
                        {
                            emailThongBao.trangThaiGui = true;
                        }
                        else
                        {
                            emailThongBao.trangThaiGui = false;
                        }
                        emailThongBaos.Add(emailThongBao);
                    }
                }
                thongBao.daGuiSMS = true;
                contentHT.Sys_SMSGuiThongBaos.InsertAllOnSubmit(emailThongBaos);
                contentHT.SubmitChanges();
                return Json(string.Empty);
            }
            catch
            {
                return View();
            }
        }
        #endregion
        #region Gửi sms
        public bool SendSMSTB(string mobile, string TextSMS)
        {
            SMSThuanViet SMSTV = new SMSThuanViet();
            if (mobile.Length > 9)
            {
                string sql = "INSERT INTO MessageOut(receiver, msg, status) VALUES('" + mobile + "', '" + TextSMS + "', 'send')";
                bool StatusSend = SMSTV.ExeCuteNonQuery(sql);
                return StatusSend;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}
