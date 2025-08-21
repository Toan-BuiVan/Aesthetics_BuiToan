﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.DataObject.Model
{
    public class Wallets
    {
        [Key]
        public int WalletsID { get; set; }
        public int UserID { get; set; }
        public int VoucherID { get; set; }
        public Vouchers Vouchers { get; set; }
        public Users Users { get; set; }
    }
}
