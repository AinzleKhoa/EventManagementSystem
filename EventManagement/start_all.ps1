$apis = @(
    "GatewayAPI",
    "AuthenticationAPI",
    "CategoryAPI",
    "EventAPI",
    "BookingAPI",
    "SocialAPI",
    "VoucherAPI",
    "WithdrawalAPI",
    "ChatAPI",
    "NotificationAPI",
    "ChatbotAPI",
    "WebClient"
)

Write-Host "Đang khởi động 12 services..."

foreach ($api in $apis) {
    Write-Host "Starting $api..."
    Start-Process -FilePath "dotnet" -ArgumentList "run" -WorkingDirectory ".\$api"
}

Write-Host "Đã mở 12 cửa sổ cho tất cả các API và WebClient."
