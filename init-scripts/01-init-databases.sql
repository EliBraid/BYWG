-- BYWG 工业边缘网关系统数据库初始化脚本 (PostgreSQL)

-- 注意：Postgres 中需在 bywg 数据库中分别创建 schema 或独立数据库。
-- 推荐使用单库多 schema，这里使用独立数据库方式请在容器中执行：
--   CREATE DATABASE bywg_admin; CREATE DATABASE bywg_auth; CREATE DATABASE bywg_config;
-- 以下脚本假定在 bywg_admin 数据库中执行。

-- 创建网关表
CREATE TABLE IF NOT EXISTS Gateways (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    IpAddress VARCHAR(45) NOT NULL,
    Port INT NOT NULL DEFAULT 8080,
    Status INT NOT NULL DEFAULT 0,
    IsEnabled BOOLEAN NOT NULL DEFAULT TRUE,
    Description VARCHAR(500),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT now(),
    UpdatedAt TIMESTAMPTZ NOT NULL DEFAULT now(),
    LastHeartbeat TIMESTAMPTZ,
    Version VARCHAR(50),
    SystemInfo JSONB
);

-- 创建设备表
CREATE TABLE IF NOT EXISTS Devices (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Type VARCHAR(50) NOT NULL,
    IpAddress VARCHAR(45) NOT NULL,
    Port INT NOT NULL,
    Protocol VARCHAR(50) NOT NULL,
    Status INT NOT NULL DEFAULT 0,
    IsEnabled BOOLEAN NOT NULL DEFAULT TRUE,
    Description VARCHAR(500),
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT now(),
    UpdatedAt TIMESTAMPTZ NOT NULL DEFAULT now(),
    LastConnectedAt TIMESTAMPTZ,
    GatewayId INT,
    Parameters JSONB,
    CONSTRAINT fk_devices_gateway FOREIGN KEY (GatewayId) REFERENCES Gateways(Id) ON DELETE SET NULL
);

-- 创建用户表
CREATE TABLE IF NOT EXISTS Users (
    Id SERIAL PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    Email VARCHAR(100) NOT NULL UNIQUE,
    FullName VARCHAR(100),
    Phone VARCHAR(20),
    Department VARCHAR(100),
    Role VARCHAR(50) NOT NULL DEFAULT 'User',
    IsEnabled BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT now(),
    UpdatedAt TIMESTAMPTZ NOT NULL DEFAULT now(),
    LastLoginAt TIMESTAMPTZ,
    LoginCount INT NOT NULL DEFAULT 0,
    Preferences JSONB
);

-- 创建报警表
CREATE TABLE IF NOT EXISTS Alerts (
    Id SERIAL PRIMARY KEY,
    Title VARCHAR(200) NOT NULL,
    Message VARCHAR(1000) NOT NULL,
    Source VARCHAR(100) NOT NULL,
    Level VARCHAR(20) NOT NULL,
    Status VARCHAR(20) NOT NULL,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT now(),
    AcknowledgedAt TIMESTAMPTZ,
    ResolvedAt TIMESTAMPTZ,
    DeviceId INT,
    GatewayId INT,
    CONSTRAINT fk_alerts_device FOREIGN KEY (DeviceId) REFERENCES Devices(Id) ON DELETE SET NULL,
    CONSTRAINT fk_alerts_gateway FOREIGN KEY (GatewayId) REFERENCES Gateways(Id) ON DELETE SET NULL
);

-- 创建数据点表
CREATE TABLE IF NOT EXISTS DataPoints (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Address VARCHAR(100) NOT NULL,
    DataType VARCHAR(50) NOT NULL,
    Description VARCHAR(500),
    IsEnabled BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT now(),
    UpdatedAt TIMESTAMPTZ NOT NULL DEFAULT now(),
    DeviceId INT NOT NULL,
    CONSTRAINT fk_datapoints_device FOREIGN KEY (DeviceId) REFERENCES Devices(Id) ON DELETE CASCADE
);

-- 创建协议配置表
CREATE TABLE IF NOT EXISTS ProtocolConfigs (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    ProtocolType VARCHAR(50) NOT NULL,
    Description VARCHAR(500),
    IsEnabled BOOLEAN NOT NULL DEFAULT TRUE,
    Parameters JSONB,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT now(),
    UpdatedAt TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- 插入默认数据
INSERT INTO Users (Username, Email, FullName, Role, IsEnabled)
VALUES ('admin', 'admin@bywg.local', '系统管理员', 'Admin', TRUE)
ON CONFLICT (Username) DO NOTHING;

-- 插入示例网关数据
INSERT INTO Gateways (Name, IpAddress, Port, Status, IsEnabled, Description, Version)
VALUES 
    ('BYWG-Gateway-01', '192.168.1.100', 8080, 1, TRUE, '主网关节点', '1.0.0'),
    ('BYWG-Gateway-02', '192.168.1.101', 8080, 1, TRUE, '备用网关节点', '1.0.0')
ON CONFLICT DO NOTHING;

-- 插入示例设备数据
INSERT INTO Devices (Name, Type, IpAddress, Port, Protocol, Status, IsEnabled, Description, GatewayId)
VALUES 
    ('PLC-001', 'PLC', '192.168.1.10', 502, 'ModbusTCP', 1, TRUE, '主控PLC', 1),
    ('PLC-002', 'PLC', '192.168.1.11', 502, 'ModbusTCP', 1, TRUE, '备用PLC', 1),
    ('Sensor-001', 'Sensor', '192.168.1.20', 1883, 'MQTT', 1, TRUE, '温度传感器', 2),
    ('Sensor-002', 'Sensor', '192.168.1.21', 1883, 'MQTT', 1, TRUE, '湿度传感器', 2)
ON CONFLICT DO NOTHING;

-- 插入示例协议配置
INSERT INTO ProtocolConfigs (Name, ProtocolType, IsEnabled, Description, Parameters)
VALUES 
    ('ModbusTCP-Config', 'ModbusTCP', TRUE, 'Modbus TCP协议配置', '{"Host":"192.168.1.10","Port":502,"Timeout":5000}'),
    ('MQTT-Config', 'MQTT', TRUE, 'MQTT协议配置', '{"Broker":"192.168.1.20","Port":1883,"Topic":"sensors/+"}'),
    ('OPCUA-Config', 'OPCUA', TRUE, 'OPC UA协议配置', '{"Endpoint":"opc.tcp://192.168.1.30:4840","SecurityMode":"None"}')
ON CONFLICT DO NOTHING;

-- 创建索引
CREATE INDEX IF NOT EXISTS IX_Devices_GatewayId ON Devices(GatewayId);
CREATE INDEX IF NOT EXISTS IX_Devices_Status ON Devices(Status);
CREATE INDEX IF NOT EXISTS IX_Alerts_Status ON Alerts(Status);
CREATE INDEX IF NOT EXISTS IX_Alerts_Level ON Alerts(Level);
CREATE INDEX IF NOT EXISTS IX_Alerts_CreatedAt ON Alerts(CreatedAt);
CREATE INDEX IF NOT EXISTS IX_DataPoints_DeviceId ON DataPoints(DeviceId);

-- 创建视图
CREATE OR REPLACE VIEW GatewayOverview AS
SELECT 
    g.Id,
    g.Name,
    g.IpAddress,
    g.Status,
    g.IsEnabled,
    g.LastHeartbeat,
    COUNT(d.Id) as DeviceCount,
    COUNT(*) FILTER (WHERE d.Status = 1) as OnlineDeviceCount
FROM Gateways g
LEFT JOIN Devices d ON g.Id = d.GatewayId
GROUP BY g.Id, g.Name, g.IpAddress, g.Status, g.IsEnabled, g.LastHeartbeat;

CREATE OR REPLACE VIEW DeviceOverview AS
SELECT 
    d.Id,
    d.Name,
    d.Type,
    d.IpAddress,
    d.Protocol,
    d.Status,
    d.IsEnabled,
    d.LastConnectedAt,
    g.Name as GatewayName,
    g.IpAddress as GatewayIp
FROM Devices d
LEFT JOIN Gateways g ON d.GatewayId = g.Id;

-- 创建存储过程
-- Postgres 使用函数代替存储过程
CREATE OR REPLACE FUNCTION GetSystemOverview()
RETURNS TABLE (
    TotalGateways INT,
    OnlineGateways INT,
    TotalDevices INT,
    OnlineDevices INT,
    TotalUsers INT,
    UnresolvedAlerts INT
) AS $$
BEGIN
    RETURN QUERY SELECT 
        (SELECT COUNT(*) FROM Gateways),
        (SELECT COUNT(*) FROM Gateways WHERE Status = 1),
        (SELECT COUNT(*) FROM Devices),
        (SELECT COUNT(*) FROM Devices WHERE Status = 1),
        (SELECT COUNT(*) FROM Users),
        (SELECT COUNT(*) FROM Alerts WHERE Status <> 'Resolved');
END;
$$ LANGUAGE plpgsql;

-- 创建触发器
-- 触发器
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_trigger WHERE tgname = 'trg_update_gateway_updatedat'
    ) THEN
        CREATE TRIGGER trg_update_gateway_updatedat
        BEFORE UPDATE ON Gateways
        FOR EACH ROW
        EXECUTE FUNCTION update_updated_at();
    END IF;
END $$;

DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_trigger WHERE tgname = 'trg_update_device_updatedat'
    ) THEN
        CREATE TRIGGER trg_update_device_updatedat
        BEFORE UPDATE ON Devices
        FOR EACH ROW
        EXECUTE FUNCTION update_updated_at();
    END IF;
END $$;

-- 更新 UpdatedAt 的通用函数
CREATE OR REPLACE FUNCTION update_updated_at()
RETURNS TRIGGER AS $$
BEGIN
  NEW.UpdatedAt = now();
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- 提交事务
COMMIT;
