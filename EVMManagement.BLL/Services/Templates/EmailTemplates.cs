using System;

namespace EVMManagement.BLL.Services.Templates
{
    public static class EmailTemplates
    {
        public static class Subjects
        {
            public const string WelcomeDealer = "Chào Mừng Đến Với Hệ Thống EVM Management";
            public const string ForgotPassword = "Yêu Cầu Đặt Lại Mật Khẩu";
            public const string PasswordResetConfirmation = "Mật Khẩu Đã Được Đặt Lại Thành Công";
            public const string PasswordChangeConfirmation = "Mật Khẩu Đã Được Thay Đổi Thành Công";
        }

        public static string GetOtpEmailTemplate(string otpCode, DateTime expiresAt)
        {
            return $@"
                <!DOCTYPE html>
                <html lang='vi'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Mã OTP Ký Điện Tử</title>
                </head>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; border-radius: 10px 10px 0 0; text-align: center;'>
                        <h1 style='color: white; margin: 0; font-size: 28px;'>🔐 Xác Thực Chữ Ký Điện Tử</h1>
                    </div>
                    
                    <div style='background-color: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px;'>
                        <p style='font-size: 16px; margin-bottom: 20px;'>Xin chào,</p>
                        
                        <p style='font-size: 16px; margin-bottom: 25px;'>
                            Bạn đã yêu cầu mã OTP để xác thực chữ ký điện tử. Vui lòng sử dụng mã dưới đây để hoàn tất quá trình ký:
                        </p>
                        
                        <div style='background-color: white; padding: 25px; border-radius: 8px; text-align: center; margin: 30px 0; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
                            <div style='color: #666; font-size: 14px; margin-bottom: 10px; text-transform: uppercase; letter-spacing: 1px;'>Mã OTP của bạn</div>
                            <div style='font-size: 42px; font-weight: bold; color: #667eea; letter-spacing: 8px; font-family: monospace;'>{otpCode}</div>
                        </div>
                        
                        <div style='background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 25px 0; border-radius: 4px;'>
                            <p style='margin: 0; font-size: 14px; color: #856404;'>
                                ⏱️ <strong>Thời gian hết hạn:</strong> {expiresAt:dd/MM/yyyy HH:mm:ss} (UTC)
                            </p>
                            <p style='margin: 10px 0 0 0; font-size: 14px; color: #856404;'>
                                Mã OTP này chỉ có hiệu lực trong <strong>5 phút</strong>.
                            </p>
                        </div>
                        
                        <div style='background-color: #f8d7da; border-left: 4px solid #dc3545; padding: 15px; margin: 25px 0; border-radius: 4px;'>
                            <p style='margin: 0; font-size: 14px; color: #721c24;'>
                                ⚠️ <strong>Lưu ý bảo mật:</strong>
                            </p>
                            <ul style='margin: 10px 0 0 0; padding-left: 20px; font-size: 14px; color: #721c24;'>
                                <li>Không chia sẻ mã OTP này với bất kỳ ai</li>
                                <li>Hệ thống sẽ không bao giờ yêu cầu mã OTP qua điện thoại</li>
                                <li>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email</li>
                            </ul>
                        </div>
                        
                        <p style='font-size: 14px; color: #666; margin-top: 30px;'>
                            Nếu bạn gặp bất kỳ vấn đề nào, vui lòng liên hệ với bộ phận hỗ trợ của chúng tôi.
                        </p>
                        
                        <hr style='border: none; border-top: 1px solid #ddd; margin: 30px 0;'>
                        
                        <p style='font-size: 14px; color: #666; margin: 0;'>
                            Trân trọng,<br>
                            <strong style='color: #667eea;'>Hệ Thống Quản Lý EVM</strong>
                        </p>
                    </div>
                    
                    <div style='text-align: center; padding: 20px; font-size: 12px; color: #999;'>
                        <p style='margin: 5px 0;'>© 2025 EVM Management System. All rights reserved.</p>
                        <p style='margin: 5px 0;'>Email này được gửi tự động, vui lòng không trả lời.</p>
                    </div>
                </body>
                </html>
            ";
        }

        public static string WelcomeDealerEmail(string fullName, string email, string password)
        {
            return $@"
                <!DOCTYPE html>
                <html lang='vi'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Chào Mừng Đại Lý Mới</title>
                </head>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%); padding: 30px; border-radius: 10px 10px 0 0; text-align: center;'>
                        <h1 style='color: white; margin: 0; font-size: 28px;'>🎉 Chào Mừng Đến Với EVM Management</h1>
                    </div>
                    
                    <div style='background-color: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px;'>
                        <p style='font-size: 16px; margin-bottom: 20px;'>Xin chào <strong style='color: #11998e;'>{fullName}</strong>,</p>
                        
                        <p style='font-size: 16px; margin-bottom: 25px;'>
                            Chúc mừng! Tài khoản đại lý của bạn đã được tạo thành công trên hệ thống EVM Management.
                        </p>
                        
                        <div style='background-color: white; padding: 25px; border-radius: 8px; margin: 25px 0; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
                            <h3 style='margin-top: 0; color: #11998e;'>Thông Tin Đăng Nhập</h3>
                            <table style='width: 100%; border-collapse: collapse;'>
                                <tr>
                                    <td style='padding: 10px 0; border-bottom: 1px solid #eee;'><strong>Email:</strong></td>
                                    <td style='padding: 10px 0; border-bottom: 1px solid #eee; color: #11998e;'>{email}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 10px 0; border-bottom: 1px solid #eee;'><strong>Mật khẩu tạm thời:</strong></td>
                                    <td style='padding: 10px 0; border-bottom: 1px solid #eee; font-family: monospace; color: #e74c3c;'>{password}</td>
                                </tr>
                            </table>
                        </div>
                        
                        <div style='background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 25px 0; border-radius: 4px;'>
                            <p style='margin: 0; font-size: 14px; color: #856404;'>
                                ⚠️ <strong>Lưu ý quan trọng:</strong> Vui lòng đổi mật khẩu ngay sau lần đăng nhập đầu tiên để bảo mật tài khoản.
                            </p>
                        </div>
                        
                        <p style='font-size: 14px; color: #666; margin-top: 30px;'>
                            Nếu bạn gặp bất kỳ vấn đề nào trong quá trình đăng nhập, vui lòng liên hệ với bộ phận hỗ trợ của chúng tôi.
                        </p>
                        
                        <hr style='border: none; border-top: 1px solid #ddd; margin: 30px 0;'>
                        
                        <p style='font-size: 14px; color: #666; margin: 0;'>
                            Trân trọng,<br>
                            <strong style='color: #11998e;'>Hệ Thống Quản Lý EVM</strong>
                        </p>
                    </div>
                    
                    <div style='text-align: center; padding: 20px; font-size: 12px; color: #999;'>
                        <p style='margin: 5px 0;'>© 2025 EVM Management System. All rights reserved.</p>
                    </div>
                </body>
                </html>
            ";
        }

        public static string ForgotPasswordEmail(string resetToken)
        {
            return $@"
                <!DOCTYPE html>
                <html lang='vi'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Đặt Lại Mật Khẩu</title>
                </head>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; border-radius: 10px 10px 0 0; text-align: center;'>
                        <h1 style='color: white; margin: 0; font-size: 28px;'>🔑 Đặt Lại Mật Khẩu</h1>
                    </div>
                    
                    <div style='background-color: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px;'>
                        <p style='font-size: 16px; margin-bottom: 20px;'>Xin chào,</p>
                        
                        <p style='font-size: 16px; margin-bottom: 25px;'>
                            Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn. Vui lòng sử dụng mã xác thực dưới đây:
                        </p>
                        
                        <div style='background-color: white; padding: 25px; border-radius: 8px; text-align: center; margin: 30px 0; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
                            <div style='color: #666; font-size: 14px; margin-bottom: 10px; text-transform: uppercase; letter-spacing: 1px;'>Mã Xác Thực</div>
                            <div style='font-size: 32px; font-weight: bold; color: #667eea; letter-spacing: 3px; font-family: monospace; word-break: break-all;'>{resetToken}</div>
                        </div>
                        
                        <div style='background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 25px 0; border-radius: 4px;'>
                            <p style='margin: 0; font-size: 14px; color: #856404;'>
                                ⏱️ Mã này có hiệu lực trong <strong>15 phút</strong>.
                            </p>
                        </div>
                        
                        <div style='background-color: #f8d7da; border-left: 4px solid #dc3545; padding: 15px; margin: 25px 0; border-radius: 4px;'>
                            <p style='margin: 0; font-size: 14px; color: #721c24;'>
                                ⚠️ Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này và liên hệ với bộ phận hỗ trợ ngay lập tức.
                            </p>
                        </div>
                        
                        <hr style='border: none; border-top: 1px solid #ddd; margin: 30px 0;'>
                        
                        <p style='font-size: 14px; color: #666; margin: 0;'>
                            Trân trọng,<br>
                            <strong style='color: #667eea;'>Hệ Thống Quản Lý EVM</strong>
                        </p>
                    </div>
                    
                    <div style='text-align: center; padding: 20px; font-size: 12px; color: #999;'>
                        <p style='margin: 5px 0;'>© 2025 EVM Management System. All rights reserved.</p>
                    </div>
                </body>
                </html>
            ";
        }

        public static string PasswordResetConfirmationEmail(string email)
        {
            return $@"
                <!DOCTYPE html>
                <html lang='vi'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Xác Nhận Đặt Lại Mật Khẩu</title>
                </head>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='background: linear-gradient(135deg, #56ab2f 0%, #a8e063 100%); padding: 30px; border-radius: 10px 10px 0 0; text-align: center;'>
                        <h1 style='color: white; margin: 0; font-size: 28px;'>✅ Đặt Lại Mật Khẩu Thành Công</h1>
                    </div>
                    
                    <div style='background-color: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px;'>
                        <p style='font-size: 16px; margin-bottom: 20px;'>Xin chào,</p>
                        
                        <p style='font-size: 16px; margin-bottom: 25px;'>
                            Mật khẩu cho tài khoản <strong style='color: #56ab2f;'>{email}</strong> đã được đặt lại thành công.
                        </p>
                        
                        <div style='background-color: #d4edda; border-left: 4px solid #28a745; padding: 15px; margin: 25px 0; border-radius: 4px;'>
                            <p style='margin: 0; font-size: 14px; color: #155724;'>
                                ✓ Bạn có thể đăng nhập ngay bây giờ với mật khẩu mới của mình.
                            </p>
                        </div>
                        
                        <div style='background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 25px 0; border-radius: 4px;'>
                            <p style='margin: 0; font-size: 14px; color: #856404;'>
                                ⚠️ Nếu bạn không thực hiện thay đổi này, vui lòng liên hệ với bộ phận hỗ trợ ngay lập tức.
                            </p>
                        </div>
                        
                        <hr style='border: none; border-top: 1px solid #ddd; margin: 30px 0;'>
                        
                        <p style='font-size: 14px; color: #666; margin: 0;'>
                            Trân trọng,<br>
                            <strong style='color: #56ab2f;'>Hệ Thống Quản Lý EVM</strong>
                        </p>
                    </div>
                    
                    <div style='text-align: center; padding: 20px; font-size: 12px; color: #999;'>
                        <p style='margin: 5px 0;'>© 2025 EVM Management System. All rights reserved.</p>
                    </div>
                </body>
                </html>
            ";
        }

        public static string PasswordChangeConfirmationEmail(string email)
        {
            return $@"
                <!DOCTYPE html>
                <html lang='vi'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Xác Nhận Thay Đổi Mật Khẩu</title>
                </head>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); padding: 30px; border-radius: 10px 10px 0 0; text-align: center;'>
                        <h1 style='color: white; margin: 0; font-size: 28px;'>🔒 Thay Đổi Mật Khẩu Thành Công</h1>
                    </div>
                    
                    <div style='background-color: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px;'>
                        <p style='font-size: 16px; margin-bottom: 20px;'>Xin chào,</p>
                        
                        <p style='font-size: 16px; margin-bottom: 25px;'>
                            Mật khẩu cho tài khoản <strong style='color: #f5576c;'>{email}</strong> đã được thay đổi thành công.
                        </p>
                        
                        <div style='background-color: #d4edda; border-left: 4px solid #28a745; padding: 15px; margin: 25px 0; border-radius: 4px;'>
                            <p style='margin: 0; font-size: 14px; color: #155724;'>
                                ✓ Mật khẩu mới của bạn đã được cập nhật và có hiệu lực ngay lập tức.
                            </p>
                        </div>
                        
                        <div style='background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 25px 0; border-radius: 4px;'>
                            <p style='margin: 0; font-size: 14px; color: #856404;'>
                                ⚠️ Nếu bạn không thực hiện thay đổi này, tài khoản của bạn có thể đã bị xâm nhập. Vui lòng liên hệ với bộ phận hỗ trợ ngay lập tức.
                            </p>
                        </div>
                        
                        <hr style='border: none; border-top: 1px solid #ddd; margin: 30px 0;'>
                        
                        <p style='font-size: 14px; color: #666; margin: 0;'>
                            Trân trọng,<br>
                            <strong style='color: #f5576c;'>Hệ Thống Quản Lý EVM</strong>
                        </p>
                    </div>
                    
                    <div style='text-align: center; padding: 20px; font-size: 12px; color: #999;'>
                        <p style='margin: 5px 0;'>© 2025 EVM Management System. All rights reserved.</p>
                    </div>
                </body>
                </html>
            ";
        }
    }
}
