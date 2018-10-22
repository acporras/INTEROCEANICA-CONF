﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalXML
{

    [Serializable]
    public class SUNATRetentionDocumentReference
    {
        public PartyIdentificationID ID { get; set; }
        public string IssueDate { get; set; }
        public PayableAmount TotalInvoiceAmount { get; set; }
        public Payment Payment { get; set; }
        public SUNATRetentionInformation SUNATRetentionInformation { get; set; }

        public SUNATRetentionDocumentReference()
        {
            ID = new PartyIdentificationID();
            TotalInvoiceAmount = new PayableAmount();
            Payment = new Payment();
            SUNATRetentionInformation = new SUNATRetentionInformation();
        }
    }
}