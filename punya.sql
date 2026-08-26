-- Setup Enablon Data Center (merged)
-- Jalankan sebagai root, lalu aplikasi pakai user enablon_app

-- 1) Buat database
CREATE DATABASE IF NOT EXISTS enablon_dc
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

-- 2) Buat user + password + grant
CREATE USER IF NOT EXISTS 'enablon_app'@'localhost'
  IDENTIFIED BY 'Enablon#2026!App';

ALTER USER 'enablon_app'@'localhost'
  IDENTIFIED BY 'Enablon#2026!App';

GRANT ALL PRIVILEGES ON enablon_dc.* TO 'enablon_app'@'localhost';
FLUSH PRIVILEGES;

-- 3) Pakai database
USE enablon_dc;

-- 4) Buat tabel asset
CREATE TABLE IF NOT EXISTS asset (
  asset_id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  asset_code VARCHAR(30) NOT NULL,
  asset_name VARCHAR(120) NOT NULL,
  asset_type ENUM('Server','Storage','Network','Power','Cooling','Other') NOT NULL DEFAULT 'Other',
  location VARCHAR(120) NULL,
  status ENUM('Active','Maintenance','Retired') NOT NULL DEFAULT 'Active',
  installed_date DATE NULL,
  notes TEXT NULL,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (asset_id),
  UNIQUE KEY uq_asset_code (asset_code),
  INDEX idx_asset_type (asset_type),
  INDEX idx_asset_status (status)
) ENGINE=InnoDB;

-- 5) Insert contoh data (opsional)
INSERT INTO asset (asset_code, asset_name, asset_type, location, status, installed_date, notes)
VALUES
('SRV001', 'Server Dell R740', 'Server', 'Rack A1', 'Active', '2025-01-10', 'Host aplikasi internal'),
('NET001', 'Core Switch 48 Port', 'Network', 'Rack N1', 'Maintenance', '2024-11-02', 'Penggantian modul')
ON DUPLICATE KEY UPDATE asset_name=VALUES(asset_name);
