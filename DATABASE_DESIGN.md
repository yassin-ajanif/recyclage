# Recyclage — Database Design

Design document based on the handwritten ledger tables (فاتورة المشتريات، المبيعات، المصاريف، حساب آخر شهر، حساب بين شريكي، فاتورة زبون، فاتورة الفرنيسور).

**Database engine (suggested):** SQLite  
**Money fields:** `DECIMAL(18,2)` — stored in Moroccan dirhams (DH)  
**Dates:** `TEXT` (ISO `YYYY-MM-DD`) or `DATE`

---

## Table groups

### Setup — user maintains (3 tables)

| Table | Arabic | Fields user fills |
|-------|--------|-------------------|
| `Clients` | الزبائن | Name, Phone, ICE |
| `Suppliers` | الموردون | Name, Phone, ICE |
| `Products` | المنتجات | Name + type (`ForBuying` / `ForSale`) |

### Daily entry — user types rows (5 tables)

| Table | Arabic |
|-------|--------|
| `PurchaseInvoices` | فاتورة المشتريات |
| `Sales` | مبيعات |
| `Expenses` | **فاتورة المصاريف** |
| `AyoubPayments` | دفع لي أيوب (حساب الشركاء — أيوب) |
| `MustafaReturns` | رجوع لمصطفى (حساب الشركاء — مصطفى) |

### Reports — system computes (3 views)

| View | Arabic | Source |
|------|--------|--------|
| `MonthlyClosingReport` | حساب في آخر شهر | `Sales` + `PurchaseInvoices` + `Expenses` + `AyoubPayments` + `MustafaReturns` by month |
| `SupplierInvoiceReport` | فاتورة الفرنيسور | `PurchaseInvoices` by `SupplierId` |
| `CustomerInvoiceReport` | فاتورة زبون | `Sales` by `ClientId` |

**Total physical tables: 8** (3 setup + 5 entry)  
**Total views: 3**

Product type on **`Products` only:** `CHECK (ProductType IN ('ForBuying', 'ForSale'))`. No separate expense-types table.

---

## Full diagram

```
                         RECYCLAGE DATABASE (SQLite)
                                 │
         ┌───────────────────────┼───────────────────────┐
         │                       │                       │
   STORED (8 tables)        USER TYPES DAILY        COMPUTED (3 views)
         │                       │                       │
  Clients ──────────────► Sales ──┼──► CustomerInvoiceReport
  Suppliers ────► PurchaseInvoices ──► SupplierInvoiceReport
  Products ─────────────► (both)  │
                                  ├── Expenses  ← فاتورة المصاريف
                                  ├── AyoubPayments   ← دفع لي أيوب
                                  ├── MustafaReturns  ← رجوع لمصطفى
                                  └──► MonthlyClosingReport
```

---

## Entity relationship

```mermaid
erDiagram
    Suppliers ||--o{ PurchaseInvoices : SupplierId
    Clients ||--o{ Sales : ClientId
    Products |o--o{ PurchaseInvoices : ForBuying
    Products |o--o{ Sales : ForSale

    Clients {
        int Id PK
        text Name
        text Phone
        text Ice
    }

    Suppliers {
        int Id PK
        text Name
        text Phone
        text Ice
    }

    Products {
        int Id PK
        text Name
        text ProductType
    }

    PurchaseInvoices {
        int Id PK
        text Date
        int SupplierId FK
        int ProductId FK NULL
        decimal Total
    }

    Sales {
        int Id PK
        text Date
        int ClientId FK
        int ProductId FK NULL
        decimal Total
    }

    Expenses {
        int Id PK
        text Date
        text ExpenseType
        decimal Amount
    }

    AyoubPayments {
        int Id PK
        text Date
        decimal Amount
    }

    MustafaReturns {
        int Id PK
        text Date
        decimal Amount
    }
```

---

## فاتورة المشتريات / فاتورة المبيعات → `PurchaseInvoices` / `Sales`

Your notebook columns map directly. Both ledgers share one column set — the only
difference is the partner: `SupplierId` for purchases, `ClientId` for sales.

```
╔═════════════════════════════════════════════════════ فاتورة المشتريات  →  PurchaseInvoices ═════════════════════════════════════════════════════╗
╠════════════╦════════════╦══════════════════╦════════════════════════╦════════════════════╦════════════╦══════════════╦════════════════╦════════════╦════════════╣
║══ الباقي ══║═══ دفع ════║═ المجموع ب د.م ══║═ نقل مدفوع من المورد ══║══ نقل مدفوع مني ═══║═══ ثمن ════║════ مورد ════║═════ منتج ═════║══ الكمية ══║══ تاريخ ═══║
╠════════════╬════════════╬══════════════════╬════════════════════════╬════════════════════╬════════════╬══════════════╬════════════════╬════════════╬════════════╣
║ Remaining ═║═══ Paid ═══║═════ Total ══════║ TransportPaidByPartner ║ TransportPaidByMe ═║ UnitPrice ═║═ SupplierId ═║══ ProductId ═══║═ Quantity ═║═══ Date ═══║
║ DECIMAL(18,2) ║ DECIMAL(18,2) ║═ DECIMAL(18,2) ══║════ DECIMAL(18,2) ═════║══ DECIMAL(18,2) ═══║ DECIMAL(18,2) ║═ INTEGER FK ═║ INTEGER NULL FK ║ DECIMAL(18,3) ║═══ TEXT ═══║
╚════════════╪════════════╪══════════════════╪════════════════════════╪════════════════════╪════════════╪══════════════╪════════════════╪════════════╪════════════╝
```

```
╔═══════════════════════════════════════════════════════════════════ فاتورة المبيعات  →  Sales ═══════════════════════════════════════════════════════════════════╗
╠════════════╦════════════╦══════════════════╦════════════════════════╦════════════════════╦════════════╦══════════════╦════════════════╦════════════╦════════════╣
║══ الباقي ══║═══ دفع ════║═ المجموع ب د.م ══║═ نقل مدفوع من الزبون ══║══ نقل مدفوع مني ═══║═══ ثمن ════║════ زبون ════║═════ منتج ═════║══ الكمية ══║══ تاريخ ═══║
╠════════════╬════════════╬══════════════════╬════════════════════════╬════════════════════╬════════════╬══════════════╬════════════════╬════════════╬════════════╣
║ Remaining ═║═══ Paid ═══║═════ Total ══════║ TransportPaidByPartner ║ TransportPaidByMe ═║ UnitPrice ═║══ ClientId ══║══ ProductId ═══║═ Quantity ═║═══ Date ═══║
║ DECIMAL(18,2) ║ DECIMAL(18,2) ║═ DECIMAL(18,2) ══║════ DECIMAL(18,2) ═════║══ DECIMAL(18,2) ═══║ DECIMAL(18,2) ║═ INTEGER FK ═║ INTEGER NULL FK ║ DECIMAL(18,3) ║═══ TEXT ═══║
╚════════════╪════════════╪══════════════════╪════════════════════════╪════════════════════╪════════════╪══════════════╪════════════════╪════════════╪════════════╝
```
          (read right → left in Arabic UI)

| `#` | تاريخ | الكمية | منتج | مورد / زبون | ثمن | نقل مدفوع مني | نقل مدفوع من المورد / الزبون | المجموع ب د.م | دفع | الباقي |
|-----|-------|--------|------|-------------|------|---------------|---------------------------|---------------|------|--------|
| 1 | `Date` | `Quantity` | `ProductId` | `SupplierId` / `ClientId` | `UnitPrice` | `TransportPaidByMe` | `TransportPaidByPartner` | `Total` | `Paid` | `Remaining` |

### The two transport columns

Transport is recorded twice, because **who pays for it decides whether it costs us
anything**:

| Column | Arabic | In `Total`? |
|--------|--------|-------------|
| `TransportPaidByMe` | نقل مدفوع مني | **Yes** — it is our cost |
| `TransportPaidByPartner` | نقل مدفوع من المورد (`Sales`: نقل مدفوع من الزبون) | **No** — the partner carries it |

```
  Transport we paid        10 × 20  +  30   = 230
  Transport partner paid   10 × 20  +  30   = 200
                             ───────  ▲
                              excluded from Total
```

**Only one of the two is filled per line.** Typing into either box zeroes the other, so
a line can never carry both at once; clearing a box leaves the other alone, so emptying
one field never wipes the amount already in the other. A save that somehow carries both
is rejected with «املأ خانة النقل واحدة فقط» rather than silently dropping an amount —
`HasBothTransportFields` guards it.

`Total` is therefore always `Quantity × UnitPrice + TransportPaidByMe`.

The grid keeps a **summary row** under the last line, repeating three of the columns
for the whole selection — «المجموع» (`GrandTotal»), «دفع» (`TotalPaid`) and «الباقي»
(`TotalRemaining`), the last one painted red when negative.

`ProductId` is **nullable** in both ledgers, so a line can carry handed money with no
product; its `منتج` cell then reads «مبلغ نقدي». See
[Cash-only lines](#cash-only-lines-مبلغ-نقدي) for the amounts it stores.

---

## فاتورة المصاريف → `Expenses` table

Your notebook columns map directly:

```
╔══════════════════════════════════════════════════════════╗
║              فاتورة المصاريف  →  Expenses              ║
╠═══════════════╦══════════════════════╦═══════════════════╣
║    الواجب     ║    نوع المصاريف      ║      تاريخ        ║
╠═══════════════╬══════════════════════╬═══════════════════╣
║    Amount     ║    ExpenseType       ║      Date         ║
╚═══════════════╩══════════════════════╩═══════════════════╝
         (read right → left in Arabic UI)
```

| Notebook | DB column | Type |
|----------|-----------|------|
| تاريخ | `Date` | TEXT |
| نوع المصاريف | `ExpenseType` | TEXT (e.g. إيجار، وقود، صيانة) |
| الواجب | `Amount` | DECIMAL(18,2) |

No FK — user types expense type as text on each row. No `ExpenseTypes` lookup table.

---

## Stored schema (ASCII)

```
┌─────────────────┐   ┌─────────────────┐   ┌──────────────────────────────┐
│     Clients     │   │    Suppliers    │   │          Products            │
├─────────────────┤   ├─────────────────┤   ├──────────────────────────────┤
│ Id, Name        │   │ Id, Name        │   │ Id, Name                     │
│ Phone, Ice      │   │ Phone, Ice      │   │ ProductType CHECK            │
└────────┬────────┘   └────────┬────────┘   │  ('ForBuying','ForSale')     │
         │ ClientId            │ SupplierId └──────────────┬───────────────┘
         ▼                     ▼                           │ ProductId
┌─────────────────┐   ┌─────────────────┐                 │
│      Sales      │   │PurchaseInvoices │◄────────────────┘
└─────────────────┘   └─────────────────┘

┌────────────────────────────┐   ┌────────────────────────────┐
│         Expenses           │   │        AyoubPayments       │
│    (فاتورة المصاريف)       │   │       (دفع لي أيوب)        │
├────────────────────────────┤   ├────────────────────────────┤
│ Id                      PK │   │ Id                      PK │
│ Date          ← تاريخ      │   │ Date          ← تاريخ      │
│ ExpenseType   ← نوع المصاريف│   │ Amount        ← المبلغ     │
│ Amount        ← الواجب     │   │ Details       ← التفاصيل   │
│ Description                │   │ CreatedAt                  │
│ CreatedAt                  │   └────────────────────────────┘
└────────────────────────────┘   ┌────────────────────────────┐
                                 │       MustafaReturns       │
                                 │      (رجوع لمصطفى)         │
                                 ├────────────────────────────┤
                                 │ Id                      PK │
                                 │ Date          ← تاريخ      │
                                 │ Amount        ← المبلغ     │
                                 │ Details       ← التفاصيل   │
                                 │ CreatedAt                  │
                                 └────────────────────────────┘
```

---

## Setup tables

### `Clients` — الزبائن

| Column | Arabic | Type |
|--------|--------|------|
| `Id` | — | INTEGER PK |
| `Name` | الاسم | TEXT |
| `Phone` | الهاتف | TEXT |
| `Ice` | ICE | TEXT |

### `Suppliers` — الموردون

| Column | Arabic | Type |
|--------|--------|------|
| `Id` | — | INTEGER PK |
| `Name` | الاسم | TEXT |
| `Phone` | الهاتف | TEXT |
| `Ice` | ICE | TEXT |

### `Products` — المنتجات

| Column | Arabic | Type | Notes |
|--------|--------|------|-------|
| `Id` | — | INTEGER | PK |
| `Name` | الاسم / النوع | TEXT | |
| `ProductType` | للشراء / للبيع | TEXT | `CHECK IN ('ForBuying', 'ForSale')` |
| `DefaultUnit` | الوحدة | TEXT | |
| `DefaultUnitPrice` | ثمن افتراضي | DECIMAL(18,2) | |
| `IsActive` | — | INTEGER | |
| `CreatedAt` | — | TEXT | |

---

## Entry tables

### `PurchaseInvoices` — فاتورة المشتريات

| Column | Arabic | Type |
|--------|--------|------|
| `Date` | تاريخ | TEXT |
| `Quantity` | الكمية | DECIMAL(18,3) |
| `ProductId` | منتج | INTEGER **NULL** FK → `Products` (**ForBuying**) |
| `SupplierId` | مورد | INTEGER FK → `Suppliers` |
| `UnitPrice` | ثمن | DECIMAL(18,2) |
| `TransportPaidByMe` | نقل مدفوع مني | DECIMAL(18,2) |
| `TransportPaidByPartner` | نقل مدفوع من المورد | DECIMAL(18,2) |
| `Total` | المجموع ب DH | DECIMAL(18,2) |
| `Paid` | دفع | DECIMAL(18,2) |
| `Remaining` | الباقي | DECIMAL(18,2) |

### `Sales` — مبيعات

| Column | Arabic | Type |
|--------|--------|------|
| `Date` | تاريخ | TEXT |
| `Quantity` | الكمية | DECIMAL(18,3) |
| `ProductId` | منتج | INTEGER **NULL** FK → `Products` (**ForSale**) |
| `ClientId` | زبون | INTEGER FK → `Clients` |
| `UnitPrice` | ثمن | DECIMAL(18,2) |
| `TransportPaidByMe` | نقل مدفوع مني | DECIMAL(18,2) |
| `TransportPaidByPartner` | نقل مدفوع من الزبون | DECIMAL(18,2) |
| `Total` | المجموع ب DH | DECIMAL(18,2) |
| `Paid` | دفع | DECIMAL(18,2) |
| `Remaining` | الباقي | DECIMAL(18,2) |

#### Cash-only lines (مبلغ نقدي)

`ProductId` is **nullable** in both tables, so a line can record money handed over with no
product — paid to a supplier, or received from a client. In the UI the user leaves «منتج»
empty and types the amount in «دفع».

| Case | `ProductId` | `Total` | `Remaining` |
|------|-------------|---------|-------------|
| Product line | product id | `Quantity × UnitPrice + TransportPaidByMe` | `Total − Paid` |
| Cash-only line | `NULL` | `0` | `−Paid` |

Because a cash-only line has no invoice value of its own, its `Total` is zero and the
handed amount appears on `Remaining` as a negative balance — hand over 100 and the row
reads `Total 0`, `Remaining −100`, which the `NegativeAmountBrushConverter` paints red.
This keeps handed money out of the goods value while still carrying it in the balance.

**Consequence for `MonthlyClosingReport`:** it sums `Total`, so a cash-only line adds
nothing to `MonthlyPurchases` / `MonthlySales`. A cash-only line is a balance movement, not
a purchase or a sale.

A cash-only line is rejected on save when `Paid = 0`; a product line is still rejected when
`Quantity ≤ 0`. Such a line is labelled «مبلغ نقدي» everywhere a product name is shown, and
it never appears in the product picker.

**Sign rule:** `Paid` is hand-entered and is rejected when negative on both tables
(«قيمة الدفع لا يمكن أن تكون سالبة.») — a rejected save writes nothing and leaves an
already-saved row untouched. `Paid = 0` stays legal on a product line. `Remaining` is
computed rather than entered, so it is the field that legitimately carries negative values.

In the entry grids, a line with no product disables «الكمية», «ثمن» and «نقل» — none of them
contribute to a cash-only line — leaving «دفع» as the only amount box. The three boxes bind
`IsEnabled` to `HasProduct`, which raises change notifications whenever the product is picked
or cleared. Any values left in them are kept, so a line toggled back to a product is restored
intact.

### `Expenses` — فاتورة المصاريف

| Column | Arabic | Type | Notes |
|--------|--------|------|-------|
| `Id` | — | INTEGER | PK |
| `Date` | تاريخ | TEXT | NOT NULL |
| `ExpenseType` | نوع المصاريف | TEXT | NOT NULL — free text per row |
| `Amount` | الواجب | DECIMAL(18,2) | NOT NULL |
| `Description` | — | TEXT | Optional |
| `CreatedAt` | — | TEXT | Audit |

**Index:** `Date`, `ExpenseType`

### `AyoubPayments` — دفع لي أيوب

| Column | Arabic | Type | Notes |
|--------|--------|------|-------|
| `Id` | — | INTEGER | PK |
| `Date` | تاريخ | TEXT | NOT NULL |
| `Amount` | دفع لي أيوب | DECIMAL(18,2) | NOT NULL |
| `Details` | التفاصيل | TEXT | NOT NULL — kept for existing data, not shown on the page |
| `CreatedAt` | — | TEXT | Audit |

**Index:** `Date`

### `MustafaReturns` — رجوع لمصطفى

| Column | Arabic | Type | Notes |
|--------|--------|------|-------|
| `Id` | — | INTEGER | PK |
| `Date` | تاريخ | TEXT | NOT NULL |
| `Amount` | رجوع لمصطفى | DECIMAL(18,2) | NOT NULL |
| `Details` | التفاصيل | TEXT | NOT NULL — kept for existing data, not shown on the page |
| `CreatedAt` | — | TEXT | Audit |

**Index:** `Date`

Each ledger is independent: «الباقي» is computed in the app as the running total of that table's `Amount`, and is never stored.

---

## Computed views

### `MonthlyClosingReport` — حساب في آخر شهر

Computed in `MonthlyClosingReportViewModel` for the selected month (kept as the `MonthlyClosingReport` logical view for the notebook layout).

| Column | Arabic | Computed from |
|--------|--------|---------------|
| `MonthlyPurchases` | دخول في شهر | `SUM(PurchaseInvoices.Total)` of the selected month |
| `Expenses` | المصاريف في شهر | `SUM(Expenses.Amount)` of the selected month |
| `TotalIncomePurchases` | المجموعة (د+م) | month purchases + month expenses |
| `MonthlySales` | خروج في شهر | `SUM(Sales.Total)` of the selected month |
| `AyoubPayments` | دفع لي أيوب | `SUM(AyoubPayments.Amount)` over **all** rows — same number as the «دفع لي أيوب» page total |
| `MustafaReturns` | رجوع لمصطفى | `SUM(MustafaReturns.Amount)` over **all** rows — same number as the «رجوع لمصطفى» page total |
| `CapitalRemaining` | الباقي | month sales − (month purchases + month expenses) |
| `CompanyBalance` | رأس مال الشركة | `AppSettings.CompanyCapital` + cumulative (sales − purchases − expenses) up to the end of the selected month |

Summary strip below the table (not a table column):

| Field | Arabic | Computed from |
|-------|--------|---------------|
| `AyoubPaymentsTotal` | دفع لي أيوب | `SUM(AyoubPayments.Amount)` — same number as the total on the «دفع لي أيوب» page |
| `MustafaReturnsTotal` | رجوع لمصطفى | `SUM(MustafaReturns.Amount)` — same number as the total on the «رجوع لمصطفى» page |
| `RemainingToPartner` | الباقي لأيوب / الباقي لمصطفى | `ABS(SUM(AyoubPayments.Amount) − SUM(MustafaReturns.Amount))` (bigger total − smaller), labelled with the partner holding the bigger total |

### `SupplierInvoiceReport` / `CustomerInvoiceReport`

Unchanged — filtered views on `PurchaseInvoices` / `Sales`.

---

## Notebook → database

| Notebook | DB |
|----------|-----|
| فاتورة المشتريات | `PurchaseInvoices` |
| مبيعات | `Sales` |
| **فاتورة المصاريف** | **`Expenses`** |
| حساب بين شريكي (أيوب) | `AyoubPayments` |
| حساب بين شريكي (مصطفى) | `MustafaReturns` |
| حساب آخر شهر | `MonthlyClosingReport` (view) |
| فاتورة الفرنيسور | `SupplierInvoiceReport` (view) |
| فاتورة زبون | `CustomerInvoiceReport` (view) |

---

## Changelog

| Date | Change |
|------|--------|
| 2026-10-02 | **Split `TransportCost` into two mutually exclusive columns** in `PurchaseInvoices` and `Sales`: `TransportPaidByMe` (نقل مدفوع مني) and `TransportPaidByPartner` (نقل مدفوع من المورد / من الزبون). Only `TransportPaidByMe` feeds `Total`, so `Total = Quantity × UnitPrice + TransportPaidByMe`; transport the partner pays is recorded but excluded. Typing in either box zeroes the other, and a save carrying both is rejected («املأ خانة النقل واحدة فقط»). The old column was **renamed** to `TransportPaidByMe` rather than copied, so existing amounts — and every already-stored `Total` — stay correct; `TransportPaidByPartner` starts at 0. Migration `SplitTransportCosts`. |
| 2026-09-29 | **Made `ProductId` nullable in `PurchaseInvoices` and `Sales`** so a line can record money handed over with no product. The amount is typed in «دفع»; such a line stores `Total = 0` and `Remaining = −Paid`, so a handed amount reads as a negative balance rather than as a purchase or a sale. Cash-only lines are labelled «مبلغ نقدي». Migration `AllowCashOnlyInvoiceLines`. |
| 2026-09-28 | **Split `PartnerTransactions` into two independent tables**: `AyoubPayments` (دفع لي أيوب) and `MustafaReturns` (رجوع لمصطفى) — one table per partner ledger page. Existing rows were copied by SQL inside the migration, then the combined table was dropped. Money moved: 57000.00 → `AyoubPayments`, 2000.00 → `MustafaReturns`. 8 stored tables, 3 views. |
| 2026-09-23 | **Restored `Expenses` table** for فاتورة المصاريف (Date, ExpenseType, Amount). 7 stored tables, 3 views. |
