# Gmail Refresh Token Generator

Tool này giúp bạn lấy Refresh Token để sử dụng Gmail API.

## Cách sử dụng:

### 1. Đảm bảo file credentials.json đã có trong thư mục này
File credentials.json đã được tạo sẵn với OAuth credentials của bạn.

### 2. Chạy tool:

```bash
cd Tools/GmailTokenGenerator
dotnet run
```

### 3. Làm theo hướng dẫn:
- Browser sẽ tự động mở
- Đăng nhập với Gmail account của bạn
- Click "Allow" để cấp quyền
- Tool sẽ hiển thị REFRESH TOKEN

### 4. Copy Refresh Token
- Copy toàn bộ REFRESH TOKEN (dòng dài)
- Lưu vào appsettings.json

### 5. Thêm vào .gitignore
Đảm bảo các file sau đã có trong .gitignore:
- credentials.json
- token.json

## Lưu ý:
- Refresh Token chỉ cần lấy 1 lần
- Token này có thể dùng cho đến khi bị thu hồi
- Không share Refresh Token cho người khác
- Nếu token bị lộ, revoke và tạo lại
