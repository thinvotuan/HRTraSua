namespace BatDongSan.Models.NhanSu.LinqERPNewCity
{
    partial class LinqERPNewCityDataContext
    {
        public LinqERPNewCityDataContext()
            : base(global::System.Configuration.ConfigurationManager.ConnectionStrings["ERPNewCityconnectionString"].ToString(), mappingSource)
        {
            OnCreated();
            this.CommandTimeout = 3600;
        }
    }
}
