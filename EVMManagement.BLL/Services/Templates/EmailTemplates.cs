namespace EVMManagement.BLL.Services.Templates
{
    public static class EmailTemplates
    {
        public static string WelcomeDealerEmail(string fullName, string email, string password)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Chào mừng đến với hệ thống EVM Management</h2>
                    <p>Xin chào <strong>{fullName}</strong>,</p>
                    <p>Tài khoản đại lý của bạn đã được tạo thành công.</p>
                    <p><strong>Thông tin đăng nhập:</strong></p>
                    <ul>
                        <li>Email: <strong>{email}</strong></li>
                        <li>Mật khẩu tạm thời: <strong>{password}</strong></li>
                    </ul>
                    <p style='color: #d9534f;'><strong>Lưu ý:</strong> Vui lòng đổi mật khẩu sau khi đăng nhập lần đầu.</p>
                    <p>Trân trọng,<br/>Đội ngũ EVM Management</p>
                </body>
                </html>";
        }

        public static string ForgotPasswordEmail(string resetToken)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Yêu cầu đặt lại mật khẩu</h2>
                    <p>Xin chào,</p>
                    <p>Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
                    <p><strong>Mã xác nhận của bạn:</strong></p>
                    <div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <code style='font-size: 18px; font-weight: bold; color: #333;'>{resetToken}</code>
                    </div>
                    <p>Mã này có hiệu lực trong <strong>5 phút</strong>.</p>
                    <p style='color: #d9534f;'><strong>Lưu ý:</strong> Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                    <p>Trân trọng,<br/>Đội ngũ EVM Management</p>
                </body>
                </html>";
        }

        public static string PasswordResetConfirmationEmail(string email)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Mật khẩu của bạn đã được đặt lại</h2>
                    <p>Xin chào,</p>
                    <p>Mật khẩu cho tài khoản <strong>{email}</strong> đã được đặt lại thành công.</p>
                    <p>Bạn có thể đăng nhập ngay bây giờ với mật khẩu mới.</p>
                    <p style='color: #d9534f;'><strong>Lưu ý:</strong> Nếu bạn không thực hiện thay đổi này, vui lòng liên hệ với chúng tôi ngay lập tức.</p>
                    <p>Trân trọng,<br/>Đội ngũ EVM Management</p>
                </body>
                </html>";
        }

        public static string PasswordChangeConfirmationEmail(string email)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Mật khẩu đã được thay đổi</h2>
                    <p>Xin chào,</p>
                    <p>Mật khẩu cho tài khoản <strong>{email}</strong> đã được thay đổi thành công.</p>
                    <p style='color: #d9534f;'><strong>Lưu ý:</strong> Nếu bạn không thực hiện thay đổi này, vui lòng liên hệ với chúng tôi ngay lập tức để bảo mật tài khoản.</p>
                    <p>Trân trọng,<br/>Đội ngũ EVM Management</p>
                </body>
                </html>";
        }

        public static class Subjects
        {
            public const string WelcomeDealer = "Tài khoản đại lý EVM - Thông tin đăng nhập";
            public const string ForgotPassword = "Đặt lại mật khẩu - EVM Management";
            public const string PasswordResetConfirmation = "Mật khẩu đã được đặt lại - EVM Management";
            public const string PasswordChangeConfirmation = "Mật khẩu đã được thay đổi - EVM Management";
        }
    }
}
