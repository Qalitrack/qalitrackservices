namespace Transaction.Core.Entities;
    public class WeighingRecord : BaseEntity
    {
        public string WeighbridgeTransactionId { get; set; } = string.Empty;
        public int WeighingSequence { get; set; } // 1, 2, 3, etc.
        public decimal Weight { get; set; }
        public DateTime WeighingDate { get; set; }
        public Guid? WeighBridgeId { get; set; }
        public string WeighBridgeName { get; set; } = string.Empty;
        public string ScaleName { get; set; } = string.Empty;
        public Guid? OperatorId { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    
        // Navigation
        public virtual WeighbridgeTransaction Transaction { get; set; } = null!;
    }
