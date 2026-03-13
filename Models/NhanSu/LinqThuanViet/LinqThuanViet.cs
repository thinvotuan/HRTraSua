namespace BatDongSan.Models.NhanSu.LinqThuanViet
{
    partial class LinqThuanVietDataContext
    {
        public LinqThuanVietDataContext()
            : base(global::System.Configuration.ConfigurationManager.ConnectionStrings["dbThuanVietConnectionString"].ToString(), mappingSource)
        {
            OnCreated();
            this.CommandTimeout = 3600;
        }
    }
}
