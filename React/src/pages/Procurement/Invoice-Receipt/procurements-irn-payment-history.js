import React, { useEffect, useState } from "react";
import { Row, Col, Label, Button, Input } from "reactstrap";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import Flatpickr from "react-flatpickr";

import {GetPaymentHistory} from "../../../common/data/mastersapi";
 

const PaymentHistory = ({ irnId }) => {
  const today = new Date();
  const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);

  const [data, setData] = useState([]);
  const [fromDate, setFromDate] = useState(firstDay);
  const [toDate, setToDate] = useState(today);

  useEffect(() => {
    if (irnId) {
      fetchPaymentHistory(irnId);
    }
  }, [irnId]);
  const formatDate = (date) => {
    if (!date) return "";
    const d = new Date(date);
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, "0");
    const day = String(d.getDate()).padStart(2, "0");
    return `${year}-${month}-${day}`;
  };
  const fetchPaymentHistory = async (irnId) => {
    console.log('fromDate > ', fromDate)
    console.log('toDate > ', toDate)
    try {

 
            const res = await GetPaymentHistory(
            1,
            1,
            irnId,
            formatDate(fromDate),
            formatDate(today)
          );
            if (res.status) {
              setData(res.data);
    
            } 

      // Replace with API call
      // const response = await fetch(`/api/payment-history/${irnId}`);
      // const result = await response.json();
      // setData(result);

      // Mock data
    
   
    } catch (err) {
      console.error("Error fetching payment history:", err);
    }
  };

  const handleExport = () => {
    // Export logic here (CSV/Excel)
    console.log("Exporting data:", data);
  };

  return (
    <div className="container mt-3">
      {/* Filters */}
      <Row className="mb-3">
        <Col md="3">
          <Label>From Date</Label>
          <Flatpickr
            name="fromDate"
            className="form-control"
            value={fromDate || null}
            onChange={(date) => setFromDate(date[0])}
            options={{
              altInput: true,
              altFormat: "d-M-Y",   // display format (ex: 24-Sep-2025)
              dateFormat: "Y-m-d",  // actual value format (ex: 2025-09-24)
            }}
          />
        </Col>

        <Col md="3">
          <Label>To Date</Label>
          <Flatpickr
            name="toDate"
            className="form-control"
            value={toDate || null}
            onChange={(date) => setToDate(date[0])}
            options={{
              altInput: true,
              altFormat: "d-M-Y",
              dateFormat: "Y-m-d",
            }}
          />
        </Col>

        <Col md="3" className="d-flex align-items-end">
          <Button color="primary" onClick={() => fetchPaymentHistory(irnId)}>
            Search
          </Button>
        </Col>

        {/* <Col md="3" className="d-flex align-items-end justify-content-end">
          <Button color="success" onClick={handleExport}>
            <i className="bx bx-export me-2"></i> Export
          </Button>
        </Col> */}
      </Row>

      {/* Data Table */}
      <DataTable value={data} paginator rows={5} responsiveLayout="scroll">
        <Column header="#" body={(_, { rowIndex }) => rowIndex + 1} />
        <Column field="suppliername" header="PO Supplier" />
        <Column field="pono" header="PO No." />
        <Column field="receipt_no" header="IRN No." />
        <Column
          field="createddate"
          header="Payment Date"
          body={(row) => row.createddate?.split("T")[0]}
        />
        <Column field="balance_payment" header="Payment" body={(row) => parseFloat(row.balance_payment || 0).toLocaleString("en-US")} />
      </DataTable>
    </div>
  );
};

export default PaymentHistory;
