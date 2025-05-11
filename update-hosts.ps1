[System.Console]::Title = "SmartBuy Hosts Updater"

# Kiểm tra quyền admin
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "`n*** THONG BAO LOI ***" -ForegroundColor Red
    Write-Host "Vui long chay voi quyen quan tri vien" -ForegroundColor Yellow
    Write-Host "Dung file 'run-update-hosts.bat' de chay tu dong voi quyen admin" -ForegroundColor Green
    
    Write-Host "`nNhan phim bat ky de thoat..." -ForegroundColor Cyan
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit
}

$hostsPath = "C:\Windows\System32\drivers\etc\hosts"

$entries = @(
    "127.0.0.1 admin.smartbuy.com",
    "127.0.0.1 smartbuy.com",
    "127.0.0.1 api.smartbuy.com"
)

Write-Host "`n=== Dang cap nhat file hosts ===" -ForegroundColor Cyan

try {
    $currentContent = Get-Content -Path $hostsPath -ErrorAction Stop
    
    $updateCount = 0
    foreach ($entry in $entries) {
        if ($currentContent -notcontains $entry) {
            Add-Content -Path $hostsPath -Value $entry -ErrorAction Stop
            Write-Host "Da them: $entry" -ForegroundColor Green
            $updateCount++
        } else {
            Write-Host "Da ton tai: $entry" -ForegroundColor Yellow
        }
    }
    
    if ($updateCount -gt 0) {
        Write-Host "`nDa cap nhat thanh cong file hosts voi $updateCount muc." -ForegroundColor Green
    } else {
        Write-Host "`nKhong co muc nao can cap nhat, file hosts da co day du cac muc." -ForegroundColor Cyan
    }
}
catch {
    Write-Host "`n*** LOI: Khong the cap nhat file hosts ***" -ForegroundColor Red
    Write-Host "Chi tiet loi: $_" -ForegroundColor Red
    Write-Host "`nGoi y: Hay chay lai voi quyen quan tri vien" -ForegroundColor Yellow
}

# Hiển thị nội dung hiện tại của file hosts
Write-Host "`n=== Noi dung hien tai cua file hosts ===" -ForegroundColor Cyan
try {
    Get-Content -Path $hostsPath -ErrorAction Stop | ForEach-Object {
        if ($_ -match "smartbuy\.com") {
            Write-Host $_ -ForegroundColor Green
        } else {
            Write-Host $_
        }
    }
}
catch {
    Write-Host "Khong the doc file hosts: $_" -ForegroundColor Red
}

# Hiển thị thông báo hoàn tất và chờ người dùng
Write-Host "`n=== Hoàn tất ===" -ForegroundColor Green
Write-Host "Cac domain SmartBuy da duoc cap nhat trong file hosts." -ForegroundColor Cyan
Write-Host "Ban co the dong cua so nay hoac nhan phim bat ky de thoat..." -ForegroundColor White
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
