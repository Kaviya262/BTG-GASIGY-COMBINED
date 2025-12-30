import React, { useState, useEffect, useRef, useMemo } from "react";
import { Container, Row, Col, Card, CardBody, Label, Button, Spinner, Modal, ModalHeader, ModalBody, ModalFooter, Input } from "reactstrap";
import Breadcrumbs from "../../../components/Common/Breadcrumb";
import Flatpickr from "react-flatpickr";
import "flatpickr/dist/themes/material_green.css";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { InputText } from "primereact/inputtext";
import Select from "react-select";
import * as XLSX from "xlsx";
import { format } from "date-fns";
import { toast } from "react-toastify";
import axios from "axios";
import { PYTHON_API_URL } from "common/pyapiconfig";

import { getARBook, GetCustomerFilter } from "../service/financeapi";
import { GetAllCurrencies } from "../../../common/data/mastersapi";

const ARBookDOReport = () => {
  const today = new Date();
  const firstDay = new Date(today.getFullYear(), today.getMonth() - 2, 1);

  const [arBook, setArBook] = useState([]);
  const [customers, setCustomers] = useState([]);
  const [selectedCustomer, setSelectedCustomer] = useState(null);
  const [fromDate, setFromDate] = useState(firstDay);
  const [toDate, setToDate] = useState(today);
  const [globalFilter, setGlobalFilter] = useState("");
  const dtRef = useRef(null);

  // --- SELECTION & CONVERSION STATES ---
  const [selectedRows, setSelectedRows] = useState([]); // Stores selected rows
  const [isConvertModalOpen, setIsConvertModalOpen] = useState(false);
  const [newInvoiceNo, setNewInvoiceNo] = useState(""); // Single Input for Group Update
  const [isSaving, setIsSaving] = useState(false);

  const [currencyRates, setCurrencyRates] = useState({});
  const [currencyOptions, setCurrencyOptions] = useState([{ label: "All", value: "All" }]);
  const [selectedCurrency, setSelectedCurrency] = useState({ label: "All", value: "All" });

  useEffect(() => {
    const loadMasters = async () => {
      try {
        const custRes = await GetCustomerFilter(1, "%");
        setCustomers(custRes);
        if (custRes && custRes.length > 0) {
          setSelectedCustomer(custRes[0]);
        }

        const currRes = await GetAllCurrencies({ currencyCode: "", currencyName: "" });
        const currencyData = currRes.data || currRes;

        if (Array.isArray(currencyData)) {
          const rates = {};
          currencyData.forEach(c => {
            rates[c.CurrencyCode] = c.ExchangeRate || c.Rate || c.SellingRate || 1;
          });
          setCurrencyRates(rates);
        }
      } catch (error) {
        console.error("Error loading masters:", error);
      }
    };
    loadMasters();
  }, []);

  useEffect(() => {
    if (selectedCustomer) {
      fetchARBook();
    }
  }, [selectedCustomer]);

  const parseDate = (dateStr) => {
    if (!dateStr) return new Date(0);
    return new Date(dateStr);
  };

  const fetchARBook = async () => {
    try {
      setSelectedRows([]); // Reset selection on refresh
      const data = await getARBook(
        selectedCustomer ? selectedCustomer.value : 0,
        1,
        1,
        fromDate ? format(fromDate, "yyyy-MM-dd") : null,
        toDate ? format(toDate, "yyyy-MM-dd") : null
      );

      if (data.status && data.data?.length > 0) {
        let rawData = data.data;
        rawData.sort((a, b) => parseDate(a.ledger_date) - parseDate(b.ledger_date));

        const uniqueCurrencies = [...new Set(rawData.map(item => item.currencycode || item.CurrencyCode))];
        const newCurrencyOptions = [
          { label: "All", value: "All" },
          ...uniqueCurrencies.filter(c => c).map(c => ({ label: c, value: c }))
        ];
        setCurrencyOptions(newCurrencyOptions);

        const processedData = rawData.map(row => {
          const currency = row.currencycode || row.CurrencyCode || "IDR";
          const rate = (currency === "IDR") ? 1 : (currencyRates[currency] || 1);

          return {
            ...row,
            uniqueId: row.id || row.invoice_id || Math.random(),
            convertedInvoiceAmount: (parseFloat(row.invoice_amount) || 0) * rate,
            convertedReceiptAmount: (parseFloat(row.receipt_amount) || 0) * rate,
            convertedDebitNote: (parseFloat(row.debit_note_amount) || 0) * rate,
            convertedCreditNote: (parseFloat(row.credit_note_amount) || 0) * rate,
          };
        });

        setArBook(processedData);
      } else {
        setArBook([]);
      }
    } catch (err) {
      toast.error("Failed to load AR data");
      setArBook([]);
    }
  };

  // --- CONVERT TO INVOICE HANDLER ---
  const handleConvertSubmit = async () => {
    if (!newInvoiceNo.trim()) {
      toast.error("Please enter an Invoice Number");
      return;
    }
    if (selectedRows.length === 0) return;

    setIsSaving(true);
    let successCount = 0;

    try {
      // Loop through all selected rows and update them to the SAME newInvoiceNo
      const updatePromises = selectedRows.map(row => {
        return axios.put(`${PYTHON_API_URL}/AR/update-reference`, {
          id: parseInt(row.uniqueId),
          new_reference: newInvoiceNo // Apply same invoice no to all
        }).then(() => {
          successCount++;
        }).catch(err => {
          console.error(`Failed to update ID ${row.uniqueId}`, err);
        });
      });

      await Promise.all(updatePromises);

      if (successCount > 0) {
        toast.success(`Converted ${successCount} DO(s) to Invoice ${newInvoiceNo}`);
        setIsConvertModalOpen(false);
        setNewInvoiceNo("");
        fetchARBook();
      } else {
        toast.error("Failed to convert records");
      }

    } catch (err) {
      console.error(err);
      toast.error("An error occurred while converting");
    } finally {
      setIsSaving(false);
    }
  };

  const finalProcessedData = useMemo(() => {
    let filtered = selectedCurrency && selectedCurrency.value !== "All"
      ? arBook.filter((x) => (x.CurrencyCode || x.currencycode) === selectedCurrency.value)
      : arBook;

    filtered = filtered.filter(item => item.invoice_no?.includes("DO"));

    let runningBalance = 0;
    return filtered.map(row => {
      runningBalance += (row.convertedInvoiceAmount + row.convertedDebitNote - row.convertedReceiptAmount - row.convertedCreditNote);
      return { ...row, cumulativeBalance: runningBalance };
    });
  }, [arBook, selectedCurrency]);

  const exportExcel = () => {
    const exportData = finalProcessedData.map(item => ({
      Date: format(new Date(item.ledger_date), "dd-MMM-yyyy"),
      "Reference No.": item.invoice_no,
      "Invoice Amount (A)": item.convertedInvoiceAmount,
      "Debit Note (B)": item.convertedDebitNote,
      "Receipt (C)": item.convertedReceiptAmount,
      "Credit Note (D)": item.convertedCreditNote,
      "Balance ((A+B)-(C+D))": item.cumulativeBalance
    }));
    const ws = XLSX.utils.json_to_sheet(exportData);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, "AR Book DO");
    XLSX.writeFile(wb, "AR_Book_DO.xlsx");
  };

  return (
    <div className="page-content">
      <Container fluid>
        <Breadcrumbs title="Reports" breadcrumbItem="AR Book DO" />
        <Row>
          <Col lg="12">
            <Card>
              <CardBody>
                {/* Filters Section */}
                <Row className="mb-3 align-items-end">
                  <Col md="3" className="d-flex align-items-center">
                    <Label className="me-2 mb-0">Customer:</Label>
                    <Select options={customers} onChange={setSelectedCustomer} value={selectedCustomer} isClearable className="flex-grow-1" />
                  </Col>
                  <Col md="3" className="d-flex align-items-center">
                    <Label className="me-2 mb-0">Currency:</Label>
                    <Select options={currencyOptions} value={selectedCurrency} onChange={setSelectedCurrency} isClearable className="flex-grow-1" />
                  </Col>
                  <Col md="3" className="d-flex align-items-center">
                    <Label className="me-2 mb-0">From:</Label>
                    <Flatpickr className="form-control" value={fromDate} onChange={(date) => setFromDate(date[0])} options={{ altInput: true, altFormat: "d-M-Y", dateFormat: "Y-m-d" }} />
                  </Col>
                  <Col md="3" className="d-flex align-items-center">
                    <Label className="me-2 mb-0">To:</Label>
                    <Flatpickr className="form-control" value={toDate} onChange={(date) => setToDate(date[0])} options={{ altInput: true, altFormat: "d-M-Y", dateFormat: "Y-m-d" }} />
                  </Col>

                  {/* BUTTONS ROW */}
                  <Col md="12" className="text-end mt-3">
                    {/* NEW: Convert To Invoice Button */}
                    <Button
                      color="warning"
                      className="me-2"
                      onClick={() => setIsConvertModalOpen(true)}
                      disabled={selectedRows.length === 0}
                    >
                      Convert to Invoice
                    </Button>

                    <button type="button" className="btn btn-primary me-2" onClick={fetchARBook}>Search</button>
                    <button type="button" className="btn btn-success me-2" onClick={exportExcel}>Export</button>
                    <button type="button" className="btn btn-secondary" onClick={() => window.print()}>Print</button>
                  </Col>
                </Row>

                <div className="table-responsive">
                  <DataTable
                    ref={dtRef}
                    value={finalProcessedData}
                    paginator rows={20}
                    globalFilter={globalFilter}
                    header={
                      <div className="d-flex justify-content-end">
                        <InputText type="search" placeholder="Global Search" className="form-control" style={{ width: "250px" }} value={globalFilter} onChange={(e) => setGlobalFilter(e.target.value)} />
                      </div>
                    }
                    responsiveLayout="scroll"
                    selection={selectedRows}
                    onSelectionChange={e => setSelectedRows(e.value)}
                    dataKey="uniqueId"
                  >
                    {/* NEW: Selection Column */}
                    <Column selectionMode="multiple" headerStyle={{ width: '3em' }}></Column>

                    <Column field="ledger_date" header="Date"
                      body={(row) => format(new Date(row.ledger_date), "dd-MMM-yyyy")}
                      headerStyle={{ whiteSpace: 'nowrap' }} />

                    {/* CHANGED: Reference No is now Read-Only */}
                    <Column field="invoice_no" header="Reference No." headerStyle={{ whiteSpace: 'nowrap' }} />

                    <Column field="convertedInvoiceAmount" header="Invoice Amount (A)"
                      body={(d) => d.convertedInvoiceAmount?.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                      headerStyle={{ whiteSpace: 'nowrap' }} />

                    <Column field="convertedDebitNote" header="Debit Note (B)"
                      body={(d) => d.convertedDebitNote?.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                      headerStyle={{ whiteSpace: 'nowrap' }}
                    />

                    <Column field="convertedReceiptAmount" header="Receipt (C)"
                      body={(d) => d.convertedReceiptAmount?.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                      headerStyle={{ whiteSpace: 'nowrap' }}
                    />

                    <Column field="convertedCreditNote" header="Credit Note (D)"
                      body={(d) => d.convertedCreditNote?.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                      headerStyle={{ whiteSpace: 'nowrap' }}
                    />

                    <Column field="cumulativeBalance" header="Balance ((A+B)-(C+D))"
                      body={(d) => d.cumulativeBalance?.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                      headerStyle={{ whiteSpace: 'nowrap' }}
                    />
                  </DataTable>
                </div>
              </CardBody>
            </Card>
          </Col>
        </Row>

        {/* --- CONVERT TO INVOICE POPUP --- */}
        <Modal isOpen={isConvertModalOpen} toggle={() => setIsConvertModalOpen(false)} centered>
          <ModalHeader toggle={() => setIsConvertModalOpen(false)}>Convert DO to Invoice</ModalHeader>
          <ModalBody>
            <div className="alert alert-info">
              You have selected <strong>{selectedRows.length}</strong> DO(s) to convert.
            </div>
            <div className="mb-3">
              <Label className="fw-bold">New Invoice Number:</Label>
              <Input
                type="text"
                placeholder="Enter Invoice No (e.g. INV-2025-001)"
                value={newInvoiceNo}
                onChange={(e) => setNewInvoiceNo(e.target.value)}
              />
              <small className="text-muted">This invoice number will be applied to all selected entries.</small>
            </div>
          </ModalBody>
          <ModalFooter>
            <Button color="secondary" onClick={() => setIsConvertModalOpen(false)}>Cancel</Button>
            <Button color="primary" onClick={handleConvertSubmit} disabled={isSaving}>
              {isSaving ? <Spinner size="sm" /> : "Save"}
            </Button>
          </ModalFooter>
        </Modal>

      </Container>
    </div>
  );
};

export default ARBookDOReport;