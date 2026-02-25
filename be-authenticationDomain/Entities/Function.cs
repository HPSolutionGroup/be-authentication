using be_authenticationDomain.Utils;

namespace be_authenticationDomain.Entities
{
    public class Function : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }     // Tên màn hình hoặc API
        public string Code { get; set; }
        public string? Url { get; set; }      // Đường dẫn truy cập
        public int? SortOrder { get; set; }   // Thứ tự hiển thị
        public Guid? ParentId { get; set; }  // Dùng cho menu cấp cha/con
        public bool IsVisible { get; set; } = true;
        public ICollection<CommandInFunction> CommandInFunctions { get; set; }
    }
}
