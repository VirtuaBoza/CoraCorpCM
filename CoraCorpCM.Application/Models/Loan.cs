﻿using System;
using System.Collections.Generic;

namespace CoraCorpCM.Application.Models
{
    public class Loan : IEntity<int>
    {
        public Loan()
        {
            LoanPieces = new HashSet<LoanPiece>();
        }

        public int Id { get; set; }

        public Museum Museum { get; set; }
        public int MuseumId { get; set; }

        public Location FromLocation { get; set; }
        public Location ToLocation { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public Exhibition Exhibition { get; set; }
        public string Terms { get; set; }

        public ICollection<LoanPiece> LoanPieces { get; set; }
    }
}
