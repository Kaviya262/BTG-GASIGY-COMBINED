from fastapi import APIRouter, HTTPException
from pydantic import BaseModel
from typing import List, Optional
from sqlalchemy import text
from ..database import engine 

router = APIRouter()

# --- Request Model ---
class InvoiceFilter(BaseModel):
    customerid: int = 0
    FromDate: str
    ToDate: str
    BranchId: int = 1

# --- Response Model ---
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

# --- API Endpoint ---
@router.post("/GetALLInvoices", response_model=List[InvoiceListItem])
async def get_all_invoices(filter_data: InvoiceFilter):
    try:
        # SQL Query with IDR Conversion Logic
        sql = text("""
        SELECT 
            h.id AS InvoiceId,
            h.salesinvoicenbr AS InvoiceNbr,
            DATE_FORMAT(h.Salesinvoicesdate, '%d-%m-%Y') AS Salesinvoicesdate,
            COALESCE(c.CustomerName, 'Unknown') AS CustomerName,
            
            -- Use empty string for PO Number
            '' AS PONumber, 
            
            COALESCE(MAX(cur.CurrencyCode), '') AS CurrencyCode,
            
            h.TotalAmount,
            
            -- FIX: Calculate IDR Price = Total Amount * Exchange Rate
            -- If ExchangeRate is missing, default to 1 (treat as IDR/Same Currency)
            COALESCE(h.TotalAmount * MAX(cur.ExchangeRate), h.TotalAmount) AS CalculatedPrice,
            
            CASE 
                WHEN h.IsSubmitted = 1 THEN 'Posted' 
                ELSE 'Saved' 
            END AS Status

        FROM btg_userpanel_uat.tbl_salesinvoices_header h
        LEFT JOIN btg_userpanel_uat.master_customer c ON h.customerid = c.Id
        LEFT JOIN btg_userpanel_uat.tbl_salesinvoices_details d ON h.id = d.salesinvoicesheaderid
        LEFT JOIN btg_userpanel_uat.master_currency cur ON d.CurrencyId = cur.CurrencyId
        
        WHERE h.Salesinvoicesdate BETWEEN :from_date AND :to_date
          AND (:customer_id = 0 OR h.customerid = :customer_id)
          
        GROUP BY h.id, h.salesinvoicenbr, h.Salesinvoicesdate, c.CustomerName, h.TotalAmount, h.IsSubmitted
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