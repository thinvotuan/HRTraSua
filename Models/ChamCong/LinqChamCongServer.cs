namespace BatDongSan.Models.ChamCong
{
    partial class LinqChamCongServerDataContext
    {
        public LinqChamCongServerDataContext()
            : base(global::System.Configuration.ConfigurationManager.ConnectionStrings["ChamCongServerConnectionString"].ToString(), mappingSource)
        {
            OnCreated();
            this.CommandTimeout = 3600;
        }
    }
}
