public record eSigEvent(
    string EvidenceUserID,
    string PdfDocumentBase64,
    string DocumentSignature,
    string Timestamp,
    string UserEmail,
    string Customer);
