# BYWG 工业边缘网关系统 - 快速启动指南

## 🚀 一键启动

### 1. 环境准备

确保您的系统已安装：
- Docker 20.10+
- Docker Compose 2.0+
- PowerShell 5.1+ (Windows)

### 2. 快速启动

```powershell
# 克隆项目（如果还没有）
git clone <repository-url>
cd BYWG

# 复制环境变量配置
copy env.example .env

# 编辑环境变量（可选）
notepad .env

# 一键启动开发环境
.\deploy.ps1 -Build -Start
```

### 3. 访问系统

启动完成后，您可以通过以下地址访问系统：

| 服务 | 地址 | 说明 |
|------|------|------|
| **前端管理界面** | http://localhost | 🎯 主要管理界面 |
| **管理API** | http://localhost:5000 | 后端API服务 |
| **认证服务** | http://localhost:5001 | 用户认证服务 |
| **配置服务** | http://localhost:5002 | 配置管理服务 |
| **数据库** | localhost:5432 | PostgreSQL数据库 |
| **Redis** | localhost:6379 | 缓存服务 |
| **RabbitMQ管理** | http://localhost:15672 | 消息队列管理 |
| **MinIO控制台** | http://localhost:9001 | 对象存储管理 |
| **Prometheus** | http://localhost:9090 | 监控数据收集 |
| **Grafana** | http://localhost:3000 | 监控面板 |
| **网关1** | http://localhost:8080 | 边缘网关节点1 |
| **网关2** | http://localhost:8081 | 边缘网关节点2 |

### 4. 默认登录信息

- **用户名**: admin
- **密码**: 123456

## 🔧 常用命令

### 服务管理
```powershell
# 查看服务状态
.\deploy.ps1 -Status

# 查看日志
.\deploy.ps1 -Logs

# 重启服务
.\deploy.ps1 -Restart

# 停止服务
.\deploy.ps1 -Stop
```

### 生产环境部署
```powershell
# 生产环境启动
.\deploy.ps1 -Environment production -Build -Start

# 查看生产环境状态
.\deploy.ps1 -Environment production -Status
```

### 清理资源
```powershell
# 清理所有资源
.\deploy.ps1 -Clean
```

## 📊 系统监控

### 1. Grafana监控面板
- 访问: http://localhost:3000
- 默认用户名: admin
- 默认密码: admin

### 2. Prometheus指标
- 访问: http://localhost:9090
- 查看系统指标和告警规则

### 3. 系统健康检查
```powershell
# 检查所有服务状态
docker-compose ps

# 查看服务日志
docker-compose logs -f [service-name]
```

## 🛠️ 开发调试

### 1. 前端开发
```powershell
cd device-admin
npm install
npm run dev
```

### 2. 后端开发
```powershell
# 启动管理后端
cd BYWG.Admin
dotnet run

# 启动网关服务
cd BYWG.Gateway
dotnet run
```

### 3. 数据库管理
```powershell
# 连接数据库
docker exec -it postgres psql -U bywg -d bywg_admin

# 查看数据库状态
docker exec -it postgres pg_isready -U bywg
```

## 🔒 安全配置

### 1. 修改默认密码
编辑 `.env` 文件，修改以下配置：
```env
DB_PASSWORD=your-secure-password
JWT_SECRET_KEY=your-secret-key
REDIS_PASSWORD=your-redis-password
```

### 2. 启用HTTPS
```powershell
# 生成SSL证书（开发环境）
mkdir nginx/ssl
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout nginx/ssl/key.pem \
  -out nginx/ssl/cert.pem
```

## 📈 性能优化

### 1. 资源限制
编辑 `docker-compose.yml`，调整资源限制：
```yaml
services:
  admin-api:
    deploy:
      resources:
        limits:
          memory: 2G
          cpus: '1.0'
```

### 2. 数据库优化
```sql
-- 创建索引
CREATE INDEX idx_devices_status ON Devices(Status);
CREATE INDEX idx_alerts_created_at ON Alerts(CreatedAt);
```

## 🐛 故障排除

### 1. 服务启动失败
```powershell
# 查看详细日志
docker-compose logs [service-name]

# 重启特定服务
docker-compose restart [service-name]
```

### 2. 数据库连接问题
```powershell
# 检查数据库状态
docker-compose exec postgres pg_isready

# 重置数据库
docker-compose down -v
docker-compose up -d postgres
```

### 3. 端口冲突
```powershell
# 查看端口占用
netstat -ano | findstr :5000

# 修改端口配置
# 编辑 docker-compose.yml 中的 ports 配置
```

## 📞 获取帮助

- 📖 详细文档: [README.md](README.md)
- 🐛 问题反馈: [GitHub Issues](issues/)
- 💬 讨论交流: [GitHub Discussions](discussions/)

---

**快速开始，让工业数据采集更简单！** 🎉
