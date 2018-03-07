﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CoraCorpCM.Domain
{
    public class Exhibition
    {
        public Exhibition()
        {
            ExhibitionPieces = new HashSet<ExhibitionPiece>();
        }

        public int Id { get; set; }
        public Museum Museum { get; set; }
        public string Name { get; set; }
        public string Curator { get; set; }
        public string Theme { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Location Location { get; set; }
        public ICollection<ExhibitionPiece> ExhibitionPieces { get; set; }
    }
}