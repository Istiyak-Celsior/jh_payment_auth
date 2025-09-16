namespace jh_payment_auth.Entity
{
    /// <summary>
    /// 
    /// </summary>
    public class User
    {
        /// <summary>
        /// 
        /// </summary>
        public long UserId { set; get; }
        /// <summary>
        /// 
        /// </summary>
        public string FirstName { set; get; }
        public string LastName { set; get; }
        public string Email { set; get; }
        public string Mobile { set; get; }
        public string AccountNumber { set; get; }
        public string BankName { set; get; }
        public string IFCCode { set; get; }
        public string BankCode { set; get; }
        public string City { set; get; }
        public string Branch { set; get; }
        public string UPIID { set; get; }
        public string CVV { set; get; }
        public bool IsActive { set; get; }
        public string Password { set;get; }
        public int Age { set;get; }
        public string Address { set; get; }
        public DateTime DateOfExpiry { set; get; }
    }
}
