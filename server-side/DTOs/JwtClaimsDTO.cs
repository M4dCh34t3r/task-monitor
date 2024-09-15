namespace TaskMonitor.DTOs
{
    public struct JwtClaimsDTO
    {
        public int Nbf { get; set; }
        public int Exp { get; set; }
        public int Iat { get; set; }
        public int Uid { get; set; }
        public int Unm { get; set; }
        public bool Adm { get; set; }
    }
}
