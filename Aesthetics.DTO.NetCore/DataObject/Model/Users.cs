﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.DataObject.Model
{
    public class Users
    {
        [Key]
        public int UserID { get; set; }
        public string? UserName { get; set; }
        public string? PassWord { get; set; }
        public string? Email { get; set; }
        public DateTime? DateBirth { get; set; }
        public string? Sex { get; set; }
        public DateTime? Creation { get; set; }
        public string? Phone { get; set; }
        public string? Addres { get; set; }
        public string? IDCard { get; set; }
        public string? TypePerson { get; set; }
		public string? ReferralCode { get; set; }
		public int? AccumulatedPoints { get; set; }
		public decimal? SalesPoints { get; set; }
		public int? RatingPoints { get; set; }
		public string? RankMember { get; set; }
        public string? RefeshToken { get; set; }
        public DateTime? TokenExprired { get; set; }
		public int? DeleteStatus { get; set; }
		public Carts Carts { get; set; }
        public ICollection<Wallets> Wallets { get; set; }
		public ICollection<Booking> Bookings { get; set; }
		public ICollection<UserSession> UserSession { get; set; }
        public ICollection<Comments> Comments { get; set; }
        public ICollection<Invoice> Invoice { get; set; }
        public ICollection<InvoiceDetail> InvoicesDetail { get; set; }
        public ICollection<Permission> Permissions { get; set; }
		public ICollection<Clinic_Staff> Clinic_Staff { get; set; }
	}
}
