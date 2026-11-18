# CongoBiz API Documentation

**Base URL (local development):** `http://localhost:5176/api`
**Real-time hub:** `http://localhost:5176/hubs/orders`

## Authentication

Every endpoint except `POST /auth/register` and `POST /auth/login` requires a JWT bearer token.

```
Authorization: Bearer <token>
```

Tokens are issued at login/register and expire after 8 hours. The token encodes `restaurantId` and the user's role as claims — every endpoint automatically scopes data to the caller's own restaurant; there is no way to read or write another restaurant's data with a valid token.

### Roles

| Role | Assigned to |
|---|---|
| `Owner` | Automatically assigned to whoever registers the restaurant |
| `Manager` | Invited by Owner/Manager |
| `Cashier` | Invited by Owner/Manager |
| `Waiter` | Invited by Owner/Manager |
| `Kitchen` | Invited by Owner/Manager |

Endpoints marked **Owner/Manager only** reject other roles with `403 Forbidden`.

---

## Auth

### `POST /auth/register`
Creates a new restaurant and its first user (`Owner` role). No auth required.

**Body**
```json
{
  "restaurantName": "Mama Congo Restaurant",
  "firstName": "David",
  "lastName": "Mumpasa",
  "email": "owner@mamacongo.co.za",
  "password": "SecurePass123!"
}
```

**Response `200`**
```json
{
  "token": "eyJhbGciOi...",
  "restaurantId": "b6690b6c-362f-4117-8a25-123d5e97a128",
  "email": "owner@mamacongo.co.za",
  "role": "Owner"
}
```

**Errors**
- `400` — email already registered, or password doesn't meet requirements (8+ chars, upper, lower, digit)

---

### `POST /auth/login`
No auth required.

**Body**
```json
{ "email": "owner@mamacongo.co.za", "password": "SecurePass123!" }
```

**Response `200`** — same shape as register.

**Errors**
- `401` — invalid email or password

---

## Categories

### `GET /categories`
Returns all menu categories for the caller's restaurant, ordered by `displayOrder`.

**Response `200`**
```json
[
  { "id": "...", "name": "Main Meals", "description": "Hearty Congolese classics", "displayOrder": 0, "isActive": true }
]
```

### `POST /categories`
**Owner/Manager only.**

**Body**
```json
{ "name": "Main Meals", "description": "Hearty Congolese classics", "displayOrder": 0 }
```
**Response `200`** — created `CategoryDto`.

### `DELETE /categories/{id}`
**Owner/Manager only.** Returns `204` on success, `404` if not found (or belongs to another restaurant).

---

## Menu Items

### `GET /menu-items`
Returns every menu item for the caller's restaurant (all categories).

**Response `200`**
```json
[
  {
    "id": "...", "categoryId": "...", "name": "Poulet Mayo",
    "description": "Grilled chicken with mayo sauce", "price": 120.00,
    "imageUrl": null, "isAvailable": true
  }
]
```

### `POST /menu-items`
**Owner/Manager only.**

**Body**
```json
{
  "categoryId": "...",
  "name": "Poulet Mayo",
  "description": "Grilled chicken with mayo sauce",
  "price": 120.00,
  "imageUrl": null
}
```
**Errors:** `400` if `categoryId` doesn't belong to the caller's restaurant.

### `PUT /menu-items/{id}`
**Owner/Manager only.** Full replace — all fields required.

**Body**
```json
{
  "name": "Poulet Mayo",
  "description": "Grilled chicken with mayo sauce",
  "price": 130.00,
  "imageUrl": null,
  "isAvailable": true
}
```

### `DELETE /menu-items/{id}`
**Owner/Manager only.** `204` on success.

---

## Orders

Orders are the one resource that broadcasts over **SignalR** (see [Real-time](#real-time-signalr) below) — every create and status change pushes to all connected clients for that restaurant, in addition to returning the normal HTTP response.

### `GET /orders`
Returns all orders for the caller's restaurant, newest first, with items expanded.

**Response `200`**
```json
[
  {
    "id": "...", "orderNumber": "#1024", "status": "Preparing", "orderType": "DineIn",
    "tableId": "...", "subtotal": 280.00, "tax": 0, "total": 280.00,
    "createdAt": "2026-08-21T10:30:00Z",
    "items": [
      { "menuItemId": "...", "menuItemName": "Poulet Mayo", "quantity": 2, "unitPrice": 120.00, "subtotal": 240.00, "notes": "No onions" }
    ]
  }
]
```

### `POST /orders`
Creates an order and broadcasts `OrderCreated` to the restaurant's SignalR group.

**Body**
```json
{
  "tableId": "...",
  "orderType": "DineIn",
  "items": [
    { "menuItemId": "...", "quantity": 2, "notes": "No onions" }
  ]
}
```
`orderType` — one of `DineIn`, `Takeaway`, `Delivery`. `tableId` is optional (omit for takeaway/delivery).

**Errors:** `400` if any `menuItemId` doesn't belong to the caller's restaurant.

### `PUT /orders/{id}/status`
Updates status and broadcasts `OrderStatusChanged`.

**Body**
```json
{ "status": "Preparing" }
```
Valid values: `Pending`, `Confirmed`, `Preparing`, `Ready`, `Completed`, `Cancelled`.

---

## Real-time (SignalR)

**Hub URL:** `/hubs/orders`
**Auth:** JWT passed as `?access_token=` query param (handled automatically by the SignalR client SDK) — the hub requires a custom `OnMessageReceived` handler server-side to read it from the query string instead of the `Authorization` header, since browsers can't set custom headers during a WebSocket handshake.

### Client → Server methods
| Method | Params | Purpose |
|---|---|---|
| `JoinRestaurantGroup` | `restaurantId: string` | Subscribes the connection to that restaurant's broadcast group. Call immediately after connecting. |
| `LeaveRestaurantGroup` | `restaurantId: string` | Unsubscribes. Called automatically on disconnect. |

### Server → Client events
| Event | Payload | Fired when |
|---|---|---|
| `OrderCreated` | Full `OrderDto` (same shape as `GET /orders` items) | A new order is created via `POST /orders` |
| `OrderStatusChanged` | Full `OrderDto` | An order's status changes via `PUT /orders/{id}/status` |

Broadcasts are scoped per-restaurant group — a connection only receives events for the restaurant it joined.

---

## Tables

### `GET /tables`
Returns all tables, ordered by table number.

**Response `200`**
```json
[ { "id": "...", "tableNumber": 4, "capacity": 4, "status": "Available" } ]
```

### `POST /tables`
**Body**
```json
{ "tableNumber": 4, "capacity": 4 }
```

### `PUT /tables/{id}/status`
**Body**
```json
{ "status": "Occupied" }
```
Valid values: `Available`, `Occupied`, `Reserved`.

### `DELETE /tables/{id}`
`204` on success.

---

## Customers

### `GET /customers`
Newest first.

### `POST /customers`
**Body**
```json
{
  "firstName": "Jean",
  "lastName": "Mbala",
  "phone": "0821234567",
  "email": null,
  "notes": null
}
```
Only `firstName` is required.

### `DELETE /customers/{id}`
`204` on success.

---

## Inventory

Stock quantities are **never edited directly** — every change goes through `/inventory/movements`, which keeps a full audit trail (`StockMovement` records) and recalculates the item's quantity server-side. This matches the original spec's requirement to track *why* stock changed, not just the current number.

### `GET /inventory`
Returns active items only. `isLow` is computed server-side (`quantity <= minimumQuantity`).

**Response `200`**
```json
[
  {
    "id": "...", "name": "Chicken", "unit": "kg",
    "quantity": 32.00, "minimumQuantity": 10.00, "costPerUnit": 45.00,
    "isLow": false
  }
]
```

### `POST /inventory`
Creates a new item with a starting quantity.

**Body**
```json
{ "name": "Chicken", "unit": "kg", "quantity": 35.00, "minimumQuantity": 10.00, "costPerUnit": 45.00 }
```

### `POST /inventory/movements`
Records a stock change and returns the updated item.

**Body**
```json
{ "inventoryItemId": "...", "type": "Sale", "quantity": 3, "reason": "Order #1024" }
```
`type` — one of `Purchase`, `Sale`, `Adjustment`, `Waste`, `Return`.
`Purchase` and `Return` **add** to quantity; `Sale`, `Adjustment`, `Waste` **subtract**. Quantity is clamped at 0 (never goes negative).

---

## Expenses

### `GET /expenses`
Newest first (by `date`).

### `POST /expenses`
**Body**
```json
{ "category": "Electricity", "description": "Monthly bill", "amount": 2500.00, "date": "2026-08-01" }
```

---

## Employees

**All endpoints in this section are Owner/Manager only.**

### `GET /employees`
Returns everyone tied to the caller's restaurant, including inactive users.

**Response `200`**
```json
[ { "id": "...", "firstName": "Sarah", "lastName": "K", "email": "sarah@mamacongo.co.za", "role": "Cashier", "isActive": true } ]
```

### `POST /employees/invite`
Creates a new user account under the caller's restaurant.

**Body**
```json
{
  "firstName": "Sarah",
  "lastName": "K",
  "email": "sarah@mamacongo.co.za",
  "temporaryPassword": "TempPass123!",
  "role": "Cashier"
}
```
`role` — one of `Manager`, `Cashier`, `Waiter`, `Kitchen` (not `Owner` — there is exactly one Owner, set at registration).

**Errors:** `400` if email already exists or role is invalid.

### `PUT /employees/{id}/role`
**Body**
```json
{ "role": "Manager" }
```
`204` on success.

### `PUT /employees/{id}/deactivate`
No body. Sets `isActive` to `false`. `204` on success.

---

## Reports

All three endpoints are read-only aggregations — no request body, optional date-range query params.

### `GET /reports/sales?from=2026-08-01&to=2026-08-21`
`from`/`to` are optional `date` values (`yyyy-MM-dd`); defaults to the last 30 days. Excludes cancelled orders.

**Response `200`**
```json
{
  "totalSales": 4850.00,
  "orderCount": 62,
  "averageOrderValue": 78.23,
  "bestSellers": [
    { "menuItemName": "Poulet Mayo", "quantitySold": 40, "revenue": 4800.00 }
  ],
  "dailyBreakdown": [
    { "date": "2026-08-21", "total": 4850.00, "orderCount": 62 }
  ]
}
```

### `GET /reports/profit?from=2026-08-01&to=2026-08-21`
Revenue (non-cancelled orders) minus expenses over the same window.

**Response `200`**
```json
{
  "totalRevenue": 4850.00,
  "totalExpenses": 2500.00,
  "netProfit": 2350.00,
  "expensesByCategory": [
    { "category": "Electricity", "total": 2500.00 }
  ]
}
```

### `GET /reports/inventory-valuation`
No date range — current snapshot only. `value = quantity × costPerUnit` per item.

**Response `200`**
```json
{
  "totalValue": 2925.00,
  "items": [
    { "name": "Chicken", "quantity": 32.00, "unit": "kg", "costPerUnit": 45.00, "value": 1440.00 }
  ]
}
```

---

## Enum reference

For quick lookup when reading raw values from SQL Server or writing SQL directly (stored as `int`):

**OrderStatus:** `0` Pending · `1` Confirmed · `2` Preparing · `3` Ready · `4` Completed · `5` Cancelled
**OrderType:** `0` DineIn · `1` Takeaway · `2` Delivery
**TableStatus:** `0` Available · `1` Occupied · `2` Reserved
**StockMovementType:** `0` Purchase · `1` Sale · `2` Adjustment · `3` Waste · `4` Return

The API always sends/receives these as **strings** (e.g. `"Preparing"`), not integers — the table above only matters if you're querying the database directly.

---

## Error format

Validation failures return either a plain string or an array of strings in the body, depending on the source (ASP.NET Identity errors come back as arrays):

```json
"Category not found."
```
or
```json
["Passwords must be at least 8 characters."]
```

Standard HTTP status codes apply throughout: `200` success, `204` success/no content, `400` validation error, `401` missing/invalid token, `403` insufficient role, `404` not found or belongs to another restaurant.

---

## Postman quick-start

1. `POST /auth/login` → copy `token`
2. Set collection-level **Authorization → Bearer Token** → paste it
3. Every other request in the collection now authenticates automatically

Swagger UI is also available at `http://localhost:5176/swagger` in development, with a built-in **Authorize** button for the same purpose.
