using HP.Authentication.Domain.Utils;

namespace HP.Authentication.Domain.Entities
{
    public class Function : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }     // Tên màn hình hoặc API
        public string Code { get; set; }
        public string? Url { get; set; }      // Đường dẫn truy cập
        public int SortOrder { get; set; }   // Thứ tự hiển thị
        public Guid? ParentId { get; set; }  // Dùng cho menu cấp cha/con
        public bool IsVisible { get; set; } = true; // Ẩn hiện menu động

        // ==== Navigate ====

        public Guid SubsystemId { get; set; }
        public Subsystem Subsystem { get; set; }
        public ICollection<CommandInFunction> CommandInFunctions { get; set; } = new List<CommandInFunction>();
        public ICollection<Permission> Permissions { get; set; } = new List<Permission>();

    }
}
