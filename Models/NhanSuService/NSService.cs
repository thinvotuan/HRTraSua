namespace BatDongSan.Models.NhanSuService
{
    partial class NSServiceDataContext
    {
        public NSServiceDataContext()
            : base(global::System.Configuration.ConfigurationManager.ConnectionStrings["NhanSuServiceConnectionString"].ToString(), mappingSource)
        {
            OnCreated();
            this.CommandTimeout = 3600;
        }
    }
}
