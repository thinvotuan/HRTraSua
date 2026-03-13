namespace BatDongSan.Models.NhanSu.NhanSuThuanViet
{
    partial class HRThuanVietDataContext
    {
        public HRThuanVietDataContext()
            : base(global::System.Configuration.ConfigurationManager.ConnectionStrings["NhanSuThuanVietConnectionString"].ToString(), mappingSource)
        {
            OnCreated();
            this.CommandTimeout = 3600;
        }
    }
}
