
CREATE DATABASE QL_SAN_THE_THAO;
GO
USE QL_SAN_THE_THAO;
GO

CREATE TABLE KHACHHANG (
    MaKH INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    SDT NVARCHAR(20),
    AnhDaiDien NVARCHAR(200) DEFAULT '/Content/images/default-avatar.png',
    NgayTao DATETIME DEFAULT GETDATE(),
	MatKhau varchar(255)
);
GO

CREATE TABLE TaiKhoanUser (
    MaUser INT PRIMARY KEY,
    TenDangNhap NVARCHAR(50) NOT NULL UNIQUE,
    MatKhau NVARCHAR(255) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    SoDienThoai NVARCHAR(15),
    TrangThai NVARCHAR(20) DEFAULT N'Hoạt động'
        CHECK (TrangThai IN (N'Hoạt động', N'Khóa'))
);
GO

CREATE TABLE DoiTac (
    MaDoiTac CHAR(5) PRIMARY KEY,
    TenDoiTac NVARCHAR(100),
    Email NVARCHAR(100),
    SoDienThoai NVARCHAR(15),
    DiaChi NVARCHAR(255),
    MoTa NVARCHAR(255)
);
GO

CREATE TABLE LoaiTheThao (
    MaLoai CHAR(5) PRIMARY KEY,
    TenLoai NVARCHAR(50) NOT NULL,
    MoTa NVARCHAR(255)
);
GO

CREATE TABLE KhungGio (
    MaKhung CHAR(5) PRIMARY KEY,
    GioBatDau TIME NOT NULL,
    GioKetThuc TIME NOT NULL,
    CHECK (GioKetThuc > GioBatDau)
);
GO

CREATE TABLE DichVu (
    MaDV CHAR(5) PRIMARY KEY,
    TenDichVu NVARCHAR(100),
    DonViTinh NVARCHAR(50),
	SoLuongTon INT NOT NULL DEFAULT 0,
    DonGia DECIMAL(10,2)
);
GO
CREATE TABLE PhieuNhapDichVu (
    MaPhieuNhap CHAR(5) PRIMARY KEY,
    NgayNhap DATETIME DEFAULT GETDATE(),
    NguoiNhap NVARCHAR(100),
    GhiChu NVARCHAR(255)
);
GO
CREATE TABLE ChiTietPhieuNhap (
    MaCTPN CHAR(5) PRIMARY KEY,
    MaPhieuNhap CHAR(5),
    MaDV CHAR(5),
    SoLuongNhap INT CHECK (SoLuongNhap > 0),
    DonGiaNhap DECIMAL(10,2),

    FOREIGN KEY (MaPhieuNhap) REFERENCES PhieuNhapDichVu(MaPhieuNhap),
    FOREIGN KEY (MaDV) REFERENCES DichVu(MaDV)
);
GO


CREATE TABLE Voucher (
    MaVoucher CHAR(5) PRIMARY KEY,
    MaCode NVARCHAR(50) UNIQUE,
    PhanTramGiam INT CHECK (PhanTramGiam BETWEEN 1 AND 100),
    NgayBatDau DATE,
    NgayKetThuc DATE,
    SoLanSuDungToiDa INT DEFAULT 1
);
GO

CREATE TABLE Trainer (
    TrainerId INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    MoTaNgan NVARCHAR(250),
    KhuVuc NVARCHAR(100),
    AnhDaiDienUrl NVARCHAR(500),
    SoSaoDanhGia DECIMAL(3,2) DEFAULT 0,
    GiaMoiBuoi DECIMAL(18,2),
    SoNamKinhNghiem INT,
    ChungChi NVARCHAR(MAX),
    IsAcademy BIT DEFAULT 0,
    AgeGroup NVARCHAR(50),
    Services NVARCHAR(255)
);
GO

CREATE TABLE TinTuc (
    MaTin INT IDENTITY(1,1) PRIMARY KEY,
    TieuDe NVARCHAR(255) NOT NULL,
    MoTaNgan NVARCHAR(500),
    NoiDung NVARCHAR(MAX),
    HinhAnh NVARCHAR(500),
    NgayDang DATETIME DEFAULT GETDATE(),
    TacGia NVARCHAR(100) DEFAULT 'Playo'
);
GO

CREATE TABLE KhuTheThao (
    MaKhu CHAR(5) PRIMARY KEY,
    MaDoiTac CHAR(5),
    TenKhu NVARCHAR(100),
    DiaChi NVARCHAR(255),
    SoDienThoai NVARCHAR(15),
    GioMoCua TIME,
    GioDongCua TIME,
    MoTa NVARCHAR(255),
    KhuVucHienThi NVARCHAR(50),
    FOREIGN KEY (MaDoiTac) REFERENCES DoiTac(MaDoiTac)
);
GO

CREATE TABLE SanTheThao (
    MaSan CHAR(5) PRIMARY KEY,
    MaKhu CHAR(5),
    MaLoai CHAR(5),
    TenSan NVARCHAR(100),
    GiaThueTheoGio DECIMAL(10,2) CHECK (GiaThueTheoGio > 0),
    TrangThai NVARCHAR(20)
        CHECK (TrangThai IN (N'Bảo trì', N'Đã đặt', N'Trống')),
    MoTa NVARCHAR(255),
    HinhAnh NVARCHAR(500) DEFAULT '/Content/images/default-venue.jpg',
    IsNoiBat BIT DEFAULT 0,
    FOREIGN KEY (MaKhu) REFERENCES KhuTheThao(MaKhu),
    FOREIGN KEY (MaLoai) REFERENCES LoaiTheThao(MaLoai)
);
GO

CREATE TABLE DatSan (
    MaDat CHAR(5) PRIMARY KEY,
    MaUser INT,
    MaSan CHAR(5),
    NgayDat DATE NOT NULL,
    MaKhung CHAR(5),
    TongTien DECIMAL(12,2),
    TrangThai NVARCHAR(30)
        CHECK (TrangThai IN (N'Chờ xác nhận', N'Đã xác nhận', N'Đã thanh toán', N'Đã hủy')),
    GhiChu NVARCHAR(255),
    FOREIGN KEY (MaUser) REFERENCES TaiKhoanUser(MaUser),
    FOREIGN KEY (MaSan) REFERENCES SanTheThao(MaSan),
    FOREIGN KEY (MaKhung) REFERENCES KhungGio(MaKhung)
);
GO

CREATE TABLE ThanhToan (
    MaThanhToan CHAR(5) PRIMARY KEY,
    MaDat CHAR(5),
    SoTien DECIMAL(12,2),
    PhuongThuc NVARCHAR(50),
    NgayThanhToan DATETIME DEFAULT GETDATE(),
    TrangThai NVARCHAR(20)
        CHECK (TrangThai IN (N'Chưa thanh toán', N'Đã thanh toán')),
    FOREIGN KEY (MaDat) REFERENCES DatSan(MaDat)
);
GO

CREATE TABLE ChiTietDichVu (
    MaCTDV CHAR(5) PRIMARY KEY,
    MaDat CHAR(5),
    MaDV CHAR(5),
    SoLuong INT CHECK (SoLuong > 0),
    FOREIGN KEY (MaDat) REFERENCES DatSan(MaDat),
    FOREIGN KEY (MaDV) REFERENCES DichVu(MaDV)
);
GO

CREATE TABLE DanhGia (
    MaDanhGia CHAR(5) PRIMARY KEY,
    MaUser INT,
    MaSan CHAR(5),
    Diem INT CHECK (Diem BETWEEN 1 AND 5),
    NoiDung NVARCHAR(255),
    NgayDanhGia DATE DEFAULT GETDATE(),
    FOREIGN KEY (MaUser) REFERENCES TaiKhoanUser(MaUser),
    FOREIGN KEY (MaSan) REFERENCES SanTheThao(MaSan)
);
GO

CREATE TABLE BaoTri (
    MaBaoTri CHAR(5) PRIMARY KEY,
    MaSan CHAR(5),
    NgayBatDau DATE,
    NgayKetThuc DATE,
    NoiDung NVARCHAR(255),
    TrangThai NVARCHAR(30),
    FOREIGN KEY (MaSan) REFERENCES SanTheThao(MaSan)
);
GO

CREATE TABLE TranDau (
    MaTran CHAR(5) PRIMARY KEY,
    MaSan CHAR(5),
    MaLoai CHAR(5),
    MaNguoiToChuc INT,
    Ngay DATE,
    MaKhung CHAR(5),
    TenTran NVARCHAR(100),
    MoTa NVARCHAR(255),
    SoNguoiToiDa INT,
    TrangThai NVARCHAR(20)
        CHECK (TrangThai IN (N'Riêng tư', N'Công khai')),
    LoaiHinh_Tran NVARCHAR(50) DEFAULT N'Giao lưu',
    TrinhDo NVARCHAR(50) DEFAULT N'Cơ bản',
    FOREIGN KEY (MaSan) REFERENCES SanTheThao(MaSan),
    FOREIGN KEY (MaLoai) REFERENCES LoaiTheThao(MaLoai),
    FOREIGN KEY (MaNguoiToChuc) REFERENCES TaiKhoanUser(MaUser),
    FOREIGN KEY (MaKhung) REFERENCES KhungGio(MaKhung)
);
GO

CREATE TABLE NguoiThamGiaTran (
    MaTran CHAR(5),
    MaUser INT,
    TrangThai NVARCHAR(20)
        CHECK (TrangThai IN (N'Hủy', N'Tham gia')),
    PRIMARY KEY (MaTran, MaUser),
    FOREIGN KEY (MaTran) REFERENCES TranDau(MaTran),
    FOREIGN KEY (MaUser) REFERENCES TaiKhoanUser(MaUser)
);
GO

CREATE TABLE ApDungVoucher (
    MaDat CHAR(5),
    MaVoucher CHAR(5),
    PRIMARY KEY (MaDat, MaVoucher),
    FOREIGN KEY (MaDat) REFERENCES DatSan(MaDat),
    FOREIGN KEY (MaVoucher) REFERENCES Voucher(MaVoucher)
);
GO

CREATE TABLE TrainerMonTheThao (
    TrainerId INT,
    MaLoai CHAR(5),
    PRIMARY KEY (TrainerId, MaLoai),
    FOREIGN KEY (TrainerId) REFERENCES Trainer(TrainerId),
    FOREIGN KEY (MaLoai) REFERENCES LoaiTheThao(MaLoai)
);
GO

CREATE TABLE LichTrongSan (
    MaSan CHAR(5),
    Ngay DATE,
    MaKhung CHAR(5),
    TrangThai NVARCHAR(20)
        CHECK (TrangThai IN (N'Bảo trì', N'Đã đặt', N'Trống')),
    PRIMARY KEY (MaSan, Ngay, MaKhung),
    FOREIGN KEY (MaSan) REFERENCES SanTheThao(MaSan),
    FOREIGN KEY (MaKhung) REFERENCES KhungGio(MaKhung)
);
GO
CREATE TABLE HoSoNguoiChoi (
    MaUser INT PRIMARY KEY,
    HoTen NVARCHAR(100),
    NgaySinh DATE,
    GioiTinh NVARCHAR(10),
    AnhDaiDien NVARCHAR(255),
    SoTranThamGia INT DEFAULT 0,
    SoTranThang INT DEFAULT 0,
    DiemUyTin INT DEFAULT 0,
    GioiThieu NVARCHAR(255),
    FOREIGN KEY (MaUser) REFERENCES TaiKhoanUser(MaUser)
);
GO


INSERT INTO DoiTac VALUES
('DT001', N'Công ty Thể thao VinSports', NULL, N'02838383838', N'Quận 7, TP.HCM', NULL),
('DT002', N'Trung tâm TDTT Quận 10', NULL, N'02839393939', N'Quận 10, TP.HCM', NULL),
('P0001', N'Công ty TDTT Nam Huy', N'contact@namhuy.vn', N'090777888', N'120 Lũy Bán Bích, Tân Phú', N'Đơn vị hợp tác chính'),
('P0002', N'Trung tâm SportLife', N'info@sportlife.vn', N'091234567', N'200 Ung Văn Khiêm, Bình Thạnh', N'Trung tâm liên kết');
GO

INSERT INTO LoaiTheThao VALUES
('L0001', N'Bóng đá', N'Sân cỏ nhân tạo'),
('L0002', N'Cầu lông', N'Sân tiêu chuẩn đôi'),
('L0003', N'Tennis', N'Sân xi măng'),
('L0004', N'Bóng rổ', N'Sân ngoài trời'),
('L0005', N'Bơi lội', N'Hồ bơi tiêu chuẩn 25m');
GO

INSERT INTO KhungGio VALUES
('K0001','06:00','07:00'),
('K0002','07:00','08:00'),
('K0003','17:00','18:00'),
('K0004','18:00','19:00'),
('K0005','19:00','20:00'),
('K001','06:00','07:30'),
('K002','07:30','09:00'),
('K003','09:00','10:30'),
('K004','16:00','17:30'),
('K005','17:30','19:00');
GO

INSERT INTO DichVu VALUES
('DV001', N'Nước suối', N'Chai', 10000),
('DV002', N'Thuê bóng', N'Quả', 30000),
('DV003', N'Thuê vợt', N'Cây', 40000),
('DV004', N'Khăn lạnh', N'Cái', 5000);
GO

INSERT INTO Voucher VALUES
('V0001', 'MORNING10', 10, '2025-11-01', '2025-11-30', 3),
('V0002', 'SPORT20', 20, '2025-11-01', '2025-12-31', 5);
GO


INSERT INTO TaiKhoanUser VALUES
('1','nam123','123','nam@gmail.com','090111222',N'Hoạt động'),
('2','huongtran','abc','huong@gmail.com','090333444',N'Hoạt động'),
('3','tuanle','123','tuan@gmail.com','090555666',N'Hoạt động'),
('4','admin01','admin','admin@gmail.com','0911223344',N'Hoạt động');
GO

INSERT INTO HoSoNguoiChoi VALUES
('1',N'Nguyễn Văn Nam','2000-03-15',N'Nam',NULL,12,7,90,N'Đam mê bóng đá sáng sớm'),
('2',N'Trần Thị Hương','2001-06-21',N'Nữ',NULL,6,3,80,N'Thích cầu lông'),
('3',N'Lê Minh Tuấn','1999-08-20',N'Nam',NULL,3,1,50,N'Người mới tham gia'),
('4',N'Admin Hệ thống',NULL,NULL,NULL,0,0,100,N'Quản trị viên');
GO

INSERT INTO KhuTheThao VALUES
('K0001','P0001',N'Thể thao Tân Phú',N'120 Lũy Bán Bích','090111222','06:00','22:00',N'Khu đa năng',NULL),
('K0002','P0002',N'SportLife Bình Thạnh',N'200 Ung Văn Khiêm','090333444','06:00','21:00',N'Khu luyện tập cao cấp',NULL),
('KT001','DT001',N'Nisha Millets Swimming Academy',N'123 Nguyễn Văn Linh',NULL,NULL,NULL,NULL,N'Quận 7'),
('KT002','DT002',N'Pickle Social Club',N'45 Lý Thường Kiệt',NULL,NULL,NULL,NULL,N'Quận 10');
GO

INSERT INTO SanTheThao VALUES
('S0001','K0001','L0001',N'Sân A - Bóng đá',250000,N'Đã đặt',N'Sân 5 người','bongda.jpeg',0),
('S0002','K0001','L0002',N'Sân B - Cầu lông',120000,N'Đã đặt',N'Sân đôi','caulong.jpg',0),
('S0003','K0001','L0003',N'Sân C - Tennis',180000,N'Đã đặt',N'Sân xi măng','tenis.jpeg',0),
('S0004','K0002','L0004',N'Sân D - Bóng rổ',200000,N'Bảo trì',N'Sân ngoài trời','bongro.jpg',0),
('S0005','K0002','L0005',N'Hồ E - Bơi lội',100000,N'Trống',N'Hồ 25m','boiloi.jpeg',0);
GO


INSERT INTO DatSan VALUES
('D0001','1','S0001','2025-11-12','K001',250000,N'Đã xác nhận',N'Đá sớm'),
('D0002','2','S0002','2025-11-13','K002',120000,N'Đã thanh toán',N'Tập luyện'),
('D0003','3','S0003','2025-11-14','K004',180000,N'Đã xác nhận',N'Tennis');
GO

INSERT INTO ThanhToan VALUES
('T0001','D0002',120000,N'Chuyển khoản',GETDATE(),N'Đã thanh toán');
GO

INSERT INTO DanhGia VALUES
('DG001','2','S0001',5,N'Sân sạch','2025-11-10'),
('DG002','1','S0002',4,N'Hơi trơn','2025-11-11');
GO

INSERT INTO BaoTri VALUES
('B0001','S0004','2025-11-01','2025-11-03',N'Sửa đèn',N'Đã hoàn thành');
GO

INSERT INTO TranDau VALUES
('M0001','S0001','L0001','1','2025-11-15','K004',N'Trận sáng T7',N'Giao hữu',10,N'Công khai',DEFAULT,DEFAULT),
('M0002','S0003','L0003','2','2025-11-16','K005',N'Tennis cuối tuần',N'Giao lưu',4,N'Công khai',DEFAULT,DEFAULT);
GO

INSERT INTO NguoiThamGiaTran VALUES
('M0001','1',N'Tham gia'),
('M0001','2',N'Tham gia'),
('M0002','2',N'Tham gia'),
('M0002','3',N'Tham gia');


--Function--
GO
CREATE FUNCTION dbo.FUNC_TinhTienThue
(
    @MaSan CHAR(5),
    @MaKhung CHAR(5)
)
RETURNS DECIMAL(12,2)
AS
BEGIN
    DECLARE @Gia DECIMAL(10,2),
            @GioBD TIME,
            @GioKT TIME;

    SELECT @Gia = GiaThueTheoGio
    FROM SanTheThao
    WHERE MaSan = @MaSan;

    SELECT @GioBD = GioBatDau,
           @GioKT = GioKetThuc
    FROM KhungGio
    WHERE MaKhung = @MaKhung;

    IF @Gia IS NULL OR @GioBD IS NULL OR @GioKT IS NULL
        RETURN 0;

    RETURN (DATEDIFF(MINUTE, @GioBD, @GioKT) / 60.0) * @Gia;
END;
GO


CREATE FUNCTION dbo.FUNC_TongTienDichVuTheoDatSan
(
    @MaDat CHAR(5)
)
RETURNS DECIMAL(12,2)
AS
BEGIN
    DECLARE @Tong DECIMAL(12,2);

    SELECT @Tong = ISNULL(SUM(ct.SoLuong * dv.DonGia), 0)
    FROM ChiTietDichVu ct
    JOIN DichVu dv ON ct.MaDV = dv.MaDV
    WHERE ct.MaDat = @MaDat;

    RETURN @Tong;
END;
GO


CREATE FUNCTION dbo.FUNC_TinhTongDoanhThuThang
(
    @Thang INT,
    @Nam INT
)
RETURNS DECIMAL(14,2)
AS
BEGIN
    DECLARE @Tong DECIMAL(14,2);

    SELECT @Tong = ISNULL(SUM(DoanhThuSan + DoanhThuDichVu), 0)
    FROM DoanhThuNgay
    WHERE MONTH(Ngay) = @Thang
      AND YEAR(Ngay) = @Nam;

    RETURN @Tong;
END;
GO

--Procedure--
CREATE PROCEDURE dbo.PROC_TaoDatSan
    @MaUser CHAR(5),
    @MaSan CHAR(5),
    @MaKhung CHAR(5),
    @NgayDat DATE
AS
BEGIN
    IF EXISTS (
        SELECT 1 FROM DatSan
        WHERE MaSan = @MaSan
          AND MaKhung = @MaKhung
          AND NgayDat = @NgayDat
          AND TrangThai <> N'Đã hủy'
    )
    BEGIN
        RAISERROR(N'Sân đã được đặt trong khung giờ này',16,1);
        RETURN;
    END;

    DECLARE @MaDat CHAR(5),
            @Tien DECIMAL(12,2);

    SELECT @MaDat = 'D' + RIGHT('0000' + CAST(ISNULL(MAX(CAST(SUBSTRING(MaDat,2,4) AS INT)),0)+1 AS VARCHAR),4)
    FROM DatSan;

    SET @Tien = dbo.FUNC_TinhTienThue(@MaSan, @MaKhung);

    INSERT INTO DatSan
    VALUES (@MaDat, @MaUser, @MaSan, @NgayDat, @MaKhung, @Tien, N'Chờ xác nhận', NULL);

    PRINT N'Đặt sân thành công. Mã: ' + @MaDat;
END;
GO


CREATE PROCEDURE dbo.PROC_XacNhanThanhToan
    @MaDat CHAR(5),
    @PhuongThuc NVARCHAR(50)
AS
BEGIN
    BEGIN TRAN;

    DECLARE @Tien DECIMAL(12,2),
            @MaTT CHAR(5);

    SELECT @Tien = TongTien FROM DatSan WHERE MaDat = @MaDat;

    SELECT @MaTT = 'T' + RIGHT('0000' + CAST(ISNULL(MAX(CAST(SUBSTRING(MaThanhToan,2,4) AS INT)),0)+1 AS VARCHAR),4)
    FROM ThanhToan;

    INSERT INTO ThanhToan
    VALUES (@MaTT, @MaDat, @Tien, @PhuongThuc, GETDATE(), N'Đã thanh toán');

    UPDATE DatSan
    SET TrangThai = N'Đã thanh toán'
    WHERE MaDat = @MaDat;

    COMMIT TRAN;
END;
GO


CREATE PROCEDURE dbo.PROC_TaoTranDau
    @MaSan CHAR(5),
    @MaLoai CHAR(5),
    @MaNguoiToChuc CHAR(5),
    @Ngay DATE,
    @MaKhung CHAR(5),
    @TenTran NVARCHAR(100),
    @MoTa NVARCHAR(255),
    @SoNguoi INT
AS
BEGIN
    DECLARE @MaTran CHAR(5);

    SELECT @MaTran = 'M' + RIGHT('0000' + CAST(ISNULL(MAX(CAST(SUBSTRING(MaTran,2,4) AS INT)),0)+1 AS VARCHAR),4)
    FROM TranDau;

    INSERT INTO TranDau
    VALUES (@MaTran,@MaSan,@MaLoai,@MaNguoiToChuc,@Ngay,@MaKhung,@TenTran,@MoTa,@SoNguoi,N'Công khai',DEFAULT,DEFAULT);

    PRINT N'Đã tạo trận đấu: ' + @MaTran;
END;
GO

--Trigger--
CREATE TRIGGER TRG_CapNhatTrangThaiSan
ON DatSan
AFTER INSERT
AS
BEGIN
    UPDATE SanTheThao
    SET TrangThai = N'Đã đặt'
    WHERE MaSan IN (SELECT MaSan FROM inserted);
END;
GO


CREATE TRIGGER TRG_CapNhatHoSoNguoiChoi
ON NguoiThamGiaTran
AFTER INSERT
AS
BEGIN
    UPDATE HoSoNguoiChoi
    SET SoTranThamGia = SoTranThamGia + 1
    WHERE MaUser IN (SELECT MaUser FROM inserted);
END;
GO


CREATE TRIGGER TRG_CapNhatTonKhoDichVu
ON ChiTietPhieuNhap
AFTER INSERT
AS
BEGIN
    UPDATE DichVu
    SET SoLuongTon = SoLuongTon + i.SoLuongNhap
    FROM DichVu dv
    JOIN inserted i ON dv.MaDV = i.MaDV;
END;
GO

CREATE TRIGGER TRG_CheckTrungLichDatSan
ON DatSan
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM DatSan d
        JOIN inserted i ON d.MaSan = i.MaSan 
                        AND d.NgayDat = i.NgayDat 
                        AND d.MaKhung = i.MaKhung
        WHERE d.MaDat <> i.MaDat 
          AND d.TrangThai <> N'Đã hủy'
          AND i.TrangThai <> N'Đã hủy'
    )
    BEGIN
        RAISERROR (N'Lỗi: Sân này đã có người đặt trong ngày và khung giờ này!', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO

INSERT INTO KhungGio (MaKhung, GioBatDau, GioKetThuc) VALUES
('K0006', '08:00', '09:00'),
('K0007', '09:00', '10:00'),
('K0008', '10:00', '11:00'),
('K0009', '11:00', '12:00'),
('K0010', '12:00', '13:00'),
('K0011', '13:00', '14:00'),
('K0012', '14:00', '15:00'),
('K0013', '15:00', '16:00'),
('K0014', '16:00', '17:00'),
('K0015', '20:00', '21:00'),
('K0016', '21:00', '22:00'),
('K0017', '22:00', '23:00'); 

INSERT INTO KhungGio (MaKhung, GioBatDau, GioKetThuc) VALUES
('K006', '10:30', '12:00'),
('K007', '13:30', '15:00'),
('K008', '15:00', '16:30'),
('K009', '19:00', '20:30'), 
('K010', '20:30', '22:00'),
('K011', '22:00', '23:30');
GO

INSERT INTO Trainer (HoTen, MoTaNgan, KhuVuc, AnhDaiDienUrl, SoSaoDanhGia, GiaMoiBuoi, SoNamKinhNghiem, ChungChi, IsAcademy, AgeGroup, Services) 
VALUES 
(N'Trần Đức Huy', N'Chuyên gia huấn luyện bóng đá trẻ, cựu tuyển thủ quốc gia.', N'Hồ Chí Minh', N'/Content/images/trainers/DucHuy.jpg', 4.8, 300000, 5, N'Bằng C AFC, Chứng chỉ Sư phạm Thể thao', 0, N'6-15 tuổi', N'Bóng đá căn bản, Kỹ thuật nâng cao'),

(N'Võ Thị Thủy Tiên', N'HLV Yoga & Pilates với 5 năm kinh nghiệm trị liệu.', N'Hà Nội', N'/Content/images/trainers/VTTT.jpg', 4.9, 250000, 5, N'Yoga Alliance 200H, Pilates Mat Level 1', 0, N'Mọi lứa tuổi', N'Yoga trị liệu, Pilates thảm'),

(N'Lê Minh Tuấn', N'Huấn luyện viên Tennis chuyên nghiệp, đào tạo VĐV năng khiếu.', N'Đà Nẵng', N'/Content/images/trainers/DucHuy2.jpg', 4.7, 500000, 8, N'ITF Level 2, Chứng nhận HLV Quốc gia', 1, N'10-18 tuổi', N'Tennis chuyên nghiệp, Sửa lỗi kỹ thuật'),

(N'Hoàng Văn Nam', N'HLV Thể hình (Gym/PT) chuyên giảm mỡ tăng cơ.', N'Hồ Chí Minh', N'/Content/images/trainers/DucHuy3.jpg', 4.6, 400000, 6, N'NASM-CPT, Chứng chỉ Dinh dưỡng thể thao', 0, N'18-40 tuổi', N'Giảm cân, Tăng cơ, Bodybuilding');

CREATE TRIGGER TRG_TruTonKhoKhiDatDichVu
ON ChiTietDichVu
AFTER INSERT
AS
BEGIN
    UPDATE DichVu
    SET SoLuongTon = SoLuongTon - i.SoLuong
    FROM DichVu dv
    JOIN inserted i ON dv.MaDV = i.MaDV;
END;
GO

ALTER TABLE TaiKhoanUser
ADD VaiTro NVARCHAR(50);
GO

UPDATE KHACHHANG
SET VaiTro = N'KhachHang';



ALTER TABLE KHACHHANG
ADD VaiTro NVARCHAR(50);
GO

SELECT * FROM KHACHHANG

UPDATE KHACHHANG
SET VaiTro = N'Admin'
WHERE MaKH = 3;