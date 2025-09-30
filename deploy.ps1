# BYWG 工业边缘网关系统部署脚本
# PowerShell脚本，用于Windows环境部署

param(
    [string]$Environment = "development",
    [switch]$Build = $false,
    [switch]$Start = $false,
    [switch]$Stop = $false,
    [switch]$Restart = $false,
    [switch]$Logs = $false,
    [switch]$Clean = $false,
    [switch]$Status = $false
)

Write-Host "BYWG 工业边缘网关系统部署脚本" -ForegroundColor Green
Write-Host "环境: $Environment" -ForegroundColor Yellow

# 检查Docker是否安装
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Error "Docker未安装或未在PATH中找到"
    exit 1
}

# 检查Docker Compose是否安装
if (-not (Get-Command docker-compose -ErrorAction SilentlyContinue)) {
    Write-Error "Docker Compose未安装或未在PATH中找到"
    exit 1
}

# 创建必要的目录
Write-Host "创建必要的目录..." -ForegroundColor Blue
$directories = @(
    "logs/admin",
    "logs/gateway1", 
    "logs/gateway2",
    "nginx/ssl",
    "init-scripts",
    "monitoring",
    "backups"
)

foreach ($dir in $directories) {
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        Write-Host "创建目录: $dir" -ForegroundColor Green
    }
}

# 检查环境变量文件
if (-not (Test-Path ".env")) {
    if (Test-Path "env.example") {
        Copy-Item "env.example" ".env"
        Write-Host "已创建 .env 文件，请修改其中的配置" -ForegroundColor Yellow
    } else {
        Write-Warning "未找到环境变量配置文件"
    }
}

# 构建镜像
if ($Build) {
    Write-Host "构建Docker镜像..." -ForegroundColor Blue
    
    # 构建前端
    Write-Host "构建前端镜像..." -ForegroundColor Yellow
    Set-Location device-admin
    docker build -t bywg/frontend:latest .
    Set-Location ..
    
    # 构建后端服务
    Write-Host "构建后端服务镜像..." -ForegroundColor Yellow
    if ($Environment -eq "production") {
        docker-compose -f docker-compose.yml -f docker-compose.prod.yml build
    } else {
        docker-compose build
    }
    
    Write-Host "镜像构建完成" -ForegroundColor Green
}

# 启动服务
if ($Start) {
    Write-Host "启动服务..." -ForegroundColor Blue
    
    if ($Environment -eq "production") {
        docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
    } else {
        docker-compose up -d
    }
    
    Write-Host "服务启动完成" -ForegroundColor Green
    Write-Host ""
    Write-Host "=== 服务访问地址 ===" -ForegroundColor Cyan
    Write-Host "前端管理界面: http://localhost" -ForegroundColor White
    Write-Host "管理API: http://localhost:5000" -ForegroundColor White
    Write-Host "认证服务: http://localhost:5001" -ForegroundColor White
    Write-Host "配置服务: http://localhost:5002" -ForegroundColor White
    Write-Host "数据库: localhost:5432" -ForegroundColor White
    Write-Host "Redis: localhost:6379" -ForegroundColor White
    Write-Host "RabbitMQ管理: http://localhost:15672" -ForegroundColor White
    Write-Host "MinIO控制台: http://localhost:9001" -ForegroundColor White
    Write-Host "Prometheus: http://localhost:9090" -ForegroundColor White
    Write-Host "Grafana: http://localhost:3000" -ForegroundColor White
    Write-Host "网关1: http://localhost:8080" -ForegroundColor White
    Write-Host "网关2: http://localhost:8081" -ForegroundColor White
}

# 停止服务
if ($Stop) {
    Write-Host "停止服务..." -ForegroundColor Blue
    if ($Environment -eq "production") {
        docker-compose -f docker-compose.yml -f docker-compose.prod.yml down
    } else {
        docker-compose down
    }
    Write-Host "服务已停止" -ForegroundColor Green
}

# 重启服务
if ($Restart) {
    Write-Host "重启服务..." -ForegroundColor Blue
    if ($Environment -eq "production") {
        docker-compose -f docker-compose.yml -f docker-compose.prod.yml restart
    } else {
        docker-compose restart
    }
    Write-Host "服务已重启" -ForegroundColor Green
}

# 查看日志
if ($Logs) {
    Write-Host "查看服务日志..." -ForegroundColor Blue
    if ($Environment -eq "production") {
        docker-compose -f docker-compose.yml -f docker-compose.prod.yml logs -f
    } else {
        docker-compose logs -f
    }
}

# 查看状态
if ($Status) {
    Write-Host "查看服务状态..." -ForegroundColor Blue
    if ($Environment -eq "production") {
        docker-compose -f docker-compose.yml -f docker-compose.prod.yml ps
    } else {
        docker-compose ps
    }
}

# 清理
if ($Clean) {
    Write-Host "清理Docker资源..." -ForegroundColor Blue
    
    # 停止并删除容器
    if ($Environment -eq "production") {
        docker-compose -f docker-compose.yml -f docker-compose.prod.yml down -v
    } else {
        docker-compose down -v
    }
    
    # 删除镜像
    docker rmi $(docker images "bywg/*" -q) -f 2>$null
    
    # 清理未使用的资源
    docker system prune -f
    
    Write-Host "清理完成" -ForegroundColor Green
}

# 显示帮助信息
if (-not ($Build -or $Start -or $Stop -or $Restart -or $Logs -or $Clean -or $Status)) {
    Write-Host ""
    Write-Host "=== BYWG 工业边缘网关系统部署脚本 ===" -ForegroundColor Green
    Write-Host ""
    Write-Host "使用方法:" -ForegroundColor Yellow
    Write-Host "  .\deploy.ps1 -Build                    # 构建镜像" -ForegroundColor White
    Write-Host "  .\deploy.ps1 -Start                    # 启动服务" -ForegroundColor White
    Write-Host "  .\deploy.ps1 -Stop                    # 停止服务" -ForegroundColor White
    Write-Host "  .\deploy.ps1 -Restart                  # 重启服务" -ForegroundColor White
    Write-Host "  .\deploy.ps1 -Logs                    # 查看日志" -ForegroundColor White
    Write-Host "  .\deploy.ps1 -Status                  # 查看状态" -ForegroundColor White
    Write-Host "  .\deploy.ps1 -Clean                  # 清理资源" -ForegroundColor White
    Write-Host "  .\deploy.ps1 -Environment production  # 指定环境" -ForegroundColor White
    Write-Host ""
    Write-Host "示例:" -ForegroundColor Yellow
    Write-Host "  .\deploy.ps1 -Build -Start            # 构建并启动开发环境" -ForegroundColor White
    Write-Host "  .\deploy.ps1 -Environment production -Start  # 启动生产环境" -ForegroundColor White
    Write-Host ""
}
