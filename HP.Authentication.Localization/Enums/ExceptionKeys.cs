namespace HP.Authentication.Localization.Enums
{
    public enum ExceptionKeys
    {
        // === INTERNAL SERVER ERRORS (500) ===
        INTERNAL_SERVER_ERROR,      // Lỗi hệ thống từ server
        UNKNOWN_ERROR,              // Lỗi hệ thống không xác định - Default

        // === AUTHENTICATION & AUTHORIZATION ERRORS (401 / 403) ===
        UNAUTHORIZED,               // Lỗi chưa xác thực (chưa đăng nhập / token hết hạn)
        FORBIDDEN,                  // Lỗi không có quyền truy cập tài nguyên

        // === BAD REQUEST ERRORS (400) ===
        VALIDATION_FAILED,          // Dữ liệu đầu vào không hợp lệ (FluentValidation)
        INVALID_DATA,               // Dữ liệu xử lý không hợp lệ
        DATA_NOT_FOUND,             // Không tìm thấy dữ liệu yêu cầu

        // === CONFLICT ERRORS (409) ===
        DATA_EXIST                  // Dữ liệu đã tồn tại trong hệ thống
    }
}
