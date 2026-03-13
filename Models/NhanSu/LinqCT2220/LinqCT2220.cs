namespace BatDongSan.Models.NhanSu.LinqCT2220
{
    partial class LinqCT2220DataContext
    {
        public LinqCT2220DataContext()
            : base(global::System.Configuration.ConfigurationManager.ConnectionStrings["dbCT2220connectionString"].ToString(), mappingSource)
        {
            OnCreated();
            this.CommandTimeout = 3600;
        }
    }
}
