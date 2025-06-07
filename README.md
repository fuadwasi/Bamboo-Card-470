# 🛒 nopCommerce Custom Deployment (Dockerized)

This repository provides a containerized nopCommerce setup with:

- ✅ A custom **Discount Plugin**: `DiscountRules.DiscountOnOrderCount`
- ✅ Custom **Misc Plugin**: `NopStation.Core`
- ✅ Custom **Misc Plugin**: `Misc.BambooCard.Core`
- ✅ Custom **Misc Plugin** for api: `NopStation.Plugin.Misc.AdminApi`
- ✅ A **JWT-secured API endpoint** for order retrieval by email
- ✅ Docker-based local development and testing
- ✅ Cloud-ready deployment support (AWS & Azure)

---

## 📁 Project Structure

```
/
├── Dockerfile                   # Multi-stage Docker build
├── docker-compose.yml          # Services: nopCommerce + SQL Server
├── entrypoint.sh               # App start script
├── /src/
│   ├── /Presentation/Nop.Web/  # Main nopCommerce app
│   └── /Plugins/
│       └── /DiscountRules.DiscountOnOrderCount/  # Custom plugin
│       └── /NopStation.Core/  # Custom plugin
│       └── /Misc.BambooCard.Core/  # Custom plugin
│       └── /NopStation.Plugin.Misc.AdminApi/  # Custom plugin
```

---

## ⚙️ Installation: Custom Discount Plugin

The plugin applies a configurable discount for loyal customers:
- 🔁 Customers with 3+ previous orders get a discount (default: 10%)
- 🔧 Discount percentage is configurable via the admin panel

### 🪛 Steps

1. Start the application (see Docker instructions below).
2. Navigate to:
   ```
   http://localhost:8010/Admin
   ```
3. Log in with the admin credentials created during installation.
4. Go to:
   ```
   Configuration > Local Plugins
   ```
5. Find `DiscountRules.CustomerOrderHistory` → Click **Install** and **Restart** the application.
6. Configure the discount from:
   ```
   Promotions > Discounts
   ```
7. Configure a Discount as per the need and → Click **Save and Continue Edit**
8. After saving go to **Requirements** section.
9. **Discount requirement type**  → Select → **Discount requirement for customer after X orders**
10. **Order Count**  → Set Value 
11. **Discount requirement type**  → Select → **Processing** , **Complete** (Or you can select as per you requirment. But must need to select 1 at least)
12. Click **Save**

Now thhe discount will be work based on the configuration. 

**Note:** At the time of the install the plugin will be automatically creates a discoutn applied on OrderTotal discount of 10% with this requirment rule with 3 orders and Order Status of Complete, Process, Prending.

---

## 📝 Configuration: Checkout Attribute – Gift Message

NopCommerce out of the box support checkout attribute. By creating checkout attribute this requirment can be fullfilled easily.

### 🪛 Steps

1. Start the application (see Docker instructions below).
2. Navigate to:
   ```
   http://localhost:8010/Admin
   ```
3. Log in with the admin credentials created during installation.
4. Go to:
   ```
   Catalog > Checkout attributes
   ```
5. Click **Add new** (top right corner).
6. Fill in the following fields:
   - **Name**: `Gift Message`
   - **Text prompt**: `Enter your gift message (required)`
   - **Required**: ✅ Check this box
   - **Attribute control type**: `TextBox`
7. Optional fields:
   - **Display order**: `1` (or any number depending on where you want it to appear)
   - **Limited to stores**: Select if applicable
8. Click **Save**

Now the **Gift Message** field will appear on the checkout page and must be filled in by the customer before placing an order. The message will be saved and visible in the order details section in both admin and customer views.

**Note:** There is also a customer misc plgin name **Misc.BambooCard.Core** on install it automaically insert the **Gift Message** checkout attribute.


## ⚙️ Allow to search by "Name" on the product attribute page: Installation: Misc.BambooCard.Core Plugin for 

The plugin enable the functionality to  search by "Name" on the product attribute page. It has a dependency to NopStation.Core plguin. To install Misc.BambooCard.Core need to install the NopStation Core Plugin and apply the license key.

### 🪛 Steps

1. Start the application (see Docker instructions below).
2. Navigate to:
   ```
   http://localhost:8010/Admin
   ```
3. Log in with the admin credentials created during installation.
4. Go to:
   ```
   Configuration > Local Plugins
   ```
5. Find `NopStation Core` → Click **Install**.
6. Find `BambooCard Core` → Click **Install**.
7. **Restart** the application to complete the installation.
8. Navigate to:
   ```
   http://localhost:8010/Admin
   ```
9. Go to:
   ```
   NopStation > Core settings > License
   ```
10. On the **License string** field put the provided license → click **Save**

After **License string** is successfully applied the new product attribute functionality will be enabled. Now you can go to the Product Attribute Page and can search with Attribute Name



## ⚙️ API Development (Order Retrieval): Installation: Nop-Station Admin API Plugin for 

The **Nop-Station Admin API** plugin enable the functionality of Web Api. It has a dependency to NopStation.Core plguin. To install Nop-Station Admin API need to install the NopStation Core Plugin and apply the license key.

### 🪛 Steps

1. Start the application (see Docker instructions below).
2. Navigate to:
   ```
   http://localhost:8010/Admin
   ```
3. Log in with the admin credentials created during installation.
4. Go to:
   ```
   Configuration > Local Plugins
   ```
5. Find `NopStation Core` → Click **Install**.
6. Find `Nop-Station Admin API` → Click **Install**.
7. **Restart** the application to complete the installation.
8. Navigate to:
   ```
   http://localhost:8010/Admin
   ```
9. Go to:
   ```
   NopStation > Core settings > License
   ```
10. On the **License string** field put the provided license → click **Save**

After **License string** is successfully applied the new product attribute functionality will be enabled. Now you can go to the Product Attribute Page and can search with Attribute Name


## 🔐 API: Admin Authentication & Order List

This project includes admin-authenticated APIs. To use the api properly you need to login first. After login Postman automatically set the token if login is successful. Then you can access the other apis.


This project includes admin-authenticated APIs. To use the api properly you need to login first. After login Postman automatically set the token if login is successful. Then you can access the other apis.

```http
  POST /admincustomer/login
```

| Parameter       | Type     | Description                              |
| :-------------- | :------- | :--------------------------------------- |
| `Email`         | `string` | **Required**. Admin email address        |
| `Password`      | `string` | **Required**. Admin password             |
| `RememberMe`    | `bool`   | Optional. Persist session across requests |

#### Login

Authenticates the admin and returns a JWT token in the response under `Data.Token`.

---

```http
  GET /admincustomer/logout
```

| Header         | Type     | Description                             |
| :------------- | :------- | :-------------------------------------- |
| `Admin-Token`  | `string` | **Required**. Token to invalidate       |

#### Logout

Terminates the admin session and invalidates the token.

---

```http
  GET /order/List
```

| Header         | Type     | Description                             |
| :------------- | :------- | :-------------------------------------- |
| `Admin-Token`  | `string` | **Required**. Token from login response |
| `Admin-NST`    | `string` | Optional. Session tracking key          |
| `User-Agent`   | `string` | Optional. Client identifier             |

#### Get Order List

Returns a list of all orders available to the authenticated admin user.

---

```http
  POST /order/List
```

| Header         | Type     | Description                             |
| :------------- | :------- | :-------------------------------------- |
| `Admin-Token`  | `string` | **Required**. Token from login response |
| `Admin-NST`    | `string` | Optional. Session tracking key          |
| `User-Agent`   | `string` | Optional. Client identifier             |

| Body Field                  | Type       | Description                            |
| :-------------------------- | :--------- | :------------------------------------- |
| `StartDate`                | `string`   | Optional. Filter start date            |
| `EndDate`                  | `string`   | Optional. Filter end date              |
| `ShippingStatusIds`        | `int[]`    | Optional. Filter by shipping statuses  |
| `BillingEmail`             | `string`   | Optional. Customer's billing email     |
| `BillingPhoneEnabled`      | `bool`     | Default: true                          |
| `Page`, `PageSize`, etc.   | `int`      | Pagination controls                    |

#### Get Order List (Filtered)

Returns filtered list of orders using advanced search options. Response supports paging.

---

```http
  POST /order/CustomerOrderList
```

| Header         | Type     | Description                             |
| :------------- | :------- | :-------------------------------------- |
| `Admin-Token`  | `string` | **Required**. Token from login response |
| `Admin-NST`    | `string` | Optional. Session tracking key          |
| `User-Agent`   | `string` | Optional. Client identifier             |

| Body Field       | Type     | Description                             |
| :--------------- | :------- | :-------------------------------------- |
| `CustomerEmail`  | `string` | Customer's email address  |
| `Page`           | `int`    | Page number                             |
| `PageSize`       | `int`    | Number of items per page                |

#### Get Orders by Customer Email

Returns a list of orders of the customer with the provided email. Response supports paging. If email is null or empty then it will consider orders for all customer. This can be modify based on the requirment.


## 🐳 Docker: Build and Run Locally

### 🔧 Prerequisites

- [Docker](https://www.docker.com/products/docker-desktop)
- [Docker Compose](https://docs.docker.com/compose/install/)

### ▶️ Start Application

```bash
docker-compose up --build
```

Then open:
```
http://localhost:8010
```

> On the setup page, use:
- **Database Server**: `nopcommerce_database`
- **DB Username**: `sa`
- **DB Password**: `nopCommerce_db_password`

### 📦 Environment Variables

In `docker-compose.yml`:

```yaml
SA_PASSWORD: "nopCommerce_db_password"
ACCEPT_EULA: "Y"
MSSQL_PID: "Express"
```

---

## 📄 Docker Compose Overview

```yaml
services:
  nopcommerce_web:
    build: .
    ports:
      - "8010:80"
    depends_on:
      - nopcommerce_database
    volumes:
      - ./App_Data:/app/App_Data
      - ./logs:/app/logs
  nopcommerce_database:
    image: "mcr.microsoft.com/mssql/server:2019-latest"
    environment:
      SA_PASSWORD: "nopCommerce_db_password"
      ACCEPT_EULA: "Y"
      MSSQL_PID: "Express"
volumes:
  nopcommerce_data:
```

---

## ☁️ Cloud Deployment Instructions

### 🚀 Deploy to AWS ECS (Fargate)

1. **Build & push the image:**
   ```bash
   docker build -t <your-ecr-repo>/nopcommerce-app .
   docker push <your-ecr-repo>/nopcommerce-app
   ```

2. **Create RDS (SQL Server Express)**:
   - Use `sa`/`nopCommerce_db_password`

3. **Set up ECS Fargate Service**:
   - Image: from ECR
   - Environment: pass DB connection string

4. **Open port 80** for public access

---

### ☁️ Deploy to Azure (Web App for Containers)

1. **Push to Azure Container Registry (ACR)**:
   ```bash
   docker build -t youracr.azurecr.io/nopcommerce-app .
   docker push youracr.azurecr.io/nopcommerce-app
   ```

2. **Create Azure SQL Database**

3. **Create a Web App**:
   - Type: Docker
   - Source: ACR
   - Set App Settings for DB connection string

---

## 📬 Postman Collection

You can use the included Postman collection (`nopcommerce-api.postman_collection.json`) to:
- Authenticate and get JWT token
- Call the `orders/by-email` endpoint

---

## ✅ Summary

| Feature                        | Status |
|-------------------------------|--------|
| Custom Plugin        | ✅ Yes |
| API Endpoint with JWT Auth    | ✅ Yes |
| Dockerfile                    | ✅ Yes |
| docker-compose.yml            | ✅ Yes |
| Cloud Deployment Instructions | ✅ Yes |
| Postman Collection            | ✅ Yes |

---

## 👨‍💻 Author

**Fuad Hasan**  
Senior nopCommerce Developer  
[Github](https://github.com/fuadwasi) | [LinkedIn](https://www.linkedin.com/in/fuadwasi/) 

