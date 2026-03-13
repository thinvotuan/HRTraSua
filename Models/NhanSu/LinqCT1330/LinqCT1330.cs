namespace BatDongSan.Models.NhanSu.LinqCT1330
{
    partial class LinqCT1330DataContext
    {
        public LinqCT1330DataContext()
            : base(global::System.Configuration.ConfigurationManager.ConnectionStrings["dbCT1330connectionString"].ToString(), mappingSource)
        {
            OnCreated();
            this.CommandTimeout = 3600;
        }
    }
}
