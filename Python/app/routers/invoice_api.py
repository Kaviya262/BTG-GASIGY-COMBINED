from fastapi import APIRouter, HTTPException, Depends
from pydantic import BaseModel
from typing import List, Optional
from sqlalchemy import text
from ..database import engine 

router = APIRouter()

# ==========================================
# 1. DEFINE MODELS FIRST (Before using them)
# ==========================================

class InvoiceFilter(BaseModel):
    customerid: int = 0
    FromDate: str
    ToDate: str
    BranchId: int = 1

# This is the missing class causing your error
class InvoiceListItem(BaseModel):
    InvoiceId: int
    InvoiceNbr: str
    Salesinvoicesdate: str
    CustomerName: str
    PONumber: Optional[str] = ""
    CurrencyCode: Optional[str] = ""
    TotalAmount: float
    CalculatedPrice: float
    Status: str

# --- Request Models (Adjust to match your existing frontend payload) ---
class InvoiceDetailItem(BaseModel):
    gascodeid: int
    PickedQty: float
    UnitPrice: float
    Currencyid: int  # We need this to find the rate
    # ... other detail fields

class CreateInvoiceRequest(BaseModel):
    customerid: int
    Salesinvoicesdate: str
    items: List[InvoiceDetailItem]
    # ... other header fields

@router.post("/CreateInvoice")
async def create_invoice(invoice: CreateInvoiceRequest):
    async with engine.begin() as conn: # automatic transaction handling
        try:
            # 1. Create the Header first (Initial Insert)
            # We initialize CalculatedPrice to 0, we will update it after processing details
            header_query = text("""
                INSERT INTO btg_userpanel_uat.tbl_salesinvoices_header 
                (customerid, Salesinvoicesdate, TotalAmount, IsSubmitted, CalculatedPrice, createdby)
                VALUES (:cust, :date, 0, 0, 0, 1)
            """)
            result = await conn.execute(header_query, {
                "cust": invoice.customerid,
                "date": invoice.Salesinvoicesdate
            })
            new_header_id = result.lastrowid

            total_header_amount = 0.0
            total_calculated_price_idr = 0.0

            # 2. Process Details
            for item in invoice.items:
                # A. FETCH LIVE RATE (The "Freeze" Step)
                # If currency is IDR (usually id 1), rate is 1. Otherwise fetch from master.
                rate_query = text("""
                    SELECT COALESCE(ExchangeRate, 1) 
                    FROM btg_userpanel_uat.master_currency 
                    WHERE CurrencyId = :cid
                """)
                rate_result = await conn.execute(rate_query, {"cid": item.Currencyid})
                exchange_rate = rate_result.scalar() or 1.0

                # B. Calculate Line Totals
                line_total = item.PickedQty * item.UnitPrice
                
                # C. Calculate Frozen IDR Value for this line
                line_calculated_price = line_total * float(exchange_rate)

                # D. Add to Header Accumulators
                total_header_amount += line_total
                total_calculated_price_idr += line_calculated_price

                # E. Insert Detail with the FROZEN RATE
                detail_query = text("""
                    INSERT INTO btg_userpanel_uat.tbl_salesinvoices_details
                    (salesinvoicesheaderid, gascodeid, PickedQty, UnitPrice, TotalPrice, Currencyid, ExchangeRate)
                    VALUES (:hid, :gas, :qty, :price, :total, :cur, :rate)
                """)
                await conn.execute(detail_query, {
                    "hid": new_header_id,
                    "gas": item.gascodeid,
                    "qty": item.PickedQty,
                    "price": item.UnitPrice,
                    "total": line_total,
                    "cur": item.Currencyid,
                    "rate": exchange_rate # <--- Storing the rate permanently!
                })

            # 3. Update Header with Final Totals
            update_header = text("""
                UPDATE btg_userpanel_uat.tbl_salesinvoices_header
                SET TotalAmount = :total,
                    CalculatedPrice = :calc_price
                WHERE id = :hid
            """)
            await conn.execute(update_header, {
                "total": total_header_amount,
                "calc_price": total_calculated_price_idr, # <--- The Frozen IDR Total
                "hid": new_header_id
            })

            return {"status": "success", "message": "Invoice Created", "InvoiceId": new_header_id}

        except Exception as e:
            # engine.begin() will auto-rollback on error
            print(f"Error creating invoice: {e}")
            raise HTTPException(status_code=500, detail=str(e))
        
# --- API Endpoint ---
@router.post("/GetALLInvoices", response_model=List[InvoiceListItem])
async def get_all_invoices(filter_data: InvoiceFilter):
    try:
        # SQL Query: Reads directly from CalculatedPrice column
        sql = text("""
        SELECT 
            h.id AS InvoiceId,
            h.salesinvoicenbr AS InvoiceNbr,
            DATE_FORMAT(h.Salesinvoicesdate, '%d-%m-%Y') AS Salesinvoicesdate,
            COALESCE(c.CustomerName, 'Unknown') AS CustomerName,
            
            -- Subquery for PO Number (Cleanest way for 1:N relationship in list view)
            (SELECT d.PONumber FROM btg_userpanel_uat.tbl_salesinvoices_details d 
             WHERE d.salesinvoicesheaderid = h.id LIMIT 1) AS PONumber, 
            
            -- Subquery for Currency Code
            (SELECT mc.CurrencyCode 
             FROM btg_userpanel_uat.tbl_salesinvoices_details d 
             JOIN btg_userpanel_uat.master_currency mc ON d.Currencyid = mc.CurrencyId
             WHERE d.salesinvoicesheaderid = h.id LIMIT 1) AS CurrencyCode,
            
            h.TotalAmount,
            
            -- THE FIX: Direct Read
            -- We fallback to TotalAmount only if CalculatedPrice is NULL (old data)
            COALESCE(h.CalculatedPrice, h.TotalAmount) AS CalculatedPrice,
            
            CASE 
                WHEN h.IsSubmitted = 1 THEN 'Posted' 
                ELSE 'Saved' 
            END AS Status

        FROM btg_userpanel_uat.tbl_salesinvoices_header h
        LEFT JOIN btg_userpanel_uat.master_customer c ON h.customerid = c.Id
        
        WHERE h.Salesinvoicesdate BETWEEN :from_date AND :to_date
          AND (:customer_id = 0 OR h.customerid = :customer_id)
          AND h.isactive = 1 
          
        ORDER BY h.id DESC;
        """)

        async with engine.connect() as conn:
            result = await conn.execute(sql, {
                "from_date": filter_data.FromDate,
                "to_date": filter_data.ToDate,
                "customer_id": filter_data.customerid
            })
            
            rows = result.fetchall()
            return [dict(row._mapping) for row in rows]

    except Exception as e:
        print(f"Error fetching invoices: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))