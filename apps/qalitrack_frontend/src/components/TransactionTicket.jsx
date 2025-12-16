import React from "react";
import dayjs from "dayjs";

export default function TransactionTicket({ transaction }) {
  if (!transaction) return null;

  const {
    receiptNo,
    expectedWeighings,
    noPlate,
    driverName,
    transporterName,
    weighBridgeName,
    commodityName,
    supplierName,
    customerName,
    originName,
    destinationName,
    operation,
    weighMode,
    firstWeight,
    secondWeight,
    netWeight,
    operatorName,
    scaleName,
    createdAt,
  } = transaction;

  return (
    <div style={styles.page}>
      <div style={styles.header}>
        <strong>QALIBRATED SYSTEMS</strong>
        <div>WEIGHING TICKET</div>
        <div style={{ fontSize: 12 }}>
          {dayjs(createdAt).format("DD MMM YYYY HH:mm")}
        </div>
      </div>

      <div style={styles.grid}>
        <Field label="Receipt Number" value={receiptNo} />
        <Field label="Expected Weighings" value={expectedWeighings} />
        <Field label="Vehicle Plate" value={noPlate} />
        <Field label="Driver Name" value={driverName} />
        <Field label="Transporter" value={transporterName} />
        <Field label="Weighbridge" value={weighBridgeName} />
        <Field label="Commodity" value={commodityName} />
        <Field label="Supplier" value={supplierName} />
        <Field label="Customer" value={customerName} />
        <Field label="Origin" value={originName} />
        <Field label="Destination" value={destinationName} />
        <Field label="Operation" value={operation} />
        <Field label="Weigh Mode" value={weighMode} />
        <Field label="Operator" value={operatorName} />
        <Field label="Scale Name" value={scaleName || "Scale-01"} />
      </div>

      <div style={styles.weights}>
        <Weight label="FIRST WEIGHT" value={firstWeight} />
        <Weight label="SECOND WEIGHT" value={secondWeight} />
        <Weight label="NET WEIGHT" value={netWeight} />
      </div>

      <div style={styles.footer}>
        Powered by Qalibrated Systems <br />
        www.qalibrated.co.ke
      </div>
    </div>
  );
}

const Field = ({ label, value }) => (
  <div>
    <strong>{label}:</strong> {value || "-"}
  </div>
);

const Weight = ({ label, value }) => (
  <div>
    <div>{label}</div>
    <div style={{ fontSize: 20, fontWeight: "bold" }}>
      {value ?? "-"} kg
    </div>
  </div>
);

const styles = {
  page: {
    width: "210mm",
    minHeight: "297mm",
    padding: "20mm",
    fontFamily: "Arial",
  },
  header: {
    textAlign: "center",
    marginBottom: 20,
  },
  grid: {
    display: "grid",
    gridTemplateColumns: "1fr 1fr",
    gap: 8,
    fontSize: 13,
  },
  weights: {
    marginTop: 24,
    padding: 12,
    borderTop: "1px solid #000",
    borderBottom: "1px solid #000",
    display: "grid",
    gridTemplateColumns: "1fr 1fr 1fr",
    textAlign: "center",
  },
  footer: {
    marginTop: 40,
    textAlign: "center",
    fontSize: 12,
  },
};
