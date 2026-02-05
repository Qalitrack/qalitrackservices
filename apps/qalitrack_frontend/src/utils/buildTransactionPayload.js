const isValidGuid = (guid) =>
  typeof guid === "string" &&
  /^[0-9a-fA-F-]{36}$/.test(guid) &&
  guid !== "00000000-0000-0000-0000-000000000000";

export const buildTransactionPayload = ({
  form,
  weight,
  operator,
}) => {
  const payload = {
    noPlate: form.noPlate.toUpperCase().trim(),
    firstWeight: String(weight),
    weighMode: form.weighMode || "entry",
    operation: form.operation || "weighing",
  };

  if (isValidGuid(form.vehicleID)) payload.vehicleID = form.vehicleID;
  if (isValidGuid(form.customerID)) payload.customerID = form.customerID;
  if (isValidGuid(form.productID)) payload.productID = form.productID;

  payload.operatorName = operator?.name || "Self-Service Kiosk";
  payload.operatorID = operator?.id ?? null;

  return payload;
};
