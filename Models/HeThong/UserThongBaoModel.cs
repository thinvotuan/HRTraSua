using BatDongSan.Models.HeThong;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BatDongSan.Models.HeThong
{

    public class UserThongBaoModel
    {


        public int id { get; set; }

        public string tenThongBao { get; set; }

        public int? trangThaiXem { get; set; }
    }
}