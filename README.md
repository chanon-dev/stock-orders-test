# Stock Orders

ระบบแสดงสินค้าพร้อมระบบสต็อกและตะกร้าสินค้า พัฒนาด้วย Next.js (Frontend) และ .NET 10 C# (Backend)

## Tech Stack

| Layer     | Technology              |
|-----------|------------------------|
| Frontend  | Next.js 16, TypeScript |
| Backend   | .NET 10, ASP.NET Core  |
| Database  | PostgreSQL 16          |
| Container | Docker, Docker Compose |

## Prerequisites

### Docker (แนะนำ)

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### รันแบบไม่ใช้ Docker

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- PostgreSQL 16 (รันอยู่ที่ localhost:5432)

## วิธีรัน

### วิธีที่ 1 — Docker (แนะนำ)

```bash
docker compose up -d --build
```

เมื่อ build เสร็จ เข้าใช้งานได้ที่:

| Service     | URL                          |
|-------------|------------------------------|
| Frontend    | <http://localhost:3000>      |
| Backend API | <http://localhost:5001/api>  |
| Database    | localhost:5432               |

---

### วิธีที่ 2 — รันแบบไม่ใช้ Docker

#### 1. เตรียม Database

สร้าง PostgreSQL database ชื่อ `StockOrdersDb` (user: `postgres`, password: `postgres`) หรือแก้ connection string ใน [backend/src/StockOrders.Api/appsettings.json](backend/src/StockOrders.Api/appsettings.json)

#### 2. รัน Backend

```bash
cd backend
dotnet run --project src/StockOrders.Api
```

Backend จะรันที่ <http://localhost:5295> และจะทำ Migration + Seed ข้อมูลอัตโนมัติ

#### 3. รัน Frontend

สร้างไฟล์ `frontend/.env.local`:

```env
NEXT_PUBLIC_API_URL=http://localhost:5295/api
```

จากนั้นรัน:

```bash
cd frontend
npm install
npm run dev
```

เข้าใช้งานได้ที่:

| Service     | URL                              |
|-------------|----------------------------------|
| Frontend    | <http://localhost:3000>          |
| Backend API | <http://localhost:5295/api>      |
| API Docs    | <http://localhost:5295/scalar>   |

## API Endpoints

### Products

| Method | Path             | Description       |
|--------|-----------------|-------------------|
| GET    | /api/products   | ดึงรายการสินค้าทั้งหมด |

### Cart

| Method | Path                                    | Description              |
|--------|-----------------------------------------|--------------------------|
| GET    | /api/cart/{sessionId}                   | ดูตะกร้าสินค้า              |
| POST   | /api/cart/{sessionId}/items             | เพิ่มสินค้าลงตะกร้า          |
| PUT    | /api/cart/{sessionId}/items/{productId} | อัปเดตจำนวนสินค้าในตะกร้า    |
| DELETE | /api/cart/{sessionId}/items/{itemId}    | ลบสินค้าออกจากตะกร้า        |
| DELETE | /api/cart/{sessionId}/items             | ล้างตะกร้าทั้งหมด            |
| POST   | /api/cart/{sessionId}/checkout          | ชำระเงิน / ตัด Stock        |

## หยุดการทำงาน

```bash
docker compose down
```

หากต้องการลบข้อมูลใน Database ด้วย:

```bash
docker compose down -v
```
