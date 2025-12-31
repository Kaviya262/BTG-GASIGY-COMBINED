from fastapi import APIRouter, HTTPException, Request
from pydantic import BaseModel
from typing import Optional
import mysql.connector
from datetime import datetime
import os
from dotenv import load_dotenv

load_dotenv()

router = APIRouter(
    prefix="/procurement",
    tags=["Procurement"]
)

class ReplyRequest(BaseModel):
    pr_id: int
    org_id: int
    branch_id: int
    user_id: int

class SavePRReplyRequest(BaseModel):
    pr_id: int
    reply: str
    name: str
    sender: str

def get_db_connection_sync():
    return mysql.connector.connect(
        host=os.getenv('DB_HOST'),
        user=os.getenv('DB_USER'),
        password=os.getenv('DB_PASSWORD'),
        database=os.getenv('DB_NAME_PURCHASE'),
        ssl_disabled=True
    )

@router.post("/save_pr_reply")
async def save_pr_reply(req: SavePRReplyRequest):
    print("Received data:", req.dict())

    pr_id = req.pr_id
    reply = req.reply.strip()
    name = req.name
    sender = req.sender

    if not pr_id or not reply:
        raise HTTPException(status_code=400, detail="Missing pr_id or reply")

    timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    reply_entry = f"[{name} at {timestamp}]: {reply}"

    # Determine status based on sender
    # If GM replies, status -> 'S' (Processed/Seen?) and IsSubmitted=0?
    # If User replies, status -> 'Posted' and IsSubmitted=1 (Pending GM review?)
    if sender == "GM":
        is_submitted = 0
        status_value = 'S'
    else:
        is_submitted = 1
        status_value = 'Posted'

    conn = None
    cursor = None

    try:
        conn = get_db_connection_sync()
        cursor = conn.cursor()

        # Get current pr_comment
        cursor.execute("SELECT pr_comment FROM tbl_PurchaseRequisition_Header WHERE PRId = %s", (pr_id,))
        row = cursor.fetchone()
        if not row:
            raise HTTPException(status_code=404, detail=f"PR not found with ID {pr_id}")

        current_comment = row[0] or ""
        new_comment = current_comment + "\n" + reply_entry if current_comment else reply_entry

        # Update pr_comment, IsSubmitted
        update_query = """
            UPDATE tbl_PurchaseRequisition_Header
            SET pr_comment = %s, IsSubmitted = %s
            WHERE PRId = %s
        """
        cursor.execute(update_query, (new_comment, is_submitted, pr_id))

        if cursor.rowcount == 0:
            # Should not happen if select found it, unless race condition
            pass 

        conn.commit()
        return {
            "success": True,
            "message": "Reply saved",
            "reply_saved": True,
            "submitted": is_submitted == 1,
            "status": status_value
        }

    except mysql.connector.Error as e:
        print("MYSQL ERROR >>>", repr(e))
        if conn:
            conn.rollback()
        raise HTTPException(status_code=500, detail=f"Database error: {e}")

    except Exception as e:
        print("GENERIC ERROR >>>", repr(e))
        raise HTTPException(status_code=500, detail=f"Internal Server Error: {e}")

    finally:
        if cursor:
            cursor.close()
        if conn:
            conn.close()
