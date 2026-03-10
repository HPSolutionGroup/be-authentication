namespace be_authenticationDomain.Utils
{
    public abstract class BaseEntity
    {
        public string? CreatedBy { get; set; }
        public string? LastUpdatedBy { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime LastUpdatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }

        public bool IsDeleted { get; set; }
    }
}
