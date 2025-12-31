from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from .database import engine, Base

# 1. IMPORT THE ROUTERS
from .routers import finance, invoice_api, bankbook  # <--- Add bankbook here

app = FastAPI(title="Finance API (Python)")

# Allow CORS
origins = [
    "http://localhost:3000",
    "http://localhost:5072",
]

app.add_middleware(
    CORSMiddleware,
    allow_origins=origins,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# 2. INCLUDE THE ROUTERS

# Existing Finance Router
app.include_router(finance.router)

# Existing Invoice Router (Prefixed with /api)
app.include_router(invoice_api.router, prefix="/api", tags=["Invoices"])

# --- NEW: Include BankBook Router ---
# Since your request URL was /api/AR/..., we must add the /api prefix here too.
app.include_router(bankbook.router, prefix="/api", tags=["Bank Book"]) 


@app.get("/")
def read_root():
    return {"message": "Finance API is running"}